using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
#pragma warning disable CS8603

namespace TEngine.DataStructure
{
    public class ConcurrentOneToManyListPool<TKey, TValue> : ConcurrentOneToManyList<TKey, TValue>, IDisposable where TKey : notnull
    {
        private bool _isDispose;

        public static ConcurrentOneToManyListPool<TKey, TValue> Create()
        {
            var a = Pool<ConcurrentOneToManyListPool<TKey, TValue>>.Rent();
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
            Pool<ConcurrentOneToManyListPool<TKey, TValue>>.Return(this);
        }
    }

    public class ConcurrentOneToManyList<TKey, TValue> : ConcurrentDictionary<TKey, List<TValue>> where TKey : notnull
    {
        private readonly Queue<List<TValue>> _queue = new Queue<List<TValue>>();
        private readonly int _recyclingLimit = 120;

        public ConcurrentOneToManyList()
        {
        }

        /// <summary>
        /// 设置最大缓存数量
        /// </summary>
        /// <param name="recyclingLimit">
        /// 1:防止数据量过大、所以超过recyclingLimit的数据还是走GC.
        /// 2:设置成0不控制数量，全部缓存
        /// </param>
        public ConcurrentOneToManyList(int recyclingLimit)
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
            if (!TryGetValue(key, out var list)) return;

            list.Remove(value);

            if (list.Count == 0) RemoveKey(key);
        }

        public void RemoveKey(TKey key)
        {
            if (!TryRemove(key, out var list)) return;

            Recycle(list);
        }

        private List<TValue> Fetch()
        {
            return _queue.Count <= 0 ? new List<TValue>() : _queue.Dequeue();
        }

        private void Recycle(List<TValue> list)
        {
            list.Clear();

            if (_recyclingLimit != 0 && _queue.Count > _recyclingLimit) return;

            _queue.Enqueue(list);
        }

        protected new void Clear()
        {
            base.Clear();
            _queue.Clear();
        }
    }
}