using System.Collections.Generic;

namespace GameLogic
{
    public class UnOrderMultiMapSet<TKey, TValue>: Dictionary<TKey, HashSet<TValue>>
    {
        public new HashSet<TValue> this[TKey t]
        {
            get
            {
                HashSet<TValue> set;
                if (!TryGetValue(t, out set))
                {
                    set = new HashSet<TValue>();
                }
                return set;
            }
        }
        
        public Dictionary<TKey, HashSet<TValue>> GetDictionary()
        {
            return this;
        }
        
        public void Add(TKey t, TValue k)
        {
            HashSet<TValue> set;
            TryGetValue(t, out set);
            if (set == null)
            {
                set = new HashSet<TValue>();
                base[t] = set;
            }
            set.Add(k);
        }

        public bool Remove(TKey t, TValue k)
        {
            HashSet<TValue> set;
            TryGetValue(t, out set);
            if (set == null)
            {
                return false;
            }
            if (!set.Remove(k))
            {
                return false;
            }
            if (set.Count == 0)
            {
                Remove(t);
            }
            return true;
        }

        public bool Contains(TKey t, TValue k)
        {
            HashSet<TValue> set;
            TryGetValue(t, out set);
            if (set == null)
            {
                return false;
            }
            return set.Contains(k);
        }

        public new int Count
        {
            get
            {
                int count = 0;
                foreach (KeyValuePair<TKey,HashSet<TValue>> kv in this)
                {
                    count += kv.Value.Count;
                }
                return count;
            }
        }
    }
}