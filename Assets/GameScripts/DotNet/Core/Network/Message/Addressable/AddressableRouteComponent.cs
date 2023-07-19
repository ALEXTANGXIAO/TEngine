#pragma warning disable CS8603
#pragma warning disable CS8600
#if TENGINE_NET
namespace TEngine.Core.Network;

public sealed class AddressableRouteComponentAwakeSystem : AwakeSystem<AddressableRouteComponent>
{
    protected override void Awake(AddressableRouteComponent self)
    {
        self.Awake();
    }
}

/// <summary>
/// 可寻址消息组件、挂载了这个组件可以接收和发送Addressable消息
/// </summary>
public sealed class AddressableRouteComponent : Entity
{
    private long _parentId;
    private long _addressableRouteId;
    public static readonly CoroutineLockQueueType AddressableRouteMessageLock = new CoroutineLockQueueType("AddressableRouteMessageLock");

    public override void Dispose()
    {
        _parentId = 0;
        _addressableRouteId = 0;
        base.Dispose();
    }
    
    public void Awake()
    {
        if (Parent == null)
        {
            throw new Exception("AddressableRouteComponent must be mounted under a component");
        }
        
        if (Parent.RuntimeId == 0)
        {
            throw new Exception("AddressableRouteComponent.Parent.RuntimeId is null");
        }

        _parentId = Parent.Id;
    }

    public void SetAddressableRouteId(long addressableRouteId)
    {
        _addressableRouteId = addressableRouteId;
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

        using (await AddressableRouteMessageLock.Lock(_parentId, "AddressableRouteComponent Call MemoryStream"))
        {
            while (!IsDisposed)
            {
                if (_addressableRouteId == 0)
                {
                    _addressableRouteId = await AddressableHelper.GetAddressableRouteId(Scene, _parentId);
                }

                if (_addressableRouteId == 0)
                {
                    return MessageDispatcherSystem.Instance.CreateResponse(requestType, CoreErrorCode.ErrNotFoundRoute);
                }

                iRouteResponse = await MessageHelper.CallInnerRoute(Scene, _addressableRouteId, routeTypeOpCode, requestType, request);

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
                            Log.Error($"AddressableComponent.Call failCount > 20 route send message fail, routeId: {_addressableRouteId} AddressableRouteComponent:{Id}");
                            return iRouteResponse;
                        }

                        await TimerScheduler.Instance.Core.WaitAsync(500);

                        if (runtimeId != RuntimeId)
                        {
                            iRouteResponse.ErrorCode = CoreErrorCode.ErrRouteTimeout;
                        }

                        _addressableRouteId = 0;
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

        using (await AddressableRouteMessageLock.Lock(_parentId,"AddressableRouteComponent Call"))
        {
            while (true)
            {
                if (_addressableRouteId == 0)
                {
                    _addressableRouteId = await AddressableHelper.GetAddressableRouteId(Scene, _parentId);
                }

                if (_addressableRouteId == 0)
                {
                    return MessageDispatcherSystem.Instance.CreateResponse(request, CoreErrorCode.ErrNotFoundRoute);
                }

                var iRouteResponse = await MessageHelper.CallInnerRoute(Scene, _addressableRouteId, request);

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
                            Log.Error($"AddressableRouteComponent.Call failCount > 20 route send message fail, routeId: {_addressableRouteId} AddressableRouteComponent:{Id}");
                            return iRouteResponse;
                        }

                        await TimerScheduler.Instance.Core.WaitAsync(500);

                        if (runtimeId != RuntimeId)
                        {
                            iRouteResponse.ErrorCode = CoreErrorCode.ErrRouteTimeout;
                        }

                        _addressableRouteId = 0;
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