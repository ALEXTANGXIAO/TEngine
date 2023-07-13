using System;
using System.Collections.Generic;
using System.Linq;
#pragma warning disable CS8603
#pragma warning disable CS8601

namespace TEngine.DataStructure
{
    public class OneToManyDictionaryPool<TKey, TValueKey, TValue> : OneToManyDictionary<TKey, TValueKey, TValue>,
        IDisposable where TKey : notnull where TValueKey : notnull
    {
        private bool _isDispose;

        public static OneToManyDictionaryPool<TKey, TValueKey, TValue> Create()
        {
            var a = Pool<OneToManyDictionaryPool<TKey, TValueKey, TValue>>.Rent();
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
            Pool<OneToManyDictionaryPool<TKey, TValueKey, TValue>>.Return(this);
        }
    }

    public class OneToManyDictionary<TKey, TValueKey, TValue> : Dictionary<TKey, Dictionary<TValueKey, TValue>>
        where TKey : notnull where TValueKey : notnull
    {
        private readonly Queue<Dictionary<TValueKey, TValue>> _queue = new Queue<Dictionary<TValueKey, TValue>>();
        private readonly int _recyclingLimit = 120;

        public OneToManyDictionary()
        {
        }

        /// <summary>
        /// 设置最大缓存数量
        /// </summary>
        /// <param name="recyclingLimit">
        /// 1:防止数据量过大、所以超过recyclingLimit的数据还是走GC.
        /// 2:设置成0不控制数量，全部缓存
        /// </param>
        public OneToManyDictionary(int recyclingLimit = 0)
        {
            _recyclingLimit = recyclingLimit;
        }

        public bool Contains(TKey key, TValueKey valueKey)
        {
            TryGetValue(key, out var dic);

            return dic != null && dic.ContainsKey(valueKey);
        }

        public bool TryGetValue(TKey key, TValueKey valueKey, out TValue value)
        {
            value = default;
            return TryGetValue(key, out var dic) && dic.TryGetValue(valueKey, out value);
        }

        public TValue First(TKey key)
        {
            return !TryGetValue(key, out var dic) ? default : dic.First().Value;
        }

        public void Add(TKey key, TValueKey valueKey, TValue value)
        {
            if (!TryGetValue(key, out var dic))
            {
                dic = Fetch();
                dic[valueKey] = value;
                // dic.Add(valueKey, value);
                Add(key, dic);

                return;
            }

            dic[valueKey] = value;
            // dic.Add(valueKey, value);
        }

        public bool Remove(TKey key, TValueKey valueKey)
        {
            if (!TryGetValue(key, out var dic)) return false;

            var result = dic.Remove(valueKey);

            if (dic.Count == 0) RemoveKey(key);

            return result;
        }

        public bool Remove(TKey key, TValueKey valueKey, out TValue value)
        {
            if (!TryGetValue(key, out var dic))
            {
                value = default;
                return false;
            }

            var result = dic.TryGetValue(valueKey, out value);

            if (result) dic.Remove(valueKey);

            if (dic.Count == 0) RemoveKey(key);

            return result;
        }

        public void RemoveKey(TKey key)
        {
            if (!TryGetValue(key, out var dic)) return;

            Remove(key);
            Recycle(dic);
        }

        private Dictionary<TValueKey, TValue> Fetch()
        {
            return _queue.Count <= 0 ? new Dictionary<TValueKey, TValue>() : _queue.Dequeue();
        }

        private void Recycle(Dictionary<TValueKey, TValue> dic)
        {
            dic.Clear();

            if (_recyclingLimit != 0 && _queue.Count > _recyclingLimit) return;

            _queue.Enqueue(dic);
        }

        public new void Clear()
        {
            foreach (var keyValuePair in this) Recycle(keyValuePair.Value);

            base.Clear();
        }
    }
}