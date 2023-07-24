#if TENGINE_NET
namespace TEngine.Core.Network;

/// <summary>
/// 可寻址消息组件、挂载了这个组件可以接收和发送Addressable消息
/// </summary>
public sealed class AddressableRouteComponent : Entity
{
    private long _routeId;
    public long AddressableId { get; private set; }
    
    public static readonly CoroutineLockQueueType AddressableRouteMessageLock = new CoroutineLockQueueType("AddressableRouteMessageLock");

    public override void Dispose()
    {
        _routeId = 0;
        this.AddressableId = 0;
        base.Dispose();
    }

    public void SetAddressableId(long addressableId)
    {
        this.AddressableId = addressableId;
    }

    public void Send(IAddressableRouteMessage message)
    {
        Call(message).Coroutine();
    }

    public void Send(long routeTypeOpCode, Type requestType, MemoryStream message)
    {
        Call(routeTypeOpCode, requestType, message).Coroutine();
    }

    public async FTask<IResponse> Call(long routeTypeOpCode, Type requestType, MemoryStream request)
    {
        if (IsDisposed)
        {
            return MessageDispatcherSystem.Instance.CreateResponse(requestType, CoreErrorCode.ErrNotFoundRoute);
        }

        var failCount = 0;
        var runtimeId = RuntimeId;
        IResponse iRouteResponse = null;

        using (await AddressableRouteMessageLock.Lock(this.AddressableId, "AddressableRouteComponent Call MemoryStream"))
        {
            while (!IsDisposed)
            {
                if (_routeId == 0)
                {
                    _routeId = await AddressableHelper.GetAddressableRouteId(Scene, this.AddressableId);
                }

                if (_routeId == 0)
                {
                    return MessageDispatcherSystem.Instance.CreateResponse(requestType, CoreErrorCode.ErrNotFoundRoute);
                }

                iRouteResponse = await MessageHelper.CallInnerRoute(Scene, _routeId, routeTypeOpCode, requestType, request);

                if (runtimeId != RuntimeId)
                {
                    iRouteResponse.ErrorCode = CoreErrorCode.ErrRouteTimeout;
                }

                switch (iRouteResponse.ErrorCode)
                {
                    case CoreErrorCode.ErrRouteTimeout:
                    {
                        return iRouteResponse;
                    }
                    case CoreErrorCode.ErrNotFoundRoute:
                    {
                        if (++failCount > 20)
                        {
                            Log.Error($"AddressableComponent.Call failCount > 20 route send message fail, routeId: {_routeId} AddressableRouteComponent:{Id}");
                            return iRouteResponse;
                        }

                        await TimerScheduler.Instance.Core.WaitAsync(500);

                        if (runtimeId != RuntimeId)
                        {
                            iRouteResponse.ErrorCode = CoreErrorCode.ErrRouteTimeout;
                        }

                        _routeId = 0;
                        continue;
                    }
                    default:
                    {
                        return iRouteResponse;
                    }
                }
            }
        }

        return iRouteResponse;
    }

    public async FTask<IResponse> Call(IAddressableRouteMessage request)
    {
        if (IsDisposed)
        {
            return MessageDispatcherSystem.Instance.CreateResponse(request, CoreErrorCode.ErrNotFoundRoute);
        }

        var failCount = 0;
        var runtimeId = RuntimeId;

        using (await AddressableRouteMessageLock.Lock(this.AddressableId,"AddressableRouteComponent Call"))
        {
            while (true)
            {
                if (_routeId == 0)
                {
                    _routeId = await AddressableHelper.GetAddressableRouteId(Scene, this.AddressableId);
                }

                if (_routeId == 0)
                {
                    return MessageDispatcherSystem.Instance.CreateResponse(request, CoreErrorCode.ErrNotFoundRoute);
                }

                var iRouteResponse = await MessageHelper.CallInnerRoute(Scene, _routeId, request);

                if (runtimeId != RuntimeId)
                {
                    iRouteResponse.ErrorCode = CoreErrorCode.ErrRouteTimeout;
                }

                switch (iRouteResponse.ErrorCode)
                {
                    case CoreErrorCode.ErrNotFoundRoute:
                    {
                        if (++failCount > 20)
                        {
                            Log.Error($"AddressableRouteComponent.Call failCount > 20 route send message fail, routeId: {_routeId} AddressableRouteComponent:{Id}");
                            return iRouteResponse;
                        }

                        await TimerScheduler.Instance.Core.WaitAsync(500);

                        if (runtimeId != RuntimeId)
                        {
                            iRouteResponse.ErrorCode = CoreErrorCode.ErrRouteTimeout;
                        }

                        _routeId = 0;
                        continue;
                    }
                    case CoreErrorCode.ErrRouteTimeout:
                    {
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
}
#endif