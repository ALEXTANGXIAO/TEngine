#if TENGINE_NET
using TEngine.Core.Network;
using TEngine.Core;

namespace TEngine.Logic;

public class H_C2M_MessageRequestHandler : AddressableRPC<Unit, H_C2M_MessageRequest, H_M2C_MessageResponse>
{
    protected override async FTask Run(Unit unit, H_C2M_MessageRequest request, H_M2C_MessageResponse response, Action reply)
    {
        Log.Debug($"接收到一个AddressRPC消息 Unit:{unit.Id} message:{request.ToJson()}");
        response.Message = "来自MAP服务器发送的消息";
        await FTask.CompletedTask;
    }
}
#endif