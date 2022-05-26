using System;
using System.Collections.Generic;
using TEngineProto;
using UnityEngine;

namespace TEngineCore.Net
{
    /// <summary>
    /// 客户端状态
    /// </summary>
    public enum GameClientStatus
    {
        StatusInit,         //初始化
        StatusReconnect,    //重新连接
        StatusClose,        //断开连接
        StatusConnect,      //连接中
        StatusEnter,        //Login登录成功
    }

    public delegate void CsMsgDelegate(MainPack mainPack);

    public class GameClient:TSingleton<GameClient>
    {

        #region Propriety
        private string m_lastHost = null;
        private int m_lastPort = 0;
        private GameClientStatus m_status = GameClientStatus.StatusInit;
        /// <summary>
        /// GameClient状态
        /// </summary>
        public GameClientStatus Status
        {
            get
            {
                return m_status;
            }
            set
            {
                m_status = value;
            }
        }

        /// <summary>
        /// 最新连接错误的时间
        /// </summary>
        private float m_lastLogDisconnectErrTime = 0f;
        /// <summary>
        /// 最新的错误码
        /// </summary>
        private int m_lastNetErrCode = 0;
        /// <summary>
        /// 最近一次心跳的时间
        /// </summary>
        private float m_lastHbTime = 0f;
        /// <summary>
        /// 心跳间隔
        /// </summary>
        private const float m_heartBeatDurTime = 15;
        /// <summary>
        /// 连续心跳超时
        /// </summary>
        private int m_heatBeatTimeoutNum = 0;
        private int m_ping = -1;
        public bool IsEntered
        {
            get { return m_status == GameClientStatus.StatusEnter; }
        }
        public bool IsNetworkOkAndLogined
        {
            get
            {
                return m_status == GameClientStatus.StatusEnter;
            }
        }

        public int LastNetErrCode
        {
            get { return m_lastNetErrCode; }
        }

        private ClientConnectWatcher m_connectWatcher;

        private void ResetParam()
        {
            m_lastLogDisconnectErrTime = 0f;
            m_heatBeatTimeoutNum = 0;
            m_lastHbTime = 0f;
            m_ping = -1;
            m_lastNetErrCode = 0;
        }
        #endregion

        private TcpConnection m_connect;

        public GameClient()
        {
            m_connect = new TcpConnection(this);
            m_connectWatcher = new ClientConnectWatcher(this);
        }

        ~GameClient()
        {
            if (m_connect != null)
            {
                m_connect.Close();
            }
            m_connect = null;
        }

        public bool Connect(string host, int port, bool reconnect = false)
        {
            ResetParam();
            if (!reconnect)
            {
                SetWatchReconnect(false);
            }
            //GameEventMgr.Instance.Send(ShowWaitingUI);
            m_lastHost = host;
            m_lastPort = port;
            Status = reconnect ? GameClientStatus.StatusReconnect : GameClientStatus.StatusInit;
            TLogger.LogInfo("Start connect server {0}:{1} Reconnect:{2}", host, port, reconnect);
            return m_connect.Connect(host, port);
        }

        public void Shutdown()
        {
            m_connect.Close();
            m_status = GameClientStatus.StatusInit;
            TLogger.LogWarning("GameClient Shut Down");
        }

        #region 发送网络消息
        /// <summary>
        /// 发送消息包
        /// </summary>
        /// <param name="reqPkg"></param>
        /// <returns></returns>
        public bool SendCsMsg(MainPack reqPkg)
        {
            if (!CheckPack(reqPkg))
            {
                return false;
            }
            return DoSendData(reqPkg);
        }

        /// <summary>
        /// 发送消息包并注册回调
        /// </summary>
        /// <param name="pack"></param>
        /// <param name="resHandler"></param>
        /// <param name="needShowWaitUI"></param>
        /// <returns></returns>
        public bool SendCsMsg(MainPack pack, CsMsgDelegate resHandler = null, bool needShowWaitUI = true)
        {
            if (!CheckPack(pack))
            {
                return false;
            }

            var ret = DoSendData(pack);

            if (!ret)
            {
                TLogger.LogError("SendCSMsg Error");
            }
            else
            {
                if (resHandler != null)
                {
                    RegTimeOutHandle((uint)pack.Actioncode, resHandler);
                    RegActionHandle((int)pack.Actioncode, resHandler);
                }
            }

            return ret;
        }

