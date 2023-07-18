using System;
using System.IO;
using System.Net;

namespace TEngine.Core.Network
{
    public abstract class AClientNetwork: ANetwork
    {
        public uint ChannelId { get; protected set; }
        public abstract event Action OnDispose;
        public abstract event Action OnConnectFail;
        public abstract event Action OnConnectComplete;
        public abstract event Action OnConnectDisconnect;
        public abstract event Action<uint> OnChangeChannelId;
        public abstract event Action<APackInfo> OnReceiveMemoryStream;

        protected AClientNetwork(Scene scene, NetworkType networkType, NetworkProtocolType networkProtocolType, NetworkTarget networkTarget): base(
            scene, networkType, networkProtocolType, networkTarget)
        {
        }

        public abstract uint Connect(IPEndPoint remoteEndPoint, Action onConnectComplete, Action onConnectFail, Action onConnectDisconnect,
        int connectTimeout = 5000);

        public override void Dispose()
        {
            ChannelId = 0;
            base.Dispose();
        }
    }
}