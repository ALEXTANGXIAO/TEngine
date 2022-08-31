namespace TEngine.Runtime
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
        TcpWithSyncReceive,

        /// <summary>
        /// Udp 网络服务。
        /// </summary>
        Udp,

        /// <summary>
        /// Kcp 网络服务。
        /// </summary>
        Kcp,
    }
}
