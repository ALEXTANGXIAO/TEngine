using System;
using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Net.Sockets.Kcp;
using System.Threading.Tasks;

namespace Kcp
{
    public class KcpClient : IKcpCallback
    {
        UdpClient client;

        public KcpClient(int port)
            : this(port, null)
        {

        }

        public KcpClient(int port, IPEndPoint endPoint)
        {
            client = new UdpClient(port);
            kcp = new System.Net.Sockets.Kcp.Kcp(2001, this);
            this.EndPoint = endPoint;
            BeginRecv();
        }

        public System.Net.Sockets.Kcp.Kcp kcp { get; }
        public IPEndPoint EndPoint { get; set; }

        public void Output(IMemoryOwner<byte> buffer, int avalidLength)
        {
            var s = buffer.Memory.Span.Slice(0, avalidLength).ToArray();
            client.SendAsync(s, s.Length, EndPoint);
            buffer.Dispose();
        }

        public async void SendAsync(byte[] datagram, int bytes)
        {
            kcp.Send(datagram.AsSpan().Slice(0, bytes));
        }

        public async Task<byte[]> ReceiveAsync()
        {
            var (buffer, avalidLength) = kcp.TryRecv();
            if (buffer == null)
            {
                await Task.Delay(10);
                return await ReceiveAsync();
            }
            else
            {
                var s = buffer.Memory.Span.Slice(0, avalidLength).ToArray();
                return s;
            }
        }

        private async void BeginRecv()
        {
            var res = await client.ReceiveAsync();
            EndPoint = res.RemoteEndPoint;
            kcp.Input(res.Buffer);
            BeginRecv();
        }

        public IMemoryOwner<byte> RentBuffer(int length)
        {
            return null;
        }
    }
}
