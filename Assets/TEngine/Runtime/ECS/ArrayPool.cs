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
        private readonly ArrayPool<T> _set;

        public HashSetDebugView(ArrayPool<T> set)
        {
            _set = set ?? throw new ArgumentNullException(nameof(set));
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public T[] Items
        {
            get
            {
                return _set.ToArray();
            }
        }
    }

    [DebuggerTypeProxy(typeof(HashSetDebugView<>))]
    [DebuggerDisplay("Count = {Count}")]
    public class ArrayPool<T> where T:IIndex
    {
        internal T[] Items = new T[256];
        internal bool[] Buckets = new bool[256];
        private int _index;
        private int _count;

        public T this[int index]
        {
            get
            {
                return Items[index];
            }
            set
            {
                Items[index] = value;
            }
        }

        public int Count
        {
            get
            {
                return _count;
            }
        }

        public T[] ToArray()
        {
            List<T> elements = new List<T>();
            for (int i = 0; i < Items.Length; i++)
            {
                if (Buckets[i])
                {
                    elements.Add(Items[i]);
                }
            }
            return elements.ToArray();
        }

        public void Remove(T item)
        {
            lock (this)
            {
                Items[item.Index] = default;
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
                        Items[item.Index] = item;
                        Buckets[item.Index] = true;
                        return;
                    }
                }

                Items[_index] = item;
                Buckets[_index] = true;
                item.Index = _index;
                _index++;
                if (_index >= Items.Length)
                {
                    T[] newItems = new T[Items.Length * 2];
                    bool[] newBuckets = new bool[Items.Length * 2];
                    Array.Copy(Items,0,newItems,0,Items.Length);
                    Array.Copy(Buckets, 0, newBuckets, 0, Buckets.Length);
                    Items = newItems;
                    Buckets = newBuckets;
                }
                _count = _index;
            }
        }
    }
}