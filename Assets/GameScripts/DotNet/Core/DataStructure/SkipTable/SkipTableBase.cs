using System;
using System.Collections;
using System.Collections.Generic;
#pragma warning disable CS8601
#pragma warning disable CS8603
#pragma warning disable CS8625
#pragma warning disable CS8604

namespace TEngine.DataStructure
{
    public abstract class SkipTableBase<TValue> : IEnumerable<SkipTableNode<TValue>>
    {
        public readonly int MaxLayer;
        public readonly SkipTableNode<TValue> TopHeader;
        public SkipTableNode<TValue> BottomHeader;

        public int Count => Node.Count;
        protected readonly Random Random = new Random();
        protected readonly Dictionary<long, SkipTableNode<TValue>> Node = new();
        protected readonly Stack<SkipTableNode<TValue>> AntiFindStack = new Stack<SkipTableNode<TValue>>();

        protected SkipTableBase(int maxLayer = 8)
        {
            MaxLayer = maxLayer;
            var cur = TopHeader = new SkipTableNode<TValue>(long.MinValue, 0, 0, default, 0, null, null, null);

            for (var layer = MaxLayer - 1; layer >= 1; --layer)
            {
                cur.Down = new SkipTableNode<TValue>(long.MinValue, 0, 0, default, 0, null, null, null);
                cur = cur.Down;
            }

            BottomHeader = cur;
        }

        public TValue this[long key] => !TryGetValueByKey(key, out TValue value) ? default : value;

        public int GetRanking(long key)
        {
            if (!Node.TryGetValue(key, out var node))
            {
                return 0;
            }

            return node.Index;
        }

        public int GetAntiRanking(long key)
        {
            var ranking = GetRanking(key);

            if (ranking == 0)
            {
                return 0;
            }

            return Count + 1 - ranking;
        }

        public bool TryGetValueByKey(long key, out TValue value)
        {
            if (!Node.TryGetValue(key, out var node))
            {
                value = default;
                return false;
            }

            value = node.Value;
            return true;
        }

        public bool TryGetNodeByKey(long key, out SkipTableNode<TValue> node)
        {
            if (Node.TryGetValue(key, out node))
            {
                return true;
            }

            return false;
        }

        public void Find(int start, int end, ListPool<SkipTableNode<TValue>> list)
        {
            var cur = BottomHeader;
            var count = end - start;

            for (var i = 0; i < start; i++)
            {
                cur = cur.Right;
            }

            for (var i = 0; i <= count; i++)
            {
                if (cur == null)
                {
                    break;
                }

                list.Add(cur);
                cur = cur.Right;
            }
        }

        public void AntiFind(int start, int end, ListPool<SkipTableNode<TValue>> list)
        {
            var cur = BottomHeader;
            start = Count + 1 - start;
            end = start - end;

            for (var i = 0; i < start; i++)
            {
                cur = cur.Right;

                if (cur == null)
                {
                    break;
                }

                if (i < end)
                {
                    continue;
                }

                AntiFindStack.Push(cur);
            }

            while (AntiFindStack.TryPop(out var node))
            {
                list.Add(node);
            }
        }

        public TValue GetLastValue()
        {
            var cur = TopHeader;

            while (cur.Right != null || cur.Down != null)
            {
                while (cur.Right != null)
                {
                    cur = cur.Right;
                }

                if (cur.Down != null)
                {
                    cur = cur.Down;
                }
            }

            return cur.Value;
        }

        public bool Remove(long key)
        {
            if (!Node.TryGetValue(key, out var node))
            {
                return false;
            }

            return Remove(node.SortKey, node.ViceKey, key, out _);
        }

        public abstract void Add(long sortKey, long viceKey, long key, TValue value);
        public abstract bool Remove(long sortKey, long viceKey, long key, out TValue value);

        public IEnumerator<SkipTableNode<TValue>> GetEnumerator()
        {
            var cur = BottomHeader.Right;
            while (cur != null)
            {
                yield return cur;
                cur = cur.Right;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}