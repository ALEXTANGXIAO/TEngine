namespace TEngine.Runtime
{
    /// <summary>
    /// 网络心跳包丢失事件。
    /// </summary>
    public sealed class NetworkMissHeartBeatEventArgs : GameEventArgs
    {
        /// <summary>
        /// 网络心跳包丢失事件编号。
        /// </summary>
        public static readonly int EventId = typeof(NetworkMissHeartBeatEventArgs).GetHashCode();

        /// <summary>
        /// 初始化网络心跳包丢失事件的新实例。
        /// </summary>
        public NetworkMissHeartBeatEventArgs()
        {
            NetworkChannel = null;
            MissCount = 0;
        }

        /// <summary>
        /// 获取网络心跳包丢失事件编号。
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
        /// 获取心跳包已丢失次数。
        /// </summary>
        public int MissCount
        {
            get;
            private set;
        }

        /// <summary>
        /// 创建网络心跳包丢失事件。
        /// </summary>
        /// <param name="networkChannel">网络频道。</param>
        /// <param name="missCount">心跳包已丢失次数。</param>
        /// <returns>创建的网络心跳包丢失事件。</returns>
        public static NetworkMissHeartBeatEventArgs Create(INetworkChannel networkChannel, int missCount)
        {
            NetworkMissHeartBeatEventArgs networkMissHeartBeatEventArgs = MemoryPool.Acquire<NetworkMissHeartBeatEventArgs>();
            networkMissHeartBeatEventArgs.NetworkChannel = networkChannel;
            networkMissHeartBeatEventArgs.MissCount = missCount;
            return networkMissHeartBeatEventArgs;
        }

        /// <summary>
        /// 清理网络心跳包丢失事件。
        /// </summary>
        public override void Clear()
        {
            NetworkChannel = null;
            MissCount = 0;
        }
    }
    
    /// <summary>
    /// 网络心跳包丢失事件。
    /// </summary>
    public sealed class NetworkMissHeartBeatEvent : GameEventArgs
    {
        /// <summary>
        /// 网络心跳包丢失事件编号。
        /// </summary>
        public static readonly int EventId = typeof(NetworkMissHeartBeatEvent).GetHashCode();

        /// <summary>
        /// 初始化网络心跳包丢失事件的新实例。
        /// </summary>
        public NetworkMissHeartBeatEvent()
        {
            NetworkChannel = null;
            MissCount = 0;
        }

        /// <summary>
        /// 获取网络心跳包丢失事件编号。
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
        /// 获取心跳包已丢失次数。
        /// </summary>
        public int MissCount
        {
            get;
            private set;
        }

        /// <summary>
        /// 创建网络心跳包丢失事件。
        /// </summary>
        /// <param name="e">内部事件。</param>
        /// <returns>创建的网络心跳包丢失事件。</returns>
        public static NetworkMissHeartBeatEvent Create(NetworkMissHeartBeatEventArgs e)
        {
            NetworkMissHeartBeatEvent networkMissHeartBeatEventArgs = MemoryPool.Acquire<NetworkMissHeartBeatEvent>();
            networkMissHeartBeatEventArgs.NetworkChannel = e.NetworkChannel;
            networkMissHeartBeatEventArgs.MissCount = e.MissCount;
            return networkMissHeartBeatEventArgs;
        }

        /// <summary>
        /// 清理网络心跳包丢失事件。
        /// </summary>
        public override void Clear()
        {
            NetworkChannel = null;
            MissCount = 0;
        }
    }
}