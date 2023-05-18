namespace TEngine
{
    public class NetworkEvent
    {
        /// <summary>
        /// 网网络连接成功事件。
        /// </summary>
        public static int NetworkConnectedEvent = StringId.StringToHash("NetworkEvent.NetworkConnectedEvent");
        
        /// <summary>
        /// 网络连接关闭事件。
        /// </summary>
        public static int NetworkClosedEvent = StringId.StringToHash("NetworkEvent.NetworkClosedEvent");
        
        /// <summary>
        /// 网络错误事件。
        /// </summary>
        public static int NetworkErrorEvent = StringId.StringToHash("NetworkEvent.NetworkErrorEvent");
        
        /// <summary>
        /// 用户自定义网络错误事件。
        /// </summary>
        public static int NetworkCustomErrorEvent = StringId.StringToHash("NetworkEvent.NetworkCustomErrorEvent");
        
        /// <summary>
        /// 网络心跳包丢失事件。
        /// </summary>
        public static int NetworkMissHeartBeatEvent = StringId.StringToHash("NetworkEvent.NetworkMissHeartBeatEvent");
    }
}