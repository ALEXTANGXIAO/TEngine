using TEngine.Core.Network;
using TEngine.Core;
using TEngine.Core.DataBase;

namespace TEngine.Logic
{
    public class H_C2G_RegisterRequestHandler: MessageRPC<H_C2G_RegisterRequest, H_G2C_RegisterResponse>
    {
        protected override async FTask Run(Session session, H_C2G_RegisterRequest request, H_G2C_RegisterResponse response, Action reply)
        {
            IDateBase db = session.Scene.World.DateBase;
            bool isSDKRegister = request.SDKUID != 0;
            
            List<AccountInfo> result = !isSDKRegister ? 
                    await db.Query<AccountInfo>(t=>t.UserName == request.UserName) : 
                    await db.Query<AccountInfo>(t=>t.SDKUID == request.SDKUID) ;

            if (result.Count == 1)
            {
                response.ErrorCode = ErrorCode.ERR_AccountAlreadyRegisted;
                reply();
                return;
            }
            else if (result.Count >= 1)
            {
                response.ErrorCode = ErrorCode.ERR_AccountAlreadyRegisted;
                Log.Error("出现重复账号：" + request.UserName);
                reply();
                return;
            }

            uint uid = await GeneratorUID(db);

            AccountInfo accountInfo = Entity.Create<AccountInfo>(session.Scene);
            accountInfo.UserName = request.UserName;
            accountInfo.Password = request.Password;
            accountInfo.SDKUID = request.SDKUID;
            accountInfo.UID = uid;

            db.Save(accountInfo);

            Log.Debug($"收到注册的消息 request:{request.ToJson()}");
            response.UID = uid;
            await FTask.CompletedTask;
        }

        public async FTask<uint> GeneratorUID(IDateBase db)
        {
            var ret = await db.Last<AccountInfo>(t=>t.UID != 0);
            if (ret == null)
            {
                return 100000;
            }
            return ret.UID + 1;
        }
    }
}