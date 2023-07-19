using System;
using System.Collections.Generic;
#pragma warning disable CS8601
#pragma warning disable CS8604
#pragma warning disable CS8603

namespace TEngine.DataStructure
{
    public class DoubleMapDictionaryPool<TKey, TValue> : DoubleMapDictionary<TKey, TValue>, IDisposable
        where TKey : notnull where TValue : notnull
    {
        private bool _isDispose;

        public static DoubleMapDictionaryPool<TKey, TValue> Create()
        {
            var a = Pool<DoubleMapDictionaryPool<TKey, TValue>>.Rent();
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
            Pool<DoubleMapDictionaryPool<TKey, TValue>>.Return(this);
        }
    }

    public class DoubleMapDictionary<TK, TV> where TK : notnull where TV : notnull
    {
        private readonly Dictionary<TK, TV> _kv = new Dictionary<TK, TV>();
        private readonly Dictionary<TV, TK> _vk = new Dictionary<TV, TK>();

        public DoubleMapDictionary()
        {
        }

        public DoubleMapDictionary(int capacity)
        {
            _kv = new Dictionary<TK, TV>(capacity);
            _vk = new Dictionary<TV, TK>(capacity);
        }

        public List<TK> Keys => new List<TK>(_kv.Keys);

        public List<TV> Values => new List<TV>(_vk.Keys);

        public void ForEach(Action<TK, TV> action)
        {
            if (action == null)
            {
                return;
            }

            var keys = _kv.Keys;
            foreach (var key in keys)
            {
                action(key, _kv[key]);
            }
        }

        public void Add(TK key, TV value)
        {
            if (key == null || value == null || _kv.ContainsKey(key) || _vk.ContainsKey(value))
            {
                return;
            }

            _kv.Add(key, value);
            _vk.Add(value, key);
        }

        public TV GetValueByKey(TK key)
        {
            if (key != null && _kv.ContainsKey(key))
            {
                return _kv[key];
            }

            return default;
        }

        public bool TryGetValueByKey(TK key, out TV value)
        {
            var result = key != null && _kv.ContainsKey(key);

            value = result ? _kv[key] : default;

            return result;
        }

        public TK GetKeyByValue(TV value)
        {
            if (value != null && _vk.ContainsKey(value))
            {
                return _vk[value];
            }

            return default;
        }

        public bool TryGetKeyByValue(TV value, out TK key)
        {
            var result = value != null && _vk.ContainsKey(value);

            key = result ? _vk[value] : default;

            return result;
        }

        public void RemoveByKey(TK key)
        {
            if (key == null)
            {
                return;
            }

            if (!_kv.TryGetValue(key, out var value))
            {
                return;
            }

            _kv.Remove(key);
            _vk.Remove(value);
        }

        public void RemoveByValue(TV value)
        {
            if (value == null)
            {
                return;
            }

            if (!_vk.TryGetValue(value, out var key))
            {
                return;
            }

            _kv.Remove(key);
            _vk.Remove(value);
        }

        public void Clear()
        {
            _kv.Clear();
            _vk.Clear();
        }

        public bool ContainsKey(TK key)
        {
            return key != null && _kv.ContainsKey(key);
        }

        public bool ContainsValue(TV value)
        {
            return value != null && _vk.ContainsKey(value);
        }

        public bool Contains(TK key, TV value)
        {
            if (key == null || value == null)
            {
                return false;
            }

            return _kv.ContainsKey(key) && _vk.ContainsKey(value);
        }
    }
}