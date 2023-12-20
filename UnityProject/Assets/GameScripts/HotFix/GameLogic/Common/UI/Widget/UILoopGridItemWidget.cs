namespace GameLogic
{
    public class UILoopGridItemWidget: SelectItemBase
    {
        public LoopGridViewItem LoopItem { set; get; }

        public int Index { private set; get; }

        public virtual void UpdateItem(int index)
        {
            Index = index;
        }
    }
}