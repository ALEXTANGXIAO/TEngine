using System;
using System.IO;
using System.Net;
#pragma warning disable CS8625
#pragma warning disable CS8618

namespace TEngine.Core.Network
{
    public abstract class ANetworkChannel
    {
        public uint Id { get; private set; }
        public Scene Scene { get; protected set; }
        public long NetworkId { get; private set; }
        public bool IsDisposed { get; protected set; }
        public EndPoint RemoteEndPoint { get; protected set; }
        public APacketParser PacketParser { get; protected set; }
        
        public uint LocalConn
        {
            get
            {
                return (uint)this.Id; 
            }
            private set
            {
                this.Id = value;
            }
        }

        public abstract event Action OnDispose;
        public abstract event Action<APackInfo> OnReceiveMemoryStream;

        protected ANetworkChannel(Scene scene, uint id, long networkId)
        {
            Id = id;
            Scene = scene;
            NetworkId = networkId;
        }

        public virtual void Dispose()
        {
            NetworkThread.Instance.RemoveChannel(NetworkId, Id);
            
            Id = 0;
            Scene = null;
            NetworkId = 0;
            IsDisposed = true;
            RemoteEndPoint = null;

            if (PacketParser != null)
            {
                PacketParser.Dispose();
                PacketParser = null;
            }
        }
    }
}