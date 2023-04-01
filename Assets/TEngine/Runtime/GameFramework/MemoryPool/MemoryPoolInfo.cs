using System;
using System.Runtime.InteropServices;

namespace TEngine
{
    /// <summary>
    /// 内存池信息。
    /// </summary>
    [StructLayout(LayoutKind.Auto)]
    public struct MemoryPoolInfo
    {
        private readonly Type m_Type;
        private readonly int m_UnusedMemoryCount;
        private readonly int m_UsingMemoryCount;
        private readonly int m_AcquireMemoryCount;
        private readonly int m_ReleaseMemoryCount;
        private readonly int m_AddMemoryCount;
        private readonly int m_RemoveMemoryCount;

        /// <summary>
        /// 初始化内存池信息的新实例。
        /// </summary>
        /// <param name="type">内存池类型。</param>
        /// <param name="unusedMemoryCount">未使用内存对象数量。</param>
        /// <param name="usingMemoryCount">正在使用内存对象数量。</param>
        /// <param name="acquireMemoryCount">获取内存对象数量。</param>
        /// <param name="releaseMemoryCount">归还内存对象数量。</param>
        /// <param name="addMemoryCount">增加内存对象数量。</param>
        /// <param name="removeMemoryCount">移除内存对象数量。</param>
        public MemoryPoolInfo(Type type, int unusedMemoryCount, int usingMemoryCount, int acquireMemoryCount, int releaseMemoryCount, int addMemoryCount, int removeMemoryCount)
        {
            m_Type = type;
            m_UnusedMemoryCount = unusedMemoryCount;
            m_UsingMemoryCount = usingMemoryCount;
            m_AcquireMemoryCount = acquireMemoryCount;
            m_ReleaseMemoryCount = releaseMemoryCount;
            m_AddMemoryCount = addMemoryCount;
            m_RemoveMemoryCount = removeMemoryCount;
        }

        /// <summary>
        /// 获取内存池类型。
        /// </summary>
        public Type Type
        {
            get
            {
                return m_Type;
            }
        }

        /// <summary>
        /// 获取未使用内存对象数量。
        /// </summary>
        public int UnusedMemoryCount
        {
            get
            {
                return m_UnusedMemoryCount;
            }
        }

        /// <summary>
        /// 获取正在使用内存对象数量。
        /// </summary>
        public int UsingMemoryCount
        {
            get
            {
                return m_UsingMemoryCount;
            }
        }

        /// <summary>
        /// 获取获取内存对象数量。
        /// </summary>
        public int AcquireMemoryCount
        {
            get
            {
                return m_AcquireMemoryCount;
            }
        }

        /// <summary>
        /// 获取归还内存对象数量。
        /// </summary>
        public int ReleaseMemoryCount
        {
            get
            {
                return m_ReleaseMemoryCount;
            }
        }

        /// <summary>
        /// 获取增加内存对象数量。
        /// </summary>
        public int AddMemoryCount
        {
            get
            {
                return m_AddMemoryCount;
            }
        }

        /// <summary>
        /// 获取移除内存对象数量。
        /// </summary>
        public int RemoveMemoryCount
        {
            get
            {
                return m_RemoveMemoryCount;
            }
        }
    }
}
