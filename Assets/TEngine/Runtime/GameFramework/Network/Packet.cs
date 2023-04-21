namespace TEngine
{
    /// <summary>
    /// 网络消息包基类。
    /// </summary>
    public abstract class Packet : IMemory
    {
        /// <summary>
        /// 获取类型编号。
        /// </summary>
        public abstract int Id { get; }

        /// <summary>
        /// 清理引用。
        /// </summary>
        public abstract void Clear();
    }
}