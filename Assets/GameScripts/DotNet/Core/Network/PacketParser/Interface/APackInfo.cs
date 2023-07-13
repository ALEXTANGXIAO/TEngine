using System;
using System.IO;
#pragma warning disable CS8625
#pragma warning disable CS8618

namespace TEngine.Core.Network
{
    public abstract class APackInfo : IDisposable
    {
        public uint RpcId;
        public long RouteId;
        public uint ProtocolCode;
        public long RouteTypeCode;
        public MemoryStream MemoryStream;

        public abstract object Deserialize(Type messageType);

        public virtual void Dispose()
        {
            RpcId = 0;
            RouteId = 0;
            ProtocolCode = 0;
            RouteTypeCode = 0;
            MemoryStream = null;
        }
    }
}