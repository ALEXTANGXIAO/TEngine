using System;
using System.Collections.Generic;

namespace TEngine.DataStructure
{
    public sealed class EntityDictionary<TM, TN> : Dictionary<TM, TN>, IDisposable where TN : IDisposable where TM : notnull
    {
        private bool _isDispose;

        public static EntityDictionary<TM, TN> Create()
        {
            var entityDictionary = Pool<EntityDictionary<TM, TN>>.Rent();
            entityDictionary._isDispose = false;
            return entityDictionary;
        }

        public new void Clear()
        {
            foreach (var keyValuePair in this)
            {
                keyValuePair.Value.Dispose();
            }

            base.Clear();
        }

        public void ClearNotDispose()
        {
            base.Clear();
        }

        public void Dispose()
        {
            if (_isDispose)
            {
                return;
            }

            _isDispose = true;
            Clear();
            Pool<EntityDictionary<TM, TN>>.Return(this);
        }
    }
}