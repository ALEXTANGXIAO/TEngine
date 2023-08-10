using System;
using System.Collections.Generic;

namespace GameLogic
{
    public class ItemSizeGroup
    {
        public float[] ItemSizeArray = null;
        public float[] ItemStartPosArray = null;
        public int ItemCount = 0;
        private int _dirtyBeginIndex = ItemPosMgr.ItemMaxCountPerGroup;
        public float GroupSize = 0;
        public float GroupStartPos = 0;
        public float GroupEndPos = 0;
        public int GroupIndex = 0;
        public float ItemDefaultSize = 0;

        public ItemSizeGroup(int index, float itemDefaultSize)
        {
            GroupIndex = index;
            ItemDefaultSize = itemDefaultSize;
            Init();
        }

        public void Init()
        {
            ItemSizeArray = new float[ItemPosMgr.ItemMaxCountPerGroup];
            if (ItemDefaultSize != 0)
            {
                for (int i = 0; i < ItemSizeArray.Length; ++i)
                {
                    ItemSizeArray[i] = ItemDefaultSize;
                }
            }

            ItemStartPosArray = new float[ItemPosMgr.ItemMaxCountPerGroup];
            ItemStartPosArray[0] = 0;
            ItemCount = ItemPosMgr.ItemMaxCountPerGroup;
            GroupSize = ItemDefaultSize * ItemSizeArray.Length;
            if (ItemDefaultSize != 0)
            {
                _dirtyBeginIndex = 0;
            }
            else
            {
                _dirtyBeginIndex = ItemPosMgr.ItemMaxCountPerGroup;
            }
        }

        public float GetItemStartPos(int index)
        {
            return GroupStartPos + ItemStartPosArray[index];
        }

        public bool IsDirty
        {
            get { return (_dirtyBeginIndex < ItemCount); }
        }

        public float SetItemSize(int index, float size)
        {
            float old = ItemSizeArray[index];
            if (Math.Abs(old - size) < 0.001f)
            {
                return 0;
            }

            ItemSizeArray[index] = size;
            if (index < _dirtyBeginIndex)
            {
                _dirtyBeginIndex = index;
            }

            float ds = size - old;
            GroupSize = GroupSize + ds;
            return ds;
        }

        public void SetItemCount(int count)
        {
            if (ItemCount == count)
            {
                return;
            }

            ItemCount = count;
            RecalcGroupSize();
        }

        public void RecalcGroupSize()
        {
            GroupSize = 0;
            for (int i = 0; i < ItemCount; ++i)
            {
                GroupSize += ItemSizeArray[i];
            }
        }

        public int GetItemIndexByPos(float pos)
        {
            if (ItemCount == 0)
            {
                return -1;
            }

            int low = 0;
            int high = ItemCount - 1;
            while (low <= high)
            {
                int mid = (low + high) / 2;
                float startPos = ItemStartPosArray[mid];
                float endPos = startPos + ItemSizeArray[mid];
                if (startPos <= pos && endPos >= pos)
                {
                    return mid;
                }
                else if (pos > endPos)
                {
                    low = mid + 1;
                }
                else
                {
                    high = mid - 1;
                }
            }

            return -1;
        }

        public void UpdateAllItemStartPos()
        {
            if (_dirtyBeginIndex >= ItemCount)
            {
                return;
            }

            int startIndex = (_dirtyBeginIndex < 1) ? 1 : _dirtyBeginIndex;
            for (int i = startIndex; i < ItemCount; ++i)
            {
                ItemStartPosArray[i] = ItemStartPosArray[i - 1] + ItemSizeArray[i - 1];
            }

            _dirtyBeginIndex = ItemCount;
        }
    }

    public class ItemPosMgr
    {
        public const int ItemMaxCountPerGroup = 100;
        readonly List<ItemSizeGroup> _itemSizeGroupList = new List<ItemSizeGroup>();
        public int _dirtyBeginIndex = int.MaxValue;
        public float TotalSize = 0;
        public float ItemDefaultSize = 20;

        public ItemPosMgr(float itemDefaultSize)
        {
            ItemDefaultSize = itemDefaultSize;
        }

