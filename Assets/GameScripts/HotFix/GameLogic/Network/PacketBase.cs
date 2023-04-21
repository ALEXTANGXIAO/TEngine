using TEngine;

namespace GameLogic
{
    /// <summary>
    /// 网络消息包基类。
    /// </summary>
    public abstract class PacketBase : Packet
    {
        /// <summary>
        /// 网络消息包Id。
        /// </summary>
        public int ProtoId;

        /// <summary>
        /// 网络消息包包体。
        /// </summary>
        public byte[] ProtoBody;
    
        public void Close()
        {
        }
    }
}