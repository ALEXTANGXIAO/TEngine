using System.IO;
using GameBase;
using TEngine;
using TEngine.Core.Network;

namespace GameLogic
{
    /// <summary>
    /// 网络客户端状态。
    /// </summary>
    public enum GameClientStatus
    {
        /// <summary>
        /// 初始化。
        /// </summary>
        StatusInit, 
        /// <summary>
        /// 连接成功服务器。
        /// </summary>
        StatusConnected,
        /// <summary>
        /// 重新连接。
        /// </summary>
        StatusReconnect,
        /// <summary>
        /// 断开连接。
        /// </summary>
        StatusClose,
        /// <summary>
        /// 登录中。
        /// </summary>
        StatusLogin,
        /// <summary>
        /// AccountLogin成功，进入服务器了。
        /// </summary>
        StatusEnter,
    }
    
    /// <summary>
    /// 网络客户端。
    /// </summary>
    public class GameClient:Singleton<GameClient>
    {
        public readonly NetworkProtocolType ProtocolType = NetworkProtocolType.KCP;
        public GameClientStatus Status { get; set; } = GameClientStatus.StatusInit;
        public Scene Scene { private set; get; }
        
        private string _lastAddress = null;
        
        public GameClient()
        {
            Scene = GameApp.Instance.Scene;
        }
        
        public void Connect(string address, bool reconnect = false)
        {
            if (!reconnect)
            {
                // SetWatchReconnect(false);
            }

            if (reconnect)
            {
                // GameEvent.Get<ICommUI>().ShowWaitUITip(WaitUISeq.LOGINWORLD_SEQID, G.R(TextDefine.ID_TIPS_RECONNECTING));
            }
            else
            {
                // GameEvent.Get<ICommUI>().ShowWaitUI(WaitUISeq.LOGINWORLD_SEQID);
            }

            _lastAddress = address;

            Status = reconnect ? GameClientStatus.StatusReconnect : GameClientStatus.StatusInit;
            
            Scene.CreateSession(address, ProtocolType, OnConnectComplete, OnConnectFail);
        }

        private void OnConnectComplete()
        {
            Status = GameClientStatus.StatusConnected;
            Log.Info("Connect to server success");
        }

        private void OnConnectFail()
        {
            Status = GameClientStatus.StatusClose;
            Log.Warning("Could not connect to server");
        }
        
        public virtual void Send(object message, uint rpcId = 0, long routeId = 0)
        {
            if (Scene.Session == null)
            {
                Log.Error("Send Message Failed Because Session Is Null");
                return;
            }
            Scene.Session.Send(message,rpcId,routeId);
        }

        public virtual void Send(IRouteMessage routeMessage, uint rpcId = 0, long routeId = 0)
        {
            if (Scene.Session == null)
            {
                Log.Error("Send Message Failed Because Session Is Null");
                return;
            }
            Scene.Session.Send(routeMessage,rpcId,routeId);
        }

        public virtual void Send(MemoryStream memoryStream, uint rpcId = 0, long routeTypeOpCode = 0, long routeId = 0)
        {
            if (Scene.Session == null)
            {
                Log.Error("Send Message Failed Because Session Is Null");
                return;
            }
            Scene.Session.Send(memoryStream,rpcId,routeTypeOpCode,routeId);
        }
    }
}