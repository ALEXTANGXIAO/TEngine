using TEngine.Core;
#pragma warning disable CS8603

#if TENGINE_NET
namespace TEngine.Core.Network;

public static class MessageHelper
{
    private static uint _rpcId;
    public const long Timeout = 40000;
    public static readonly SortedDictionary<uint, MessageSender> RequestCallback = new();
    public static readonly Dictionary<uint, MessageSender> TimeoutRouteMessageSenders = new();
    
    /// <summary>
    /// 定时检查过期的Call消息事件。
    /// </summary>
    public struct NetworkMessageUpdate { }

    static MessageHelper()
    {
        TimerScheduler.Instance.Core.RepeatedTimer(10000, new NetworkMessageUpdate());
    }
    
    public static void SendInnerServer(Scene scene, uint routeId, IMessage message)
    {
        scene.Server.GetSession(routeId).Send(message);
    }

    public static void SendInnerRoute(Scene scene, long entityId, IRouteMessage message)
    {
        if (entityId == 0)
        {
            Log.Error($"SendInnerRoute appId == 0");
            return;
        }

        EntityIdStruct entityIdStruct = entityId;
        var session = scene.Server.GetSession(entityIdStruct.RouteId);
        session.Send(message, 0, entityId);
    }

    public static void SendInnerRoute(Scene scene, long entityId, long routeTypeOpCode, MemoryStream message)
    {
        if (entityId == 0)
        {
            Log.Error($"SendInnerRoute appId == 0");
            return;
        }

        EntityIdStruct entityIdStruct = entityId;
        var session = scene.Server.GetSession(entityIdStruct.RouteId);
        session.Send(message, 0, routeTypeOpCode, entityId);
    }

    public static void SendInnerRoute(Scene scene, ICollection<long> routeIdCollection, IRouteMessage message)
    {
        if (routeIdCollection.Count <= 0)
        {
            Log.Error($"SendInnerRoute routeId.Count <= 0");
            return;
        }

        foreach (var routeId in routeIdCollection)
        {
            SendInnerRoute(scene, routeId, message);
        }
    }
    
    public static void SendAddressable(Scene scene, long addressableId, IRouteMessage message)
    {
        CallAddressable(scene, addressableId, message).Coroutine();
    }

    public static async FTask<IResponse> CallInnerRoute(Scene scene, long entityId, long routeTypeOpCode, Type requestType, MemoryStream request)
    {
        if (entityId == 0)
        {
            Log.Error($"CallInnerRoute appId == 0");
            return null;
        }

        EntityIdStruct entityIdStruct = entityId;
        var rpcId = ++_rpcId;
        var session = scene.Server.GetSession(entityIdStruct.RouteId);
        var requestCallback = FTask<IResponse>.Create(false);
        RequestCallback.Add(rpcId, MessageSender.Create(rpcId, requestType, requestCallback));
        session.Send(request, rpcId, routeTypeOpCode, entityId);
        return await requestCallback;
    }

    public static async FTask<IResponse> CallInnerRoute(Scene scene, long entityId, IRouteMessage request)
    {
        if (entityId == 0)
        {
            Log.Error($"CallInnerRoute appId == 0");
            return null;
        }
        
        EntityIdStruct entityIdStruct = entityId;
        var rpcId = ++_rpcId;
        var session = scene.Server.GetSession(entityIdStruct.RouteId);
        var requestCallback = FTask<IResponse>.Create(false);
        RequestCallback.Add(rpcId, MessageSender.Create(rpcId, request, requestCallback));
        session.Send(request, rpcId, entityId);
        return await requestCallback;
    }

    public static async FTask<IResponse> CallInnerServer(Scene scene, uint targetServerId, IRequest request)
    {
        var rpcId = ++_rpcId;
        var session = scene.Server.GetSession(targetServerId);
        var requestCallback = FTask<IResponse>.Create(false);
        RequestCallback.Add(rpcId, MessageSender.Create(rpcId, request, requestCallback));
        session.Send(request, rpcId);
        return await requestCallback;
    }

    public static async FTask<IResponse> CallAddressable(Scene scene, long addressableId, IRouteMessage request)
    {
        var failCount = 0;

        using (await AddressableRouteComponent.AddressableRouteMessageLock.Lock(addressableId,"CallAddressable"))
        {
            var addressableRouteId = await AddressableHelper.GetAddressableRouteId(scene, addressableId);

            while (true)
            {
                if (addressableRouteId == 0)
                {
                    addressableRouteId = await AddressableHelper.GetAddressableRouteId(scene, addressableId);
                }

                if (addressableRouteId == 0)
                {
                    return MessageDispatcherSystem.Instance.CreateResponse(request, CoreErrorCode.ErrNotFoundRoute);
                }

                var iRouteResponse = await MessageHelper.CallInnerRoute(scene, addressableRouteId, request);

                switch (iRouteResponse.ErrorCode)
                {
                    case CoreErrorCode.ErrNotFoundRoute:
                    {
                        if (++failCount > 20)
                        {
                            Log.Error($"AddressableComponent.Call failCount > 20 route send message fail, routeId: {addressableRouteId} AddressableMessageComponent:{addressableId}");
                            return iRouteResponse;
                        }

                        await TimerScheduler.Instance.Core.WaitAsync(500);
                        addressableRouteId = 0;
                        continue;
                    }
                    case CoreErrorCode.ErrRouteTimeout:
                    {
                        Log.Error($"CallAddressableRoute ErrorCode.ErrRouteTimeout Error:{iRouteResponse.ErrorCode} Message:{request}");
                        return iRouteResponse;
                    }
                    default:
                    {
                        return iRouteResponse;
                    }
                }
            }
        }
    }
    
    public static void ResponseHandler(uint rpcId, IResponse response)
    {
        if (!RequestCallback.Remove(rpcId, out var routeMessageSender))
        {
            throw new Exception($"not found rpc, response.RpcId:{rpcId} response message: {response.GetType().Name}");
        }

        ResponseHandler(routeMessageSender, response);
    }

    private static void ResponseHandler(MessageSender messageSender, IResponse response)
    {
        if (response.ErrorCode == CoreErrorCode.ErrRouteTimeout)
        {
#if TENGINE_DEVELOP
            messageSender.Tcs.SetException(new Exception($"Rpc error: request, 注意RouteId消息超时，请注意查看是否死锁或者没有reply: RouteId: {messageSender.RouteId} {messageSender.Request.ToJson()}, response: {response}"));
#else
            messageSender.Tcs.SetException(new Exception($"Rpc error: request, 注意RouteId消息超时，请注意查看是否死锁或者没有reply: RouteId: {messageSender.RouteId} {messageSender.Request}, response: {response}"));
#endif
            messageSender.Dispose();
            return;
        }

        messageSender.Tcs.SetResult(response);
        messageSender.Dispose();
    }
}

#endif