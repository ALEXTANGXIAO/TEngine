#if TENGINE_NET
namespace TEngine.Core.Network;

public sealed class I_AddressableUnLockHandler : RouteRPC<Scene, I_AddressableUnLock_Request, I_AddressableUnLock_Response>
{
    protected override async FTask Run(Scene scene, I_AddressableUnLock_Request request, I_AddressableUnLock_Response response, Action reply)
    {
        scene.GetComponent<AddressableManageComponent>().UnLock(request.AddressableId, request.RouteId, request.Source);
        await FTask.CompletedTask;
    }
}
#endif