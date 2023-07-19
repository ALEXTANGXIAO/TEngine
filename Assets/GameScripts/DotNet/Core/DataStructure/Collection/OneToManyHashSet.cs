using System;
using System.Collections.Generic;
#pragma warning disable CS8600

namespace TEngine.DataStructure
{
    public class OneToManyHashSetPool<TKey, TValue> : OneToManyHashSet<TKey, TValue>, IDisposable where TKey : notnull
    {
        private bool _isDispose;

        public static OneToManyHashSetPool<TKey, TValue> Create()
        {
            var a = Pool<OneToManyHashSetPool<TKey, TValue>>.Rent();
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
            Pool<OneToManyHashSetPool<TKey, TValue>>.Return(this);
        }
    }

    public class OneToManyHashSet<TKey, TValue> : Dictionary<TKey, HashSet<TValue>> where TKey : notnull
    {
        private readonly Queue<HashSet<TValue>> _queue = new Queue<HashSet<TValue>>();
        private readonly int _recyclingLimit = 120;
        private static HashSet<TValue> _empty = new HashSet<TValue>();

        public OneToManyHashSet()
        {
        }

        /// <summary>
        ///     设置最大缓存数量
        /// </summary>
        /// <param name="recyclingLimit">
        ///     1:防止数据量过大、所以超过recyclingLimit的数据还是走GC.
        ///     2:设置成0不控制数量，全部缓存
        /// </param>
        public OneToManyHashSet(int recyclingLimit)
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
                Add(key, list);

                return;
            }

            list.Add(value);
        }

        public void RemoveValue(TKey key, TValue value)
        {
            if (!TryGetValue(key, out var list)) return;

            list.Remove(value);

            if (list.Count == 0) RemoveKey(key);
        }

        public void RemoveKey(TKey key)
        {
            if (!TryGetValue(key, out var list)) return;

            Remove(key);
            Recycle(list);
        }

        public HashSet<TValue> GetValue(TKey key)
        {
            if (TryGetValue(key, out HashSet<TValue> value))
            {
                return value;
            }

            return _empty;
        }

        private HashSet<TValue> Fetch()
        {
            return _queue.Count <= 0 ? new HashSet<TValue>() : _queue.Dequeue();
        }

        private void Recycle(HashSet<TValue> list)
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