using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using static System.Math;
using BufferOwner = System.Buffers.IMemoryOwner<byte>;

namespace System.Net.Sockets.Kcp
{
    public abstract class KcpConst
    {
        // 为了减少阅读难度，变量名尽量于 C版 统一
        /*
        conv 会话ID
        mtu 最大传输单元
        mss 最大分片大小
        state 连接状态（0xFFFFFFFF表示断开连接）
        snd_una 第一个未确认的包
        snd_nxt 待发送包的序号
        rcv_nxt 待接收消息序号
        ssthresh 拥塞窗口阈值
        rx_rttvar ack接收rtt浮动值
        rx_srtt ack接收rtt静态值
        rx_rto 由ack接收延迟计算出来的复原时间
        rx_minrto 最小复原时间
        snd_wnd 发送窗口大小
        rcv_wnd 接收窗口大小
        rmt_wnd,	远端接收窗口大小
        cwnd, 拥塞窗口大小
        probe 探查变量，IKCP_ASK_TELL表示告知远端窗口大小。IKCP_ASK_SEND表示请求远端告知窗口大小
        interval    内部flush刷新间隔
        ts_flush 下次flush刷新时间戳
        nodelay 是否启动无延迟模式
        updated 是否调用过update函数的标识
        ts_probe, 下次探查窗口的时间戳
        probe_wait 探查窗口需要等待的时间
        dead_link 最大重传次数
        incr 可发送的最大数据量
        fastresend 触发快速重传的重复ack个数
        nocwnd 取消拥塞控制
        stream 是否采用流传输模式

        snd_queue 发送消息的队列
        rcv_queue 接收消息的队列
        snd_buf 发送消息的缓存
        rcv_buf 接收消息的缓存
        acklist 待发送的ack列表
        buffer 存储消息字节流的内存
        output udp发送消息的回调函数
        */

        #region Const

        public const int IKCP_RTO_NDL = 30;  // no delay min rto
        public const int IKCP_RTO_MIN = 100; // normal min rto
        public const int IKCP_RTO_DEF = 200;
        public const int IKCP_RTO_MAX = 60000;
        /// <summary>
        /// 数据报文
        /// </summary>
        public const int IKCP_CMD_PUSH = 81; // cmd: push data
        /// <summary>
        /// 确认报文
        /// </summary>
        public const int IKCP_CMD_ACK = 82; // cmd: ack
        /// <summary>
        /// 窗口探测报文,询问对端剩余接收窗口的大小.
        /// </summary>
        public const int IKCP_CMD_WASK = 83; // cmd: window probe (ask)
        /// <summary>
        /// 窗口通知报文,通知对端剩余接收窗口的大小.
        /// </summary>
        public const int IKCP_CMD_WINS = 84; // cmd: window size (tell)
        /// <summary>
        /// IKCP_ASK_SEND表示请求远端告知窗口大小
        /// </summary>
        public const int IKCP_ASK_SEND = 1;  // need to send IKCP_CMD_WASK
        /// <summary>
        /// IKCP_ASK_TELL表示告知远端窗口大小。
        /// </summary>
        public const int IKCP_ASK_TELL = 2;  // need to send IKCP_CMD_WINS
        public const int IKCP_WND_SND = 32;
        /// <summary>
        /// 接收窗口默认值。必须大于最大分片数
        /// </summary>
        public const int IKCP_WND_RCV = 128; // must >= max fragment size
        /// <summary>
        /// 默认最大传输单元 常见路由值 1492 1480  默认1400保证在路由层不会被分片
        /// </summary>
        public const int IKCP_MTU_DEF = 1400;
        public const int IKCP_ACK_FAST = 3;
        public const int IKCP_INTERVAL = 100;
        public const int IKCP_OVERHEAD = 24;
        public const int IKCP_DEADLINK = 20;
        public const int IKCP_THRESH_INIT = 2;
        public const int IKCP_THRESH_MIN = 2;
        /// <summary>
        /// 窗口探查CD
        /// </summary>
        public const int IKCP_PROBE_INIT = 7000;   // 7 secs to probe window size
        public const int IKCP_PROBE_LIMIT = 120000; // up to 120 secs to probe window
        public const int IKCP_FASTACK_LIMIT = 5;        // max times to trigger fastack
        #endregion

        /// <summary>
        /// <para>https://github.com/skywind3000/kcp/issues/53</para>
        /// 按照 C版 设计，使用小端字节序
        /// </summary>
        public static bool IsLittleEndian = true;
    }

