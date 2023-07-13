#if TENGINE_NET
namespace TEngine.Core.Network;

/// <summary>
/// 自定义Route组件、如果要自定义Route协议必须使用这个组件
/// </summary>
public sealed class RouteComponent : Entity
{
    private readonly Dictionary<long, long> _routeAddress = new Dictionary<long, long>();

    public void AddAddress(long routeType, long routeId)
    {
        _routeAddress.Add(routeType, routeId);
    }

    public void RemoveAddress(long routeType)
    {
        _routeAddress.Remove(routeType);
    }

    public long GetRouteId(long routeType)
    {
        return _routeAddress.TryGetValue(routeType, out var routeId) ? routeId : 0;
    }

    public bool TryGetRouteId(long routeType, out long routeId)
    {
        return _routeAddress.TryGetValue(routeType, out routeId);
    }

    public override void Dispose()
    {
        _routeAddress.Clear();
        base.Dispose();
    }
}
#endif