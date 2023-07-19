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

            AccountInfo account = result[0];
            
            if (account.Forbid)
            {
                response.ErrorCode = ErrorCode.ERR_AccountIsForbid;
                reply();
                return;
            }

            AccountComponent accountComponent = session.Scene.GetComponent<AccountComponent>();
            if (accountComponent.Get(account.UID) != null)
            {
                response.ErrorCode = ErrorCode.ERR_AccountIsInGame;
                reply();
                return;
            }
            else
            {
                var accountInfo = session.AddComponent<AccountInfo>();
                accountInfo.UID = account.UID;
                accountInfo.SDKUID = account.SDKUID;
                accountComponent.Add(account);
            }

            Log.Debug($"收到请求登录的消息 request:{request.ToJson()}");
            response.Text = "登录成功";
            response.UID = account.UID;
            await FTask.CompletedTask;
         }
    }
}
#endif