        private bool DoSendData(MainPack reqPkg)
        {
            var sendRet = m_connect.SendCsMsg(reqPkg);

            return sendRet;
        }
        #endregion

        #region 网络消息回调，非主线程
        Dictionary<int, List<CsMsgDelegate>> m_mapCmdHandle = new Dictionary<int, List<CsMsgDelegate>>();
        /// <summary>
        /// 委托缓存堆栈
        /// </summary>
        private Queue<List<CsMsgDelegate>> cachelistHandle = new Queue<List<CsMsgDelegate>>();

        /// <summary>
        /// 消息包缓存堆栈
        /// </summary>
        private Queue<MainPack> queuepPacks = new Queue<MainPack>();

        /// <summary>
        /// 网络消息回调，非主线程
        /// </summary>
        /// <param name="pack"></param>
        public void HandleResponse(MainPack pack)
        {
            lock (cachelistHandle)
            {
                List<CsMsgDelegate> listHandle;

                if (m_mapCmdHandle.TryGetValue((int)pack.Actioncode, out listHandle))
                {
                    cachelistHandle.Enqueue(listHandle);

                    queuepPacks.Enqueue(pack);
                }
            }
        }
        /// <summary>
        /// Udp网络消息回调，Loom多线程回调到此处，主线程
        /// </summary>
        /// <param name="pack"></param>
        public void UdpHandleResponse(MainPack pack)
        {
            List<CsMsgDelegate> listHandle;

            if (m_mapCmdHandle.TryGetValue((int)pack.Actioncode, out listHandle))
            {
                foreach (CsMsgDelegate handle in listHandle)
                {
                    handle(pack);
                }
            }
        }


        #endregion

        #region 注册网络消息回调
        /// <summary>
        /// 注册静态消息
        /// </summary>
        /// <param name="iCmdID"></param>
        /// <param name="msgDelegate"></param>
        public void RegActionHandle(int actionId, CsMsgDelegate msgDelegate)
        {
            List<CsMsgDelegate> listHandle;
            if (!m_mapCmdHandle.TryGetValue(actionId, out listHandle))
            {
                listHandle = new List<CsMsgDelegate>();
                m_mapCmdHandle[actionId] = listHandle;
            }

            if (listHandle != null)
            {
                if (!listHandle.Contains(msgDelegate))
                {
                    listHandle.Add(msgDelegate);
                }
                else
                {
                    //Debug.LogFormat("-------------repeat RegCmdHandle ActionCode:{0}-----------", (ActionCode)actionId);
                }
            }
        }
        /// <summary>
        /// 注册Udp静态消息
        /// </summary>
        /// <param name="iCmdID"></param>
        /// <param name="msgDelegate"></param>
        public void UdpRegActionHandle(int actionId, CsMsgDelegate msgDelegate)
        {
            List<CsMsgDelegate> listHandle;
            if (!m_mapCmdHandle.TryGetValue(actionId, out listHandle))
            {
                listHandle = new List<CsMsgDelegate>();
                m_mapCmdHandle[actionId] = listHandle;
            }

            if (listHandle != null)
            {
                if (listHandle.Contains(msgDelegate))
                {
                    Debug.LogFormat("-------------repeat RegCmdHandle ActionCode:{0}-----------", (ActionCode)actionId);
                }
                listHandle.Add(msgDelegate);
            }
        }

        /// <summary>
        /// 移除消息处理函数
        /// </summary>
        /// <param name="cmdId"></param>
        /// <param name="msgDelegate"></param>
        public void RmvCmdHandle(int actionId, CsMsgDelegate msgDelegate)
        {
            List<CsMsgDelegate> listHandle;
            if (!m_mapCmdHandle.TryGetValue(actionId, out listHandle))
            {
                return;
            }

            if (listHandle != null)
            {
                listHandle.Remove(msgDelegate);
            }
        }

