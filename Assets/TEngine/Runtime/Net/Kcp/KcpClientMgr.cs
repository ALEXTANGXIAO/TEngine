using System;
using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Net.Sockets.Kcp;
using System.Threading.Tasks;
using TEngine;

public class KcpClientMgr : UnitySingleton<KcpClientMgr>
{
    static IPEndPoint end = new System.Net.IPEndPoint(System.Net.IPAddress.Loopback, 40001);

    private KcpClient kcpClient;
    public void StartKcpClient(string host,int port)
    {
        var ipEndPoint = new IPEndPoint(IPAddress.Parse(host), port);
        kcpClient = new KcpClient(50001, ipEndPoint);
        Task.Run(async () =>
        {
            while (true)
            {
                kcpClient.kcp.Update(DateTime.UtcNow);
                await Task.Delay(10);
            }
        });
    }

    public async void Send(string v)
    {
        UnityEngine.Debug.Log($"发送:    {v}");
        var buffer = System.Text.Encoding.UTF8.GetBytes(v);
        kcpClient.SendAsync(buffer, buffer.Length);
        var resp = await kcpClient.ReceiveAsync();
        var respstr = System.Text.Encoding.UTF8.GetString(resp);
        UnityEngine.Debug.Log($"收到服务器回复:    {respstr}");
    }
}

public class KcpClient : IKcpCallback
{
    private UdpClient client;

    public KcpClient(int port)
        : this(port, (IPEndPoint)null)
    {
    }

    public KcpClient(int port, IPEndPoint endPoint)
    {
        this.client = new UdpClient(port);
        this.kcp = new System.Net.Sockets.Kcp.Kcp(2001U, (IKcpCallback)this);
        this.EndPoint = endPoint;
        this.BeginRecv();
    }

    public System.Net.Sockets.Kcp.Kcp kcp { get; }

    public IPEndPoint EndPoint { get; set; }

    public void Output(IMemoryOwner<byte> buffer, int avalidLength)
    {
        Span<byte> span = buffer.Memory.Span;
        span = span.Slice(0, avalidLength);
        byte[] array = span.ToArray();
        this.client.SendAsync(array, array.Length, this.EndPoint);
        buffer.Dispose();
    }

    public async void SendAsync(byte[] datagram, int bytes) => this.kcp.Send(datagram.AsSpan<byte>().Slice(0, bytes));

    public async ValueTask<byte[]> ReceiveAsync()
    {
        (IMemoryOwner<byte> buffer, int num) = this.kcp.TryRecv();
        if (buffer != null)
            return buffer.Memory.Span.Slice(0, num).ToArray();
        await Task.Delay(10);
        return await this.ReceiveAsync();
    }

    private async void BeginRecv()
    {
        UdpReceiveResult async = await this.client.ReceiveAsync();
        this.EndPoint = async.RemoteEndPoint;
        this.kcp.Input((Span<byte>)async.Buffer);
        this.BeginRecv();
    }

    public IMemoryOwner<byte> RentBuffer(int length)
    {
        return null;
    }
}