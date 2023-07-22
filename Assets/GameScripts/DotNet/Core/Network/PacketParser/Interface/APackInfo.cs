using System;
using System.Buffers;
using System.IO;

namespace TEngine.Core.Network
{
    public abstract class APackInfo : IDisposable
    {
        public uint RpcId;
        public long RouteId;
        public uint ProtocolCode;
        public long RouteTypeCode;
        public int MessagePacketLength;
        public IMemoryOwner<byte> MemoryOwner;
        public bool IsDisposed;

        public static T Rent<T>() where T : APackInfo
        {
            var aPackInfo = Pool<T>.Rent();
            aPackInfo.IsDisposed = false;
            return aPackInfo;
        }
        
        public abstract object Deserialize(Type messageType);
        public abstract MemoryStream CreateMemoryStream();
        public virtual void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }
            
            RpcId = 0;
            RouteId = 0;
            ProtocolCode = 0;
            RouteTypeCode = 0;
            MessagePacketLength = 0;
            MemoryOwner.Dispose();
            MemoryOwner = null;
            IsDisposed = true;
        }
    }
}