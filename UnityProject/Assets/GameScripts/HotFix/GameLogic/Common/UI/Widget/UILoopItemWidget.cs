namespace GameLogic
{
    public class UILoopItemWidget : SelectItemBase
    {
        public LoopListViewItem LoopItem { set; get; }

        public int Index { private set; get; }

        public virtual void UpdateItem(int index)
        {
            Index = index;
        }
    }
}