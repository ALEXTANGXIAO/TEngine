namespace TEngine.Core.Network
{
    // 普通路由消息
    public interface IRouteMessage : IRequest
    {
        long RouteTypeOpCode();
    }
    public interface IRouteRequest : IRouteMessage { }
    public interface IRouteResponse : IResponse { }
    // 普通路由Bson消息
    public interface IBsonRouteMessage : IBsonMessage, IRouteMessage { }
    public interface IBsonRouteRequest : IBsonRouteMessage, IRouteRequest { }
    public interface IBsonRouteResponse : IBsonResponse, IRouteResponse { }
    // 可寻址协议
    public interface IAddressableRouteMessage : IRouteMessage { }
    public interface IAddressableRouteRequest : IRouteRequest { }
    public interface IAddressableRouteResponse : IRouteResponse { }
    // 可寻址Bson协议
    public interface IBsonAddressableRouteMessage : IBsonMessage, IAddressableRouteMessage { }
    public interface IBsonAddressableRouteRequest : IBsonRouteMessage, IAddressableRouteRequest { }
    public interface IBsonAddressableRouteResponse : IBsonResponse, IAddressableRouteResponse { }
    // 自定义Route协议
    public interface ICustomRouteMessage : IRouteMessage { }
    public interface ICustomRouteRequest : IRouteRequest { }
    public interface ICustomRouteResponse : IRouteResponse { }
}