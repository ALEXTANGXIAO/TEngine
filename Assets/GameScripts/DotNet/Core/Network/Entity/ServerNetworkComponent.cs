using System;
using System.Net;
#pragma warning disable CS8625
#pragma warning disable CS8618

namespace TEngine.Core.Network
{
    public sealed class ServerNetworkComponent : Entity, INotSupportedPool
    {
        public ANetwork Network { get; private set; }
        
        public void Initialize(NetworkProtocolType networkProtocolType, NetworkTarget networkTarget, IPEndPoint address)
        {
            switch (networkProtocolType)
            {
#if !UNITY_WEBGL
                case NetworkProtocolType.KCP:
                {
                    Network = new KCPServerNetwork(Scene, networkTarget, address);
                    // Log.Info($"NetworkProtocol:KCP IPEndPoint:{address}");
                    return;
                }
                case NetworkProtocolType.TCP:
                {
                    Network = new TCPServerNetwork(Scene, networkTarget, address);
                    // Log.Info($"NetworkProtocol:TCP IPEndPoint:{address}");
                    return;
                }
#endif
                default:
                {
                    throw new NotSupportedException($"Unsupported NetworkProtocolType:{networkProtocolType}");
                }
            }
        }

        public override void Dispose()
        {
            if (Network != null)
            {
                Network.Dispose();
                Network = null;
            }
            
            base.Dispose();
        }
    }
}