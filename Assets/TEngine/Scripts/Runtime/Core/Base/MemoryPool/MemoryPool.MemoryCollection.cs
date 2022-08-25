using System;
using System.Collections.Generic;

namespace TEngine.Runtime
{
    public static partial class MemoryPool
    {
        /// <summary>
        /// 内存池收集器
        /// </summary>
        private sealed class MemoryCollection
        {
            private readonly Queue<IMemory> m_Memories;
            private readonly Type m_MemoryType;
            private int m_UsingMemoryCount;
            private int m_AcquireMemoryCount;
            private int m_ReleaseMemoryCount;
            private int m_AddMemoryCount;
            private int m_RemoveMemoryCount;

            public MemoryCollection(Type memoryType)
            {
                m_Memories = new Queue<IMemory>();
                m_MemoryType = memoryType;
                m_UsingMemoryCount = 0;
                m_AcquireMemoryCount = 0;
                m_ReleaseMemoryCount = 0;
                m_AddMemoryCount = 0;
                m_RemoveMemoryCount = 0;
            }

            public Type MemoryType
            {
                get
                {
                    return m_MemoryType;
                }
            }

            public int UnusedMemoryCount
            {
                get
                {
                    return m_Memories.Count;
                }
            }

            public int UsingMemoryCount
            {
                get
                {
                    return m_UsingMemoryCount;
                }
            }

            public int AcquireMemoryCount
            {
                get
                {
                    return m_AcquireMemoryCount;
                }
            }

            public int ReleaseMemoryCount
            {
                get
                {
                    return m_ReleaseMemoryCount;
                }
            }

            public int AddMemoryCount
            {
                get
                {
                    return m_AddMemoryCount;
                }
            }

            public int RemoveMemoryCount
            {
                get
                {
                    return m_RemoveMemoryCount;
                }
            }

            public T Acquire<T>() where T : class, IMemory, new()
            {
                if (typeof(T) != m_MemoryType)
                {
                    throw new Exception("Type is invalid.");
                }

                m_UsingMemoryCount++;
                m_AcquireMemoryCount++;
                lock (m_Memories)
                {
                    if (m_Memories.Count > 0)
                    {
                        return (T)m_Memories.Dequeue();
                    }
                }

                m_AddMemoryCount++;
                return new T();
            }

            public IMemory Acquire()
            {
                m_UsingMemoryCount++;
                m_AcquireMemoryCount++;
                lock (m_Memories)
                {
                    if (m_Memories.Count > 0)
                    {
                        return m_Memories.Dequeue();
                    }
                }

                m_AddMemoryCount++;
                return (IMemory)Activator.CreateInstance(m_MemoryType);
            }

            public void Release(IMemory memory)
            {
                memory.Clear();
                lock (m_Memories)
                {
                    if (m_EnableStrictCheck && m_Memories.Contains(memory))
                    {
                        throw new Exception("The memory has been released.");
                    }

                    m_Memories.Enqueue(memory);
                }

                m_ReleaseMemoryCount++;
                m_UsingMemoryCount--;
            }

            public void Add<T>(int count) where T : class, IMemory, new()
            {
                if (typeof(T) != m_MemoryType)
                {
                    throw new Exception("Type is invalid.");
                }

                lock (m_Memories)
                {
                    m_AddMemoryCount += count;
                    while (count-- > 0)
                    {
                        m_Memories.Enqueue(new T());
                    }
                }
            }

            public void Add(int count)
            {
                lock (m_Memories)
                {
                    m_AddMemoryCount += count;
                    while (count-- > 0)
                    {
                        m_Memories.Enqueue((IMemory)Activator.CreateInstance(m_MemoryType));
                    }
                }
            }

            public void Remove(int count)
            {
                lock (m_Memories)
                {
                    if (count > m_Memories.Count)
                    {
                        count = m_Memories.Count;
                    }

                    m_RemoveMemoryCount += count;
                    while (count-- > 0)
                    {
                        m_Memories.Dequeue();
                    }
                }
            }

            public void RemoveAll()
            {
                lock (m_Memories)
                {
                    m_RemoveMemoryCount += m_Memories.Count;
                    m_Memories.Clear();
                }
            }
        }
    }
}
