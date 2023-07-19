namespace TEngine.Core.Network
{
    public static class Opcode
    {
        // 外网消息
        public const uint OuterMessage = 100000000;
        public const uint OuterRequest = 110000000;
        // 内网消息
        public const uint InnerMessage = 120000000;
        public const uint InnerRequest = 130000000;
        // 内网Bson消息
        public const uint InnerBsonMessage = 140000000;
        public const uint InnerBsonRequest = 150000000;
        // 回复消息
        public const uint OuterResponse = 160000000;
        public const uint InnerResponse = 170000000;
        public const uint InnerBsonResponse = 180000000;
        // 外网路由消息
        public const uint OuterRouteMessage = 190000000;
        public const uint OuterRouteRequest = 200000000;
        // 内网路由消息
        public const uint InnerRouteMessage = 210000000;
        public const uint InnerRouteRequest = 220000000;
        // 内网Bson路由消息
        public const uint InnerBsonRouteMessage = 230000000;
        public const uint InnerBsonRouteRequest = 240000000;
        // 路由回复消息
        public const uint OuterRouteResponse = 250000000;
        public const uint InnerRouteResponse = 260000000;
        public const uint InnerBsonRouteResponse = 270000000;
        // 心跳消息
        public const uint PingRequest = 1;
        public const uint PingResponse = 2;
        // 默认回复消息
        public const uint DefaultResponse = 3;
        // Addressable可寻址消息
        public const uint AddressableAddRequest = InnerRouteRequest + 1;
        public const uint AddressableAddResponse = InnerRouteResponse + 1;
        public const uint AddressableGetRequest = InnerRouteRequest + 2;
        public const uint AddressableGetResponse = InnerRouteResponse + 2;
        public const uint AddressableRemoveRequest = InnerRouteRequest + 3;
        public const uint AddressableRemoveResponse = InnerRouteResponse + 3;
        public const uint AddressableLockRequest = InnerRouteRequest + 4;
        public const uint AddressableLockResponse = InnerRouteResponse + 4;
        public const uint AddressableUnLockRequest = InnerRouteRequest + 5;
        public const uint AddressableUnLockResponse = InnerRouteResponse + 5;
        // 默认的Route返回消息
        public const uint DefaultRouteResponse = InnerRouteResponse + 6;
    }
}