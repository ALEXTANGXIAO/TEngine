namespace TEngine.Core
{
    public class CoreErrorCode
    {
        public const int ErrRpcFail = 100000002; // Rpc消息发送失败
        public const int ErrNotFoundRoute = 100000003; // 没有找到Route消息
        public const int ErrRouteTimeout = 100000004; // 发送Route消息超时
        public const int Error_NotFindEntity = 100000008; // 没有找到Entity
    }
}