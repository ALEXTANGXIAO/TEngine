#if TENGINE_NET
using TEngine.Core;

namespace TEngine.Core.Network;

public sealed class OnNetworkMessageUpdateCheckTimeout : TimerHandler<MessageHelper.NetworkMessageUpdate>
{
    public override void Handler(MessageHelper.NetworkMessageUpdate self)
    {
        var timeNow = TimeHelper.Now;
        
        foreach (var (rpcId, value) in MessageHelper.RequestCallback)
        {
            if (timeNow < value.CreateTime + MessageHelper.Timeout)
            {
                break;
            }

            MessageHelper.TimeoutRouteMessageSenders.Add(rpcId, value);
        }
        
        if (MessageHelper.TimeoutRouteMessageSenders.Count == 0)
        {
            return;
        }
        
        foreach (var (rpcId, routeMessageSender) in MessageHelper.TimeoutRouteMessageSenders)
        {
            uint responseRpcId = 0;

            try
            {
                switch (routeMessageSender.Request)
                {
                    case IRouteMessage iRouteMessage:
                    {
                        // var routeResponse = RouteMessageDispatcher.CreateResponse(iRouteMessage, ErrorCode.ErrRouteTimeout);
                        // responseRpcId = routeResponse.RpcId;
                        // routeResponse.RpcId = routeMessageSender.RpcId;
                        // MessageHelper.ResponseHandler(routeResponse);
                        break;
                    }
                    case IRequest iRequest:
                    {
                        var response = MessageDispatcherSystem.Instance.CreateResponse(iRequest, CoreErrorCode.ErrRpcFail);
                        responseRpcId = routeMessageSender.RpcId;
                        MessageHelper.ResponseHandler(responseRpcId, response);
                        Log.Warning($"timeout rpcId:{rpcId} responseRpcId:{responseRpcId} {iRequest.ToJson()}");
                        break;
                    }
                    default:
                    {
                        Log.Error(routeMessageSender.Request != null
                            ? $"Unsupported protocol type {routeMessageSender.Request.GetType()} rpcId:{rpcId}"
                            : $"Unsupported protocol type:{routeMessageSender.MessageType.FullName} rpcId:{rpcId}");

                        MessageHelper.RequestCallback.Remove(rpcId);
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error($"responseRpcId:{responseRpcId} routeMessageSender.RpcId:{routeMessageSender.RpcId} {e}");
            }
        }

        MessageHelper.TimeoutRouteMessageSenders.Clear();
    }
}
#endif