using System;
using System.Collections.Generic;

namespace TEngine.DataStructure
{
    public sealed class ReuseDictionary<TM, TN> : Dictionary<TM, TN>, IDisposable where TM : notnull
    {
        private bool _isDispose;

        public static ReuseDictionary<TM, TN> Create()
        {
            var entityDictionary = Pool<ReuseDictionary<TM, TN>>.Rent();
            entityDictionary._isDispose = false;
            return entityDictionary;
        }
        
        public void Dispose()
        {
            if (_isDispose)
            {
                return;
            }

            _isDispose = true;
            Clear();
            Pool<ReuseDictionary<TM, TN>>.Return(this);
        }
    }
}