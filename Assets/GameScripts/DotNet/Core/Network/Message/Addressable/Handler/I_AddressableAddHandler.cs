#if TENGINE_NET
namespace TEngine.Core.Network;

public sealed class I_AddressableAddHandler : RouteRPC<Scene, I_AddressableAdd_Request, I_AddressableAdd_Response>
{
    protected override async FTask Run(Scene scene, I_AddressableAdd_Request request, I_AddressableAdd_Response response, Action reply)
    {
        await scene.GetComponent<AddressableManageComponent>().Add(request.AddressableId, request.RouteId, request.IsLock);
    }
}
#endif