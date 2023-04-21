using BufferOwner = System.Buffers.IMemoryOwner<byte>;
using System.Buffers;
using Cysharp.Threading.Tasks;

namespace System.Net.Sockets.Kcp
{
    /// <summary>
    /// Kcp回调
    /// </summary>
    public interface IKcpCallback
    {
        /// <summary>
        /// kcp 发送方向输出
        /// </summary>
        /// <param name="buffer">kcp 交出发送缓冲区控制权，缓冲区来自<see cref="RentBuffer(int)"/></param>
        /// <param name="avalidLength">数据的有效长度</param>
        /// <returns>不需要返回值</returns>
        /// <remarks>通过增加 avalidLength 能够在协议栈中有效的减少数据拷贝</remarks>
        void Output(BufferOwner buffer, int avalidLength);
    }

    /// <summary>
    /// Kcp回调
    /// </summary>
    /// <remarks>
    /// 失败设计，<see cref="KcpOutputWriter.Output(BufferOwner, int)"/>。IMemoryOwner是没有办法代替的。
    /// 这里只相当于把 IKcpCallback 和 IRentable 和并。
    /// </remarks>
    public interface IKcpOutputWriter : IBufferWriter<byte>
    {
        int UnflushedBytes { get; }
        void Flush();
    }

    /// <summary>
    /// 外部提供缓冲区,可以在外部链接一个内存池
    /// </summary>
    public interface IRentable
    {
        /// <summary>
        /// 外部提供缓冲区,可以在外部链接一个内存池
        /// </summary>
        BufferOwner RentBuffer(int length);
    }

    public interface IKcpSetting
    {
        int Interval(int interval);
        /// <summary>
        /// fastest: ikcp_nodelay(kcp, 1, 20, 2, 1)
        /// </summary>
        /// <param name="nodelay">0:disable(default), 1:enable</param>
        /// <param name="interval">internal update timer interval in millisec, default is 100ms</param>
        /// <param name="resend">0:disable fast resend(default), 1:enable fast resend</param>
        /// <param name="nc">0:normal congestion control(default), 1:disable congestion control</param>
        /// <returns></returns>
        int NoDelay(int nodelay, int interval, int resend, int nc);
        /// <summary>
        /// change MTU size, default is 1400
        /// <para>** 这个方法不是线程安全的。请在没有发送和接收时调用 。</para>
        /// </summary>
        /// <param name="mtu"></param>
        /// <returns></returns>
        /// <remarks>
        /// 如果没有必要，不要修改Mtu。过小的Mtu会导致分片数大于接收窗口，造成kcp阻塞冻结。
        /// </remarks>
        int SetMtu(int mtu = 1400);
        /// <summary>
        /// set maximum window size: sndwnd=32, rcvwnd=128 by default
        /// </summary>
        /// <param name="sndwnd"></param>
        /// <param name="rcvwnd"></param>
        /// <returns></returns>
        /// <remarks>
        /// 如果没有必要请不要修改。注意确保接收窗口必须大于最大分片数。
        /// </remarks>
        int WndSize(int sndwnd = 32, int rcvwnd = 128);
    }

    public interface IKcpUpdate
    {
        void Update(in DateTimeOffset time);
    }

    public interface IKcpSendable
    {
        /// <summary>
        /// 将要发送到网络的数据Send到kcp协议中
        /// </summary>
        /// <param name="span"></param>
        /// <param name="options"></param>
        int Send(ReadOnlySpan<byte> span, object options = null);
        /// <summary>
        /// 将要发送到网络的数据Send到kcp协议中
        /// </summary>
        /// <param name="span"></param>
        /// <param name="options"></param>
        int Send(ReadOnlySequence<byte> span, object options = null);
    }

    public interface IKcpInputable
    {
        /// <summary>
        /// 下层收到数据后添加到kcp协议中
        /// </summary>
        /// <param name="span"></param>
        int Input(ReadOnlySpan<byte> span);
        /// <summary>
        /// 下层收到数据后添加到kcp协议中
        /// </summary>
        /// <param name="span"></param>
        int Input(ReadOnlySequence<byte> span);
    }

    /// <summary>
    /// kcp协议输入输出标准接口
    /// </summary>
    public interface IKcpIO : IKcpSendable, IKcpInputable
    {
        /// <summary>
        /// 从kcp中取出一个整合完毕的数据包
        /// </summary>
        /// <returns></returns>
        UniTask RecvAsync(IBufferWriter<byte> writer, object options = null);

        /// <summary>
        /// 从kcp中取出一个整合完毕的数据包
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="options"></param>
        /// <returns>接收数据长度</returns>
        UniTask<int> RecvAsync(ArraySegment<byte> buffer, object options = null);

        /// <summary>
        /// 从kcp协议中取出需要发送到网络的数据。
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        UniTask OutputAsync(IBufferWriter<byte> writer, object options = null);
    }

}




