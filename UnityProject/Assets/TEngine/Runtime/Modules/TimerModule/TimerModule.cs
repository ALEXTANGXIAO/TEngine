using UnityEngine;

namespace TEngine
{
    /// <summary>
    /// 计时器模块。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed partial class TimerModule : Module
    {
        private TimerManager _timerManager;

        /// <summary>
        /// 游戏框架组件初始化。
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            _timerManager = ModuleImpSystem.GetModule<TimerManager>();
            if (_timerManager == null)
            {
                Log.Fatal("TimerMgr is invalid.");
            }
        }

        /// <summary>
        /// 添加计时器。
        /// </summary>
        /// <param name="callback">计时器回调。</param>
        /// <param name="time">计时器间隔。</param>
        /// <param name="isLoop">是否循环。</param>
        /// <param name="isUnscaled">是否不收时间缩放影响。</param>
        /// <param name="args">传参。(避免闭包)</param>
        /// <returns>计时器Id。</returns>
        public int AddTimer(TimerHandler callback, float time, bool isLoop = false, bool isUnscaled = false, params object[] args)
        {
            if (_timerManager == null)
            {
                throw new GameFrameworkException("TimerMgr is invalid.");
            }

            return _timerManager.AddTimer(callback, time, isLoop, isUnscaled, args);
        }

        /// <summary>
        /// 暂停计时器。
        /// </summary>
        /// <param name="timerId">计时器Id。</param>
        public void Stop(int timerId)
        {
            if (_timerManager == null)
            {
                throw new GameFrameworkException("TimerMgr is invalid.");
            }

            _timerManager.Stop(timerId);
        }
        
        /// <summary>
        /// 恢复计时器。
        /// </summary>
        /// <param name="timerId">计时器Id。</param>
        public void Resume(int timerId)
        {
            if (_timerManager == null)
            {
                throw new GameFrameworkException("TimerMgr is invalid.");
            }

            _timerManager.Resume(timerId);
        }
        
        /// <summary>
        /// 计时器是否在运行中。
        /// </summary>
        /// <param name="timerId">计时器Id。</param>
        /// <returns>否在运行中。</returns>
        public bool IsRunning(int timerId)
        {
            if (_timerManager == null)
            {
                throw new GameFrameworkException("TimerMgr is invalid.");
            }

            return _timerManager.IsRunning(timerId);
        }

        /// <summary>
        /// 获得计时器剩余时间。
        /// </summary>
        public float GetLeftTime(int timerId)
        {
            if (_timerManager == null)
            {
                throw new GameFrameworkException("TimerMgr is invalid.");
            }

            return _timerManager.GetLeftTime(timerId);
        }
        
        /// <summary>
        /// 重置计时器,恢复到开始状态。
        /// </summary>
        public void Restart(int timerId)
        {
            if (_timerManager == null)
            {
                throw new GameFrameworkException("TimerMgr is invalid.");
            }

            _timerManager.Restart(timerId);
        }

        /// <summary>
        /// 重置计时器。
        /// </summary>
        public void ResetTimer(int timerId, TimerHandler callback, float time, bool isLoop = false, bool isUnscaled = false)
        {
            if (_timerManager == null)
            {
                throw new GameFrameworkException("TimerMgr is invalid.");
            }
            
            _timerManager.Reset(timerId,callback,time,isLoop,isUnscaled);
        }

        /// <summary>
        /// 重置计时器。
        /// </summary>
        public void ResetTimer(int timerId, float time, bool isLoop, bool isUnscaled)
        {
            if (_timerManager == null)
            {
                throw new GameFrameworkException("TimerMgr is invalid.");
            }
            
            _timerManager.Reset(timerId, time,isLoop,isUnscaled);
        }

        /// <summary>
        /// 移除计时器。
        /// </summary>
        /// <param name="timerId">计时器Id。</param>
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
        /// 移除所有计时器。
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