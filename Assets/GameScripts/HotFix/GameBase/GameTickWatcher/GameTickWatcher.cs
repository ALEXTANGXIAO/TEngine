using TEngine;

namespace GameBase
{
    /// <summary>
    /// 用来在多线程下检测耗时。
    /// </summary>
    public struct GameTickWatcher
    {
        private long _startTick;

        public GameTickWatcher()
        {
            _startTick = System.DateTime.Now.Ticks;
        }

        public void Refresh()
        {
            _startTick = System.DateTime.Now.Ticks;
        }

        /// <summary>
        /// 获取用时。
        /// </summary>
        /// <returns></returns>
        public float ElapseTime()
        {
            long endTick = System.DateTime.Now.Ticks;
            return (float)((endTick - _startTick) / 10000) / 1000.0f;
        }

        /// <summary>
        /// 输出用时。
        /// </summary>
        public void LogUsedTime()
        {
            Log.Info($"Used Time: {this.ElapseTime()}");
        }
    }
}