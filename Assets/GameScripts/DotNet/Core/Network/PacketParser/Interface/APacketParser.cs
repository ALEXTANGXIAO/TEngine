using System;
using System.Buffers;
using System.IO;
using TEngine.DataStructure;

namespace TEngine.Core.Network
{
    public abstract class APacketParser : IDisposable
    {
        protected MemoryPool<byte> MemoryPool;
        protected bool IsDisposed { get; private set; }

        public static APacketParser CreatePacketParser(NetworkTarget networkTarget)
        {
            switch (networkTarget)
            {
                case NetworkTarget.Inner:
                {
#if TENGINE_NET
                    return new InnerPacketParser();
#else
                    throw new NotSupportedException($"PacketParserHelper Create NotSupport {networkTarget}");
#endif
                }
                case NetworkTarget.Outer:
                {
                    return new OuterPacketParser();
                }
                default:
                {
                    throw new NotSupportedException($"PacketParserHelper Create NotSupport {networkTarget}");
                }
            }
        }

        public abstract bool UnPack(CircularBuffer buffer, out APackInfo packInfo);
        public abstract bool UnPack(IMemoryOwner<byte> memoryOwner, out APackInfo packInfo);
        public virtual void Dispose()
        {
            IsDisposed = true;
            MemoryPool.Dispose();
        }
    }
}