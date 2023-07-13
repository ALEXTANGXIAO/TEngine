namespace TEngine.DataStructure
{
    public class SkipTableNode<TValue>
    {
        public int Index;
        public long Key;
        public long SortKey;
        public long ViceKey;
        public TValue Value;
        public SkipTableNode<TValue> Left;
        public SkipTableNode<TValue> Right;
        public SkipTableNode<TValue> Down;

        public SkipTableNode(long sortKey, long viceKey, long key, TValue value, int index,
            SkipTableNode<TValue> l,
            SkipTableNode<TValue> r,
            SkipTableNode<TValue> d)
        {
            Left = l;
            Right = r;
            Down = d;
            Value = value;
            Key = key;
            Index = index;
            SortKey = sortKey;
            ViceKey = viceKey;
        }
    }
}