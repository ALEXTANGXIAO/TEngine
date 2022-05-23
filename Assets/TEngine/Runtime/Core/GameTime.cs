using UnityEngine;

namespace TEngine
{
    /// <summary>
    /// 统一获取游戏内的时间处理，减少多处调用Unity的时间函数
    /// </summary>
    public static class GameTime
    {
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
        }

        public static float time;
        public static float deltaTime;
        public static int frameCount;
        public static float unscaledTime;

        public static float realtimeSinceStartup
        {
            get
            {
                return Time.realtimeSinceStartup;
            }
        }

        public static float quickRealTime;
    }
}