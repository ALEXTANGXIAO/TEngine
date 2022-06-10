using System;
using System.IO;

namespace TEngine
{
    public class LoadUpdateLogic
    {
        private static LoadUpdateLogic _instance;

        public Action<int> Download_Complete_Action = null;
        public Action<long> Down_Progress_Action = null;
        public Action<bool, GameStatus> _Unpacked_Complete_Action = null;
        public Action<float, GameStatus> _Unpacked_Progress_Action = null;

        public static LoadUpdateLogic Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new LoadUpdateLogic();
                return _instance;
            }
        }
    }

    public class LoadMgr : TSingleton<LoadMgr>
    {
        /// <summary>
        /// 资源版本号
        /// </summary>
        public string LatestResId { get; set; }

        private Action _startGameEvent;
        private int _curTryCount;
        private const int MaxTryCount = 3;
        private bool _connectBack;
        private bool _needUpdate = false;

        public LoadMgr()
        {
            _curTryCount = 0;
            _connectBack = false;
            _startGameEvent = null;
        }

        public void StartLoadInit(Action onUpdateComplete)
        {
#if RELEASE_BUILD || _DEVELOPMENT_BUILD_
            StartLoad(() => { FinishCallBack(onUpdateComplete); });
#else
            onUpdateComplete();
#endif
        }

        /// <summary>
        /// 开启热更新逻辑
        /// </summary>
        /// <param name="action"></param>
        public void StartLoad(Action action)
        {
            _startGameEvent = action;
            _connectBack = false;
            _curTryCount = 0;
            RequestVersion();
        }

        private void FinishCallBack(Action callBack)
        {
            GameConfig.Instance.WriteVersion(LatestResId);
            if (_needUpdate)
            {
                callBack();
            }
            else
            {
                callBack();
            }
        }

        /// <summary>
        /// 请求热更数据
        /// </summary>
        private void RequestVersion()
        {
            if (_connectBack)
            {
                return;
            }

            _curTryCount++;

            if (_curTryCount > MaxTryCount)
            {

            }
        }
    }
}
