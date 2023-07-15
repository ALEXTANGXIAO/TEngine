#if TENGINE_NET
using TEngine.Core.Network;
using TEngine.Core;

namespace TEngine.Logic;

public class H_C2M_PushAddressMessageToClientHandler : Addressable<Unit, H_C2M_PushAddressMessageToClient>
{
    protected override async FTask Run(Unit unit, H_C2M_PushAddressMessageToClient message)
    {
        Log.Debug($"接收到一个Address消息 Unit:{unit.Id} message:{message.ToJson()}");
        // 发过主动推送给客户端、首先要只要Unit在Gate的RouteId、然后才可以发送
        // 这个Route在I_G2M_LoginAddressRequest、也就是注册Address的时候已经记录了
        MessageHelper.SendInnerRoute(unit.Scene, unit.GateRouteId, new H_M2C_ReceiveAddressMessageToServer()
        {
            Message = message.Message
        });
        await FTask.CompletedTask;
    }
}
#endif