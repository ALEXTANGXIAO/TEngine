using System;
using TEngine;
using TEngine.Core;
using TEngine.Core.Network;

namespace GameLogic
{
    /// <summary>
    /// 玩家信息网络模块。
    /// </summary>
    public class PlayerNetSys:DataCenterModule<PlayerNetSys>
    {
        public override void Init()
        {
            base.Init();
        }
        
        
    }
    public class H_C2G_LoginRequestHandler : MessageRPC<H_C2G_LoginRequest,H_G2C_LoginResponse>
    {
        protected override async FTask Run(Session session, H_C2G_LoginRequest request, H_G2C_LoginResponse response, Action reply)
        {
            Log.Debug($"收到请求登录的消息 request:{request.ToJson()}");
            response.Text = "登录成功";
            await FTask.CompletedTask;
        }
    }
}