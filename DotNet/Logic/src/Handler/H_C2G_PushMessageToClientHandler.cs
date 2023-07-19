#if TENGINE_NET
using TEngine.Core.Network;
using TEngine.Core;

namespace TEngine.Logic;

public class H_C2G_PushMessageToClientHandler : Message<H_C2G_PushMessageToClient>
{
    protected override async FTask Run(Session session, H_C2G_PushMessageToClient message)
    {
        Log.Debug($"接收到客户端发送给服务器的请求推送消息:{message.ToJson()}");
        // 服务器主动推送给客户端消息
        session.Send(new H_G2C_ReceiveMessageToServer()
        {
            Message = "这个是服务器推送给客户端的消息"
        });
        await FTask.CompletedTask;
    }
}
#endif