namespace GameLogic
{
    //if GridFixedType is GridFixedType.ColumnCountFixed, then the GridItemGroup is one row of the gridview
    //if GridFixedType is GridFixedType.RowCountFixed, then the GridItemGroup is one column of the gridview
    public class GridItemGroup
    {
        private int _count = 0;
        private int _groupIndex = -1;//the row index or the column index of this group
        private LoopGridViewItem _first = null;
        private LoopGridViewItem _last = null;
        public int Count => _count;

        public LoopGridViewItem First => _first;

        public LoopGridViewItem Last => _last;

        public int GroupIndex
        {
            get => _groupIndex;
            set => _groupIndex = value;
        }


        public LoopGridViewItem GetItemByColumn(int column)
        {
            LoopGridViewItem cur = _first;
            while(cur != null)
            {
                if(cur.Column == column)
                {
                    return cur;
                }
                cur = cur.NextItem;
            }
            return null;
        }
        public LoopGridViewItem GetItemByRow(int row)
        {
            LoopGridViewItem cur = _first;
            while (cur != null)
            {
                if (cur.Row == row)
                {
                    return cur;
                }
                cur = cur.NextItem;
            }
            return null;
        }


        public void ReplaceItem(LoopGridViewItem curItem,LoopGridViewItem newItem)
        {
            newItem.PrevItem = curItem.PrevItem;
            newItem.NextItem = curItem.NextItem;
            if(newItem.PrevItem != null)
            {
                newItem.PrevItem.NextItem = newItem;
            }
            if(newItem.NextItem != null)
            {
                newItem.NextItem.PrevItem = newItem;
            }
            if(_first == curItem)
            {
                _first = newItem;
            }
            if(_last == curItem)
            {
                _last = newItem;
            }
        }

        public void AddFirst(LoopGridViewItem newItem)
        {
            newItem.PrevItem = null;
            newItem.NextItem = null;
            if (_first == null)
            {
                _first = newItem;
                _last = newItem;
                _first.PrevItem = null;
                _first.NextItem = null;
                _count++;
            }
            else
            {
                _first.PrevItem = newItem;
                newItem.PrevItem = null;
                newItem.NextItem = _first;
                _first = newItem;
                _count++;
            }
        }

        public void AddLast(LoopGridViewItem newItem)
        {
            newItem.PrevItem = null;
            newItem.NextItem = null;
            if (_first == null)
            {
                _first = newItem;
                _last = newItem;
                _first.PrevItem = null;
                _first.NextItem = null;
                _count++;
            }
            else
            {
                _last.NextItem = newItem;
                newItem.PrevItem = _last;
                newItem.NextItem = null;
                _last = newItem;
                _count++;
            }
        }

        public LoopGridViewItem RemoveFirst()
        {
            LoopGridViewItem ret = _first;
            if (_first == null)
            {
                return ret;
            }
            if(_first == _last)
            {
                _first = null;
                _last = null;
                --_count;
                return ret;
            }
            _first = _first.NextItem;
            _first.PrevItem = null;
            --_count;
            return ret;
        }
        public LoopGridViewItem RemoveLast()
        {
            LoopGridViewItem ret = _last;
            if (_first == null)
            {
                return ret;
            }
            if (_first == _last)
            {
                _first = null;
                _last = null;
                --_count;
                return ret;
            }
            _last = _last.PrevItem;
            _last.NextItem = null;
            --_count;
            return ret;
        }


        public void Clear()
        {
            LoopGridViewItem current = _first;
            while (current != null)
            {
                current.PrevItem = null;
                current.NextItem = null;
                current = current.NextItem;
            }
            _first = null;
            _last = null;
            _count = 0;
        }

    }
}
