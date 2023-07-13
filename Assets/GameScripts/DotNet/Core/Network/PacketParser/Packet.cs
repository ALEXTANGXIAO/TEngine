namespace TEngine.Core.Network
{
    public struct Packet
    {
        /// <summary>
        /// 消息体最大长度
        /// </summary>
        public const int PacketBodyMaxLength = ushort.MaxValue * 16;
        /// <summary>
        /// 消息体长度在消息头占用的长度
        /// </summary>
        public const int PacketLength = sizeof(int);
        /// <summary>
        /// 协议编号在消息头占用的长度
        /// </summary>
        public const int ProtocolCodeLength = sizeof(uint);
        /// <summary>
        /// RouteId长度
        /// </summary>
        public const int PacketRouteIdLength = sizeof(long);
        /// <summary>
        /// RpcId在消息头占用的长度
        /// </summary>
        public const int RpcIdLength = sizeof(uint);
        /// <summary>
        /// RouteTypeOpCode在消息头占用的长度
        /// </summary>
        public const int RouteTypeOpCodeLength = sizeof(long);
        /// <summary>
        /// OuterRPCId所在的位置
        /// </summary>
        public const int OuterPacketRpcIdLocation = PacketLength + ProtocolCodeLength;
        /// <summary>
        /// InnerRPCId所在的位置
        /// </summary>
        public const int InnerPacketRpcIdLocation = PacketLength + ProtocolCodeLength;
        /// <summary>
        /// RouteTypeOpCode所在的位置
        /// </summary>
        public const int OuterPacketRouteTypeOpCodeLocation = OuterPacketRpcIdLocation + RpcIdLength;
        /// <summary>
        /// RouteId所在的位置
        /// </summary>
        public const int InnerPacketRouteRouteIdLocation = PacketLength + ProtocolCodeLength + RpcIdLength;
        /// <summary>
        /// 外网消息总长度（消息体最大长度 + 外网消息头长度）
        /// </summary>
        public const int PacketMaxLength = OuterPacketHeadLength + PacketBodyMaxLength;
        /// <summary>
        /// 外网消息头长度（消息体长度在消息头占用的长度 + 协议编号在消息头占用的长度 + RPCId长度 + RouteTypeOpCode长度）
        /// </summary>
        public const int OuterPacketHeadLength = PacketLength + ProtocolCodeLength + RpcIdLength + RouteTypeOpCodeLength;
        /// <summary>
        /// 内网消息头长度（消息体长度在消息头占用的长度 + 协议编号在消息头占用的长度 + RPCId长度 + RouteId长度）
        /// </summary>
        public const int InnerPacketHeadLength = PacketLength + ProtocolCodeLength + RpcIdLength + PacketRouteIdLength;
    }
}