using System.Buffers;
using Cysharp.Threading.Tasks;

namespace System.Net.Sockets.Kcp
{
    /// <summary>
    /// 用于调试的KCP IO 类，没有Kcp功能
    /// </summary>
    public class FakeKcpIO : IKcpIO
    {
        QueuePipe<byte[]> recv = new QueuePipe<byte[]>();
        public int Input(ReadOnlySpan<byte> span)
        {
            byte[] buffer = new byte[span.Length];
            span.CopyTo(buffer);
            recv.Write(buffer);
            return 0;
        }

        public int Input(ReadOnlySequence<byte> span)
        {
            byte[] buffer = new byte[span.Length];
            span.CopyTo(buffer);
            return Input(buffer);
        }

        public async UniTask RecvAsync(IBufferWriter<byte> writer, object options = null)
        {
            var buffer = await recv.ReadAsync().ConfigureAwait(false);
            var target = writer.GetMemory(buffer.Length);
            buffer.AsSpan().CopyTo(target.Span);
            writer.Advance(buffer.Length);
        }

        public async UniTask<int> RecvAsync(ArraySegment<byte> buffer, object options = null)
        {
            var temp = await recv.ReadAsync().ConfigureAwait(false);
            temp.AsSpan().CopyTo(buffer);
            return temp.Length;
        }

        QueuePipe<byte[]> send = new QueuePipe<byte[]>();
        public int Send(ReadOnlySpan<byte> span, object options = null)
        {
            byte[] buffer = new byte[span.Length];
            span.CopyTo(buffer);
            send.Write(buffer);
            return 0;
        }

        public int Send(ReadOnlySequence<byte> span, object options = null)
        {
            byte[] buffer = new byte[span.Length];
            span.CopyTo(buffer);
            return Send(buffer);
        }

        public async UniTask OutputAsync(IBufferWriter<byte> writer, object options = null)
        {
            var buffer = await send.ReadAsync().ConfigureAwait(false);
            Write(writer, buffer);
        }

        private static void Write(IBufferWriter<byte> writer, byte[] buffer)
        {
            var span = writer.GetSpan(buffer.Length);
            buffer.AsSpan().CopyTo(span);
            writer.Advance(buffer.Length);
        }
    }
}
