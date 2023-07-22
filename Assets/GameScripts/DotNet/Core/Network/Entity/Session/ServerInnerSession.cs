#if TENGINE_NET
namespace TEngine.Core.Network;

public sealed class ServerInnerSession : Session
{
    public override void Send(object message, uint rpcId = 0, long routeId = 0)
    {
        if (IsDisposed)
        {
            return;
        }
        
        NetworkMessageScheduler.InnerScheduler(this, rpcId, routeId, ((IMessage)message).OpCode(), 0, message).Coroutine();
    }

    public override void Send(IRouteMessage routeMessage, uint rpcId = 0, long routeId = 0)
    {
        if (IsDisposed)
        {
            return;
        }

        NetworkMessageScheduler.InnerScheduler(this, rpcId, routeId, routeMessage.OpCode(), routeMessage.RouteTypeOpCode(), routeMessage).Coroutine();
    }

    public override void Send(MemoryStream memoryStream, uint rpcId = 0, long routeTypeOpCode = 0, long routeId = 0)
    {
        throw new Exception("The use of this method is not supported");
    }

    public override FTask<IResponse> Call(IRequest request, long routeId = 0)
    {
        throw new Exception("The use of this method is not supported");
    }
}
#endif