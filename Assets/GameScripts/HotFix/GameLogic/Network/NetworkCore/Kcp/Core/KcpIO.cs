using System.Buffers;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using BufferOwner = System.Buffers.IMemoryOwner<byte>;

namespace System.Net.Sockets.Kcp
{
    /// <summary>
    /// <inheritdoc cref="IPipe{T}"/>
    /// <para></para>这是个简单的实现,更复杂使用微软官方实现<see cref="Channel.CreateBounded{T}(int)"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class QueuePipe<T> : Queue<T>
    {
        readonly object _innerLock = new object();
        private TaskCompletionSource<T> source;

        //线程同步上下文由Task机制保证，无需额外处理
        //SynchronizationContext callbackContext;
        //public bool UseSynchronizationContext { get; set; } = true;

        public virtual void Write(T item)
        {
            lock (_innerLock)
            {
                if (source == null)
                {
                    Enqueue(item);
                }
                else
                {
                    if (Count > 0)
                    {
                        throw new Exception("内部顺序错误，不应该出现，请联系作者");
                    }

                    var next = source;
                    source = null;
                    next.TrySetResult(item);
                }
            }
        }

        public new void Enqueue(T item)
        {
            lock (_innerLock)
            {
                base.Enqueue(item);
            }
        }

        public void Flush()
        {
            lock (_innerLock)
            {
                if (Count > 0)
                {
                    var res = Dequeue();
                    var next = source;
                    source = null;
                    next?.TrySetResult(res);
                }
            }
        }

        public virtual Task<T> ReadAsync()
        {
            lock (_innerLock)
            {
                if (this.Count > 0)
                {
                    var next = Dequeue();
                    return Task.FromResult(next);
                }
                else
                {
                    source = new TaskCompletionSource<T>();
                    return source.Task;
                }
            }
        }

        public UniTask<T> ReadValueTaskAsync()
        {
            throw new NotImplementedException();
        }
    }

    public class KcpIO<Segment> : KcpCore<Segment>, IKcpIO
        where Segment : IKcpSegment
    {
        OutputQ outq;

        public KcpIO(uint conv_) : base(conv_)
        {
            outq = new OutputQ();
            callbackHandle = outq;
        }

        internal override void Parse_data(Segment newseg)
        {
            base.Parse_data(newseg);

            lock (rcv_queueLock)
            {
                var recover = false;
                if (rcv_queue.Count >= rcv_wnd)
                {
                    recover = true;
                }

                while (TryRecv(out var arraySegment) > 0)
                {
                    recvSignal.Enqueue(arraySegment);
                }

                recvSignal.Flush();

                #region fast recover

                /// fast recover
                if (rcv_queue.Count < rcv_wnd && recover)
                {
                    // ready to send back IKCP_CMD_WINS in ikcp_flush
                    // tell remote my window size
                    probe |= IKCP_ASK_TELL;
                }

                #endregion
            }
        }

        QueuePipe<ArraySegment<Segment>> recvSignal = new QueuePipe<ArraySegment<Segment>>();

        internal int TryRecv(out ArraySegment<Segment> package)
        {
            package = default;
            lock (rcv_queueLock)
            {
                var peekSize = -1;
                if (rcv_queue.Count == 0)
                {
                    ///没有可用包
                    return -1;
                }

                var seq = rcv_queue[0];

                if (seq.frg == 0)
                {
                    peekSize = (int)seq.len;
                }

                if (rcv_queue.Count < seq.frg + 1)
                {
                    ///没有足够的包
                    return -1;
                }

                uint length = 0;

                Segment[] kcpSegments = ArrayPool<Segment>.Shared.Rent(seq.frg + 1);

                var index = 0;
                foreach (var item in rcv_queue)
                {
                    kcpSegments[index] = item;
                    index++;
                    length += item.len;
                    if (item.frg == 0)
                    {
                        break;
                    }
                }

                if (index > 0)
                {
                    rcv_queue.RemoveRange(0, index);
                }

                package = new ArraySegment<Segment>(kcpSegments, 0, index);

                peekSize = (int)length;

                if (peekSize <= 0)
                {
                    return -2;
                }

                return peekSize;
            }
        }

        public async UniTask RecvAsync(IBufferWriter<byte> writer, object options = null)
        {
            var arraySegment = await recvSignal.ReadAsync().ConfigureAwait(false);
            for (int i = arraySegment.Offset; i < arraySegment.Count; i++)
            {
                WriteRecv(writer, arraySegment.Array[i]);
            }

            ArrayPool<Segment>.Shared.Return(arraySegment.Array, true);
        }

        private void WriteRecv(IBufferWriter<byte> writer, Segment seg)
        {
            var curCount = (int)seg.len;
            var target = writer.GetSpan(curCount);
            seg.data.CopyTo(target);
            SegmentManager.Free(seg);
            writer.Advance(curCount);
        }

        public async UniTask<int> RecvAsync(ArraySegment<byte> buffer, object options = null)
        {
            var arraySegment = await recvSignal.ReadAsync().ConfigureAwait(false);
            int start = buffer.Offset;
            for (int i = arraySegment.Offset; i < arraySegment.Count; i++)
            {
                var target = new Memory<byte>(buffer.Array, start, buffer.Array.Length - start);

                var seg = arraySegment.Array[i];
                seg.data.CopyTo(target.Span);
                start += seg.data.Length;

                SegmentManager.Free(seg);
            }

            ArrayPool<Segment>.Shared.Return(arraySegment.Array, true);
            return start - buffer.Offset;
        }

        public async UniTask OutputAsync(IBufferWriter<byte> writer, object options = null)
        {
            var (Owner, Count) = await outq.ReadAsync().ConfigureAwait(false);
            WriteOut(writer, Owner, Count);
        }

        private static void WriteOut(IBufferWriter<byte> writer, BufferOwner Owner, int Count)
        {
            var target = writer.GetSpan(Count);
            Owner.Memory.Span.Slice(0, Count).CopyTo(target);
            writer.Advance(Count);
            Owner.Dispose();
        }

        protected internal override BufferOwner CreateBuffer(int needSize)
        {
            return MemoryPool<byte>.Shared.Rent(needSize);
        }

        internal class OutputQ : QueuePipe<(BufferOwner Owner, int Count)>,
            IKcpCallback
        {
            public void Output(BufferOwner buffer, int avalidLength)
            {
                Write((buffer, avalidLength));
            }
        }
    }
}