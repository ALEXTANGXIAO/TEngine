namespace TEngine.Core
{
    public class CoreErrorCode
    {
        public const uint ErrRpcFail = 100000002; // Rpc消息发送失败
        public const uint ErrNotFoundRoute = 100000003; // 没有找到Route消息
        public const uint ErrRouteTimeout = 100000004; // 发送Route消息超时
        public const uint Error_NotFindEntity = 100000008; // 没有找到Entity
        public const uint Error_CopyTimeout = 100000009; // CopyTimeout不能小于或等于0
    }
}