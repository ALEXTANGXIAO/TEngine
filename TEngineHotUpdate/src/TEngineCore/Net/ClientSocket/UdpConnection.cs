using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using TEngineProto;


namespace TEngineCore.Net
{
    public enum UdpState
    {
        None,
        Start,
        Close,
    }

    public class UdpConnection
    {
        private Thread udpThread;
        private Socket udpClient;
        private EndPoint _ePoint;
        private IPEndPoint ipEndPoint;
        private Byte[] buffer = new Byte[2048];
        private GameClient client;

        private UdpState udpState = UdpState.None;

        public UdpState State
        {
            get
            {
                return udpState;
            }
            set
            {
                udpState = value;
            }
        }

        public UdpConnection(GameClient client)
        {
            this.client = client;
        }

        public void ConnectUdp(string host,int port)
        {
            udpClient = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            ipEndPoint = new IPEndPoint(IPAddress.Parse(host), port + 1);
            _ePoint = ipEndPoint;
            udpState = UdpState.Start;
            try
            {
                TLogger.LogInfo("start connect udp server[{0}:{1}]...", host, port);
                udpClient.Connect(_ePoint);
            }
            catch
            {
                TLogger.LogError("UDP connect failed!...".ToColor("FF0000"));
                return;
            }
            TLogger.LogInfo("start connect udp server[{0}:{1}] successed...".ToColor("10FD00"), host, port);
            Loom.RunAsync(() =>
                {
                    udpThread = new Thread(ReceiveMsg);
                    udpThread.Start();
                }
            );
        }

        private void ReceiveMsg()
        {
            try
            {
                while (true)
                {
                    if (client == null || udpState != UdpState.Start)
                    {
                        return;
                    }
                    int len = udpClient.ReceiveFrom(buffer, ref _ePoint);
                    MainPack pack = (MainPack)MainPack.Descriptor.Parser.ParseFrom(buffer, 0, len);
                    Loom.QueueOnMainThread((param) =>
                    {
                        client.UdpHandleResponse(pack);
                    }, null);
                }
            }
            catch (Exception e)
            {
                TLogger.LogError(e.Message);
            }
        }
    }
}
