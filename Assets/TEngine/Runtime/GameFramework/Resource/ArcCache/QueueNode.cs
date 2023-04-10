namespace TEngine
{
    /// <summary>
    /// 队列类型。
    /// T1, recent cache entries.
    /// T2, ghost entries recently evicted from the T1 cache.
    /// B1, frequent entries.
    /// B2, ghost entries recently evicted from the T2 cache.
    /// <remarks></remarks>
    /// </summary>
    public enum QueueType
    {
        None,
        /// <summary>
        /// T1, recent cache entries.
        /// </summary>
        T1,
        /// <summary>
        /// B1, frequent entries.
        /// </summary>
        B1,
        /// <summary>
        /// T2, ghost entries recently evicted from the T1 cache.
        /// </summary>
        T2,
        /// <summary>
        /// B2, ghost entries recently evicted from the T2 cache.
        /// </summary>
        B2
    }

    public class QueueNode<TKey, TValue>
    {
        public readonly TKey Key;
        public QueueNode<TKey, TValue> Prev;
        public QueueNode<TKey, TValue> Next;
        public QueueType QueueType;
        public TValue Value;

        public QueueNode()
        {
            this.Prev = this;
            this.Next = this;
        }

        public QueueNode(TKey key, TValue data)
        {
            this.Key = key;
            this.Value = data;
        }

        public TValue Get()
        {
            return Value;
        }

        public void Set(TValue value)
        {
            this.Value = value;
        }

        public void AddToLast(QueueNode<TKey, TValue> head)
        {
            QueueNode<TKey, TValue> tail = head.Prev;
            head.Prev = this;
            tail.Next = this;
            Next = head;
            Prev = tail;
        }

        public void Remove()
        {
            if (Prev != null && Next != null)
            {
                Prev.Next = Next;
                Next.Prev = Prev;
                Prev = Next = null;
                QueueType = QueueType.None;
            }
        }
    }
}