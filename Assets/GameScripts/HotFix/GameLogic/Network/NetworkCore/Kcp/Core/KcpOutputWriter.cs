using System.Buffers;

namespace System.Net.Sockets.Kcp
{
    public abstract class KcpOutputWriter : IKcpOutputWriter
    {
        public int UnflushedBytes { get; set; }
        public IMemoryOwner<byte> MemoryOwner { get; set; }
        public void Flush()
        {
            Output(MemoryOwner, UnflushedBytes);
            MemoryOwner = null;
            UnflushedBytes = 0;
        }

        public void Advance(int count)
        {
            UnflushedBytes += count;
        }

        public Memory<byte> GetMemory(int sizeHint = 0)
        {
            if (MemoryOwner == null)
            {
                MemoryOwner = MemoryPool<byte>.Shared.Rent(2048);
            }
            return MemoryOwner.Memory.Slice(UnflushedBytes);
        }

        public Span<byte> GetSpan(int sizeHint = 0)
        {
            if (MemoryOwner == null)
            {
                MemoryOwner = MemoryPool<byte>.Shared.Rent(2048);
            }
            return MemoryOwner.Memory.Span.Slice(UnflushedBytes);
        }

        /// <summary>
        /// Socket发送是要pin byte[],为了不阻塞KcpFlush，动态缓存是必须的。
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="avalidLength"></param>
        public abstract void Output(IMemoryOwner<byte> buffer, int avalidLength);
    }
}




