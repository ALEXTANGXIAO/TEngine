namespace TEngine
{
    public enum PacketType : byte
    {
        /// <summary>
        /// 未定义。
        /// </summary>
        Undefined = 0,

        /// <summary>
        /// 客户端发往服务器的包。
        /// </summary>
        ClientToServer,

        /// <summary>
        /// 服务器发往客户端的包。
        /// </summary>
        ServerToClient,
    }
}