using System;
using System.Collections.Generic;
using System.Linq;
#pragma warning disable CS8603

namespace TEngine.DataStructure
{
    public class SortedOneToManyListPool<TKey, TValue> : SortedOneToManyList<TKey, TValue>, IDisposable
        where TKey : notnull
    {
        private bool _isDispose;

        public static SortedOneToManyListPool<TKey, TValue> Create()
        {
            var a = Pool<SortedOneToManyListPool<TKey, TValue>>.Rent();
            a._isDispose = false;
            return a;
        }

        public void Dispose()
        {
            if (_isDispose)
            {
                return;
            }

            _isDispose = true;
            Clear();
            Pool<SortedOneToManyListPool<TKey, TValue>>.Return(this);
        }
    }

    public class SortedOneToManyList<TKey, TValue> : SortedDictionary<TKey, List<TValue>> where TKey : notnull
    {
        private readonly Queue<List<TValue>> _queue = new Queue<List<TValue>>();
        private readonly int _recyclingLimit;

        public SortedOneToManyList()
        {
        }

        /// <summary>
        /// 设置最大缓存数量
        /// </summary>
        /// <param name="recyclingLimit">
        /// 1:防止数据量过大、所以超过recyclingLimit的数据还是走GC.
        /// 2:设置成0不控制数量，全部缓存
        /// </param>
        public SortedOneToManyList(int recyclingLimit = 0)
        {
            _recyclingLimit = recyclingLimit;
        }

        public bool Contains(TKey key, TValue value)
        {
            TryGetValue(key, out var list);

            return list != null && list.Contains(value);
        }

        public void Add(TKey key, TValue value)
        {
            if (!TryGetValue(key, out var list))
            {
                list = Fetch();
                list.Add(value);
                base[key] = list;
                return;
            }

            list.Add(value);
        }

        public TValue First(TKey key)
        {
            return !TryGetValue(key, out var list) ? default : list.FirstOrDefault();
        }

        public void RemoveValue(TKey key, TValue value)
        {
            if (!TryGetValue(key, out var list))
            {
                return;
            }

            list.Remove(value);

            if (list.Count == 0)
            {
                RemoveKey(key);
            }
        }

        public void RemoveKey(TKey key)
        {
            if (!TryGetValue(key, out var list))
            {
                return;
            }

            Remove(key);
            Recycle(list);
        }

        private List<TValue> Fetch()
        {
            return _queue.Count <= 0 ? new List<TValue>() : _queue.Dequeue();
        }

        private void Recycle(List<TValue> list)
        {
            list.Clear();

            if (_recyclingLimit != 0 && _queue.Count > _recyclingLimit)
            {
                return;
            }

            _queue.Enqueue(list);
        }

        protected new void Clear()
        {
            base.Clear();
            _queue.Clear();
        }
    }
}