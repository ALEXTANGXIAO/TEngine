using System;
using System.Collections.Generic;

namespace TEngine.DataStructure
{
    public sealed class HashSetPool<T> : HashSet<T>, IDisposable
    {
        private bool _isDispose;

        public void Dispose()
        {
            if (_isDispose)
            {
                return;
            }

            _isDispose = true;
            Clear();
            Pool<HashSetPool<T>>.Return(this);
        }

        public static HashSetPool<T> Create()
        {
            var list = Pool<HashSetPool<T>>.Rent();
            list._isDispose = false;
            return list;
        }
    }

    public sealed class HashSetBasePool<T> : IDisposable
    {
        public HashSet<T> Set = new HashSet<T>();

        public static HashSetBasePool<T> Create()
        {
            return Pool<HashSetBasePool<T>>.Rent();
        }

        public void Dispose()
        {
            Set.Clear();
            Pool<HashSetBasePool<T>>.Return(this);
        }
    }
}