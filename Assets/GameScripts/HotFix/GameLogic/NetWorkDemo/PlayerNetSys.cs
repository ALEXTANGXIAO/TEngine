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
            GameClient.Instance.RegisterMsgHandler(OuterOpcode.H_G2C_LoginResponse,OnLoginRes);
        }

        public void OnLoginRes(IResponse response)
        {
            if (NetworkUtils.CheckError(response))
            {
                GameClient.Instance.Status = GameClientStatus.StatusConnected;
                return;
            }
            H_G2C_LoginResponse ret = (H_G2C_LoginResponse)response;
            Log.Debug(ret.ToJson());
            GameClient.Instance.Status = GameClientStatus.StatusEnter;
        }

        public void DoLoginReq(string userName,string passWord)
        {
            if (GameClient.Instance.Status == GameClientStatus.StatusEnter)
            {
                Log.Info("当前已经登录成功。");
                return;
            }
            H_C2G_LoginRequest loginRequest =new H_C2G_LoginRequest()
            {
                UserName = userName,
                Password = passWord
            };
            GameClient.Instance.Send(loginRequest);
            GameClient.Instance.Status = GameClientStatus.StatusLogin;
        }
    }
}