        public void SetItemMaxCount(int maxCount)
        {
            _dirtyBeginIndex = 0;
            TotalSize = 0;
            int st = maxCount % ItemMaxCountPerGroup;
            int lastGroupItemCount = st;
            int needMaxGroupCount = maxCount / ItemMaxCountPerGroup;
            if (st > 0)
            {
                needMaxGroupCount++;
            }
            else
            {
                lastGroupItemCount = ItemMaxCountPerGroup;
            }

            int count = _itemSizeGroupList.Count;
            if (count > needMaxGroupCount)
            {
                int d = count - needMaxGroupCount;
                _itemSizeGroupList.RemoveRange(needMaxGroupCount, d);
            }
            else if (count < needMaxGroupCount)
            {
                int d = needMaxGroupCount - count;
                for (int i = 0; i < d; ++i)
                {
                    ItemSizeGroup tGroup = new ItemSizeGroup(count + i, ItemDefaultSize);
                    _itemSizeGroupList.Add(tGroup);
                }
            }

            count = _itemSizeGroupList.Count;
            if (count == 0)
            {
                return;
            }

            for (int i = 0; i < count - 1; ++i)
            {
                _itemSizeGroupList[i].SetItemCount(ItemMaxCountPerGroup);
            }

            _itemSizeGroupList[count - 1].SetItemCount(lastGroupItemCount);
            for (int i = 0; i < count; ++i)
            {
                TotalSize = TotalSize + _itemSizeGroupList[i].GroupSize;
            }
        }

        public void SetItemSize(int itemIndex, float size)
        {
            int groupIndex = itemIndex / ItemMaxCountPerGroup;
            int indexInGroup = itemIndex % ItemMaxCountPerGroup;
            ItemSizeGroup tGroup = _itemSizeGroupList[groupIndex];
            float changedSize = tGroup.SetItemSize(indexInGroup, size);
            if (changedSize != 0f)
            {
                if (groupIndex < _dirtyBeginIndex)
                {
                    _dirtyBeginIndex = groupIndex;
                }
            }

            TotalSize += changedSize;
        }

        public float GetItemPos(int itemIndex)
        {
            Update(true);
            int groupIndex = itemIndex / ItemMaxCountPerGroup;
            int indexInGroup = itemIndex % ItemMaxCountPerGroup;
            return _itemSizeGroupList[groupIndex].GetItemStartPos(indexInGroup);
        }

        public void GetItemIndexAndPosAtGivenPos(float pos, ref int index, ref float itemPos)
        {
            Update(true);
            index = 0;
            itemPos = 0f;
            int count = _itemSizeGroupList.Count;
            if (count == 0)
            {
                return;
            }

            ItemSizeGroup hitGroup = null;

            int low = 0;
            int high = count - 1;
            while (low <= high)
            {
                int mid = (low + high) / 2;
                ItemSizeGroup tGroup = _itemSizeGroupList[mid];
                if (tGroup.GroupStartPos <= pos && tGroup.GroupEndPos >= pos)
                {
                    hitGroup = tGroup;
                    break;
                }
                else if (pos > tGroup.GroupEndPos)
                {
                    low = mid + 1;
                }
                else
                {
                    high = mid - 1;
                }
            }

            int hitIndex = -1;
            if (hitGroup != null)
            {
                hitIndex = hitGroup.GetItemIndexByPos(pos - hitGroup.GroupStartPos);
            }
            else
            {
                return;
            }

            if (hitIndex < 0)
            {
                return;
            }

            index = hitIndex + hitGroup.GroupIndex * ItemMaxCountPerGroup;
            itemPos = hitGroup.GetItemStartPos(hitIndex);
        }

        public void Update(bool updateAll)
        {
            int count = _itemSizeGroupList.Count;
            if (count == 0)
            {
                return;
            }

            if (_dirtyBeginIndex >= count)
            {
                return;
            }

            int loopCount = 0;
            for (int i = _dirtyBeginIndex; i < count; ++i)
            {
                loopCount++;
                ItemSizeGroup tGroup = _itemSizeGroupList[i];
                _dirtyBeginIndex++;
                tGroup.UpdateAllItemStartPos();
                if (i == 0)
                {
                    tGroup.GroupStartPos = 0;
                    tGroup.GroupEndPos = tGroup.GroupSize;
                }
                else
                {
                    tGroup.GroupStartPos = _itemSizeGroupList[i - 1].GroupEndPos;
                    tGroup.GroupEndPos = tGroup.GroupStartPos + tGroup.GroupSize;
                }

                if (!updateAll && loopCount > 1)
                {
                    return;
                }
            }
        }
    }
}