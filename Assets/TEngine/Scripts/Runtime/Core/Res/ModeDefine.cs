namespace TEngine.Runtime
{
    /// <summary>
    /// 资源模式。
    /// </summary>
    public enum ResourceMode : byte
    {
        /// <summary>
        /// 未定义
        /// </summary>
        Unspecified = 0,

        /// <summary>
        /// 单机模式
        /// </summary>
        Package,

        /// <summary>
        /// 预下载的可更新模式
        /// </summary>
        Updatable,

        /// <summary>
        /// 使用时下载的可更新模式
        /// </summary>
        UpdatableWhilePlaying,
    }


    /// <summary>
    /// 读写区路径类型。
    /// </summary>
    public enum ReadWritePathType : byte
    {
        /// <summary>
        /// 未指定。
        /// </summary>
        Unspecified = 0,

        /// <summary>
        /// 临时缓存。
        /// </summary>
        TemporaryCache,

        /// <summary>
        /// 持久化数据。
        /// </summary>
        PersistentData,
    }
}
