#if TENGINE_NET
using TEngine.Core.Network;
using TEngine.Core;

namespace TEngine.Logic
{
    public class H_C2G_MessageHandler : Message<H_C2G_Message>
    {
        protected override async FTask Run(Session session, H_C2G_Message message)
        {
            Log.Debug($"接收到消息 H_C2G_Message:{message.ToJson()}");
            await FTask.CompletedTask;
        }
    }
}
#endif