        private bool CheckPack(MainPack pack)
        {
            if (pack == null)
            {
                return false;
            }

            if (pack.Actioncode == ActionCode.ActionNone)
            {
                return false;
            }

            if (pack.Requestcode == RequestCode.RequestNone)
            {
                return false;
            }

            return true;
        }
        #endregion

        #region 心跳处理
        protected bool CheckHeatBeatTimeout()
        {
            if (m_heatBeatTimeoutNum >= 2)
            {
                Shutdown();
                m_heatBeatTimeoutNum = 0;
                Status = GameClientStatus.StatusClose;
                TLogger.LogError("heat beat detect timeout");
                return false;
            }

            return true;
        }

        void TickHeartBeat()
        {
            if (Status != GameClientStatus.StatusEnter)
            {
                return;
            }

            var nowTime = GameTime.RealtimeSinceStartup;
            if (m_lastHbTime + m_heartBeatDurTime < nowTime)
            {
                m_lastHbTime = nowTime;
                MainPack pack = new MainPack
                {
                    Actioncode = ActionCode.HeartBeat
                };
                GameClient.Instance.SendCsMsg(pack, HandleHeatBeatRes);
            }
        }

        void HandleHeatBeatRes(MainPack mainPack)
        {
            if (mainPack.Returncode != ReturnCode.Success)
            {
                //如果是超时了，则标记最近收到包的次数
                if (mainPack.Returncode == ReturnCode.MsgTimeOut)
                {
                    m_heatBeatTimeoutNum++;
                    TLogger.LogError("heat beat timeout: {0}", m_heatBeatTimeoutNum);
                }
            }
            else
            {
                float diffTime = GameTime.RealtimeSinceStartup - mainPack.HeatEchoTime;
                m_ping = (int)(diffTime * 1000);
                m_heatBeatTimeoutNum = 0;
            }
        }
        #endregion

        /// <summary>
        /// 清理所有的网络消息
        /// </summary>
        public void CleanAllNetMsg()
        {
            m_mapCmdHandle.Clear();
        }

        public void Reconnect()
        {
            m_connectWatcher.OnReConnect();
            Connect(m_lastHost, m_lastPort, true);
        }

        public void OnUpdate()
        {
            HandleCsMsgOnUpdate();
            CheckCsMsgTimeOut();
            TickHeartBeat();
            CheckHeatBeatTimeout();
            m_connectWatcher.Update();
        }

        #region 超时检测

        #region TimeOutCheck
        private const int CHECK_TIMEOUT_PERFRAME = 10;
        const int MAX_MSG_HANDLE = 256;
        UInt32 m_dwLastCheckIndex = 0;
        CsMsgDelegate[] m_aMsgHandles = new CsMsgDelegate[MAX_MSG_HANDLE];
        float[] m_fMsgRegTime = new float[MAX_MSG_HANDLE];
        private float m_timeout = 15;
        #endregion
        private readonly MainPack _timeOutPack = new MainPack { Returncode = ReturnCode.MsgTimeOut };
        private void CheckCsMsgTimeOut()
        {
            float nowTime = GameTime.time;
            for (int i = 0; i < CHECK_TIMEOUT_PERFRAME; i++)
            {
                m_dwLastCheckIndex = (m_dwLastCheckIndex + 1) % MAX_MSG_HANDLE;
                if (m_aMsgHandles[m_dwLastCheckIndex] != null)
                {
                    if (m_fMsgRegTime[m_dwLastCheckIndex] + m_timeout < nowTime)
                    {
                        TLogger.LogError("msg timeout, resCmdID[{0}]", m_aMsgHandles[m_dwLastCheckIndex]);

                        NotifyTimeout(m_aMsgHandles[m_dwLastCheckIndex]);

                        RmvCheckCsMsg((int)m_dwLastCheckIndex);
                    }
                }
            }
        }