    /// <summary>
    /// https://luyuhuang.tech/2020/12/09/kcp.html
    /// https://github.com/skywind3000/kcp/wiki/Network-Layer
    /// <para>外部buffer ----拆分拷贝----等待列表 -----移动----发送列表----拷贝----发送buffer---output</para>
    /// https://github.com/skywind3000/kcp/issues/118#issuecomment-338133930
    /// </summary>
    public partial class KcpCore<Segment> : KcpConst, IKcpSetting, IKcpUpdate, IDisposable
        where Segment : IKcpSegment
    {
        #region kcp members
        /// <summary>
        /// 频道号
        /// </summary>
        public uint conv { get; protected set; }
        /// <summary>
        /// 最大传输单元（Maximum Transmission Unit，MTU）
        /// </summary>
        protected uint mtu;

        /// <summary>
        /// 缓冲区最小大小
        /// </summary>
        protected int BufferNeedSize
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return (int)((mtu/* + IKCP_OVERHEAD*/) /** 3*/);
            }
        }

        /// <summary>
        /// 最大报文段长度
        /// </summary>
        protected uint mss;
        /// <summary>
        /// 连接状态（0xFFFFFFFF表示断开连接）
        /// </summary>
        protected int state;
        /// <summary>
        /// 第一个未确认的包
        /// </summary>
        protected uint snd_una;
        /// <summary>
        /// 待发送包的序号
        /// </summary>
        protected uint snd_nxt;
        /// <summary>
        /// 下一个等待接收消息ID,待接收消息序号
        /// </summary>
        protected uint rcv_nxt;
        protected uint ts_recent;
        protected uint ts_lastack;
        /// <summary>
        /// 拥塞窗口阈值
        /// </summary>
        protected uint ssthresh;
        /// <summary>
        /// ack接收rtt浮动值
        /// </summary>
        protected uint rx_rttval;
        /// <summary>
        /// ack接收rtt静态值
        /// </summary>
        protected uint rx_srtt;
        /// <summary>
        /// 由ack接收延迟计算出来的复原时间。Retransmission TimeOut(RTO), 超时重传时间.
        /// </summary>
        protected uint rx_rto;
        /// <summary>
        /// 最小复原时间
        /// </summary>
        protected uint rx_minrto;
        /// <summary>
        /// 发送窗口大小
        /// </summary>
        protected uint snd_wnd;
        /// <summary>
        /// 接收窗口大小
        /// </summary>
        protected uint rcv_wnd;
        /// <summary>
        /// 远端接收窗口大小
        /// </summary>
        protected uint rmt_wnd;
        /// <summary>
        /// 拥塞窗口大小
        /// </summary>
        protected uint cwnd;
        /// <summary>
        /// 探查变量，IKCP_ASK_TELL表示告知远端窗口大小。IKCP_ASK_SEND表示请求远端告知窗口大小
        /// </summary>
        protected uint probe;
        protected uint current;
        /// <summary>
        /// 内部flush刷新间隔
        /// </summary>
        protected uint interval;
        /// <summary>
        /// 下次flush刷新时间戳
        /// </summary>
        protected uint ts_flush;
        protected uint xmit;
        /// <summary>
        /// 是否启动无延迟模式
        /// </summary>
        protected uint nodelay;
        /// <summary>
        /// 是否调用过update函数的标识
        /// </summary>
        protected uint updated;
        /// <summary>
        /// 下次探查窗口的时间戳
        /// </summary>
        protected uint ts_probe;
        /// <summary>
        /// 探查窗口需要等待的时间
        /// </summary>
        protected uint probe_wait;
        /// <summary>
        /// 最大重传次数
        /// </summary>
        protected uint dead_link;
        /// <summary>
        /// 可发送的最大数据量
        /// </summary>
        protected uint incr;
        /// <summary>
        /// 触发快速重传的重复ack个数
        /// </summary>
        public int fastresend;
        public int fastlimit;
        /// <summary>
        /// 取消拥塞控制
        /// </summary>
        protected int nocwnd;
        protected int logmask;
        /// <summary>
        /// 是否采用流传输模式
        /// </summary>
        public int stream;
        protected BufferOwner buffer;

        #endregion

        #region 锁和容器

        /// <summary>
        /// 增加锁保证发送线程安全，否则可能导致2个消息的分片交替入队。
        /// <para/> 用例：普通发送和广播可能会导致多个线程同时调用Send方法。
        /// </summary>
        protected readonly object snd_queueLock = new object();
        protected readonly object snd_bufLock = new object();
        protected readonly object rcv_bufLock = new object();
        protected readonly object rcv_queueLock = new object();

        /// <summary>
        /// 发送 ack 队列 
        /// </summary>
        protected ConcurrentQueue<(uint sn, uint ts)> acklist = new ConcurrentQueue<(uint sn, uint ts)>();
        /// <summary>
        /// 发送等待队列
        /// </summary>
        internal ConcurrentQueue<Segment> snd_queue = new ConcurrentQueue<Segment>();
        /// <summary>
        /// 正在发送列表
        /// </summary>
        internal LinkedList<Segment> snd_buf = new LinkedList<Segment>();
        /// <summary>
        /// 正在等待触发接收回调函数消息列表
        /// <para>需要执行的操作  添加 遍历 删除</para>
        /// </summary>
        internal List<Segment> rcv_queue = new List<Segment>();
        /// <summary>
        /// 正在等待重组消息列表
        /// <para>需要执行的操作  添加 插入 遍历 删除</para>
        /// </summary>
        internal LinkedList<Segment> rcv_buf = new LinkedList<Segment>();

        /// <summary>
        /// get how many packet is waiting to be sent
        /// </summary>
        /// <returns></returns>
        public int WaitSnd => snd_buf.Count + snd_queue.Count;

        #endregion

        public ISegmentManager<Segment> SegmentManager { get; set; }
        public KcpCore(uint conv_)
        {
            conv = conv_;

            snd_wnd = IKCP_WND_SND;
            rcv_wnd = IKCP_WND_RCV;
            rmt_wnd = IKCP_WND_RCV;
            mtu = IKCP_MTU_DEF;
            mss = mtu - IKCP_OVERHEAD;
            buffer = CreateBuffer(BufferNeedSize);

            rx_rto = IKCP_RTO_DEF;
            rx_minrto = IKCP_RTO_MIN;
            interval = IKCP_INTERVAL;
            ts_flush = IKCP_INTERVAL;
            ssthresh = IKCP_THRESH_INIT;
            fastlimit = IKCP_FASTACK_LIMIT;
            dead_link = IKCP_DEADLINK;
        }

        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用

        /// <summary>
        /// 是否正在释放
        /// </summary>
        private bool m_disposing = false;

        protected bool CheckDispose()
        {
            if (m_disposing)
            {
                return true;
            }

            if (disposedValue)
            {
                throw new ObjectDisposedException(
                    $"{nameof(Kcp)} [conv:{conv}]");
            }

            return false;
        }

        protected virtual void Dispose(bool disposing)
        {
            try
            {
                m_disposing = true;
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        // 释放托管状态(托管对象)。
                        callbackHandle = null;
                        acklist = null;
                        buffer = null;
                    }

                    // 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                    // 将大型字段设置为 null。
                    void FreeCollection(IEnumerable<Segment> collection)
                    {
                        if (collection == null)
                        {
                            return;
                        }
                        foreach (var item in collection)
                        {
                            try
                            {
                                SegmentManager.Free(item);
                            }
                            catch (Exception)
                            {
                                //理论上此处不会有任何异常
                                LogFail($"此处绝不应该出现异常。 Dispose 时出现预计外异常，联系作者");
                            }
                        }
                    }

                    lock (snd_queueLock)
                    {
                        while (snd_queue != null &&
                        (snd_queue.TryDequeue(out var segment)
                        || !snd_queue.IsEmpty)
                        )
                        {
                            try
                            {
                                SegmentManager.Free(segment);
                            }
                            catch (Exception)
                            {
                                //理论上这里没有任何异常；
                            }
                        }
                        snd_queue = null;
                    }

                    lock (snd_bufLock)
                    {
                        FreeCollection(snd_buf);
                        snd_buf?.Clear();
                        snd_buf = null;
                    }

                    lock (rcv_bufLock)
                    {
                        FreeCollection(rcv_buf);
                        rcv_buf?.Clear();
                        rcv_buf = null;
                    }

                    lock (rcv_queueLock)
                    {
                        FreeCollection(rcv_queue);
                        rcv_queue?.Clear();
                        rcv_queue = null;
                    }


                    disposedValue = true;
                }
            }
            finally
            {
                m_disposing = false;
            }

        }

        // 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        ~KcpCore()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(false);
        }

        // 添加此代码以正确实现可处置模式。
        /// <summary>
        /// 释放不是严格线程安全的，尽量使用和Update相同的线程调用，
        /// 或者等待析构时自动释放。
        /// </summary>
        public void Dispose()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(true);
            // 如果在以上内容中替代了终结器，则取消注释以下行。
            GC.SuppressFinalize(this);
        }

        #endregion

        internal protected IKcpCallback callbackHandle;
        internal protected IKcpOutputWriter OutputWriter;

        protected static uint Ibound(uint lower, uint middle, uint upper)
        {
            return Min(Max(lower, middle), upper);
        }

        protected static int Itimediff(uint later, uint earlier)
        {
            return ((int)(later - earlier));
        }

        internal protected virtual BufferOwner CreateBuffer(int needSize)
        {
            return new KcpInnerBuffer(needSize);
        }

        internal protected class KcpInnerBuffer : BufferOwner
        {
            private readonly Memory<byte> _memory;

            public Memory<byte> Memory
            {
                get
                {
                    if (alreadyDisposed)
                    {
                        throw new ObjectDisposedException(nameof(KcpInnerBuffer));
                    }
                    return _memory;
                }
            }

            public KcpInnerBuffer(int size)
            {
                _memory = new Memory<byte>(new byte[size]);
            }

            bool alreadyDisposed = false;
            public void Dispose()
            {
                alreadyDisposed = true;
            }
        }


        #region 功能逻辑

        //功能函数

        /// <summary>
        /// Determine when should you invoke ikcp_update:
        /// returns when you should invoke ikcp_update in millisec, if there
        /// is no ikcp_input/_send calling. you can call ikcp_update in that
        /// time, instead of call update repeatly.
        /// <para></para>
        /// Important to reduce unnacessary ikcp_update invoking. use it to
        /// schedule ikcp_update (eg. implementing an epoll-like mechanism,
        /// or optimize ikcp_update when handling massive kcp connections)
        /// <para></para>
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public DateTimeOffset Check(in DateTimeOffset time)
        {
            if (CheckDispose())
            {
                //检查释放
                return default;
            }

            if (updated == 0)
            {
                return time;
            }

            var current_ = time.ConvertTime();

            var ts_flush_ = ts_flush;
            var tm_flush_ = 0x7fffffff;
            var tm_packet = 0x7fffffff;
            var minimal = 0;

            if (Itimediff(current_, ts_flush_) >= 10000 || Itimediff(current_, ts_flush_) < -10000)
            {
                ts_flush_ = current_;
            }

            if (Itimediff(current_, ts_flush_) >= 0)
            {
                return time;
            }

            tm_flush_ = Itimediff(ts_flush_, current_);

            lock (snd_bufLock)
            {
                foreach (var seg in snd_buf)
                {
                    var diff = Itimediff(seg.resendts, current_);
                    if (diff <= 0)
                    {
                        return time;
                    }

                    if (diff < tm_packet)
                    {
                        tm_packet = diff;
                    }
                }
            }

            minimal = tm_packet < tm_flush_ ? tm_packet : tm_flush_;
            if (minimal >= interval) minimal = (int)interval;

            return time + TimeSpan.FromMilliseconds(minimal);
        }

        /// <summary>
        /// move available data from rcv_buf -> rcv_queue
        /// </summary>
        protected void Move_Rcv_buf_2_Rcv_queue()
        {
            lock (rcv_bufLock)
            {
                while (rcv_buf.Count > 0)
                {
                    var seg = rcv_buf.First.Value;
                    if (seg.sn == rcv_nxt && rcv_queue.Count < rcv_wnd)
                    {
                        rcv_buf.RemoveFirst();
                        lock (rcv_queueLock)
                        {
                            rcv_queue.Add(seg);
                        }

                        rcv_nxt++;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// update ack.
        /// </summary>
        /// <param name="rtt"></param>
        protected void Update_ack(int rtt)
        {
            if (rx_srtt == 0)
            {
                rx_srtt = (uint)rtt;
                rx_rttval = (uint)rtt / 2;
            }
            else
            {
                int delta = (int)((uint)rtt - rx_srtt);

                if (delta < 0)
                {
                    delta = -delta;
                }

                rx_rttval = (3 * rx_rttval + (uint)delta) / 4;
                rx_srtt = (uint)((7 * rx_srtt + rtt) / 8);

                if (rx_srtt < 1)
                {
                    rx_srtt = 1;
                }
            }

            var rto = rx_srtt + Max(interval, 4 * rx_rttval);

            rx_rto = Ibound(rx_minrto, rto, IKCP_RTO_MAX);
        }

        protected void Shrink_buf()
        {
            lock (snd_bufLock)
            {
                snd_una = snd_buf.Count > 0 ? snd_buf.First.Value.sn : snd_nxt;
            }
        }

        protected void Parse_ack(uint sn)
        {
            if (Itimediff(sn, snd_una) < 0 || Itimediff(sn, snd_nxt) >= 0)
            {
                return;
            }

            lock (snd_bufLock)
            {
                for (var p = snd_buf.First; p != null; p = p.Next)
                {
                    var seg = p.Value;
                    if (sn == seg.sn)
                    {
                        snd_buf.Remove(p);
                        SegmentManager.Free(seg);
                        break;
                    }

                    if (Itimediff(sn, seg.sn) < 0)
                    {
                        break;
                    }
                }
            }
        }

        protected void Parse_una(uint una)
        {
            /// 删除给定时间之前的片段。保留之后的片段
            lock (snd_bufLock)
            {
                while (snd_buf.First != null)
                {
                    var seg = snd_buf.First.Value;
                    if (Itimediff(una, seg.sn) > 0)
                    {
                        snd_buf.RemoveFirst();
                        SegmentManager.Free(seg);
                    }
                    else
                    {
                        break;
                    }
                }
            }

        }

        protected void Parse_fastack(uint sn, uint ts)
        {
            if (Itimediff(sn, snd_una) < 0 || Itimediff(sn, snd_nxt) >= 0)
            {
                return;
            }

            lock (snd_bufLock)
            {
                foreach (var item in snd_buf)
                {
                    var seg = item;
                    if (Itimediff(sn, seg.sn) < 0)
                    {
                        break;
                    }
                    else if (sn != seg.sn)
                    {
#if !IKCP_FASTACK_CONSERVE
                        seg.fastack++;
#else
                        if (Itimediff(ts, seg.ts) >= 0)
                        {
                            seg.fastack++;
                        }
#endif
                    }
                }
            }
        }

        /// <summary>
        /// 处理下层网络收到的数据包
        /// </summary>
        /// <param name="newseg"></param>
        internal virtual void Parse_data(Segment newseg)
        {
            var sn = newseg.sn;

            lock (rcv_bufLock)
            {
                if (Itimediff(sn, rcv_nxt + rcv_wnd) >= 0 || Itimediff(sn, rcv_nxt) < 0)
                {
                    // 如果接收到数据报文的编号大于 rcv_nxt + rcv_wnd 或小于 rcv_nxt, 这个报文就会被丢弃.
                    SegmentManager.Free(newseg);
                    return;
                }

                var repeat = false;

                ///检查是否重复消息和插入位置
                LinkedListNode<Segment> p;
                for (p = rcv_buf.Last; p != null; p = p.Previous)
                {
                    var seg = p.Value;
                    if (seg.sn == sn)
                    {
                        repeat = true;
                        break;
                    }

                    if (Itimediff(sn, seg.sn) > 0)
                    {
                        break;
                    }
                }

                if (!repeat)
                {
                    if (CanLog(KcpLogMask.IKCP_LOG_PARSE_DATA))
                    {
                        LogWriteLine($"{newseg.ToLogString()}", KcpLogMask.IKCP_LOG_PARSE_DATA.ToString());
                    }

                    if (p == null)
                    {
                        rcv_buf.AddFirst(newseg);
                        if (newseg.frg + 1 > rcv_wnd)
                        {
                            //分片数大于接收窗口，造成kcp阻塞冻结。
                            //Console.WriteLine($"分片数大于接收窗口，造成kcp阻塞冻结。frgCount:{newseg.frg + 1}  rcv_wnd:{rcv_wnd}");
                            //百分之百阻塞冻结，打印日志没有必要。直接抛出异常。
                            throw new NotSupportedException($"分片数大于接收窗口，造成kcp阻塞冻结。frgCount:{newseg.frg + 1}  rcv_wnd:{rcv_wnd}");
                        }
                    }
                    else
                    {
                        rcv_buf.AddAfter(p, newseg);
                    }

                }
                else
                {
                    SegmentManager.Free(newseg);
                }
            }

            Move_Rcv_buf_2_Rcv_queue();
        }

        protected ushort Wnd_unused()
        {
            ///此处没有加锁，所以不要内联变量，否则可能导致 判断变量和赋值变量不一致
            int waitCount = rcv_queue.Count;

            if (waitCount < rcv_wnd)
            {
                //Q:为什么要减去nrcv_que，rcv_queue中已经排好序了，还要算在接收窗口内，感觉不能理解？
                //现在问题是如果一个超大数据包，分片数大于rcv_wnd接收窗口，会导致rcv_wnd持续为0，阻塞整个流程。
                //个人理解，rcv_queue中的数据是已经确认的数据，无论用户是否recv，都不应该影响收发。
                //A:现在在发送出加一个分片数检测，过大直接抛出异常。防止阻塞发送。
                //在接收端也加一个检测，如果（frg+1）分片数 > rcv_wnd,也要抛出一个异常或者警告。至少有个提示。

                /// fix https://github.com/skywind3000/kcp/issues/126
                /// 实际上 rcv_wnd 不应该大于65535
                var count = rcv_wnd - waitCount;
                return (ushort)Min(count, ushort.MaxValue);
            }

            return 0;
        }

        /// <summary>
        /// flush pending data
        /// </summary>
        protected void Flush()
        {
            var current_ = current;
            var buffer_ = buffer;
            var change = 0;
            var lost = 0;
            var offset = 0;

            if (updated == 0)
            {
                return;
            }

            ushort wnd_ = Wnd_unused();

            unsafe
            {
                ///在栈上分配这个segment,这个segment随用随销毁，不会被保存
                const int len = KcpSegment.LocalOffset + KcpSegment.HeadOffset;
                var ptr = stackalloc byte[len];
                KcpSegment seg = new KcpSegment(ptr, 0);
                //seg = KcpSegment.AllocHGlobal(0);

                seg.conv = conv;
                seg.cmd = IKCP_CMD_ACK;
                //seg.frg = 0;
                seg.wnd = wnd_;
                seg.una = rcv_nxt;
                //seg.len = 0;
                //seg.sn = 0;
                //seg.ts = 0;

                #region flush acknowledges

                if (CheckDispose())
                {
                    //检查释放
                    return;
                }

                while (acklist.TryDequeue(out var temp))
                {
                    if (offset + IKCP_OVERHEAD > mtu)
                    {
                        callbackHandle.Output(buffer, offset);
                        offset = 0;
                        buffer = CreateBuffer(BufferNeedSize);

                        //IKcpOutputer outputer = null;
                        //var span = outputer.GetSpan(offset);
                        //buffer.Memory.Span.Slice(0, offset).CopyTo(span);
                        //outputer.Advance(offset);
                        //outputer.Flush();
                    }

                    seg.sn = temp.sn;
                    seg.ts = temp.ts;
                    offset += seg.Encode(buffer.Memory.Span.Slice(offset));
                }

                #endregion

                #region probe window size (if remote window size equals zero)
                // probe window size (if remote window size equals zero)
                if (rmt_wnd == 0)
                {
                    if (probe_wait == 0)
                    {
                        probe_wait = IKCP_PROBE_INIT;
                        ts_probe = current + probe_wait;
                    }
                    else
                    {
                        if (Itimediff(current, ts_probe) >= 0)
                        {
                            if (probe_wait < IKCP_PROBE_INIT)
                            {
                                probe_wait = IKCP_PROBE_INIT;
                            }

                            probe_wait += probe_wait / 2;

                            if (probe_wait > IKCP_PROBE_LIMIT)
                            {
                                probe_wait = IKCP_PROBE_LIMIT;
                            }

                            ts_probe = current + probe_wait;
                            probe |= IKCP_ASK_SEND;
                        }
                    }
                }
                else
                {
                    ts_probe = 0;
                    probe_wait = 0;
                }
                #endregion

                #region flush window probing commands
                // flush window probing commands
                if ((probe & IKCP_ASK_SEND) != 0)
                {
                    seg.cmd = IKCP_CMD_WASK;
                    if (offset + IKCP_OVERHEAD > (int)mtu)
                    {
                        callbackHandle.Output(buffer, offset);
                        offset = 0;
                        buffer = CreateBuffer(BufferNeedSize);
                    }
                    offset += seg.Encode(buffer.Memory.Span.Slice(offset));
                }

                if ((probe & IKCP_ASK_TELL) != 0)
                {
                    seg.cmd = IKCP_CMD_WINS;
                    if (offset + IKCP_OVERHEAD > (int)mtu)
                    {
                        callbackHandle.Output(buffer, offset);
                        offset = 0;
                        buffer = CreateBuffer(BufferNeedSize);
                    }
                    offset += seg.Encode(buffer.Memory.Span.Slice(offset));
                }

                probe = 0;
                #endregion
            }

            #region 刷新，将发送等待列表移动到发送列表

            // calculate window size
            var cwnd_ = Min(snd_wnd, rmt_wnd);
            if (nocwnd == 0)
            {
                cwnd_ = Min(cwnd, cwnd_);
            }

            while (Itimediff(snd_nxt, snd_una + cwnd_) < 0)
            {
                if (snd_queue.TryDequeue(out var newseg))
                {
                    newseg.conv = conv;
                    newseg.cmd = IKCP_CMD_PUSH;
                    newseg.wnd = wnd_;
                    newseg.ts = current_;
                    newseg.sn = snd_nxt;
                    snd_nxt++;
                    newseg.una = rcv_nxt;
                    newseg.resendts = current_;
                    newseg.rto = rx_rto;
                    newseg.fastack = 0;
                    newseg.xmit = 0;
                    lock (snd_bufLock)
                    {
                        snd_buf.AddLast(newseg);
                    }
                }
                else
                {
                    break;
                }
            }

            #endregion

            #region 刷新 发送列表，调用Output

            // calculate resent
            var resent = fastresend > 0 ? (uint)fastresend : 0xffffffff;
            var rtomin = nodelay == 0 ? (rx_rto >> 3) : 0;

            lock (snd_bufLock)
            {
                // flush data segments
                foreach (var item in snd_buf)
                {
                    var segment = item;
                    var needsend = false;
                    var debug = Itimediff(current_, segment.resendts);
                    if (segment.xmit == 0)
                    {
                        //新加入 snd_buf 中, 从未发送过的报文直接发送出去;
                        needsend = true;
                        segment.xmit++;
                        segment.rto = rx_rto;
                        segment.resendts = current_ + rx_rto + rtomin;
                    }
                    else if (Itimediff(current_, segment.resendts) >= 0)
                    {
                        //发送过的, 但是在 RTO 内未收到 ACK 的报文, 需要重传;
                        needsend = true;
                        segment.xmit++;
                        this.xmit++;
                        if (nodelay == 0)
                        {
                            segment.rto += Math.Max(segment.rto, rx_rto);
                        }
                        else
                        {
                            var step = nodelay < 2 ? segment.rto : rx_rto;
                            segment.rto += step / 2;
                        }

                        segment.resendts = current_ + segment.rto;
                        lost = 1;
                    }
                    else if (segment.fastack >= resent)
                    {
                        //发送过的, 但是 ACK 失序若干次的报文, 需要执行快速重传.
                        if (segment.xmit <= fastlimit
                            || fastlimit <= 0)
                        {
                            needsend = true;
                            segment.xmit++;
                            segment.fastack = 0;
                            segment.resendts = current_ + segment.rto;
                            change++;
                        }
                    }

                    if (needsend)
                    {
                        segment.ts = current_;
                        segment.wnd = wnd_;
                        segment.una = rcv_nxt;

                        var need = IKCP_OVERHEAD + segment.len;
                        if (offset + need > mtu)
                        {
                            callbackHandle.Output(buffer, offset);
                            offset = 0;
                            buffer = CreateBuffer(BufferNeedSize);
                        }

                        offset += segment.Encode(buffer.Memory.Span.Slice(offset));

                        if (CanLog(KcpLogMask.IKCP_LOG_NEED_SEND))
                        {
                            LogWriteLine($"{segment.ToLogString(true)}", KcpLogMask.IKCP_LOG_NEED_SEND.ToString());
                        }

                        if (segment.xmit >= dead_link)
                        {
                            state = -1;

                            if (CanLog(KcpLogMask.IKCP_LOG_DEAD_LINK))
                            {
                                LogWriteLine($"state = -1; xmit:{segment.xmit} >= dead_link:{dead_link}", KcpLogMask.IKCP_LOG_DEAD_LINK.ToString());
                            }
                        }
                    }
                }
            }

            // flash remain segments
            if (offset > 0)
            {
                callbackHandle.Output(buffer, offset);
                offset = 0;
                buffer = CreateBuffer(BufferNeedSize);
            }

            #endregion

            #region update ssthresh
            // update ssthresh 根据丢包情况计算 ssthresh 和 cwnd.
            if (change != 0)
            {
                var inflight = snd_nxt - snd_una;
                ssthresh = inflight / 2;
                if (ssthresh < IKCP_THRESH_MIN)
                {
                    ssthresh = IKCP_THRESH_MIN;
                }

                cwnd = ssthresh + resent;
                incr = cwnd * mss;
            }

            if (lost != 0)
            {
                ssthresh = cwnd / 2;
                if (ssthresh < IKCP_THRESH_MIN)
                {
                    ssthresh = IKCP_THRESH_MIN;
                }

                cwnd = 1;
                incr = mss;
            }

            if (cwnd < 1)
            {
                cwnd = 1;
                incr = mss;
            }
            #endregion

            if (state == -1)
            {
                OnDeadlink();
            }
        }

        protected virtual void OnDeadlink()
        { 

        }

        /// <summary>
        /// Test OutputWriter
        /// </summary>
        protected void Flush2()
        {
            var current_ = current;
            var change = 0;
            var lost = 0;

            if (updated == 0)
            {
                return;
            }

            ushort wnd_ = Wnd_unused();

            unsafe
            {
                ///在栈上分配这个segment,这个segment随用随销毁，不会被保存
                const int len = KcpSegment.LocalOffset + KcpSegment.HeadOffset;
                var ptr = stackalloc byte[len];
                KcpSegment seg = new KcpSegment(ptr, 0);
                //seg = KcpSegment.AllocHGlobal(0);

                seg.conv = conv;
                seg.cmd = IKCP_CMD_ACK;
                //seg.frg = 0;
                seg.wnd = wnd_;
                seg.una = rcv_nxt;
                //seg.len = 0;
                //seg.sn = 0;
                //seg.ts = 0;

                #region flush acknowledges

                if (CheckDispose())
                {
                    //检查释放
                    return;
                }

                while (acklist.TryDequeue(out var temp))
                {
                    if (OutputWriter.UnflushedBytes + IKCP_OVERHEAD > mtu)
                    {
                        OutputWriter.Flush();
                    }

                    seg.sn = temp.sn;
                    seg.ts = temp.ts;
                    seg.Encode(OutputWriter);
                }

                #endregion

                #region probe window size (if remote window size equals zero)
                // probe window size (if remote window size equals zero)
                if (rmt_wnd == 0)
                {
                    if (probe_wait == 0)
                    {
                        probe_wait = IKCP_PROBE_INIT;
                        ts_probe = current + probe_wait;
                    }
                    else
                    {
                        if (Itimediff(current, ts_probe) >= 0)
                        {
                            if (probe_wait < IKCP_PROBE_INIT)
                            {
                                probe_wait = IKCP_PROBE_INIT;
                            }

                            probe_wait += probe_wait / 2;

                            if (probe_wait > IKCP_PROBE_LIMIT)
                            {
                                probe_wait = IKCP_PROBE_LIMIT;
                            }

                            ts_probe = current + probe_wait;
                            probe |= IKCP_ASK_SEND;
                        }
                    }
                }
                else
                {
                    ts_probe = 0;
                    probe_wait = 0;
                }
                #endregion

                #region flush window probing commands
                // flush window probing commands
                if ((probe & IKCP_ASK_SEND) != 0)
                {
                    seg.cmd = IKCP_CMD_WASK;
                    if (OutputWriter.UnflushedBytes + IKCP_OVERHEAD > (int)mtu)
                    {
                        OutputWriter.Flush();
                    }
                    seg.Encode(OutputWriter);
                }

                if ((probe & IKCP_ASK_TELL) != 0)
                {
                    seg.cmd = IKCP_CMD_WINS;
                    if (OutputWriter.UnflushedBytes + IKCP_OVERHEAD > (int)mtu)
                    {
                        OutputWriter.Flush();
                    }
                    seg.Encode(OutputWriter);
                }

                probe = 0;
                #endregion
            }

            #region 刷新，将发送等待列表移动到发送列表

            // calculate window size
            var cwnd_ = Min(snd_wnd, rmt_wnd);
            if (nocwnd == 0)
            {
                cwnd_ = Min(cwnd, cwnd_);
            }

            while (Itimediff(snd_nxt, snd_una + cwnd_) < 0)
            {
                if (snd_queue.TryDequeue(out var newseg))
                {
                    newseg.conv = conv;
                    newseg.cmd = IKCP_CMD_PUSH;
                    newseg.wnd = wnd_;
                    newseg.ts = current_;
                    newseg.sn = snd_nxt;
                    snd_nxt++;
                    newseg.una = rcv_nxt;
                    newseg.resendts = current_;
                    newseg.rto = rx_rto;
                    newseg.fastack = 0;
                    newseg.xmit = 0;
                    lock (snd_bufLock)
                    {
                        snd_buf.AddLast(newseg);
                    }
                }
                else
                {
                    break;
                }
            }

            #endregion

            #region 刷新 发送列表，调用Output

            // calculate resent
            var resent = fastresend > 0 ? (uint)fastresend : 0xffffffff;
            var rtomin = nodelay == 0 ? (rx_rto >> 3) : 0;

            lock (snd_bufLock)
            {
                // flush data segments
                foreach (var item in snd_buf)
                {
                    var segment = item;
                    var needsend = false;
                    var debug = Itimediff(current_, segment.resendts);
                    if (segment.xmit == 0)
                    {
                        //新加入 snd_buf 中, 从未发送过的报文直接发送出去;
                        needsend = true;
                        segment.xmit++;
                        segment.rto = rx_rto;
                        segment.resendts = current_ + rx_rto + rtomin;
                    }
                    else if (Itimediff(current_, segment.resendts) >= 0)
                    {
                        //发送过的, 但是在 RTO 内未收到 ACK 的报文, 需要重传;
                        needsend = true;
                        segment.xmit++;
                        this.xmit++;
                        if (nodelay == 0)
                        {
                            segment.rto += Math.Max(segment.rto, rx_rto);
                        }
                        else
                        {
                            var step = nodelay < 2 ? segment.rto : rx_rto;
                            segment.rto += step / 2;
                        }

                        segment.resendts = current_ + segment.rto;
                        lost = 1;
                    }
                    else if (segment.fastack >= resent)
                    {
                        //发送过的, 但是 ACK 失序若干次的报文, 需要执行快速重传.
                        if (segment.xmit <= fastlimit
                            || fastlimit <= 0)
                        {
                            needsend = true;
                            segment.xmit++;
                            segment.fastack = 0;
                            segment.resendts = current_ + segment.rto;
                            change++;
                        }
                    }

                    if (needsend)
                    {
                        segment.ts = current_;
                        segment.wnd = wnd_;
                        segment.una = rcv_nxt;

                        var need = IKCP_OVERHEAD + segment.len;
                        if (OutputWriter.UnflushedBytes + need > mtu)
                        {
                            OutputWriter.Flush();
                        }

                        segment.Encode(OutputWriter);

                        if (CanLog(KcpLogMask.IKCP_LOG_NEED_SEND))
                        {
                            LogWriteLine($"{segment.ToLogString(true)}", KcpLogMask.IKCP_LOG_NEED_SEND.ToString());
                        }

                        if (segment.xmit >= dead_link)
                        {
                            state = -1;

                            if (CanLog(KcpLogMask.IKCP_LOG_DEAD_LINK))
                            {
                                LogWriteLine($"state = -1; xmit:{segment.xmit} >= dead_link:{dead_link}", KcpLogMask.IKCP_LOG_DEAD_LINK.ToString());
                            }
                        }
                    }
                }
            }


            // flash remain segments
            if (OutputWriter.UnflushedBytes > 0)
            {
                OutputWriter.Flush();
            }

            #endregion

            #region update ssthresh
            // update ssthresh 根据丢包情况计算 ssthresh 和 cwnd.
            if (change != 0)
            {
                var inflight = snd_nxt - snd_una;
                ssthresh = inflight / 2;
                if (ssthresh < IKCP_THRESH_MIN)
                {
                    ssthresh = IKCP_THRESH_MIN;
                }

                cwnd = ssthresh + resent;
                incr = cwnd * mss;
            }

            if (lost != 0)
            {
                ssthresh = cwnd / 2;
                if (ssthresh < IKCP_THRESH_MIN)
                {
                    ssthresh = IKCP_THRESH_MIN;
                }

                cwnd = 1;
                incr = mss;
            }

            if (cwnd < 1)
            {
                cwnd = 1;
                incr = mss;
            }
            #endregion

            if (state == -1)
            {
                OnDeadlink();
            }
        }

        /// <summary>
        /// update state (call it repeatedly, every 10ms-100ms), or you can ask
        /// ikcp_check when to call it again (without ikcp_input/_send calling).
        /// </summary>
        /// <param name="time">DateTime.UtcNow</param>
        public void Update(in DateTimeOffset time)
        {
            if (CheckDispose())
            {
                //检查释放
                return;
            }

            current = time.ConvertTime();

            if (updated == 0)
            {
                updated = 1;
                ts_flush = current;
            }

            var slap = Itimediff(current, ts_flush);

            if (slap >= 10000 || slap < -10000)
            {
                ts_flush = current;
                slap = 0;
            }

            if (slap >= 0)
            {
                ts_flush += interval;
                if (Itimediff(current, ts_flush) >= 0)
                {
                    ts_flush = current + interval;
                }

                Flush();
            }
        }

        #endregion

        #region 设置控制

        public int SetMtu(int mtu = IKCP_MTU_DEF)
        {
            if (mtu < 50 || mtu < IKCP_OVERHEAD)
            {
                return -1;
            }

            var buffer_ = CreateBuffer(BufferNeedSize);
            if (null == buffer_)
            {
                return -2;
            }

            this.mtu = (uint)mtu;
            mss = this.mtu - IKCP_OVERHEAD;
            buffer.Dispose();
            buffer = buffer_;
            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="interval_"></param>
        /// <returns></returns>
        public int Interval(int interval_)
        {
            if (interval_ > 5000)
            {
                interval_ = 5000;
            }
            else if (interval_ < 0)
            {
                /// 将最小值 10 改为 0；
                ///在特殊形况下允许CPU满负荷运转；
                interval_ = 0;
            }
            interval = (uint)interval_;
            return 0;
        }

        public int NoDelay(int nodelay_, int interval_, int resend_, int nc_)
        {

            if (nodelay_ > 0)
            {
                nodelay = (uint)nodelay_;
                if (nodelay_ != 0)
                {
                    rx_minrto = IKCP_RTO_NDL;
                }
                else
                {
                    rx_minrto = IKCP_RTO_MIN;
                }
            }

            if (resend_ >= 0)
            {
                fastresend = resend_;
            }

            if (nc_ >= 0)
            {
                nocwnd = nc_;
            }

            return Interval(interval_);
        }

        public int WndSize(int sndwnd = IKCP_WND_SND, int rcvwnd = IKCP_WND_RCV)
        {
            if (sndwnd > 0)
            {
                snd_wnd = (uint)sndwnd;
            }

            if (rcvwnd > 0)
            {
                rcv_wnd = (uint)rcvwnd;
            }

            return 0;
        }

        #endregion


    }

    public partial class KcpCore<Segment> : IKcpSendable
    {
        /// <summary>
        /// user/upper level send, returns below zero for error
        /// </summary>
        /// <param name="span"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public int Send(ReadOnlySpan<byte> span, object options = null)
        {
            if (CheckDispose())
            {
                //检查释放
                return -4;
            }

            if (mss <= 0)
            {
                throw new InvalidOperationException($" mss <= 0 ");
            }


            if (span.Length == 0)
            {
                return -1;
            }
            var offset = 0;
            int count;

            #region append to previous segment in streaming mode (if possible)
            /// 基于线程安全和数据结构的等原因,移除了追加数据到最后一个包行为。
            #endregion

            #region fragment

            if (span.Length <= mss)
            {
                count = 1;
            }
            else
            {
                count = (int)(span.Length + mss - 1) / (int)mss;
            }

            if (count > IKCP_WND_RCV)
            {
                return -2;
            }

            if (count == 0)
            {
                count = 1;
            }

            lock (snd_queueLock)
            {
                for (var i = 0; i < count; i++)
                {
                    int size;
                    if (span.Length - offset > mss)
                    {
                        size = (int)mss;
                    }
                    else
                    {
                        size = (int)span.Length - offset;
                    }

                    var seg = SegmentManager.Alloc(size);
                    span.Slice(offset, size).CopyTo(seg.data);
                    offset += size;
                    seg.frg = (byte)(count - i - 1);
                    snd_queue.Enqueue(seg);
                }
            }

            #endregion

            return 0;
        }

        //public int Send(Span<byte> span)
        //{
        //    return Send((ReadOnlySpan<byte>)span);
        //}

        public int Send(ReadOnlySequence<byte> span, object options = null)
        {
            if (CheckDispose())
            {
                //检查释放
                return -4;
            }

            if (mss <= 0)
            {
                throw new InvalidOperationException($" mss <= 0 ");
            }


            if (span.Length == 0)
            {
                return -1;
            }
            var offset = 0;
            int count;

            #region append to previous segment in streaming mode (if possible)
            /// 基于线程安全和数据结构的等原因,移除了追加数据到最后一个包行为。
            #endregion

            #region fragment

            if (span.Length <= mss)
            {
                count = 1;
            }
            else
            {
                count = (int)(span.Length + mss - 1) / (int)mss;
            }

            if (count > IKCP_WND_RCV)
            {
                return -2;
            }

            if (count == 0)
            {
                count = 1;
            }

            lock (snd_queueLock)
            {
                for (var i = 0; i < count; i++)
                {
                    int size;
                    if (span.Length - offset > mss)
                    {
                        size = (int)mss;
                    }
                    else
                    {
                        size = (int)span.Length - offset;
                    }

                    var seg = SegmentManager.Alloc(size);
                    span.Slice(offset, size).CopyTo(seg.data);
                    offset += size;
                    seg.frg = (byte)(count - i - 1);
                    snd_queue.Enqueue(seg);
                }
            }

            #endregion

            return 0;
        }
    }

    public partial class KcpCore<Segment> : IKcpInputable
    {
        /// <summary>
        /// when you received a low level packet (eg. UDP packet), call it
        /// </summary>
        /// <param name="span"></param>
        /// <returns></returns>
        public int Input(ReadOnlySpan<byte> span)
        {
            if (CheckDispose())
            {
                //检查释放
                return -4;
            }

            if (CanLog(KcpLogMask.IKCP_LOG_INPUT))
            {
                LogWriteLine($"[RI] {span.Length} bytes", KcpLogMask.IKCP_LOG_INPUT.ToString());
            }

            if (span.Length < IKCP_OVERHEAD)
            {
                return -1;
            }

            uint prev_una = snd_una;
            var offset = 0;
            int flag = 0;
            uint maxack = 0;
            uint latest_ts = 0;
            while (true)
            {
                uint ts = 0;
                uint sn = 0;
                uint length = 0;
                uint una = 0;
                uint conv_ = 0;
                ushort wnd = 0;
                byte cmd = 0;
                byte frg = 0;

                if (span.Length - offset < IKCP_OVERHEAD)
                {
                    break;
                }

                Span<byte> header = stackalloc byte[24];
                span.Slice(offset, 24).CopyTo(header);
                offset += ReadHeader(header,
                                     ref conv_,
                                     ref cmd,
                                     ref frg,
                                     ref wnd,
                                     ref ts,
                                     ref sn,
                                     ref una,
                                     ref length);

                if (conv != conv_)
                {
                    return -1;
                }

                if (span.Length - offset < length || (int)length < 0)
                {
                    return -2;
                }

                switch (cmd)
                {
                    case IKCP_CMD_PUSH:
                    case IKCP_CMD_ACK:
                    case IKCP_CMD_WASK:
                    case IKCP_CMD_WINS:
                        break;
                    default:
                        return -3;
                }

                rmt_wnd = wnd;
                Parse_una(una);
                Shrink_buf();

                if (IKCP_CMD_ACK == cmd)
                {
                    if (Itimediff(current, ts) >= 0)
                    {
                        Update_ack(Itimediff(current, ts));
                    }
                    Parse_ack(sn);
                    Shrink_buf();

                    if (flag == 0)
                    {
                        flag = 1;
                        maxack = sn;
                        latest_ts = ts;
                    }
                    else if (Itimediff(sn, maxack) > 0)
                    {
#if !IKCP_FASTACK_CONSERVE
                        maxack = sn;
                        latest_ts = ts;
#else
                        if (Itimediff(ts, latest_ts) > 0)
                        {
                            maxack = sn;
                            latest_ts = ts;
                        }
#endif
                    }

                    if (CanLog(KcpLogMask.IKCP_LOG_IN_ACK))
                    {
                        LogWriteLine($"input ack: sn={sn} rtt={Itimediff(current, ts)} rto={rx_rto}", KcpLogMask.IKCP_LOG_IN_ACK.ToString());
                    }
                }
                else if (IKCP_CMD_PUSH == cmd)
                {
                    if (CanLog(KcpLogMask.IKCP_LOG_IN_DATA))
                    {
                        LogWriteLine($"input psh: sn={sn} ts={ts}", KcpLogMask.IKCP_LOG_IN_DATA.ToString());
                    }

                    if (Itimediff(sn, rcv_nxt + rcv_wnd) < 0)
                    {
                        ///instead of ikcp_ack_push
                        acklist.Enqueue((sn, ts));

                        if (Itimediff(sn, rcv_nxt) >= 0)
                        {
                            var seg = SegmentManager.Alloc((int)length);
                            seg.conv = conv_;
                            seg.cmd = cmd;
                            seg.frg = frg;
                            seg.wnd = wnd;
                            seg.ts = ts;
                            seg.sn = sn;
                            seg.una = una;
                            //seg.len = length;  长度在分配时确定，不能改变

                            if (length > 0)
                            {
                                span.Slice(offset, (int)length).CopyTo(seg.data);
                            }

                            Parse_data(seg);
                        }
                    }
                }
                else if (IKCP_CMD_WASK == cmd)
                {
                    // ready to send back IKCP_CMD_WINS in Ikcp_flush
                    // tell remote my window size
                    probe |= IKCP_ASK_TELL;

                    if (CanLog(KcpLogMask.IKCP_LOG_IN_PROBE))
                    {
                        LogWriteLine($"input probe", KcpLogMask.IKCP_LOG_IN_PROBE.ToString());
                    }
                }
                else if (IKCP_CMD_WINS == cmd)
                {
                    // do nothing
                    if (CanLog(KcpLogMask.IKCP_LOG_IN_WINS))
                    {
                        LogWriteLine($"input wins: {wnd}", KcpLogMask.IKCP_LOG_IN_WINS.ToString());
                    }
                }
                else
                {
                    return -3;
                }

                offset += (int)length;
            }

            if (flag != 0)
            {
                Parse_fastack(maxack, latest_ts);
            }

            if (Itimediff(this.snd_una, prev_una) > 0)
            {
                if (cwnd < rmt_wnd)
                {
                    if (cwnd < ssthresh)
                    {
                        cwnd++;
                        incr += mss;
                    }
                    else
                    {
                        if (incr < mss)
                        {
                            incr = mss;
                        }
                        incr += (mss * mss) / incr + (mss / 16);
                        if ((cwnd + 1) * mss <= incr)
                        {
#if true
                            cwnd = (incr + mss - 1) / ((mss > 0) ? mss : 1);
#else
                            cwnd++;
#endif
                        }
                    }
                    if (cwnd > rmt_wnd)
                    {
                        cwnd = rmt_wnd;
                        incr = rmt_wnd * mss;
                    }
                }
            }

            return 0;
        }

        /// <summary>
        /// <inheritdoc cref="Input(ReadOnlySpan{byte})"/>
        /// </summary>
        /// <param name="span"></param>
        /// <returns></returns>
        public int Input(ReadOnlySequence<byte> span)
        {
            if (CheckDispose())
            {
                //检查释放
                return -4;
            }

            if (CanLog(KcpLogMask.IKCP_LOG_INPUT))
            {
                LogWriteLine($"[RI] {span.Length} bytes", KcpLogMask.IKCP_LOG_INPUT.ToString());
            }

            if (span.Length < IKCP_OVERHEAD)
            {
                return -1;
            }

            uint prev_una = snd_una;
            var offset = 0;
            int flag = 0;
            uint maxack = 0;
            uint latest_ts = 0;
            while (true)
            {
                uint ts = 0;
                uint sn = 0;
                uint length = 0;
                uint una = 0;
                uint conv_ = 0;
                ushort wnd = 0;
                byte cmd = 0;
                byte frg = 0;

                if (span.Length - offset < IKCP_OVERHEAD)
                {
                    break;
                }

                Span<byte> header = stackalloc byte[24];
                span.Slice(offset, 24).CopyTo(header);
                offset += ReadHeader(header,
                                     ref conv_,
                                     ref cmd,
                                     ref frg,
                                     ref wnd,
                                     ref ts,
                                     ref sn,
                                     ref una,
                                     ref length);

                if (conv != conv_)
                {
                    return -1;
                }

                if (span.Length - offset < length || (int)length < 0)
                {
                    return -2;
                }

                switch (cmd)
                {
                    case IKCP_CMD_PUSH:
                    case IKCP_CMD_ACK:
                    case IKCP_CMD_WASK:
                    case IKCP_CMD_WINS:
                        break;
                    default:
                        return -3;
                }

                rmt_wnd = wnd;
                Parse_una(una);
                Shrink_buf();

                if (IKCP_CMD_ACK == cmd)
                {
                    if (Itimediff(current, ts) >= 0)
                    {
                        Update_ack(Itimediff(current, ts));
                    }
                    Parse_ack(sn);
                    Shrink_buf();

                    if (flag == 0)
                    {
                        flag = 1;
                        maxack = sn;
                        latest_ts = ts;
                    }
                    else if (Itimediff(sn, maxack) > 0)
                    {
#if !IKCP_FASTACK_CONSERVE
                        maxack = sn;
                        latest_ts = ts;
#else
                        if (Itimediff(ts, latest_ts) > 0)
                        {
                            maxack = sn;
                            latest_ts = ts;
                        }
#endif
                    }


                    if (CanLog(KcpLogMask.IKCP_LOG_IN_ACK))
                    {
                        LogWriteLine($"input ack: sn={sn} rtt={Itimediff(current, ts)} rto={rx_rto}", KcpLogMask.IKCP_LOG_IN_ACK.ToString());
                    }
                }
                else if (IKCP_CMD_PUSH == cmd)
                {
                    if (CanLog(KcpLogMask.IKCP_LOG_IN_DATA))
                    {
                        LogWriteLine($"input psh: sn={sn} ts={ts}", KcpLogMask.IKCP_LOG_IN_DATA.ToString());
                    }

                    if (Itimediff(sn, rcv_nxt + rcv_wnd) < 0)
                    {
                        ///instead of ikcp_ack_push
                        acklist.Enqueue((sn, ts));

                        if (Itimediff(sn, rcv_nxt) >= 0)
                        {
                            var seg = SegmentManager.Alloc((int)length);
                            seg.conv = conv_;
                            seg.cmd = cmd;
                            seg.frg = frg;
                            seg.wnd = wnd;
                            seg.ts = ts;
                            seg.sn = sn;
                            seg.una = una;
                            //seg.len = length;  长度在分配时确定，不能改变

                            if (length > 0)
                            {
                                span.Slice(offset, (int)length).CopyTo(seg.data);
                            }

                            Parse_data(seg);
                        }
                    }
                }
                else if (IKCP_CMD_WASK == cmd)
                {
                    // ready to send back IKCP_CMD_WINS in Ikcp_flush
                    // tell remote my window size
                    probe |= IKCP_ASK_TELL;

                    if (CanLog(KcpLogMask.IKCP_LOG_IN_PROBE))
                    {
                        LogWriteLine($"input probe", KcpLogMask.IKCP_LOG_IN_PROBE.ToString());
                    }
                }
                else if (IKCP_CMD_WINS == cmd)
                {
                    // do nothing
                    if (CanLog(KcpLogMask.IKCP_LOG_IN_WINS))
                    {
                        LogWriteLine($"input wins: {wnd}", KcpLogMask.IKCP_LOG_IN_WINS.ToString());
                    }
                }
                else
                {
                    return -3;
                }

                offset += (int)length;
            }

            if (flag != 0)
            {
                Parse_fastack(maxack, latest_ts);
            }

            if (Itimediff(this.snd_una, prev_una) > 0)
            {
                if (cwnd < rmt_wnd)
                {
                    if (cwnd < ssthresh)
                    {
                        cwnd++;
                        incr += mss;
                    }
                    else
                    {
                        if (incr < mss)
                        {
                            incr = mss;
                        }
                        incr += (mss * mss) / incr + (mss / 16);
                        if ((cwnd + 1) * mss <= incr)
                        {
#if true
                            cwnd = (incr + mss - 1) / ((mss > 0) ? mss : 1);
#else
                            cwnd++;
#endif
                        }
                    }
                    if (cwnd > rmt_wnd)
                    {
                        cwnd = rmt_wnd;
                        incr = rmt_wnd * mss;
                    }
                }
            }

            return 0;
        }

        public static int ReadHeader(ReadOnlySpan<byte> header,
                              ref uint conv_,
                              ref byte cmd,
                              ref byte frg,
                              ref ushort wnd,
                              ref uint ts,
                              ref uint sn,
                              ref uint una,
                              ref uint length)
        {
            var offset = 0;
            if (IsLittleEndian)
            {
                conv_ = BinaryPrimitives.ReadUInt32LittleEndian(header.Slice(offset));
                offset += 4;

                cmd = header[offset];
                offset += 1;
                frg = header[offset];
                offset += 1;
                wnd = BinaryPrimitives.ReadUInt16LittleEndian(header.Slice(offset));
                offset += 2;

                ts = BinaryPrimitives.ReadUInt32LittleEndian(header.Slice(offset));
                offset += 4;
                sn = BinaryPrimitives.ReadUInt32LittleEndian(header.Slice(offset));
                offset += 4;
                una = BinaryPrimitives.ReadUInt32LittleEndian(header.Slice(offset));
                offset += 4;
                length = BinaryPrimitives.ReadUInt32LittleEndian(header.Slice(offset));
                offset += 4;
            }
            else
            {
                conv_ = BinaryPrimitives.ReadUInt32BigEndian(header.Slice(offset));
                offset += 4;
                cmd = header[offset];
                offset += 1;
                frg = header[offset];
                offset += 1;
                wnd = BinaryPrimitives.ReadUInt16BigEndian(header.Slice(offset));
                offset += 2;

                ts = BinaryPrimitives.ReadUInt32BigEndian(header.Slice(offset));
                offset += 4;
                sn = BinaryPrimitives.ReadUInt32BigEndian(header.Slice(offset));
                offset += 4;
                una = BinaryPrimitives.ReadUInt32BigEndian(header.Slice(offset));
                offset += 4;
                length = BinaryPrimitives.ReadUInt32BigEndian(header.Slice(offset));
                offset += 4;
            }


            return offset;
        }
    }
}
