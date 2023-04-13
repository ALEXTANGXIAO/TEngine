using System.Collections.Generic;

namespace TEngine
{
    /// <summary>
    /// LRU缓存表。
    /// </summary>
    /// <typeparam name="TKey">LRUKey。</typeparam>
    /// <typeparam name="TValue">LRUValue</typeparam>
    /// <remarks>缓存替换策略（Cache Replacement Policy）- LRU（Least Recently Used）。</remarks>
    public class LruCacheTable<TKey, TValue>
    {
        /// <summary>
        /// LRU缓存表头节点。
        /// </summary>
        protected readonly DoubleLinkedListNode<TKey, TValue> Head;

        /// <summary>
        /// LRU缓存表尾节点。
        /// </summary>
        protected readonly DoubleLinkedListNode<TKey, TValue> Tail;

        /// <summary>
        /// 哈希表来存储键值对。
        /// <remarks>Dictionary 中存储 key 和 LinkedList 中节点的映射关系。</remarks>
        /// <remarks>算法优化的关键在于如何降低链表的删除操作的时间复杂度。不管是在插入、删除、查找缓存的时候，都可以通过这种联系来将时间复杂度降低到 O(1)。</remarks>
        /// </summary>
        protected readonly Dictionary<TKey, DoubleLinkedListNode<TKey, TValue>> LinkedListNodesMap;

        protected readonly int Capacity;

        public delegate void OnAdd(TValue data);
        public delegate void OnRemove(TValue data);

        /// <summary>
        /// LRU缓存表添加成功回调。
        /// </summary>
        public OnAdd OnAddCallback;
        
        /// <summary>
        /// LRU缓存表移除回调。
        /// </summary>
        public OnRemove OnRemoveCallback;
        
        /// <summary>
        /// LRU缓存表构造。
        /// </summary>
        /// <param name="capacity">容量。</param>
        /// <param name="onAdd">LRU缓存表添加成功回调。</param>
        /// <param name="onRemove">LRU缓存表移除回调。</param>
        public LruCacheTable(int capacity,OnAdd onAdd = null,OnRemove onRemove = null)
        {
            Capacity = capacity;
            OnAddCallback = onAdd;
            OnRemoveCallback = onRemove;
            Head = new DoubleLinkedListNode<TKey, TValue>();
            Tail = new DoubleLinkedListNode<TKey, TValue>();
            Head.Next = Tail;
            Tail.Previous = Head;
            LinkedListNodesMap = new Dictionary<TKey, DoubleLinkedListNode<TKey, TValue>>(Capacity);
        }

        /// <summary>
        /// 从LRU缓存表中获取。
        /// </summary>
        /// <param name="key">键。</param>
        /// <returns>值。</returns>
        /// <remarks>在链表中删除 key，然后将 key 添加到链表的尾部,这样就可以保证链表的尾部就是最近访问的数据，链表的头部就是最久没有被访问的数据。</remarks>
        public virtual TValue Get(TKey key)
        {
            if (LinkedListNodesMap.TryGetValue(key, out var node))
            {
                RemoveNode(node);
                AddLastNode(node);
                return node.Value;
            }

            return default;
        }

        /// <summary>
        /// 放入LRU缓存表。
        /// </summary>
        /// <param name="key">键。</param>
        /// <param name="value">值。</param>
        public virtual void Put(TKey key, TValue value)
        {
            if (LinkedListNodesMap.TryGetValue(key, out var node))
            {
                // 如果插入的 key 已经存在，将 key 对应的值更新，然后将 key 移动到链表的尾部。
                RemoveNode(node);
                OnRemoveCallback?.Invoke(node.Value);
                AddLastNode(node);
                node.Value = value;
                OnAddCallback?.Invoke(value);
            }
            else
            {
                if (LinkedListNodesMap.Count == Capacity)
                {
                    // 缓存满了，删除链表的头部，也就是最久没有被访问的数据。
                    var firstNode = RemoveFirstNode();
                    LinkedListNodesMap.Remove(firstNode.Key);
                    OnRemoveCallback?.Invoke(firstNode.Value);
                }

                var newNode = new DoubleLinkedListNode<TKey, TValue>(key, value);
                AddLastNode(newNode);
                LinkedListNodesMap.Add(key, newNode);
                OnAddCallback?.Invoke(value);
            }
        }

        /// <summary>
        /// 从LRU缓存表中移除。
        /// </summary>
        /// <param name="key">键。</param>
        public virtual void Remove(TKey key)
        {
            if (LinkedListNodesMap.TryGetValue(key, out var node))
            {
                LinkedListNodesMap.Remove(key);
                RemoveNode(node);
                OnRemoveCallback?.Invoke(node.Value);
            }
        }

        /// <summary>
        /// 清理LRU缓存表中所有数据。
        /// </summary>
        public virtual void Clear()
        {
            int protectedIndex = Capacity;
            while (Head.Next != null)
            {
                var firstNode = RemoveFirstNode();
                LinkedListNodesMap.Remove(firstNode.Key);
                OnRemoveCallback?.Invoke(firstNode.Value);
                protectedIndex--;
                if (protectedIndex <0)
                {
                    break;
                }
            }
        }

        protected void AddLastNode(DoubleLinkedListNode<TKey, TValue> node)
        {
            node.Previous = Tail.Previous;
            node.Next = Tail;
            Tail.Previous.Next = node;
            Tail.Previous = node;
        }

        protected DoubleLinkedListNode<TKey, TValue> RemoveFirstNode()
        {
            var firstNode = Head.Next;
            if (firstNode == null)
            {
                return firstNode;
            }
            Head.Next = firstNode.Next;
            firstNode.Next.Previous = Head;
            firstNode.Next = null;
            firstNode.Previous = null;
            return firstNode;
        }

        protected void RemoveNode(DoubleLinkedListNode<TKey, TValue> node)
        {
            node.Previous.Next = node.Next;
            node.Next.Previous = node.Previous;
            node.Next = null;
            node.Previous = null;
        }

        protected class DoubleLinkedListNode<TNodeKey, TNodeValue>
        {
            public DoubleLinkedListNode()
            {
            }

            public DoubleLinkedListNode(TNodeKey key, TNodeValue value)
            {
                Key = key;
                Value = value;
            }

            public TNodeKey Key { get; set; }

            public TNodeValue Value { get; set; }

            public DoubleLinkedListNode<TNodeKey, TNodeValue> Previous { get; set; }

            public DoubleLinkedListNode<TNodeKey, TNodeValue> Next { get; set; }
        }
    }
}