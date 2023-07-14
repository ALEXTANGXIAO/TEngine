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
        /// <summary>
        /// 网络模块初始化。
        /// </summary>
        public override void Init()
        {
            base.Init();
            //注册登录消息回调。
            GameClient.Instance.RegisterMsgHandler(OuterOpcode.H_G2C_LoginResponse,OnLoginRes);
        }

        /// <summary>
        /// 登录消息回调。
        /// </summary>
        /// <param name="response">网络回复消息包。</param>
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

        /// <summary>
        /// 登录消息请求。
        /// </summary>
        /// <param name="userName">用户名。</param>
        /// <param name="passWord">用户密码。</param>
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