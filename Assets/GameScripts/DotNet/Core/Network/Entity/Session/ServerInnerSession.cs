using TEngine.Core;

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

        // 序列化消息到流中
        var memoryStream = MemoryStreamHelper.GetRecyclableMemoryStream();
        InnerPacketParser.Serialize(message, memoryStream);
        memoryStream.Seek(0, SeekOrigin.Begin);
        // 分发消息
        var packInfo = InnerPackInfo.Create(rpcId, routeId, ((IMessage)message).OpCode(), 0, memoryStream);
        NetworkMessageScheduler.Scheduler(this, packInfo).Coroutine();
    }

    public override void Send(IRouteMessage routeMessage, uint rpcId = 0, long routeId = 0)
    {
        if (IsDisposed)
        {
            return;
        }
        // 序列化消息到流中
        var memoryStream = MemoryStreamHelper.GetRecyclableMemoryStream();
        InnerPacketParser.Serialize(routeMessage, memoryStream);
        memoryStream.Seek(0, SeekOrigin.Begin);
        // 分发消息
        var packInfo = InnerPackInfo.Create(rpcId, routeId, routeMessage.OpCode(), routeMessage.RouteTypeOpCode(), memoryStream);
        NetworkMessageScheduler.Scheduler(this, packInfo).Coroutine();
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