using System;
using System.Collections.Generic;
using UnityEngine;


namespace GameLogic
{
    public class GridItemPool
    {
        private GameObject _prefabObj;
        private string _prefabName;
        private int _initCreateCount = 1;
        private readonly List<LoopGridViewItem> _tmpPooledItemList = new List<LoopGridViewItem>();
        private readonly List<LoopGridViewItem> _pooledItemList = new List<LoopGridViewItem>();
        private static int _curItemIdCount = 0;
        private RectTransform _itemParent = null;
        public GridItemPool()
        {

        }
        public void Init(GameObject prefabObj, int createCount, RectTransform parent)
        {
            _prefabObj = prefabObj;
            _prefabName = _prefabObj.name;
            _initCreateCount = createCount;
            _itemParent = parent;
            _prefabObj.SetActive(false);
            for (int i = 0; i < _initCreateCount; ++i)
            {
                LoopGridViewItem tViewItem = CreateItem();
                RecycleItemReal(tViewItem);
            }
        }
        public LoopGridViewItem GetItem()
        {
            _curItemIdCount++;
            LoopGridViewItem tItem = null;
            if (_tmpPooledItemList.Count > 0)
            {
                int count = _tmpPooledItemList.Count;
                tItem = _tmpPooledItemList[count - 1];
                _tmpPooledItemList.RemoveAt(count - 1);
                tItem.gameObject.SetActive(true);
            }
            else
            {
                int count = _pooledItemList.Count;
                if (count == 0)
                {
                    tItem = CreateItem();
                }
                else
                {
                    tItem = _pooledItemList[count - 1];
                    _pooledItemList.RemoveAt(count - 1);
                    tItem.gameObject.SetActive(true);
                }
            }
            tItem.ItemId = _curItemIdCount;
            return tItem;

        }

        public void DestroyAllItem()
        {
            ClearTmpRecycledItem();
            int count = _pooledItemList.Count;
            for (int i = 0; i < count; ++i)
            {
                GameObject.DestroyImmediate(_pooledItemList[i].gameObject);
            }
            _pooledItemList.Clear();
        }
        public LoopGridViewItem CreateItem()
        {

            GameObject go = GameObject.Instantiate<GameObject>(_prefabObj, Vector3.zero, Quaternion.identity, _itemParent);
            go.SetActive(true);
            RectTransform rf = go.GetComponent<RectTransform>();
            rf.localScale = Vector3.one;
            rf.anchoredPosition3D = Vector3.zero;
            rf.localEulerAngles = Vector3.zero;
            LoopGridViewItem tViewItem = go.GetComponent<LoopGridViewItem>();
            tViewItem.ItemPrefabName = _prefabName;
            tViewItem.GoId = go.GetHashCode();
            return tViewItem;
        }
        void RecycleItemReal(LoopGridViewItem item)
        {
            item.gameObject.SetActive(false);
            _pooledItemList.Add(item);
        }
        public void RecycleItem(LoopGridViewItem item)
        {
            item.PrevItem = null;
            item.NextItem = null;
            _tmpPooledItemList.Add(item);
        }
        public void ClearTmpRecycledItem()
        {
            int count = _tmpPooledItemList.Count;
            if (count == 0)
            {
                return;
            }
            for (int i = 0; i < count; ++i)
            {
                RecycleItemReal(_tmpPooledItemList[i]);
            }
            _tmpPooledItemList.Clear();
        }
    }
}
