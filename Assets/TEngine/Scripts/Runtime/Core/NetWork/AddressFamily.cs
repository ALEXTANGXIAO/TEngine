namespace TEngine.Runtime
{
    /// <summary>
    /// 网络地址类型。
    /// </summary>
    public enum AddressFamily : byte
    {
        /// <summary>
        /// 未知。
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// IP 版本 4。
        /// </summary>
        IPv4,

        /// <summary>
        /// IP 版本 6。
        /// </summary>
        IPv6
    }
}