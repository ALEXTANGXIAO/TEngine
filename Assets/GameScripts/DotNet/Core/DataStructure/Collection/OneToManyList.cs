using System;
using System.Collections.Generic;
using System.Linq;
#pragma warning disable CS8600
#pragma warning disable CS8603

namespace TEngine.DataStructure
{
    public class OneToManyListPool<TKey, TValue> : OneToManyList<TKey, TValue>, IDisposable where TKey : notnull
    {
        private bool _isDispose;

        public static OneToManyListPool<TKey, TValue> Create()
        {
            var list = Pool<OneToManyListPool<TKey, TValue>>.Rent();
            list._isDispose = false;
            return list;
        }

        public void Dispose()
        {
            if (_isDispose)
            {
                return;
            }

            _isDispose = true;
            Clear();
            Pool<OneToManyListPool<TKey, TValue>>.Return(this);
        }
    }

    public class OneToManyList<TKey, TValue> : Dictionary<TKey, List<TValue>> where TKey : notnull
    {
        private readonly Queue<List<TValue>> _queue = new Queue<List<TValue>>();
        private readonly int _recyclingLimit = 120;
        private static List<TValue> _empty = new List<TValue>();

        public OneToManyList()
        {
        }

        /// <summary>
        /// 设置最大缓存数量
        /// </summary>
        /// <param name="recyclingLimit">
        /// 1:防止数据量过大、所以超过recyclingLimit的数据还是走GC.
        /// 2:设置成0不控制数量，全部缓存
        /// </param>
        public OneToManyList(int recyclingLimit)
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

        public TValue First(TKey key)
        {
            return !TryGetValue(key, out var list) ? default : list.FirstOrDefault();
        }

        public bool RemoveValue(TKey key, TValue value)
        {
            if (!TryGetValue(key, out var list))
            {
                return true;
            }

            var isRemove = list.Remove(value);

            if (list.Count == 0)
            {
                isRemove = RemoveByKey(key);
            }

            return isRemove;
        }

        public bool RemoveByKey(TKey key)
        {
            if (!TryGetValue(key, out var list))
            {
                return false;
            }

            Remove(key);
            Recycle(list);
            return true;
        }

        public List<TValue> GetValues(TKey key)
        {
            if (TryGetValue(key, out List<TValue> list))
            {
                return list;
            }

            return _empty;
        }

        public new void Clear()
        {
            foreach (var keyValuePair in this) Recycle(keyValuePair.Value);

            base.Clear();
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
    }
}