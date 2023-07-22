using UnityEngine;

namespace TEngine
{
    /// <summary>
    /// 计时器模块。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed partial class TimerModule : GameFrameworkModuleBase
    {
        private TimerManager _timerManager;

        /// <summary>
        /// 游戏框架组件初始化。
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            _timerManager = GameFrameworkModuleSystem.GetModule<TimerManager>();
            if (_timerManager == null)
            {
                Log.Fatal("TimerMgr is invalid.");
            }
        }

        /// <summary>
        /// 添加计时器
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="time"></param>
        /// <param name="isLoop"></param>
        /// <param name="isUnscaled"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public int AddTimer(TimerHandler callback, float time, bool isLoop = false, bool isUnscaled = false, params object[] args)
        {
            if (_timerManager == null)
            {
                Log.Fatal("TimerMgr is invalid.");
                throw new GameFrameworkException("TimerMgr is invalid.");
            }

            return _timerManager.AddTimer(callback, time, isLoop, isUnscaled, args);
        }

        /// <summary>
        /// 暂停计时
        /// </summary>
        public void Stop(int timerId)
        {
            if (_timerManager == null)
            {
                Log.Fatal("TimerMgr is invalid.");
                throw new GameFrameworkException("TimerMgr is invalid.");
            }
            _timerManager.Stop(timerId);
        }

        /// <summary>
        /// 移除计时器
        /// </summary>
        /// <param name="timerId"></param>
        public void RemoveTimer(int timerId)
        {
            if (_timerManager == null)
            {
                Log.Fatal("TimerMgr is invalid.");
                throw new GameFrameworkException("TimerMgr is invalid.");
            }
            _timerManager.RemoveTimer(timerId);
        }

        /// <summary>
        /// 恢复计时
        /// </summary>
        public void Resume(int timerId)
        {
            if (_timerManager == null)
            {
                Log.Fatal("TimerMgr is invalid.");
                throw new GameFrameworkException("TimerMgr is invalid.");
            }
            _timerManager.Resume(timerId);
        }

        /// <summary>
        /// 计时器是否在运行中
        /// </summary>
        public bool IsRunning(int timerId)
        {
            if (_timerManager == null)
            {
                Log.Fatal("TimerMgr is invalid.");
                throw new GameFrameworkException("TimerMgr is invalid.");
            }

            return _timerManager.IsRunning(timerId);
        }

        /// <summary>
        /// 移除所有计时器
        /// </summary>
        public void RemoveAllTimer()
        {
            if (_timerManager == null)
            {
                Log.Fatal("TimerMgr is invalid.");
                throw new GameFrameworkException("TimerMgr is invalid.");
            }

            _timerManager.RemoveAllTimer();
        }
    }
}