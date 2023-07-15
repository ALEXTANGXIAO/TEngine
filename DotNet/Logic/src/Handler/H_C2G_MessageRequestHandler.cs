#if TENGINE_NET
using TEngine.Core.Network;
using TEngine.Core;

namespace TEngine.Logic;

public class H_C2G_MessageRequestHandler : MessageRPC<H_C2G_MessageRequest,H_G2C_MessageResponse>
{
    protected override async FTask Run(Session session, H_C2G_MessageRequest request, H_G2C_MessageResponse response, Action reply)
    {
        // 这里是接收到客户端发送的消息
        Log.Debug($"接收到RPC消息 H_C2G_MessageRequest:{request.ToJson()}");
        // response是要给客户端返回的消息、数据结构是在proto文件里定义的
        response.Message = "Hello world，您现在收到的消息是一个RPC消息";
        await FTask.CompletedTask;
    }
}
#endif