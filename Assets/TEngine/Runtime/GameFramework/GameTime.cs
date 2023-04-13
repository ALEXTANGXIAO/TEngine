using UnityEngine;

namespace TEngine
{
    /// <summary>
    /// 游戏时间。
    /// <remarks>统一封装调用。</remarks>
    /// </summary>
    public static class GameTime
    {
        public static float time;

        public static float deltaTime;

        public static float unscaledDeltaTime;

        public static float fixedDeltaTime;
        
        public static float frameCount;
        
        public static float unscaledTime;

        public static void StartFrame()
        {
            time = Time.time;
            deltaTime = Time.deltaTime;
            unscaledDeltaTime = Time.unscaledDeltaTime;
            fixedDeltaTime = Time.fixedDeltaTime;
            frameCount = Time.frameCount;
            unscaledTime = Time.unscaledTime;
        }
    }
}