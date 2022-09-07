namespace TEngine.Runtime
{
    /// <summary>
    /// 用来在多线程下检测耗时
    /// </summary>
    public class GameTickWatcher
    {
        private long m_startTick = 0;

        public GameTickWatcher()
        {
            Refresh();
        }

        public void Refresh()
        {
            m_startTick = System.DateTime.Now.Ticks;
        }

        /// <summary>
        /// 用时检测
        /// </summary>
        /// <param name="logTime">是否输出日志</param>
        /// <returns></returns>
        public float ElapseTime(bool logTime = false)
        {
            long endTick = System.DateTime.Now.Ticks;
            float ret = (float)((endTick - m_startTick) / 10000) / 1000.0f;
            if (logTime)
            {
                Log.Debug($"GameTickWatcher ElapseTime :{ret} s");
            }
            return ret;
        }

        public void LogUsedTime()
        {
            TLogger.LogException($"Used Time: {this.ElapseTime()}");
        }
    }
}