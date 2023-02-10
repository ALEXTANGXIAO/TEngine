namespace TEngine.Runtime
{
    /// <summary>
    /// 加载任务抽象基类。
    /// </summary>
    public abstract class LoadJob
    {
        protected bool _isDone;

        /// <summary>
        /// 任务是否完成，LoadJobManager会根据该标志判断是否加载下一个任务。
        /// </summary>
        public bool IsDone => _isDone;

        /// <summary>
        /// 开始任务执行，一次性初始化工作可放在其中。
        /// </summary>
        public abstract void Start();

        /// <summary>
        /// 任务处理。
        /// </summary>
        /// <returns>执行进入（0 - 1）。</returns>
        public abstract float Process();
    }
}
