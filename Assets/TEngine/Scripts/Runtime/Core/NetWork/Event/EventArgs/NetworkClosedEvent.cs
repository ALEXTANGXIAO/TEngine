namespace TEngine.Runtime
{
    /// <summary>
    /// 网络连接关闭事件。
    /// </summary>
    public sealed class NetworkClosedEventArgs : GameEventArgs
    {
        /// <summary>
        /// 网络连接关闭事件编号。
        /// </summary>
        public static readonly int EventId = typeof(NetworkClosedEventArgs).GetHashCode();
        
        /// <summary>
        /// 初始化网络连接关闭事件的新实例。
        /// </summary>
        public NetworkClosedEventArgs()
        {
            NetworkChannel = null;
        }
        
        /// <summary>
        /// 获取网络连接关闭事件编号。
        /// </summary>
        public override int Id
        {
            get
            {
                return EventId;
            }
        }

        /// <summary>
        /// 获取网络频道。
        /// </summary>
        public INetworkChannel NetworkChannel
        {
            get;
            private set;
        }

        /// <summary>
        /// 创建网络连接关闭事件。
        /// </summary>
        /// <param name="networkChannel">网络频道。</param>
        /// <returns>创建的网络连接关闭事件。</returns>
        public static NetworkClosedEventArgs Create(INetworkChannel networkChannel)
        {
            NetworkClosedEventArgs networkClosedEventArgs = MemoryPool.Acquire<NetworkClosedEventArgs>();
            networkClosedEventArgs.NetworkChannel = networkChannel;
            return networkClosedEventArgs;
        }

        /// <summary>
        /// 清理网络连接关闭事件。
        /// </summary>
        public override void Clear()
        {
            NetworkChannel = null;
        }
    }
    
    /// <summary>
    /// 网络连接关闭事件。
    /// </summary>
    public sealed class NetworkClosedEvent : GameEventArgs
    {
        /// <summary>
        /// 网络连接关闭事件编号。
        /// </summary>
        public static readonly int EventId = typeof(NetworkClosedEvent).GetHashCode();

        /// <summary>
        /// 初始化网络连接关闭事件的新实例。
        /// </summary>
        public NetworkClosedEvent()
        {
            NetworkChannel = null;
        }

        /// <summary>
        /// 获取网络连接关闭事件编号。
        /// </summary>
        public override int Id
        {
            get
            {
                return EventId;
            }
        }

        /// <summary>
        /// 获取网络频道。
        /// </summary>
        public INetworkChannel NetworkChannel
        {
            get;
            private set;
        }

        /// <summary>
        /// 创建网络连接关闭事件。
        /// </summary>
        /// <param name="e">内部事件。</param>
        /// <returns>创建的网络连接关闭事件。</returns>
        public static NetworkClosedEvent Create(NetworkClosedEventArgs e)
        {
            NetworkClosedEvent networkClosedEventArgs = MemoryPool.Acquire<NetworkClosedEvent>();
            networkClosedEventArgs.NetworkChannel = e.NetworkChannel;
            return networkClosedEventArgs;
        }

        /// <summary>
        /// 清理网络连接关闭事件。
        /// </summary>
        public override void Clear()
        {
            NetworkChannel = null;
        }
    }
}