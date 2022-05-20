using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TEngine
{
    public interface IIndex
    {
        int Index { get; set; }
    }

    internal class HashSetDebugView<T> where T : IIndex
    {
        private readonly ArrayPool<T> m_Set;

        public HashSetDebugView(ArrayPool<T> set)
        {
            m_Set = set ?? throw new ArgumentNullException(nameof(set));
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public T[] Items
        {
            get
            {
                return m_Set.ToArray();
            }
        }
    }

    [DebuggerTypeProxy(typeof(HashSetDebugView<>))]
    [DebuggerDisplay("Count = {Count}")]
    public class ArrayPool<T> where T:IIndex
    {
        internal T[] m_Items = new T[256];
        internal bool[] Buckets = new bool[256];
        private int m_Index;
        private int count;

        public T this[int index]
        {
            get
            {
                return m_Items[index];
            }
            set
            {
                m_Items[index] = value;
            }
        }

        public int Count
        {
            get
            {
                return count;
            }
        }

        public T[] ToArray()
        {
            List<T> elements = new List<T>();
            for (int i = 0; i < m_Items.Length; i++)
            {
                if (Buckets[i])
                {
                    elements.Add(m_Items[i]);
                }
            }
            return elements.ToArray();
        }

        public void Remove(T item)
        {
            lock (this)
            {
                m_Items[item.Index] = default;
                Buckets[item.Index] = false;
            }
        }

        public void Add(T item)
        {
            lock (this)
            {
                if (item.Index != -1)
                {
                    if (!Buckets[item.Index])
                    {
                        m_Items[item.Index] = item;
                        Buckets[item.Index] = true;
                        return;
                    }
                }

                m_Items[m_Index] = item;
                Buckets[m_Index] = true;
                item.Index = m_Index;
                m_Index++;
                if (m_Index >= m_Items.Length)
                {
                    T[] newItems = new T[m_Items.Length * 2];
                    bool[] newBuckets = new bool[m_Items.Length * 2];
                    Array.Copy(m_Items,0,newItems,0,m_Items.Length);
                    Array.Copy(Buckets, 0, newBuckets, 0, Buckets.Length);
                    m_Items = newItems;
                    Buckets = newBuckets;
                }
                count = m_Index;
            }
        }
    }
}