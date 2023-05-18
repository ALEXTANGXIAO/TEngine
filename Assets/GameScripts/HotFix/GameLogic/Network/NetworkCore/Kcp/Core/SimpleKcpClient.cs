using System.Buffers;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

namespace System.Net.Sockets.Kcp.Simple
{
    /// <summary>
    /// 简单例子。
    /// </summary>
    public class SimpleKcpClient : IKcpCallback
    {
        UdpClient client;

        public SimpleKcpClient(int port)
            : this(port, null)
        {

        }

        public SimpleKcpClient(int port, IPEndPoint endPoint)
        {
            client = new UdpClient(port);
            kcp = new SimpleSegManager.Kcp(2001, this);
            this.EndPoint = endPoint;
            BeginRecv();
        }

        public SimpleSegManager.Kcp kcp { get; }
        public IPEndPoint EndPoint { get; set; }

        public void Output(IMemoryOwner<byte> buffer, int avalidLength)
        {
            var s = buffer.Memory.Span.Slice(0, avalidLength).ToArray();
            client.SendAsync(s, s.Length, EndPoint);
            buffer.Dispose();
        }

        public void SendAsync(byte[] datagram, int bytes)
        {
            kcp.Send(datagram.AsSpan().Slice(0, bytes));
        }

        public async UniTask<byte[]> ReceiveAsync()
        {
            var (buffer, avalidLength) = kcp.TryRecv();
            while (buffer == null)
            {
                await Task.Delay(10);
                (buffer, avalidLength) = kcp.TryRecv();
            }

            var s = buffer.Memory.Span.Slice(0, avalidLength).ToArray();
            return s;
        }

        private async void BeginRecv()
        {
            var res = await client.ReceiveAsync();
            EndPoint = res.RemoteEndPoint;
            kcp.Input(res.Buffer);
            BeginRecv();
        }
    }
}

