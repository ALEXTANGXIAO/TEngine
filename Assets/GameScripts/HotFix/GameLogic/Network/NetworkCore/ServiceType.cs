namespace TEngine
{
    /// <summary>
    /// 网络服务类型。
    /// </summary>
    public enum ServiceType : byte
    {
        /// <summary>
        /// TCP 网络服务。
        /// </summary>
        Tcp = 0,

        /// <summary>
        /// 使用同步接收的 TCP 网络服务。
        /// </summary>
        TcpWithSyncReceive = 1,
        
        /// <summary>
        /// UDP 网络服务。
        /// </summary>
        Udp = 2,
        
        /// <summary>
        /// KCP 网络服务。
        /// </summary>
        Kcp = 3,
    }
}
