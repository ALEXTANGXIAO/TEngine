using System;
using System.Collections.Generic;

namespace TEngine.DataStructure
{
    public sealed class ReuseList<T> : List<T>, IDisposable
    {
        private bool _isDispose;

        public static ReuseList<T> Create()
        {
            var list = Pool<ReuseList<T>>.Rent();
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
            Pool<ReuseList<T>>.Return(this);
        }
    }
}