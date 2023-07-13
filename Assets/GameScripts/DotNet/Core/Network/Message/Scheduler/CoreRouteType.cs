namespace TEngine.Core.Network
{
    public class CoreRouteType
    {
        /// <summary>
        /// 基础Route协议、框架内置千万不要删除
        /// </summary>
        public const long Route = 1; 
        /// <summary>
        /// 基础BsonRoute协议、框架内置千万不要删除
        /// </summary>
        public const long BsonRoute = 2; 
        /// <summary>
        /// 基础Addressable协议、框架内置千万不要删除
        /// </summary>
        public const long Addressable = 3;
        /// <summary>
        /// 自定义RouteType、框架内置千万不要删除
        /// </summary>
        public const long CustomRouteType = 1000;
    }
}