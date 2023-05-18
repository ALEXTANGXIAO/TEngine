using TEngine;

namespace GameLogic
{
    public enum ClientConnectWatcherStatus
    {
        StatusInit,
        StatusReconnectAuto,
        StatusReconnectConfirm,
        StatusWaitExit
    }

    public class ClientConnectWatcher
    {
        private readonly GameClient _client;
        private ClientConnectWatcherStatus _status;
        private float _statusTime;
        private int _reconnectCnt = 0;
        private int _disconnectReason = 0;

        private bool _enable = false;

        public bool Enable
        {
            get => _enable;
            set
            {
                if (_enable != value)
                {
                    _enable = value;
                    if (_enable)
                    {
                        OnEnable();
                    }
                    else
                    {
                        OnDisable();
                    }
                }
            }
        }

        private ClientConnectWatcherStatus Status
        {
            get => _status;
            set
            {
                if (_status == value) return;
                _status = value;
                _statusTime = NowTime;
            }
        }

        private float NowTime => GameTime.unscaledTime;

        public ClientConnectWatcher(GameClient client)
        {
            _client = client;
            _statusTime = NowTime;
            _status = ClientConnectWatcherStatus.StatusInit;
        }

        public void Update()
        {
            if (!_enable)
            {
                return;
            }

            if (_client.IsEntered)
            {
                return;
            }

            switch (_status)
            {
                case ClientConnectWatcherStatus.StatusInit:
                    UpdateOnInitStatus();
                    break;
                case ClientConnectWatcherStatus.StatusReconnectAuto:
                    UpdateOnReconnectAuto();
                    break;
                case ClientConnectWatcherStatus.StatusReconnectConfirm:
                    UpdateOnReconnectConfirm();
                    break;
                case ClientConnectWatcherStatus.StatusWaitExit:
                    UpdateOnWaitExit();
                    break;
            }
        }

        public void OnReConnect()
        {
            if (_status == ClientConnectWatcherStatus.StatusReconnectConfirm)
            {
                Status = ClientConnectWatcherStatus.StatusReconnectAuto;
            }
        }

        void UpdateOnInitStatus()
        {
            int autoReconnectMaxCount = 4;
            if (_reconnectCnt <= autoReconnectMaxCount)
            {
                if (_reconnectCnt == 0)
                {
                    _disconnectReason = _client.LastNetErrCode;
                }

                Status = ClientConnectWatcherStatus.StatusReconnectAuto;
                _reconnectCnt++;

                //重连
                _client.Reconnect();
            }
            else
            {
                Status = ClientConnectWatcherStatus.StatusReconnectConfirm;
                _reconnectCnt++;
                // UISys.Mgr.ShowUI(GAME_UI_TYPE.Tip_NetDisconn, UISys.Mgr.GetUIWindowParam().SetParam("errCode", m_disconnectReason));
            }
        }

        void UpdateOnReconnectAuto()
        {
            if (_client.IsEntered)
            {
                Status = ClientConnectWatcherStatus.StatusInit;
                _reconnectCnt = 0;
                return;
            }

            float nowTime = NowTime;
            var timeoutTime = 10f;

            if (_statusTime + timeoutTime < nowTime)
            {
                Log.Error("UpdateOnReconnectAuto timeout: {0}", timeoutTime);

                //切换到默认的，下一帧继续判断是否需要自动还是手动
                Status = ClientConnectWatcherStatus.StatusInit;
            }
        }

        void UpdateOnReconnectConfirm()
        {
            
        }

        void UpdateOnWaitExit()
        {
        }

        private void OnDisable()
        {
            Status = ClientConnectWatcherStatus.StatusInit;
            _reconnectCnt = 0;
        }

        private void OnEnable()
        {
            Status = ClientConnectWatcherStatus.StatusInit;
            _reconnectCnt = 0;
        }
    }
}