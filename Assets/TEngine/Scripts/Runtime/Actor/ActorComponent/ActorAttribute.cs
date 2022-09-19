using System;
using System.Collections.Generic;

namespace TEngine.Runtime.Actor
{
    public class ActorAttribute
    {
        private class AttrChangeHandler
        {
            public Delegate Handle;

            public void AddDelegate(Delegate handler)
            {
                Handle = Delegate.Combine(Handle, handler);
            }

            public void RmvDelegate(Delegate handler)
            {
                Handle = Delegate.Remove(Handle, handler);
            }
        }

        private readonly Dictionary<int, object> _dictAttr = new Dictionary<int, object>();
        private readonly Dictionary<int, AttrChangeHandler> _attrChangedListener = new Dictionary<int, AttrChangeHandler>();

        public void ClearAttribute(int attrId)
        {
            _dictAttr.Remove((int)attrId);
        }

        public void SetAttribute<T>(int attrId, T val)
        {
            bool changed = false;

            T existVal;
            object exist;
            if (_dictAttr.TryGetValue(attrId, out exist))
            {
                existVal = (T)exist;

                if (!EqualityComparer<T>.Default.Equals(existVal, val))
                {
                    changed = true;
                }
            }
            else
            {
                existVal = default(T);
                changed = true;
            }


            if (changed)
            {
                _dictAttr[attrId] = val;

                AttrChangeHandler handler;
                if (_attrChangedListener.TryGetValue(attrId, out handler))
                {
                    Action<T, T> deleHandle = handler.Handle as Action<T, T>;
                    if (deleHandle != null)
                    {
                        deleHandle(existVal, val);
                    }
                }
            }
        }

        public T GetAttribute<T>(int attrId)
        {
            T val;
            TryGetAttribute(attrId, out val);
            return val;
        }

        public bool IsHaveAttribute(int attrId)
        {
            return _dictAttr.ContainsKey((int)attrId);
        }

        public bool TryGetAttribute<T>(int attrId, out T val)
        {
            return GetAttributeValue((int)attrId, out val);
        }

        public bool GetAttributeValue<T>(int attrId, out T val)
        {
            object objVal;
            var ret = _dictAttr.TryGetValue(attrId, out objVal);
            if (ret)
            {
                val = (T)objVal;
            }
            else
            {
                val = default(T);
            }

            return ret;
        }

        public void RegAttrChangeEvent<T>(int attrId, Action<T, T> handler)
        {
            AttrChangeHandler handleNode;
            if (!_attrChangedListener.TryGetValue(attrId, out handleNode))
            {
                handleNode = new AttrChangeHandler();
                _attrChangedListener[attrId] = handleNode;
            }

            handleNode.AddDelegate(handler);
        }

        public void UnRegAttrChangeEvent<T>(int attrId, Action<T, T> handler)
        {
            UnRegAttrChangeEvent(attrId, handler);
        }

        public void UnRegAttrChangeEvent(int attrId, Delegate handler)
        {
            AttrChangeHandler handleNode;
            if (_attrChangedListener.TryGetValue(attrId, out handleNode))
            {
                handleNode.RmvDelegate(handler);
            }
        }
    }
}