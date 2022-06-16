using UnityEngine;

namespace TEngine
{
    /// <summary>
    /// 统一获取游戏内的时间处理，减少多处调用Unity的时间函数
    /// </summary>
    public static class GameTime
    {
        /// <summary>
        /// 开始记录
        /// </summary>
        public static void Start()
        {
            _lastFrameTimeStamp = TimeStamp;
        }

        /// <summary>
        /// 这一帧的记录
        /// </summary>
        public static void StartFrame()
        {
            time = Time.time;
            deltaTime = Time.deltaTime;
            quickRealTime = Time.realtimeSinceStartup;
            frameCount = Time.frameCount;
            unscaledTime = Time.unscaledTime;

            long now = TimeStamp;

            if (ServerTimeStamp > 0)
            {
                ServerTimeStamp += now - _lastFrameTimeStamp;
            }
            else
            {
                ServerTimeStamp = now;
            }
            _lastFrameTimeStamp = now;
        }

        public static float time;
        public static float deltaTime;
        public static int frameCount;
        public static float unscaledTime;
        public static long ServerTimeStamp;
        private static System.DateTime _epochDateTime = System.TimeZoneInfo.ConvertTime(new System.DateTime(1970, 1, 1), System.TimeZoneInfo.Local);
        public static float quickRealTime;
        public static long TimeStamp => System.DateTimeOffset.Now.ToUnixTimeMilliseconds();
        private static long _lastFrameTimeStamp;
        private static bool _isPause = false;
        private static float _keepTimeScale = 1f;

        /// <summary>
        /// 从游戏启动到现在的真实时长（秒）
        /// </summary>
        public static float RealtimeSinceStartup
        {
            get
            {
                return Time.realtimeSinceStartup;
            }
        }

        /// <summary>
        /// 服务器同步时间
        /// </summary>
        /// <param name="serverTime"></param>
        public static void ServerTimeSync(long serverTime)
        {
            ServerTimeStamp = serverTime;
        }


        /// <summary>
        /// 获取、设置TimeScale
        /// </summary>
        public static float TimeScale
        {
            get
            {
                return Time.timeScale;
            }
            set
            {
                if (!_isPause)
                {
                    _keepTimeScale = Time.timeScale;
                    Time.timeScale = value;
                }
            }
        }

        /// <summary>
        /// 时间停止，所有基于DeltaTime更新的逻辑都会停滞
        /// </summary>
        public static void Pause()
        {
            if (!_isPause)
            {
                _keepTimeScale = Time.timeScale;
                Time.timeScale = 0f;
                _isPause = true;
            }
        }

        /// <summary>
        /// 时间恢复
        /// </summary>
        public static void Resume()
        {
            if (_isPause)
            {
                Time.timeScale = _keepTimeScale;
                _isPause = false;
            }
        }
    }
}