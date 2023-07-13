using System;
using System.Collections.Generic;

namespace TEngine.DataStructure
{
    public sealed class ListPool<T> : List<T>, IDisposable
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
            Pool<ListPool<T>>.Return(this);
        }

        public static ListPool<T> Create(params T[] args)
        {
            var list = Pool<ListPool<T>>.Rent();
            list._isDispose = false;
            if (args != null) list.AddRange(args);
            return list;
        }

        public static ListPool<T> Create(List<T> args)
        {
            var list = Pool<ListPool<T>>.Rent();
            list._isDispose = false;
            if (args != null) list.AddRange(args);
            return list;
        }
    }
}