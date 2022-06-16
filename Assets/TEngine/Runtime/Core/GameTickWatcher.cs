namespace TEngine
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
        /// 用时
        /// </summary>
        /// <returns></returns>
        public float ElapseTime()
        {
            long endTick = System.DateTime.Now.Ticks;
            return (float)((endTick - m_startTick) / 10000) / 1000.0f;
        }
    }
}
