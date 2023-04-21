using TEngine;

namespace GameLogic
{
    /// <summary>
    /// 网络消息包头。
    /// </summary>
    public class PacketHeader : IPacketHeader, IMemory
    {
        /// <summary>
        /// 网络消息包Id。
        /// </summary>
        public short Id
        {
            get;
            set;
        }
    
        /// <summary>
        /// 网络消息包长度。
        /// </summary>
        public int PacketLength
        {
            get;
            set;
        }
    
        /// <summary>
        /// 网络消息包是否合法。
        /// </summary>
        public bool IsValid
        {
            get
            {
                return PacketLength >= 0;
            }
        }

        /// <summary>
        /// 清除网络消息包头。
        /// </summary>
        public void Clear()
        {
            PacketLength = 0;
        }
    }
}