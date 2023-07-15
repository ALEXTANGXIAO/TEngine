#if TENGINE_NET
using System;
using TEngine.Core.Network;
using TEngine.Core;
using TEngine.Core.DataBase;

namespace TEngine.Logic
{
    public class H_C2G_LoginRequestHandler: MessageRPC<H_C2G_LoginRequest, H_G2C_LoginResponse>
    {
        protected override async FTask Run(Session session, H_C2G_LoginRequest request, H_G2C_LoginResponse response, Action reply)
        {
            IDateBase db = session.Scene.World.DateBase;
            List<AccountInfo> result = await db.Query<AccountInfo>(
                t=>t.UserName == request.UserName && 
                        t.Password == request.Password);

            if (result.Count < 1)
            {
                response.ErrorCode = ErrorCode.ERR_AccountOrPasswordError;
                reply();
                return;
            }

            if (result[0].Forbid)
            {
                response.ErrorCode = ErrorCode.ERR_AccountIsForbid;
                reply();
                return;
            }

            Log.Debug($"收到请求登录的消息 request:{request.ToJson()}");
            response.Text = "登录成功";
            response.UID = result[0].UID;
            await FTask.CompletedTask;
        }
    }
}
#endif