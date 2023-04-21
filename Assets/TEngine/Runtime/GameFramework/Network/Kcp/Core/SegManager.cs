using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Buffers.Binary;

namespace System.Net.Sockets.Kcp
{
    /// <summary>
    /// 动态申请非托管内存
    /// </summary>
    public class SimpleSegManager : ISegmentManager<KcpSegment>
    {
        public static SimpleSegManager Default { get; } = new SimpleSegManager();
        public KcpSegment Alloc(int appendDateSize)
        {
            return KcpSegment.AllocHGlobal(appendDateSize);
        }

        public void Free(KcpSegment seg)
        {
            KcpSegment.FreeHGlobal(seg);
        }

        public class Kcp : Kcp<KcpSegment>
        {
            public Kcp(uint conv_, IKcpCallback callback, IRentable rentable = null)
                : base(conv_, callback, rentable)
            {
                SegmentManager = Default;
            }
        }

        public class KcpIO : KcpIO<KcpSegment>
        {
            public KcpIO(uint conv_)
                : base(conv_)
            {
                SegmentManager = Default;
            }
        }
    }

    /// <summary>
    /// 申请固定大小非托管内存。使用这个就不能SetMtu了，大小已经写死。
    /// </summary>
    /// <remarks>需要大量测试</remarks>
    public unsafe class UnSafeSegManager : ISegmentManager<KcpSegment>
    {
        public static UnSafeSegManager Default { get; } = new UnSafeSegManager();
        /// <summary>
        /// 因为默认mtu是1400，并且内存需要内存行/内存页对齐。这里直接512对齐。
        /// </summary>
        public const int blockSize = 512 * 3;
        public HashSet<IntPtr> header = new HashSet<IntPtr>();
        public Stack<IntPtr> blocks = new Stack<IntPtr>();
        public readonly object locker = new object();
        public UnSafeSegManager()
        {
            Alloc();
        }

        void Alloc()
        {
            int count = 50;
            IntPtr intPtr = Marshal.AllocHGlobal(blockSize * count);
            header.Add(intPtr);
            for (int i = 0; i < count; i++)
            {
                blocks.Push(intPtr + blockSize * i);
            }
        }

        ~UnSafeSegManager()
        {
            foreach (var item in header)
            {
                Marshal.FreeHGlobal(item);
            }
        }

        public KcpSegment Alloc(int appendDateSize)
        {
            lock (locker)
            {
                var total = KcpSegment.LocalOffset + KcpSegment.HeadOffset + appendDateSize;
                if (total > blockSize)
                {
                    throw new ArgumentOutOfRangeException();
                }

                if (blocks.Count > 0)
                {

                }
                else
                {
                    Alloc();
                }

                var ptr = blocks.Pop();
                Span<byte> span = new Span<byte>(ptr.ToPointer(), blockSize);
                span.Clear();
                return new KcpSegment((byte*)ptr.ToPointer(), (uint)appendDateSize);
            }
        }

        public void Free(KcpSegment seg)
        {
            IntPtr ptr = (IntPtr)seg.ptr;
            blocks.Push(ptr);
        }

        public class Kcp : Kcp<KcpSegment>
        {
            public Kcp(uint conv_, IKcpCallback callback, IRentable rentable = null)
                : base(conv_, callback, rentable)
            {
                SegmentManager = Default;
            }
        }

        public class KcpIO : KcpIO<KcpSegment>
        {
            public KcpIO(uint conv_)
                : base(conv_)
            {
                SegmentManager = Default;
            }
        }
    }


    /// <summary>
    /// 使用内存池，而不是非托管内存，有内存alloc，但是不多。可以解决Marshal.AllocHGlobal 内核调用带来的性能问题
    /// </summary>
    public class PoolSegManager : ISegmentManager<PoolSegManager.Seg>
    {
        public static PoolSegManager Default { get; } = new PoolSegManager();

