using System.Collections.Generic;

namespace TEngine
{
    /// <summary>
    /// 游戏框架字典类。
    /// </summary>
    /// <typeparam name="TKey">指定字典Key的元素类型。</typeparam>
    /// <typeparam name="TValue">指定字典Value的元素类型。</typeparam>
    public class GameFrameworkDictionary<TKey, TValue>
    {
        protected readonly List<TKey> KeyList = new List<TKey>();
        protected readonly Dictionary<TKey, TValue> Dictionary = new Dictionary<TKey, TValue>();

        /// <summary>
        /// 存储键的列表。
        /// </summary>
        public List<TKey> Keys => KeyList;

        /// <summary>
        /// 存储字典实例。
        /// </summary>
        public int Count => KeyList.Count;

        /// <summary>
        /// 通过KEY的数组下标获取元素。
        /// </summary>
        /// <param name="index">下标。</param>
        /// <returns>TValue。</returns>
        public TValue GetValueByIndex(int index)
        {
            return Dictionary[KeyList[index]];
        }

        /// <summary>
        /// 通过KEY的数组下标设置元素。
        /// </summary>
        /// <param name="index">下标。</param>
        /// <param name="item">TValue。</param>
        public void SetValue(int index, TValue item)
        {
            Dictionary[KeyList[index]] = item;
        }

        /// <summary>
        /// 字典索引器。
        /// </summary>
        /// <param name="key">TKey。</param>
        public TValue this[TKey key]
        {
            get => Dictionary[key];
            set
            {
                if (!ContainsKey(key))
                {
                    Add(key, value);
                }
                else
                {
                    Dictionary[key] = value;
                }
            }
        }

        /// <summary>Removes all keys and values from the <see cref="T:TEngine.GameFrameworkDictionary`2" />.</summary>
        public void Clear()
        {
            KeyList.Clear();
            Dictionary.Clear();
        }

        /// <summary>Adds the specified key and value to the dictionary.</summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="item">The value of the element to add. The value can be <see langword="null" /> for reference types.</param>
        public virtual void Add(TKey key, TValue item)
        {
            KeyList.Add(key);
            Dictionary.Add(key, item);
        }

        /// <summary>Gets the value associated with the specified key.</summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="value">When this method returns, contains the value associated with the specified key, if the key is found; otherwise, the default value for the type of the <paramref name="value" /> parameter. This parameter is passed uninitialized.</param>
        public bool TryGetValue(TKey key, out TValue value)
        {
            return Dictionary.TryGetValue(key, out value);
        }

        /// <summary>Determines whether the <see cref="T:System.Collections.Generic.Dictionary`2" /> contains the specified key.</summary>
        /// <param name="key">The key to locate in the </param>
        public bool ContainsKey(TKey key)
        {
            return Dictionary.ContainsKey(key);
        }

        public TKey GetKey(int index)
        {
            return KeyList[index];
        }

        public bool Remove(TKey key)
        {
            return KeyList.Remove(key) && Dictionary.Remove(key);
        }
    }

    /// <summary>
    /// 游戏框架顺序字典类。
    /// </summary>
    /// <typeparam name="TKey">指定字典Key的元素类型。</typeparam>
    /// <typeparam name="TValue">指定字典Value的元素类型。</typeparam>
    public class GameFrameworkSortedDictionary<TKey, TValue> : GameFrameworkDictionary<TKey, TValue>
    {
        public override void Add(TKey key, TValue item)
        {
            base.Add(key, item);
            KeyList.Sort();
        }
    }
}