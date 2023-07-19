using System;
using System.Collections.Generic;

namespace TEngine.DataStructure
{
    public sealed class DictionaryPool<TM, TN> : Dictionary<TM, TN>, IDisposable where TM : notnull
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
            Pool<DictionaryPool<TM, TN>>.Return(this);
        }

        public static DictionaryPool<TM, TN> Create()
        {
            var dictionary = Pool<DictionaryPool<TM, TN>>.Rent();
            dictionary._isDispose = false;
            return dictionary;
        }
    }
}