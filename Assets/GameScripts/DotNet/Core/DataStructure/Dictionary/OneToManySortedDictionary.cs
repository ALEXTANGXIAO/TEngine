using System;
using System.Collections.Generic;
#pragma warning disable CS8601

namespace TEngine.DataStructure
{
    public class
        OneToManySortedDictionaryPool<TKey, TSortedKey, TValue> : OneToManySortedDictionary<TKey, TSortedKey, TValue>,
            IDisposable where TKey : notnull where TSortedKey : notnull
    {
        private bool _isDispose;

        public static OneToManySortedDictionaryPool<TKey, TSortedKey, TValue> Create()
        {
            var a = Pool<OneToManySortedDictionaryPool<TKey, TSortedKey, TValue>>.Rent();
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
            Pool<OneToManySortedDictionaryPool<TKey, TSortedKey, TValue>>.Return(this);
        }
    }

    public class
        OneToManySortedDictionary<TKey, TSortedKey, TValue> : Dictionary<TKey, SortedDictionary<TSortedKey, TValue>>
        where TSortedKey : notnull where TKey : notnull
    {
        private readonly int _recyclingLimit = 120;

        private readonly Queue<SortedDictionary<TSortedKey, TValue>> _queue =
            new Queue<SortedDictionary<TSortedKey, TValue>>();

        protected OneToManySortedDictionary()
        {
        }

        /// <summary>
        /// 设置最大缓存数量
        /// </summary>
        /// <param name="recyclingLimit">
        /// 1:防止数据量过大、所以超过recyclingLimit的数据还是走GC.
        /// 2:设置成0不控制数量，全部缓存
        /// </param>
        public OneToManySortedDictionary(int recyclingLimit)
        {
            _recyclingLimit = recyclingLimit;
        }

        public bool Contains(TKey key)
        {
            return this.ContainsKey(key);
        }

        public bool Contains(TKey key, TSortedKey sortedKey)
        {
            return TryGetValue(key, out var dic) && dic.ContainsKey(sortedKey);
        }

        public new bool TryGetValue(TKey key, out SortedDictionary<TSortedKey, TValue> dic)
        {
            return base.TryGetValue(key, out dic);
        }

        public bool TryGetValueBySortedKey(TKey key, TSortedKey sortedKey, out TValue value)
        {
            if (base.TryGetValue(key, out var dic))
            {
                return dic.TryGetValue(sortedKey, out value);
            }

            value = default;
            return false;
        }

        public void Add(TKey key, TSortedKey sortedKey, TValue value)
        {
            if (!TryGetValue(key, out var dic))
            {
                dic = Fetch();
                dic.Add(sortedKey, value);
                Add(key, dic);

                return;
            }

            dic.Add(sortedKey, value);
        }

        public bool RemoveSortedKey(TKey key, TSortedKey sortedKey)
        {
            if (!TryGetValue(key, out var dic))
            {
                return false;
            }

            var isRemove = dic.Remove(sortedKey);

            if (dic.Count == 0)
            {
                isRemove = RemoveKey(key);
            }

            return isRemove;
        }

        public bool RemoveKey(TKey key)
        {
            if (!TryGetValue(key, out var list))
            {
                return false;
            }

            Remove(key);
            Recycle(list);
            return true;
        }

        private SortedDictionary<TSortedKey, TValue> Fetch()
        {
            return _queue.Count <= 0 ? new SortedDictionary<TSortedKey, TValue>() : _queue.Dequeue();
        }

        private void Recycle(SortedDictionary<TSortedKey, TValue> dic)
        {
            dic.Clear();

            if (_recyclingLimit != 0 && _queue.Count > _recyclingLimit)
            {
                return;
            }

            _queue.Enqueue(dic);
        }

        protected new void Clear()
        {
            base.Clear();
            _queue.Clear();
        }
    }
}