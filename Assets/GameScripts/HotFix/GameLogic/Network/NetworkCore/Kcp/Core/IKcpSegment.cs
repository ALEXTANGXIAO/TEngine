namespace System.Net.Sockets.Kcp
{
    /// <summary>
    /// Kcp报头
    /// https://zhuanlan.zhihu.com/p/559191428
    /// </summary>
    public interface IKcpHeader
    {
        /// <summary>
        /// 会话编号，两方一致才会通信
        /// </summary>
        uint conv { get; set; }
        /// <summary>
        /// 指令类型
        /// </summary>
        /// <remarks>
        /// <para/> IKCP_CMD_PUSH = 81                 // cmd: push data 数据报文
        /// <para/> IKCP_CMD_ACK  = 82                 // cmd: ack 确认报文
        /// <para/> IKCP_CMD_WASK = 83                 // cmd: window probe (ask) 窗口探测报文,询问对端剩余接收窗口的大小.
        /// <para/> IKCP_CMD_WINS = 84                 // cmd: window size (tell) 窗口通知报文,通知对端剩余接收窗口的大小.
        /// </remarks>
        byte cmd { get; set; }
        /// <summary>
        /// 剩余分片数量，表示随后还有多少个报文属于同一个包。
        /// </summary>
        byte frg { get; set; }
        /// <summary>
        /// 自己可用窗口大小    
        /// </summary>
        ushort wnd { get; set; }
        /// <summary>
        /// 发送时的时间戳 <seealso cref="DateTimeOffset.ToUnixTimeMilliseconds"/>
        /// </summary>
        uint ts { get; set; }
        /// <summary>
        /// 编号 确认编号或者报文编号
        /// </summary>
        uint sn { get; set; }
        /// <summary>
        /// 代表编号前面的所有报都收到了的标志
        /// </summary>
        uint una { get; set; }
        /// <summary>
        /// 数据内容长度
        /// </summary>
        uint len { get; }
    }
    public interface IKcpSegment : IKcpHeader
    {
        /// <summary>
        /// 重传的时间戳。超过当前时间重发这个包
        /// </summary>
        uint resendts { get; set; }
        /// <summary>
        /// 超时重传时间，根据网络去定
        /// </summary>
        uint rto { get; set; }
        /// <summary>
        /// 快速重传机制，记录被跳过的次数，超过次数进行快速重传
        /// </summary>
        uint fastack { get; set; }
        /// <summary>
        /// 重传次数
        /// </summary>
        uint xmit { get; set; }

        /// <summary>
        /// 数据内容
        /// </summary>
        Span<byte> data { get; }
        /// <summary>
        /// 将IKcpSegment编码成字节数组，并返回总长度（包括Kcp报头）
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        int Encode(Span<byte> buffer);
    }

    public interface ISegmentManager<Segment> where Segment : IKcpSegment
    {
        Segment Alloc(int appendDateSize);
        void Free(Segment seg);
    }

}