        public void RmvCheckCsMsg(int index)
        {
            m_aMsgHandles[index] = null;
            m_fMsgRegTime[index] = 0;
        }

        private void RegTimeOutHandle(uint actionCode, CsMsgDelegate resHandler)
        {
            uint hashIndex = actionCode % MAX_MSG_HANDLE;
            if (m_aMsgHandles[hashIndex] != null)
            {
                //NotifyTimeout(m_aMsgHandles[hashIndex]);
                RmvCheckCsMsg((int)hashIndex);
            }
            m_aMsgHandles[hashIndex] = resHandler;
            m_fMsgRegTime[hashIndex] = GameTime.time;
        }

        protected void NotifyTimeout(CsMsgDelegate msgHandler)
        {
            msgHandler(_timeOutPack);
        }

        #endregion
        /// <summary>
        /// 主线程从消息包缓存堆栈/委托缓存堆栈中出列
        /// </summary>
        private void HandleCsMsgOnUpdate()
        {
            if (cachelistHandle.Count <= 0 || queuepPacks.Count <= 0)
            {
                return;
            }
            try
            {
                foreach (CsMsgDelegate handle in cachelistHandle.Dequeue())
                {
                    var pack = queuepPacks.Peek();

                    if (pack != null)
                    {
                        handle(pack);

                        UInt32 hashIndex = (uint)pack.Actioncode % MAX_MSG_HANDLE;

                        RmvCheckCsMsg((int)hashIndex);
                    }
                }
                queuepPacks.Dequeue();
            }
            catch (Exception e)
            {
                TLogger.LogError(e.Message);
            }
        }

        protected override void Init()
        {
            base.Init();
        }

        public override void Active()
        {
            base.Active();
        }

        public override void Release()
        {
            if (m_connect != null)
            {
                m_connect.Close();
            }
            m_connect = null;
            base.Release();
        }

        public bool IsStatusCanSendMsg()
        {
            if (m_status == GameClientStatus.StatusEnter)
            {
                return true;
            }

            float nowTime = GameTime.time;
            if (m_lastLogDisconnectErrTime + 5 < nowTime)
            {
                TLogger.LogError("GameClient not connected, send msg failed");
                m_lastLogDisconnectErrTime = nowTime;
            }

            return false;
        }

        /// <summary>
        /// 设置是否需要监控网络重连
        /// </summary>
        /// <param name="needWatch"></param>
        public void SetWatchReconnect(bool needWatch)
        {
            m_connectWatcher.Enable = needWatch;
        }

        #region Ping
        /// <summary>
        /// ping值
        /// </summary>
        public int Ping
        {
            get
            {
                if (IsPingValid())
                {
                    return m_ping / 4;
                }
                else
                {
                    return 0;
                }
            }
        }

        public bool IsPingValid()
        {
            if (IsNetworkOkAndLogined)
            {
                return m_ping >= 0;
            }
            return false;
        }
        #endregion

        #region GetNetworkType
        public static CsNetworkType GetNetworkType()
        {
            CsNetworkType csNetType = CsNetworkType.CSNETWORK_UNKNOWN;
            NetworkReachability reachability = Application.internetReachability;
            switch (reachability)
            {
                case NetworkReachability.NotReachable:
                    break;
                case NetworkReachability.ReachableViaLocalAreaNetwork:
                    csNetType = CsNetworkType.CSNETWORK_WIFI;
                    break;
                case NetworkReachability.ReachableViaCarrierDataNetwork:
                    csNetType = CsNetworkType.CSNETWORK_3G;
                    break;
            }
            return csNetType;
        }

        public enum CsNetworkType
        {
            CSNETWORK_UNKNOWN = 0,    /*未知类型*/
            CSNETWORK_WIFI = 1,    /*Wifi类型*/
            CSNETWORK_3G = 2,    /*3G类型*/
            CSNETWORK_2G = 3    /*2G类型*/
        };
        #endregion
    }
}