        /// <summary>
        /// 因为默认mtu是1400，并且内存需要内存行/内存页对齐。这里直接512对齐。
        /// </summary>
        public const int blockSize = 512 * 3;
        public class Seg : IKcpSegment
        {
            byte[] cache;
            public Seg(int blockSize)
            {
                cache = Buffers.ArrayPool<byte>.Shared.Rent(blockSize);
            }

            ///以下为需要网络传输的参数
            public const int LocalOffset = 4 * 4;
            public const int HeadOffset = Kcp.IKCP_OVERHEAD;

            public byte cmd { get; set; }
            public uint conv { get; set; }
            public Span<byte> data => cache.AsSpan().Slice(0, (int)len);
            public uint fastack { get; set; }
            public byte frg { get; set; }
            public uint len { get; internal set; }
            public uint resendts { get; set; }
            public uint rto { get; set; }
            public uint sn { get; set; }
            public uint ts { get; set; }
            public uint una { get; set; }
            public ushort wnd { get; set; }
            public uint xmit { get; set; }

            public int Encode(Span<byte> buffer)
            {
                var datelen = (int)(HeadOffset + len);

                ///备用偏移值 现阶段没有使用
                const int offset = 0;

                if (BitConverter.IsLittleEndian)
                {
                    BinaryPrimitives.WriteUInt32LittleEndian(buffer.Slice(offset), conv);
                    buffer[offset + 4] = cmd;
                    buffer[offset + 5] = frg;
                    BinaryPrimitives.WriteUInt16LittleEndian(buffer.Slice(offset + 6), wnd);

                    BinaryPrimitives.WriteUInt32LittleEndian(buffer.Slice(offset + 8), ts);
                    BinaryPrimitives.WriteUInt32LittleEndian(buffer.Slice(offset + 12), sn);
                    BinaryPrimitives.WriteUInt32LittleEndian(buffer.Slice(offset + 16), una);
                    BinaryPrimitives.WriteUInt32LittleEndian(buffer.Slice(offset + 20), len);

                    data.CopyTo(buffer.Slice(HeadOffset));
                }
                else
                {
                    BinaryPrimitives.WriteUInt32BigEndian(buffer.Slice(offset), conv);
                    buffer[offset + 4] = cmd;
                    buffer[offset + 5] = frg;
                    BinaryPrimitives.WriteUInt16BigEndian(buffer.Slice(offset + 6), wnd);

                    BinaryPrimitives.WriteUInt32BigEndian(buffer.Slice(offset + 8), ts);
                    BinaryPrimitives.WriteUInt32BigEndian(buffer.Slice(offset + 12), sn);
                    BinaryPrimitives.WriteUInt32BigEndian(buffer.Slice(offset + 16), una);
                    BinaryPrimitives.WriteUInt32BigEndian(buffer.Slice(offset + 20), len);

                    data.CopyTo(buffer.Slice(HeadOffset));
                }

                return datelen;
            }
        }
        ConcurrentStack<Seg> Pool = new ConcurrentStack<Seg>();
        public Seg Alloc(int appendDateSize)
        {
            if (appendDateSize > blockSize)
            {
                throw new NotSupportedException();
            }
            if (Pool.TryPop(out var ret))
            {
            }
            else
            {
                ret = new Seg(blockSize);
            }
            ret.len = (uint)appendDateSize;
            return ret;
        }

        public void Free(Seg seg)
        {
            seg.cmd = 0;
            seg.conv = 0;
            seg.fastack = 0;
            seg.frg = 0;
            seg.len = 0;
            seg.resendts = 0;
            seg.rto = 0;
            seg.sn = 0;
            seg.ts = 0;
            seg.una = 0;
            seg.wnd = 0;
            seg.xmit = 0;
            Pool.Push(seg);
        }

        public class Kcp : Kcp<Seg>
        {
            public Kcp(uint conv_, IKcpCallback callback, IRentable rentable = null)
                : base(conv_, callback, rentable)
            {
                SegmentManager = Default;
            }
        }

        public class KcpIO : KcpIO<Seg>
        {
            public KcpIO(uint conv_)
                : base(conv_)
            {
                SegmentManager = Default;
            }
        }
    }
}



