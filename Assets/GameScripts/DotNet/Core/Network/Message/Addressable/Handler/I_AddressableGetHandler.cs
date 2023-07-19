#if TENGINE_NET
namespace TEngine.Core.Network;

public sealed class I_AddressableGetHandler : RouteRPC<Scene, I_AddressableGet_Request, I_AddressableGet_Response>
{
    protected override async FTask Run(Scene scene, I_AddressableGet_Request request, I_AddressableGet_Response response, Action reply)
    {
        response.RouteId =  await scene.GetComponent<AddressableManageComponent>().Get(request.AddressableId);
    }
}
#endif
