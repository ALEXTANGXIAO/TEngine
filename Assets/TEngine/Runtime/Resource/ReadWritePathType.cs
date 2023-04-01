namespace TEngine
{
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
