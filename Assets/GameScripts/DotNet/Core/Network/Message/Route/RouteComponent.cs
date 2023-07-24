#if TENGINE_NET
namespace TEngine.Core.Network;

/// <summary>
/// 自定义Route组件、如果要自定义Route协议必须使用这个组件
/// </summary>
public sealed class RouteComponent : Entity
{
    public readonly Dictionary<long, long> RouteAddress = new Dictionary<long, long>();

    public void AddAddress(long routeType, long routeId)
    {
        this.RouteAddress.Add(routeType, routeId);
    }

    public void RemoveAddress(long routeType)
    {
        this.RouteAddress.Remove(routeType);
    }

    public long GetRouteId(long routeType)
    {
        return this.RouteAddress.TryGetValue(routeType, out var routeId) ? routeId : 0;
    }

    public bool TryGetRouteId(long routeType, out long routeId)
    {
        return this.RouteAddress.TryGetValue(routeType, out routeId);
    }

    public override void Dispose()
    {
        this.RouteAddress.Clear();
        base.Dispose();
    }
}
#endif