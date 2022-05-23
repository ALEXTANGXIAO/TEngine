using UnityEngine;

namespace TEngineCore
{
    public static class GameTime
    {
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