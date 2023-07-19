using System;
using System.Collections.Generic;

namespace TEngine.DataStructure
{
    public sealed class EntityList<T> : List<T>, IDisposable where T : IDisposable
    {
        private bool _isDispose;

        public static EntityList<T> Create()
        {
            var list = Pool<EntityList<T>>.Rent();
            list._isDispose = false;
            return list;
        }

        public new void Clear()
        {
            for (var i = 0; i < this.Count; i++)
            {
                this[i].Dispose();
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
            Pool<EntityList<T>>.Return(this);
        }
    }
}