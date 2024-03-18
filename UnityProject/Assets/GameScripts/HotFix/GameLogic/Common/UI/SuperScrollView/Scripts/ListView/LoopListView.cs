using System;
using System.Collections.Generic;
using TEngine;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace GameLogic
{
    public class ItemPool
    {
        private GameObject _prefabObj;
        private string _prefabName;
        private int _initCreateCount = 1;
        private float _startPosOffset = 0;
        private List<LoopListViewItem> _tmpPooledItemList = new List<LoopListViewItem>();
        private List<LoopListViewItem> _pooledItemList = new List<LoopListViewItem>();
        private static int _curItemIdCount = 0;
        private RectTransform _itemParent = null;

        public ItemPool()
        {
        }

        public void Init(GameObject prefabObj, float startPosOffset, int createCount,
            RectTransform parent)
        {
            _prefabObj = prefabObj;
            _prefabName = _prefabObj.name;
            _initCreateCount = createCount;
            _startPosOffset = startPosOffset;
            _itemParent = parent;
            _prefabObj.SetActive(false);
            for (int i = 0; i < _initCreateCount; ++i)
            {
                LoopListViewItem tViewItem = CreateItem();
                RecycleItemReal(tViewItem);
            }
        }

        public LoopListViewItem GetItem()
        {
            _curItemIdCount++;
            LoopListViewItem tItem = null;
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
                Object.DestroyImmediate(_pooledItemList[i].gameObject);
            }

            _pooledItemList.Clear();
        }

        public LoopListViewItem CreateItem()
        {
            GameObject go = Object.Instantiate<GameObject>(_prefabObj, Vector3.zero, Quaternion.identity, _itemParent);
            go.SetActive(true);
            RectTransform rf = go.GetComponent<RectTransform>();
            rf.localScale = Vector3.one;
            rf.localPosition = Vector3.zero;
            rf.localEulerAngles = Vector3.zero;
            LoopListViewItem tViewItem = go.GetComponent<LoopListViewItem>();
            tViewItem.ItemPrefabName = _prefabName;
            tViewItem.StartPosOffset = _startPosOffset;
            tViewItem.GoId = go.GetHashCode();
            return tViewItem;
        }

        void RecycleItemReal(LoopListViewItem item)
        {
            item.gameObject.SetActive(false);
            _pooledItemList.Add(item);
        }

        public void RecycleItem(LoopListViewItem item)
        {
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

    [System.Serializable]
    public class ItemPrefabConfData
    {
        public GameObject mItemPrefab = null;
        public int mInitCreateCount = 0;
        public float mStartPosOffset = 0;
    }


    public class LoopListViewInitParam
    {
        // all the default values
        public float DistanceForRecycle0 = 300; //mDistanceForRecycle0 should be larger than mDistanceForNew0
        public float DistanceForNew0 = 200;
        public float DistanceForRecycle1 = 300; //mDistanceForRecycle1 should be larger than mDistanceForNew1
        public float DistanceForNew1 = 200;
        public float SmoothDumpRate = 0.3f;
        public float SnapFinishThreshold = 0.01f;
        public float SnapVecThreshold = 145;
        public float ItemDefaultWithPaddingSize = 20;

        public static LoopListViewInitParam CopyDefaultInitParam()
        {
            return new LoopListViewInitParam();
        }
    }

    public delegate LoopListViewItem
        OnGetItemByIndex(LoopListView listView, int index); // System.Func<LoopListView, int, LoopListViewItem> 


    public class LoopListView : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        class SnapData
        {
            public SnapStatus SnapStatus = SnapStatus.NoTargetSet;
            public int SnapTargetIndex = 0;
            public float TargetSnapVal = 0;
            public float CurSnapVal = 0;
            public bool IsForceSnapTo = false;

            public void Clear()
            {
                SnapStatus = SnapStatus.NoTargetSet;
                IsForceSnapTo = false;
            }
        }

        private readonly Dictionary<string, ItemPool> _itemPoolDict = new Dictionary<string, ItemPool>();
        private readonly List<ItemPool> _itemPoolList = new List<ItemPool>();
        [SerializeField] private List<ItemPrefabConfData> itemPrefabDataList = new List<ItemPrefabConfData>();
        [SerializeField] private ListItemArrangeType arrangeType = ListItemArrangeType.TopToBottom;

        public ListItemArrangeType ArrangeType
        {
            get => arrangeType;
            set => arrangeType = value;
        }

        private readonly List<LoopListViewItem> _itemList = new List<LoopListViewItem>();
        private RectTransform _containerTrans;
        private ScrollRect _scrollRect = null;
        private RectTransform _scrollRectTransform = null;
        private RectTransform _viewPortRectTransform = null;
        private float _itemDefaultWithPaddingSize = 20;
        private int _itemTotalCount = 0;
        private bool _isVertList = false;
        private OnGetItemByIndex _onGetItemByIndex;
        private readonly Vector3[] _itemWorldCorners = new Vector3[4];
        private readonly Vector3[] _viewPortRectLocalCorners = new Vector3[4];
        private int _curReadyMinItemIndex = 0;
        private int _curReadyMaxItemIndex = 0;
        private bool _needCheckNextMinItem = true;
        private bool _needCheckNextMaxItem = true;
        private ItemPosMgr _itemPosMgr = null;
        private float _distanceForRecycle0 = 300;
        private float _distanceForNew0 = 200;
        private float _distanceForRecycle1 = 300;
        private float _distanceForNew1 = 200;
        [SerializeField] private bool supportScrollBar = true;
        private bool _isDraging = false;
        private PointerEventData _pointerEventData = null;
        public System.Action OnBeginDragAction = null;
        public System.Action OnDragingAction = null;
        public System.Action OnEndDragAction = null;
        private int _lastItemIndex = 0;
        private float _lastItemPadding = 0;
        private float _smoothDumpVel = 0;
        private float _smoothDumpRate = 0.3f;
        private float _snapFinishThreshold = 0.1f;
        private float _snapVecThreshold = 145;
        [SerializeField] private bool itemSnapEnable = false;


        private Vector3 _lastFrameContainerPos = Vector3.zero;
        public System.Action<LoopListView, LoopListViewItem> OnSnapItemFinished = null;
        public System.Action<LoopListView, LoopListViewItem> OnSnapNearestChanged = null;
        private int _curSnapNearestItemIndex = -1;
        private Vector2 _adjustedVec;
        private bool _needAdjustVec = false;
        private int _leftSnapUpdateExtraCount = 1;
        [SerializeField] Vector2 viewPortSnapPivot = Vector2.zero;
        [SerializeField] Vector2 itemSnapPivot = Vector2.zero;
        private ClickEventListener _scrollBarClickEventListener = null;
        private SnapData _curSnapData = new SnapData();
        private Vector3 lastSnapCheckPos = Vector3.zero;
        private bool _listViewInited = false;
        private int _listUpdateCheckFrameCount = 0;

        public bool IsVertList => _isVertList;

        public int ItemTotalCount => _itemTotalCount;

        public RectTransform ContainerTrans => _containerTrans;

        public ScrollRect ScrollRect => _scrollRect;

        public bool IsDraging => _isDraging;

        public bool ItemSnapEnable
        {
            get => itemSnapEnable;
            set => itemSnapEnable = value;
        }

        public bool SupportScrollBar
        {
            get => supportScrollBar;
            set => supportScrollBar = value;
        }

        public ItemPrefabConfData GetItemPrefabConfData(string prefabName)
        {
            foreach (ItemPrefabConfData data in itemPrefabDataList)
            {
                if (data.mItemPrefab == null)
                {
                    Debug.LogError("A item prefab is null ");
                    continue;
                }

                if (prefabName == data.mItemPrefab.name)
                {
                    return data;
                }
            }

            return null;
        }

        public void OnItemPrefabChanged(string prefabName)
        {
            ItemPrefabConfData data = GetItemPrefabConfData(prefabName);
            if (data == null)
            {
                return;
            }

            if (_itemPoolDict.TryGetValue(prefabName, out var pool) == false)
            {
                return;
            }

            int firstItemIndex = -1;
            Vector3 pos = Vector3.zero;
            if (_itemList.Count > 0)
            {
                firstItemIndex = _itemList[0].ItemIndex;
                pos = _itemList[0].CachedRectTransform.localPosition;
            }

            RecycleAllItem();
            ClearAllTmpRecycledItem();
            pool.DestroyAllItem();
            pool.Init(data.mItemPrefab, data.mStartPosOffset, data.mInitCreateCount, _containerTrans);
            if (firstItemIndex >= 0)
            {
                RefreshAllShownItemWithFirstIndexAndPos(firstItemIndex, pos);
            }
        }

        /*
        InitListView method is to initiate the LoopListView component. There are 3 parameters:
        itemTotalCount: the total item count in the listview. If this parameter is set -1, then means there are infinite items, and scrollbar would not be supported, and the ItemIndex can be from –MaxInt to +MaxInt. If this parameter is set a value >=0 , then the ItemIndex can only be from 0 to itemTotalCount -1.
        onGetItemByIndex: when a item is getting in the scrollrect viewport, and this Action will be called with the item’ index as a parameter, to let you create the item and update its content.
        */
        public void InitListView(int itemTotalCount, OnGetItemByIndex
                onGetItemByIndex,
            LoopListViewInitParam initParam = null)
        {
            if (initParam != null)
            {
                _distanceForRecycle0 = initParam.DistanceForRecycle0;
                _distanceForNew0 = initParam.DistanceForNew0;
                _distanceForRecycle1 = initParam.DistanceForRecycle1;
                _distanceForNew1 = initParam.DistanceForNew1;
                _smoothDumpRate = initParam.SmoothDumpRate;
                _snapFinishThreshold = initParam.SnapFinishThreshold;
                _snapVecThreshold = initParam.SnapVecThreshold;
                _itemDefaultWithPaddingSize = initParam.ItemDefaultWithPaddingSize;
            }

            _scrollRect = gameObject.GetComponent<ScrollRect>();
            if (_scrollRect == null)
            {
                Debug.LogError("ListView Init Failed! ScrollRect component not found!");
                return;
            }

            if (_distanceForRecycle0 <= _distanceForNew0)
            {
                Debug.LogError("mDistanceForRecycle0 should be bigger than mDistanceForNew0");
            }

            if (_distanceForRecycle1 <= _distanceForNew1)
            {
                Debug.LogError("mDistanceForRecycle1 should be bigger than mDistanceForNew1");
            }

            _curSnapData.Clear();
            _itemPosMgr = new ItemPosMgr(_itemDefaultWithPaddingSize);
            _scrollRectTransform = _scrollRect.GetComponent<RectTransform>();
            _containerTrans = _scrollRect.content;
            _viewPortRectTransform = _scrollRect.viewport;
            if (_viewPortRectTransform == null)
            {
                _viewPortRectTransform = _scrollRectTransform;
            }

            if (_scrollRect.horizontalScrollbarVisibility == ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport &&
                _scrollRect.horizontalScrollbar != null)
            {
                Debug.LogError("ScrollRect.horizontalScrollbarVisibility cannot be set to AutoHideAndExpandViewport");
            }

            if (_scrollRect.verticalScrollbarVisibility == ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport &&
                _scrollRect.verticalScrollbar != null)
            {
                Debug.LogError("ScrollRect.verticalScrollbarVisibility cannot be set to AutoHideAndExpandViewport");
            }

            _isVertList = (arrangeType == ListItemArrangeType.TopToBottom ||
                           arrangeType == ListItemArrangeType.BottomToTop);
            _scrollRect.horizontal = !_isVertList;
            _scrollRect.vertical = _isVertList;
            SetScrollbarListener();
            AdjustPivot(_viewPortRectTransform);
            AdjustAnchor(_containerTrans);
            AdjustContainerPivot(_containerTrans);
            InitItemPool();
            _onGetItemByIndex = onGetItemByIndex;
            if (_listViewInited)
            {
                Debug.LogError("LoopListView.InitListView method can be called only once.");
            }

            _listViewInited = true;
            ResetListView();
            SetListItemCount(itemTotalCount, true);
        }

        void SetScrollbarListener()
        {
            _scrollBarClickEventListener = null;
            Scrollbar curScrollBar = null;
            if (_isVertList && _scrollRect.verticalScrollbar != null)
            {
                curScrollBar = _scrollRect.verticalScrollbar;
            }

            if (!_isVertList && _scrollRect.horizontalScrollbar != null)
            {
                curScrollBar = _scrollRect.horizontalScrollbar;
            }

            if (curScrollBar == null)
            {
                return;
            }

            ClickEventListener listener = ClickEventListener.Get(curScrollBar.gameObject);
            _scrollBarClickEventListener = listener;
            listener.SetPointerUpHandler(OnPointerUpInScrollBar);
            listener.SetPointerDownHandler(OnPointerDownInScrollBar);
        }

        void OnPointerDownInScrollBar(GameObject obj)
        {
            _curSnapData.Clear();
        }

        void OnPointerUpInScrollBar(GameObject obj)
        {
            ForceSnapUpdateCheck();
        }

        public void ResetListView()
        {
            _viewPortRectTransform.GetLocalCorners(_viewPortRectLocalCorners);
            _containerTrans.localPosition = Vector3.zero;
            ForceSnapUpdateCheck();
        }

        public void ResetListView(Vector3 vector3)
        {
            _viewPortRectTransform.GetLocalCorners(_viewPortRectLocalCorners);
            _containerTrans.localPosition = vector3;
            ForceSnapUpdateCheck();
        }

        public Vector3 GetContainerTrans()
        {
            return _containerTrans.localPosition;
        }


        /*
        This method may use to set the item total count of the scrollview at runtime. 
        If this parameter is set -1, then means there are infinite items,
        and scrollbar would not be supported, and the ItemIndex can be from –MaxInt to +MaxInt. 
        If this parameter is set a value >=0 , then the ItemIndex can only be from 0 to itemTotalCount -1.  
        If resetPos is set false, then the scrollrect’s content position will not changed after this method finished.
        */
        public void SetListItemCount(int itemCount, bool resetPos = true)
        {
            if (itemCount == _itemTotalCount)
            {
                return;
            }

            _curSnapData.Clear();
            _itemTotalCount = itemCount;
            if (_itemTotalCount < 0)
            {
                supportScrollBar = false;
            }

            if (supportScrollBar)
            {
                _itemPosMgr.SetItemMaxCount(_itemTotalCount);
            }
            else
            {
                _itemPosMgr.SetItemMaxCount(0);
            }

            if (_itemTotalCount == 0)
            {
                _curReadyMaxItemIndex = 0;
                _curReadyMinItemIndex = 0;
                _needCheckNextMaxItem = false;
                _needCheckNextMinItem = false;
                RecycleAllItem();
                ClearAllTmpRecycledItem();
                UpdateContentSize();
                return;
            }

            _leftSnapUpdateExtraCount = 1;
            _needCheckNextMaxItem = true;
            _needCheckNextMinItem = true;
            if (resetPos)
            {
                MovePanelToItemIndex(0, 0);
                return;
            }

            if (_itemList.Count == 0)
            {
                MovePanelToItemIndex(0, 0);
                return;
            }

            int maxItemIndex = _itemTotalCount - 1;
            int lastItemIndex = _itemList[^1].ItemIndex;
            if (lastItemIndex <= maxItemIndex)
            {
                UpdateContentSize();
                UpdateAllShownItemsPos();
                return;
            }

            MovePanelToItemIndex(maxItemIndex, 0);
        }

        //To get the visible item by itemIndex. If the item is not visible, then this method return null.
        public LoopListViewItem GetShownItemByItemIndex(int itemIndex)
        {
            int count = _itemList.Count;
            if (count == 0)
            {
                return null;
            }

            if (itemIndex < _itemList[0].ItemIndex || itemIndex > _itemList[count - 1].ItemIndex)
            {
                return null;
            }

            int i = itemIndex - _itemList[0].ItemIndex;
            return _itemList[i];
        }

        public int ShownItemCount => _itemList.Count;

        public float ViewPortSize
        {
            get
            {
                if (_isVertList)
                {
                    return _viewPortRectTransform.rect.height;
                }
                else
                {
                    return _viewPortRectTransform.rect.width;
                }
            }
        }

        public float ViewPortWidth => _viewPortRectTransform.rect.width;

        public float ViewPortHeight => _viewPortRectTransform.rect.height;


        /*
         All visible items is stored in a List<LoopListViewItem> , which is named mItemList;
         this method is to get the visible item by the index in visible items list. The parameter index is from 0 to mItemList.Count.
        */
        public LoopListViewItem GetShownItemByIndex(int index)
        {
            int count = _itemList.Count;
            if (index < 0 || index >= count)
            {
                return null;
            }

            return _itemList[index];
        }

        public LoopListViewItem GetShownItemByIndexWithoutCheck(int index)
        {
            return _itemList[index];
        }

        public int GetIndexInShownItemList(LoopListViewItem item)
        {
            if (item == null)
            {
                return -1;
            }

            int count = _itemList.Count;
            if (count == 0)
            {
                return -1;
            }

            for (int i = 0; i < count; ++i)
            {
                if (_itemList[i] == item)
                {
                    return i;
                }
            }

            return -1;
        }


        public void DoActionForEachShownItem(System.Action<LoopListViewItem, object> action, object param)
        {
            if (action == null)
            {
                return;
            }

            int count = _itemList.Count;
            if (count == 0)
            {
                return;
            }

            for (int i = 0; i < count; ++i)
            {
                action(_itemList[i], param);
            }
        }

        public LoopListViewItem AllocOrNewListViewItem(string itemPrefabName)
        {
            ItemPool pool = null;
            if (!string.IsNullOrEmpty(itemPrefabName))
            {
                if (_itemPoolDict.TryGetValue(itemPrefabName, out pool) == false)
                {
                    pool = TryCreateItemPool(itemPrefabName);
                    if (pool == null)
                    {
                        return null;
                    }
                }

                return AllocOrNewListViewItem(pool);
            }

            return null;
        }

        public LoopListViewItem AllocOrNewListViewItem(GameObject prefabGo)
        {
            ItemPool pool = null;
            if (prefabGo != null)
            {
                if (_itemPoolDict.TryGetValue(prefabGo.name, out pool) == false)
                {
                    pool = TryCreateItemPool(prefabGo);
                    if (pool == null)
                    {
                        return null;
                    }
                }

                return AllocOrNewListViewItem(pool);
            }

            return null;
        }

        private LoopListViewItem AllocOrNewListViewItem(ItemPool pool)
        {
            LoopListViewItem item = pool.GetItem();
            RectTransform rf = item.GetComponent<RectTransform>();
            rf.SetParent(_containerTrans);
            rf.localScale = Vector3.one;
            rf.localPosition = Vector3.zero;
            rf.localEulerAngles = Vector3.zero;
            item.ParentListView = this;
            return item;
        }

        private ItemPool TryCreateItemPool(string itemPrefabName)
        {
            string resPath = itemPrefabName;
            GameObject go = GameModule.Resource.LoadGameObject(resPath, parent: _containerTrans);
            if (go != null)
            {
                go.SetActive(false);
                go.name = itemPrefabName;
                ItemPool pool = new ItemPool();
                pool.Init(go, 0, 0, _containerTrans);
                _itemPoolDict.Add(itemPrefabName, pool);
                _itemPoolList.Add(pool);
                return pool;
            }

            return null;
        }

        private ItemPool TryCreateItemPool(GameObject itemPrefab)
        {
            if (itemPrefab != null)
            {
                itemPrefab.SetActive(false);
                ItemPool pool = new ItemPool();
                pool.Init(itemPrefab, 0, 0, _containerTrans);
                _itemPoolDict.Add(itemPrefab.name, pool);
                _itemPoolList.Add(pool);
                return pool;
            }

            return null;
        }

        /*
        For a vertical scrollrect, when a visible item’s height changed at runtime, then this method should be called to let the LoopListView component reposition all visible items’ position.
        For a horizontal scrollrect, when a visible item’s width changed at runtime, then this method should be called to let the LoopListView component reposition all visible items’ position.
        */
        public void OnItemSizeChanged(int itemIndex)
        {
            LoopListViewItem item = GetShownItemByItemIndex(itemIndex);
            if (item == null)
            {
                return;
            }

            if (supportScrollBar)
            {
                if (_isVertList)
                {
                    SetItemSize(itemIndex, item.CachedRectTransform.rect.height, item.Padding);
                }
                else
                {
                    SetItemSize(itemIndex, item.CachedRectTransform.rect.width, item.Padding);
                }
            }

            UpdateContentSize();
            UpdateAllShownItemsPos();
        }


        /*
        To update a item by itemIndex.if the itemIndex-th item is not visible, then this method will do nothing.
        Otherwise this method will first call onGetItemByIndex(itemIndex) to get a updated item and then reposition all visible items'position. 
        */
        public void RefreshItemByItemIndex(int itemIndex)
        {
            int count = _itemList.Count;
            if (count == 0)
            {
                return;
            }

            if (itemIndex < _itemList[0].ItemIndex || itemIndex > _itemList[count - 1].ItemIndex)
            {
                return;
            }

            int firstItemIndex = _itemList[0].ItemIndex;
            int i = itemIndex - firstItemIndex;
            LoopListViewItem curItem = _itemList[i];
            Vector3 pos = curItem.CachedRectTransform.localPosition;
            RecycleItemTmp(curItem);
            LoopListViewItem newItem = GetNewItemByIndex(itemIndex);
            if (newItem == null)
            {
                RefreshAllShownItemWithFirstIndex(firstItemIndex);
                return;
            }

            _itemList[i] = newItem;
            if (_isVertList)
            {
                pos.x = newItem.StartPosOffset;
            }
            else
            {
                pos.y = newItem.StartPosOffset;
            }

            newItem.CachedRectTransform.localPosition = pos;
            OnItemSizeChanged(itemIndex);
            ClearAllTmpRecycledItem();
        }

        //snap move will finish at once.
        public void FinishSnapImmediately()
        {
            UpdateSnapMove(true);
        }

        /*
        This method will move the scrollrect content’s position to ( the positon of itemIndex-th item + offset ),
        and in current version the itemIndex is from 0 to MaxInt, offset is from 0 to scrollrect viewport size. 
        */
        public void MovePanelToItemIndex(int itemIndex, float offset = 0)
        {
            _scrollRect.StopMovement();
            _curSnapData.Clear();
            if (itemIndex < 0 || _itemTotalCount == 0)
            {
                return;
            }

            if (_itemTotalCount > 0 && itemIndex >= _itemTotalCount)
            {
                itemIndex = _itemTotalCount - 1;
            }

            if (offset < 0)
            {
                offset = 0;
            }

            Vector3 pos = Vector3.zero;
            float viewPortSize = ViewPortSize;
            if (offset > viewPortSize)
            {
                offset = viewPortSize;
            }

            if (arrangeType == ListItemArrangeType.TopToBottom)
            {
                float containerPos = _containerTrans.localPosition.y;
                if (containerPos < 0)
                {
                    containerPos = 0;
                }

                pos.y = -containerPos - offset;
            }
            else if (arrangeType == ListItemArrangeType.BottomToTop)
            {
                float containerPos = _containerTrans.localPosition.y;
                if (containerPos > 0)
                {
                    containerPos = 0;
                }

                pos.y = -containerPos + offset;
            }
            else if (arrangeType == ListItemArrangeType.LeftToRight)
            {
                float containerPos = _containerTrans.localPosition.x;
                if (containerPos > 0)
                {
                    containerPos = 0;
                }

                pos.x = -containerPos + offset;
            }
            else if (arrangeType == ListItemArrangeType.RightToLeft)
            {
                float containerPos = _containerTrans.localPosition.x;
                if (containerPos < 0)
                {
                    containerPos = 0;
                }

                pos.x = -containerPos - offset;
            }

            RecycleAllItem();
            LoopListViewItem newItem = GetNewItemByIndex(itemIndex);
            if (newItem == null)
            {
                ClearAllTmpRecycledItem();
                return;
            }

            if (_isVertList)
            {
                pos.x = newItem.StartPosOffset;
            }
            else
            {
                pos.y = newItem.StartPosOffset;
            }

            newItem.CachedRectTransform.localPosition = pos;
            if (supportScrollBar)
            {
                if (_isVertList)
                {
                    SetItemSize(itemIndex, newItem.CachedRectTransform.rect.height, newItem.Padding);
                }
                else
                {
                    SetItemSize(itemIndex, newItem.CachedRectTransform.rect.width, newItem.Padding);
                }
            }

            _itemList.Add(newItem);
            UpdateContentSize();
            UpdateListView(viewPortSize + 100, viewPortSize + 100, viewPortSize, viewPortSize);
            AdjustPanelPos();
            ClearAllTmpRecycledItem();
        }

        //update all visible items.
        public void RefreshAllShownItem()
        {
            int count = _itemList.Count;
            if (count == 0)
            {
                return;
            }

            RefreshAllShownItemWithFirstIndex(_itemList[0].ItemIndex);
        }


        public void RefreshAllShownItemWithFirstIndex(int firstItemIndex)
        {
            int count = _itemList.Count;
            if (count == 0)
            {
                return;
            }

            LoopListViewItem firstItem = _itemList[0];
            Vector3 pos = firstItem.CachedRectTransform.localPosition;
            RecycleAllItem();
            for (int i = 0; i < count; ++i)
            {
                int curIndex = firstItemIndex + i;
                LoopListViewItem newItem = GetNewItemByIndex(curIndex);
                if (newItem == null)
                {
                    break;
                }

                if (_isVertList)
                {
                    pos.x = newItem.StartPosOffset;
                }
                else
                {
                    pos.y = newItem.StartPosOffset;
                }

                newItem.CachedRectTransform.localPosition = pos;
                if (supportScrollBar)
                {
                    if (_isVertList)
                    {
                        SetItemSize(curIndex, newItem.CachedRectTransform.rect.height, newItem.Padding);
                    }
                    else
                    {
                        SetItemSize(curIndex, newItem.CachedRectTransform.rect.width, newItem.Padding);
                    }
                }

                _itemList.Add(newItem);
            }

            UpdateContentSize();
            UpdateAllShownItemsPos();
            ClearAllTmpRecycledItem();
        }


        public void RefreshAllShownItemWithFirstIndexAndPos(int firstItemIndex, Vector3 pos)
        {
            RecycleAllItem();
            LoopListViewItem newItem = GetNewItemByIndex(firstItemIndex);
            if (newItem == null)
            {
                return;
            }

            if (_isVertList)
            {
                pos.x = newItem.StartPosOffset;
            }
            else
            {
                pos.y = newItem.StartPosOffset;
            }

            newItem.CachedRectTransform.localPosition = pos;
            if (supportScrollBar)
            {
                if (_isVertList)
                {
                    SetItemSize(firstItemIndex, newItem.CachedRectTransform.rect.height, newItem.Padding);
                }
                else
                {
                    SetItemSize(firstItemIndex, newItem.CachedRectTransform.rect.width, newItem.Padding);
                }
            }

            _itemList.Add(newItem);
            UpdateContentSize();
            UpdateAllShownItemsPos();
            UpdateListView(_distanceForRecycle0, _distanceForRecycle1, _distanceForNew0, _distanceForNew1);
            ClearAllTmpRecycledItem();
        }


        void RecycleItemTmp(LoopListViewItem item)
        {
            if (item == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(item.ItemPrefabName))
            {
                return;
            }

            ItemPool pool = null;
            if (_itemPoolDict.TryGetValue(item.ItemPrefabName, out pool) == false)
            {
                return;
            }

            pool.RecycleItem(item);
        }


        void ClearAllTmpRecycledItem()
        {
            int count = _itemPoolList.Count;
            for (int i = 0; i < count; ++i)
            {
                _itemPoolList[i].ClearTmpRecycledItem();
            }
        }


        void RecycleAllItem()
        {
            foreach (LoopListViewItem item in _itemList)
            {
                RecycleItemTmp(item);
            }

            _itemList.Clear();
        }


        void AdjustContainerPivot(RectTransform rtf)
        {
            Vector2 pivot = rtf.pivot;
            if (arrangeType == ListItemArrangeType.BottomToTop)
            {
                pivot.y = 0;
            }
            else if (arrangeType == ListItemArrangeType.TopToBottom)
            {
                pivot.y = 1;
            }
            else if (arrangeType == ListItemArrangeType.LeftToRight)
            {
                pivot.x = 0;
            }
            else if (arrangeType == ListItemArrangeType.RightToLeft)
            {
                pivot.x = 1;
            }

            rtf.pivot = pivot;
        }


        void AdjustPivot(RectTransform rtf)
        {
            Vector2 pivot = rtf.pivot;

            if (arrangeType == ListItemArrangeType.BottomToTop)
            {
                pivot.y = 0;
            }
            else if (arrangeType == ListItemArrangeType.TopToBottom)
            {
                pivot.y = 1;
            }
            else if (arrangeType == ListItemArrangeType.LeftToRight)
            {
                pivot.x = 0;
            }
            else if (arrangeType == ListItemArrangeType.RightToLeft)
            {
                pivot.x = 1;
            }

            rtf.pivot = pivot;
        }

        void AdjustContainerAnchor(RectTransform rtf)
        {
            Vector2 anchorMin = rtf.anchorMin;
            Vector2 anchorMax = rtf.anchorMax;
            if (arrangeType == ListItemArrangeType.BottomToTop)
            {
                anchorMin.y = 0;
                anchorMax.y = 0;
            }
            else if (arrangeType == ListItemArrangeType.TopToBottom)
            {
                anchorMin.y = 1;
                anchorMax.y = 1;
            }
            else if (arrangeType == ListItemArrangeType.LeftToRight)
            {
                anchorMin.x = 0;
                anchorMax.x = 0;
            }
            else if (arrangeType == ListItemArrangeType.RightToLeft)
            {
                anchorMin.x = 1;
                anchorMax.x = 1;
            }

            rtf.anchorMin = anchorMin;
            rtf.anchorMax = anchorMax;
        }


        void AdjustAnchor(RectTransform rtf)
        {
            Vector2 anchorMin = rtf.anchorMin;
            Vector2 anchorMax = rtf.anchorMax;
            if (arrangeType == ListItemArrangeType.BottomToTop)
            {
                anchorMin.y = 0;
                anchorMax.y = 0;
            }
            else if (arrangeType == ListItemArrangeType.TopToBottom)
            {
                anchorMin.y = 1;
                anchorMax.y = 1;
            }
            else if (arrangeType == ListItemArrangeType.LeftToRight)
            {
                anchorMin.x = 0;
                anchorMax.x = 0;
            }
            else if (arrangeType == ListItemArrangeType.RightToLeft)
            {
                anchorMin.x = 1;
                anchorMax.x = 1;
            }

            rtf.anchorMin = anchorMin;
            rtf.anchorMax = anchorMax;
        }

        void InitItemPool()
        {
            foreach (ItemPrefabConfData data in itemPrefabDataList)
            {
                if (data.mItemPrefab == null)
                {
                    Debug.LogError("A item prefab is null ");
                    continue;
                }

                string prefabName = data.mItemPrefab.name;
                if (_itemPoolDict.ContainsKey(prefabName))
                {
                    Debug.LogError("A item prefab with name " + prefabName + " has existed!");
                    continue;
                }

                RectTransform rtf = data.mItemPrefab.GetComponent<RectTransform>();
                if (rtf == null)
                {
                    Debug.LogError("RectTransform component is not found in the prefab " + prefabName);
                    continue;
                }

                AdjustAnchor(rtf);
                AdjustPivot(rtf);
                LoopListViewItem tItem = data.mItemPrefab.GetComponent<LoopListViewItem>();
                if (tItem == null)
                {
                    data.mItemPrefab.AddComponent<LoopListViewItem>();
                }

                ItemPool pool = new ItemPool();
                pool.Init(data.mItemPrefab, data.mStartPosOffset, data.mInitCreateCount,
                    _containerTrans);
                _itemPoolDict.Add(prefabName, pool);
                _itemPoolList.Add(pool);
            }
        }


        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            _isDraging = true;
            CacheDragPointerEventData(eventData);
            _curSnapData.Clear();
            if (OnBeginDragAction != null)
            {
                OnBeginDragAction();
            }
        }

        public virtual void OnEndDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            _isDraging = false;
            _pointerEventData = null;
            if (OnEndDragAction != null)
            {
                OnEndDragAction();
            }

            ForceSnapUpdateCheck();
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            CacheDragPointerEventData(eventData);
            if (OnDragingAction != null)
            {
                OnDragingAction();
            }
        }

        void CacheDragPointerEventData(PointerEventData eventData)
        {
            if (_pointerEventData == null)
            {
                _pointerEventData = new PointerEventData(EventSystem.current);
            }

            _pointerEventData.button = eventData.button;
            _pointerEventData.position = eventData.position;
            _pointerEventData.pointerPressRaycast = eventData.pointerPressRaycast;
            _pointerEventData.pointerCurrentRaycast = eventData.pointerCurrentRaycast;
        }

        LoopListViewItem GetNewItemByIndex(int index)
        {
            if (supportScrollBar && index < 0)
            {
                return null;
            }

            if (_itemTotalCount > 0 && index >= _itemTotalCount)
            {
                return null;
            }

            LoopListViewItem newItem = _onGetItemByIndex(this, index);
            if (newItem == null)
            {
                return null;
            }

            newItem.ItemIndex = index;
            newItem.ItemCreatedCheckFrameCount = _listUpdateCheckFrameCount;
            return newItem;
        }


        void SetItemSize(int itemIndex, float itemSize, float padding)
        {
            _itemPosMgr.SetItemSize(itemIndex, itemSize + padding);
            if (itemIndex >= _lastItemIndex)
            {
                _lastItemIndex = itemIndex;
                _lastItemPadding = padding;
            }
        }

        void GetPlusItemIndexAndPosAtGivenPos(float pos, ref int index, ref float itemPos)
        {
            _itemPosMgr.GetItemIndexAndPosAtGivenPos(pos, ref index, ref itemPos);
        }


        float GetItemPos(int itemIndex)
        {
            return _itemPosMgr.GetItemPos(itemIndex);
        }


        public Vector3 GetItemCornerPosInViewPort(LoopListViewItem item,
            ItemCornerEnum corner = ItemCornerEnum.LeftBottom)
        {
            item.CachedRectTransform.GetWorldCorners(_itemWorldCorners);
            return _viewPortRectTransform.InverseTransformPoint(_itemWorldCorners[(int)corner]);
        }


        void AdjustPanelPos()
        {
            int count = _itemList.Count;
            if (count == 0)
            {
                return;
            }

            UpdateAllShownItemsPos();
            float viewPortSize = ViewPortSize;
            float contentSize = GetContentPanelSize();
            if (arrangeType == ListItemArrangeType.TopToBottom)
            {
                if (contentSize <= viewPortSize)
                {
                    Vector3 pos = _containerTrans.localPosition;
                    pos.y = 0;
                    _containerTrans.localPosition = pos;
                    _itemList[0].CachedRectTransform.localPosition = new Vector3(_itemList[0].StartPosOffset, 0, 0);
                    UpdateAllShownItemsPos();
                    return;
                }

                LoopListViewItem tViewItem0 = _itemList[0];
                tViewItem0.CachedRectTransform.GetWorldCorners(_itemWorldCorners);
                Vector3 topPos0 = _viewPortRectTransform.InverseTransformPoint(_itemWorldCorners[1]);
                if (topPos0.y < _viewPortRectLocalCorners[1].y)
                {
                    Vector3 pos = _containerTrans.localPosition;
                    pos.y = 0;
                    _containerTrans.localPosition = pos;
                    _itemList[0].CachedRectTransform.localPosition = new Vector3(_itemList[0].StartPosOffset, 0, 0);
                    UpdateAllShownItemsPos();
                    return;
                }

                LoopListViewItem tViewItem1 = _itemList[^1];
                tViewItem1.CachedRectTransform.GetWorldCorners(_itemWorldCorners);
                Vector3 downPos1 = _viewPortRectTransform.InverseTransformPoint(_itemWorldCorners[0]);
                float d = downPos1.y - _viewPortRectLocalCorners[0].y;
                if (d > 0)
                {
                    Vector3 pos = _itemList[0].CachedRectTransform.localPosition;
                    pos.y = pos.y - d;
                    _itemList[0].CachedRectTransform.localPosition = pos;
                    UpdateAllShownItemsPos();
                    return;
                }
            }
            else if (arrangeType == ListItemArrangeType.BottomToTop)
            {
                if (contentSize <= viewPortSize)
                {
                    Vector3 pos = _containerTrans.localPosition;
                    pos.y = 0;
                    _containerTrans.localPosition = pos;
                    _itemList[0].CachedRectTransform.localPosition = new Vector3(_itemList[0].StartPosOffset, 0, 0);
                    UpdateAllShownItemsPos();
                    return;
                }

                LoopListViewItem tViewItem0 = _itemList[0];
                tViewItem0.CachedRectTransform.GetWorldCorners(_itemWorldCorners);
                Vector3 downPos0 = _viewPortRectTransform.InverseTransformPoint(_itemWorldCorners[0]);
                if (downPos0.y > _viewPortRectLocalCorners[0].y)
                {
                    Vector3 pos = _containerTrans.localPosition;
                    pos.y = 0;
                    _containerTrans.localPosition = pos;
                    _itemList[0].CachedRectTransform.localPosition = new Vector3(_itemList[0].StartPosOffset, 0, 0);
                    UpdateAllShownItemsPos();
                    return;
                }

                LoopListViewItem tViewItem1 = _itemList[^1];
                tViewItem1.CachedRectTransform.GetWorldCorners(_itemWorldCorners);
                Vector3 topPos1 = _viewPortRectTransform.InverseTransformPoint(_itemWorldCorners[1]);
                float d = _viewPortRectLocalCorners[1].y - topPos1.y;
                if (d > 0)
                {
                    Vector3 pos = _itemList[0].CachedRectTransform.localPosition;
                    pos.y = pos.y + d;
                    _itemList[0].CachedRectTransform.localPosition = pos;
                    UpdateAllShownItemsPos();
                    return;
                }
            }
            else if (arrangeType == ListItemArrangeType.LeftToRight)
            {
                if (contentSize <= viewPortSize)
                {
                    Vector3 pos = _containerTrans.localPosition;
                    pos.x = 0;
                    _containerTrans.localPosition = pos;
                    _itemList[0].CachedRectTransform.localPosition = new Vector3(0, _itemList[0].StartPosOffset, 0);
                    UpdateAllShownItemsPos();
                    return;
                }

                LoopListViewItem tViewItem0 = _itemList[0];
                tViewItem0.CachedRectTransform.GetWorldCorners(_itemWorldCorners);
                Vector3 leftPos0 = _viewPortRectTransform.InverseTransformPoint(_itemWorldCorners[1]);
                if (leftPos0.x > _viewPortRectLocalCorners[1].x)
                {
                    Vector3 pos = _containerTrans.localPosition;
                    pos.x = 0;
                    _containerTrans.localPosition = pos;
                    _itemList[0].CachedRectTransform.localPosition = new Vector3(0, _itemList[0].StartPosOffset, 0);
                    UpdateAllShownItemsPos();
                    return;
                }

                LoopListViewItem tViewItem1 = _itemList[^1];
                tViewItem1.CachedRectTransform.GetWorldCorners(_itemWorldCorners);
                Vector3 rightPos1 = _viewPortRectTransform.InverseTransformPoint(_itemWorldCorners[2]);
                float d = _viewPortRectLocalCorners[2].x - rightPos1.x;
                if (d > 0)
                {
                    Vector3 pos = _itemList[0].CachedRectTransform.localPosition;
                    pos.x = pos.x + d;
                    _itemList[0].CachedRectTransform.localPosition = pos;
                    UpdateAllShownItemsPos();
                    return;
                }
            }
            else if (arrangeType == ListItemArrangeType.RightToLeft)
            {
                if (contentSize <= viewPortSize)
                {
                    Vector3 pos = _containerTrans.localPosition;
                    pos.x = 0;
                    _containerTrans.localPosition = pos;
                    _itemList[0].CachedRectTransform.localPosition = new Vector3(0, _itemList[0].StartPosOffset, 0);
                    UpdateAllShownItemsPos();
                    return;
                }

                LoopListViewItem tViewItem0 = _itemList[0];
                tViewItem0.CachedRectTransform.GetWorldCorners(_itemWorldCorners);
                Vector3 rightPos0 = _viewPortRectTransform.InverseTransformPoint(_itemWorldCorners[2]);
                if (rightPos0.x < _viewPortRectLocalCorners[2].x)
                {
                    Vector3 pos = _containerTrans.localPosition;
                    pos.x = 0;
                    _containerTrans.localPosition = pos;
                    _itemList[0].CachedRectTransform.localPosition = new Vector3(0, _itemList[0].StartPosOffset, 0);
                    UpdateAllShownItemsPos();
                    return;
                }

                LoopListViewItem tViewItem1 = _itemList[^1];
                tViewItem1.CachedRectTransform.GetWorldCorners(_itemWorldCorners);
                Vector3 leftPos1 = _viewPortRectTransform.InverseTransformPoint(_itemWorldCorners[1]);
                float d = leftPos1.x - _viewPortRectLocalCorners[1].x;
                if (d > 0)
                {
                    Vector3 pos = _itemList[0].CachedRectTransform.localPosition;
                    pos.x = pos.x - d;
                    _itemList[0].CachedRectTransform.localPosition = pos;
                    UpdateAllShownItemsPos();
                    return;
                }
            }
        }


        void Update()
        {
            if (_listViewInited == false)
            {
                return;
            }

            if (_needAdjustVec)
            {
                _needAdjustVec = false;
                if (_isVertList)
                {
                    if (_scrollRect.velocity.y * _adjustedVec.y > 0)
                    {
                        _scrollRect.velocity = _adjustedVec;
                    }
                }
                else
                {
                    if (_scrollRect.velocity.x * _adjustedVec.x > 0)
                    {
                        _scrollRect.velocity = _adjustedVec;
                    }
                }
            }

            if (supportScrollBar)
            {
                _itemPosMgr.Update(false);
            }

            UpdateSnapMove();
            UpdateListView(_distanceForRecycle0, _distanceForRecycle1, _distanceForNew0, _distanceForNew1);
            ClearAllTmpRecycledItem();
            _lastFrameContainerPos = _containerTrans.localPosition;
        }

        //update snap move. if immediate is set true, then the snap move will finish at once.
        void UpdateSnapMove(bool immediate = false)
        {
            if (itemSnapEnable == false)
            {
                return;
            }

            if (_isVertList)
            {
                UpdateSnapVertical(immediate);
            }
            else
            {
                UpdateSnapHorizontal(immediate);
            }
        }


        public void UpdateAllShownItemSnapData()
        {
            if (itemSnapEnable == false)
            {
                return;
            }

            int count = _itemList.Count;
            if (count == 0)
            {
                return;
            }

            Vector3 pos = _containerTrans.localPosition;
            LoopListViewItem tViewItem0 = _itemList[0];
            tViewItem0.CachedRectTransform.GetWorldCorners(_itemWorldCorners);
            float start = 0;
            float end = 0;
            float itemSnapCenter = 0;
            float snapCenter = 0;
            if (arrangeType == ListItemArrangeType.TopToBottom)
            {
                snapCenter = -(1 - viewPortSnapPivot.y) * _viewPortRectTransform.rect.height;
                Vector3 topPos1 = _viewPortRectTransform.InverseTransformPoint(_itemWorldCorners[1]);
                start = topPos1.y;
                end = start - tViewItem0.ItemSizeWithPadding;
                itemSnapCenter = start - tViewItem0.ItemSize * (1 - itemSnapPivot.y);
                for (int i = 0; i < count; ++i)
                {
                    _itemList[i].DistanceWithViewPortSnapCenter = snapCenter - itemSnapCenter;
                    if ((i + 1) < count)
                    {
                        start = end;
                        end = end - _itemList[i + 1].ItemSizeWithPadding;
                        itemSnapCenter = start - _itemList[i + 1].ItemSize * (1 - itemSnapPivot.y);
                    }
                }
            }
            else if (arrangeType == ListItemArrangeType.BottomToTop)
            {
                snapCenter = viewPortSnapPivot.y * _viewPortRectTransform.rect.height;
                Vector3 bottomPos1 = _viewPortRectTransform.InverseTransformPoint(_itemWorldCorners[0]);
                start = bottomPos1.y;
                end = start + tViewItem0.ItemSizeWithPadding;
                itemSnapCenter = start + tViewItem0.ItemSize * itemSnapPivot.y;
                for (int i = 0; i < count; ++i)
                {
                    _itemList[i].DistanceWithViewPortSnapCenter = snapCenter - itemSnapCenter;
                    if ((i + 1) < count)
                    {
                        start = end;
                        end = end + _itemList[i + 1].ItemSizeWithPadding;
                        itemSnapCenter = start + _itemList[i + 1].ItemSize * itemSnapPivot.y;
                    }
                }
            }
            else if (arrangeType == ListItemArrangeType.RightToLeft)
            {
                snapCenter = -(1 - viewPortSnapPivot.x) * _viewPortRectTransform.rect.width;
                Vector3 rightPos1 = _viewPortRectTransform.InverseTransformPoint(_itemWorldCorners[2]);
                start = rightPos1.x;
                end = start - tViewItem0.ItemSizeWithPadding;
                itemSnapCenter = start - tViewItem0.ItemSize * (1 - itemSnapPivot.x);
                for (int i = 0; i < count; ++i)
                {
                    _itemList[i].DistanceWithViewPortSnapCenter = snapCenter - itemSnapCenter;
                    if ((i + 1) < count)
                    {
                        start = end;
                        end = end - _itemList[i + 1].ItemSizeWithPadding;
                        itemSnapCenter = start - _itemList[i + 1].ItemSize * (1 - itemSnapPivot.x);
                    }
                }
            }
            else if (arrangeType == ListItemArrangeType.LeftToRight)
            {
                snapCenter = viewPortSnapPivot.x * _viewPortRectTransform.rect.width;
                Vector3 leftPos1 = _viewPortRectTransform.InverseTransformPoint(_itemWorldCorners[1]);
                start = leftPos1.x;
                end = start + tViewItem0.ItemSizeWithPadding;
                itemSnapCenter = start + tViewItem0.ItemSize * itemSnapPivot.x;
                for (int i = 0; i < count; ++i)
                {
                    _itemList[i].DistanceWithViewPortSnapCenter = snapCenter - itemSnapCenter;
                    if ((i + 1) < count)
                    {
                        start = end;
                        end = end + _itemList[i + 1].ItemSizeWithPadding;
                        itemSnapCenter = start + _itemList[i + 1].ItemSize * itemSnapPivot.x;
                    }
                }
            }
        }


        void UpdateSnapVertical(bool immediate = false)
        {
            if (itemSnapEnable == false)
            {
                return;
            }

            int count = _itemList.Count;
            if (count == 0)
            {
                return;
            }

            Vector3 pos = _containerTrans.localPosition;
            bool needCheck = Math.Abs(pos.y - lastSnapCheckPos.y) > 0.001f;
            lastSnapCheckPos = pos;
            if (!needCheck)
            {
                if (_leftSnapUpdateExtraCount > 0)
                {
                    _leftSnapUpdateExtraCount--;
                    needCheck = true;
                }
            }

            if (needCheck)
            {
                LoopListViewItem tViewItem0 = _itemList[0];
                tViewItem0.CachedRectTransform.GetWorldCorners(_itemWorldCorners);
                int curIndex = -1;
                float start = 0;
                float end = 0;
                float itemSnapCenter = 0;
                float curMinDist = float.MaxValue;
                float curDist = 0;
                float curDistAbs = 0;
                float snapCenter = 0;
                if (arrangeType == ListItemArrangeType.TopToBottom)
                {
                    snapCenter = -(1 - viewPortSnapPivot.y) * _viewPortRectTransform.rect.height;
                    Vector3 topPos1 = _viewPortRectTransform.InverseTransformPoint(_itemWorldCorners[1]);
                    start = topPos1.y;
                    end = start - tViewItem0.ItemSizeWithPadding;
                    itemSnapCenter = start - tViewItem0.ItemSize * (1 - itemSnapPivot.y);
                    for (int i = 0; i < count; ++i)
                    {
                        curDist = snapCenter - itemSnapCenter;
                        curDistAbs = Mathf.Abs(curDist);
                        if (curDistAbs < curMinDist)
                        {
                            curMinDist = curDistAbs;
                            curIndex = i;
                        }
                        else
                        {
                            break;
                        }

                        if ((i + 1) < count)
                        {
                            start = end;
                            end = end - _itemList[i + 1].ItemSizeWithPadding;
                            itemSnapCenter = start - _itemList[i + 1].ItemSize * (1 - itemSnapPivot.y);
                        }
                    }
                }
                else if (arrangeType == ListItemArrangeType.BottomToTop)
                {
                    snapCenter = viewPortSnapPivot.y * _viewPortRectTransform.rect.height;
                    Vector3 bottomPos1 = _viewPortRectTransform.InverseTransformPoint(_itemWorldCorners[0]);
                    start = bottomPos1.y;
                    end = start + tViewItem0.ItemSizeWithPadding;
                    itemSnapCenter = start + tViewItem0.ItemSize * itemSnapPivot.y;
                    for (int i = 0; i < count; ++i)
                    {
                        curDist = snapCenter - itemSnapCenter;
                        curDistAbs = Mathf.Abs(curDist);
                        if (curDistAbs < curMinDist)
                        {
                            curMinDist = curDistAbs;
                            curIndex = i;
                        }
                        else
                        {
                            break;
                        }

                        if ((i + 1) < count)
                        {
                            start = end;
                            end = end + _itemList[i + 1].ItemSizeWithPadding;
                            itemSnapCenter = start + _itemList[i + 1].ItemSize * itemSnapPivot.y;
                        }
                    }
                }

                if (curIndex >= 0)
                {
                    int oldNearestItemIndex = _curSnapNearestItemIndex;
                    _curSnapNearestItemIndex = _itemList[curIndex].ItemIndex;
                    if (_itemList[curIndex].ItemIndex != oldNearestItemIndex)
                    {
                        if (OnSnapNearestChanged != null)
                        {
                            OnSnapNearestChanged(this, _itemList[curIndex]);
                        }
                    }
                }
                else
                {
                    _curSnapNearestItemIndex = -1;
                }
            }

            bool canSnap = true;
            if (_scrollBarClickEventListener != null)
            {
                canSnap = !(_scrollBarClickEventListener.IsPressed);
            }

            float v = Mathf.Abs(_scrollRect.velocity.y);
            if (canSnap && !_isDraging && v < _snapVecThreshold)
            {
                UpdateCurSnapData();
                if (_curSnapData.SnapStatus != SnapStatus.SnapMoving)
                {
                    return;
                }

                if (v > 0)
                {
                    _scrollRect.StopMovement();
                }

                float old = _curSnapData.CurSnapVal;
                _curSnapData.CurSnapVal = Mathf.SmoothDamp(_curSnapData.CurSnapVal, _curSnapData.TargetSnapVal,
                    ref _smoothDumpVel, _smoothDumpRate);
                float dt = _curSnapData.CurSnapVal - old;

                if (immediate || Mathf.Abs(_curSnapData.TargetSnapVal - _curSnapData.CurSnapVal) <
                    _snapFinishThreshold)
                {
                    pos.y = pos.y + _curSnapData.TargetSnapVal - _curSnapData.CurSnapVal;
                    _curSnapData.SnapStatus = SnapStatus.SnapMoveFinish;
                    if (OnSnapItemFinished != null)
                    {
                        LoopListViewItem targetItem = GetShownItemByItemIndex(_curSnapNearestItemIndex);
                        if (targetItem != null)
                        {
                            OnSnapItemFinished(this, targetItem);
                        }
                    }
                }
                else
                {
                    pos.y = pos.y + dt;
                }

                if (arrangeType == ListItemArrangeType.TopToBottom)
                {
                    float maxY = _viewPortRectLocalCorners[0].y + _containerTrans.rect.height;
                    if (pos.y <= maxY && pos.y >= 0)
                    {
                        _containerTrans.localPosition = pos;
                    }
                }
                else if (arrangeType == ListItemArrangeType.BottomToTop)
                {
                    float minY = _viewPortRectLocalCorners[1].y - _containerTrans.rect.height;
                    if (pos.y >= minY && pos.y <= 0)
                    {
                        _containerTrans.localPosition = pos;
                    }
                }
            }
        }


        void UpdateCurSnapData()
        {
            int count = _itemList.Count;
            if (count == 0)
            {
                _curSnapData.Clear();
                return;
            }

            if (_curSnapData.SnapStatus == SnapStatus.SnapMoveFinish)
            {
                if (_curSnapData.SnapTargetIndex == _curSnapNearestItemIndex)
                {
                    return;
                }

                _curSnapData.SnapStatus = SnapStatus.NoTargetSet;
            }

            if (_curSnapData.SnapStatus == SnapStatus.SnapMoving)
            {
                if ((_curSnapData.SnapTargetIndex == _curSnapNearestItemIndex) || _curSnapData.IsForceSnapTo)
                {
                    return;
                }

                _curSnapData.SnapStatus = SnapStatus.NoTargetSet;
            }

            if (_curSnapData.SnapStatus == SnapStatus.NoTargetSet)
            {
                LoopListViewItem nearestItem = GetShownItemByItemIndex(_curSnapNearestItemIndex);
                if (nearestItem == null)
                {
                    return;
                }

                _curSnapData.SnapTargetIndex = _curSnapNearestItemIndex;
                _curSnapData.SnapStatus = SnapStatus.TargetHasSet;
                _curSnapData.IsForceSnapTo = false;
            }

            if (_curSnapData.SnapStatus == SnapStatus.TargetHasSet)
            {
                LoopListViewItem targetItem = GetShownItemByItemIndex(_curSnapData.SnapTargetIndex);
                if (targetItem == null)
                {
                    _curSnapData.Clear();
                    return;
                }

                UpdateAllShownItemSnapData();
                _curSnapData.TargetSnapVal = targetItem.DistanceWithViewPortSnapCenter;
                _curSnapData.CurSnapVal = 0;
                _curSnapData.SnapStatus = SnapStatus.SnapMoving;
            }
        }

        //Clear current snap target and then the LoopScrollView2 will auto snap to the CurSnapNearestItemIndex.
        public void ClearSnapData()
        {
            _curSnapData.Clear();
        }

        public void SetSnapTargetItemIndex(int itemIndex)
        {
            _curSnapData.SnapTargetIndex = itemIndex;
            _curSnapData.SnapStatus = SnapStatus.TargetHasSet;
            _curSnapData.IsForceSnapTo = true;
        }

        //Get the nearest item index with the viewport snap point.
        public int CurSnapNearestItemIndex => _curSnapNearestItemIndex;

        public void ForceSnapUpdateCheck()
        {
            if (_leftSnapUpdateExtraCount <= 0)
            {
                _leftSnapUpdateExtraCount = 1;
            }
        }

        void UpdateSnapHorizontal(bool immediate = false)
        {
            if (itemSnapEnable == false)
            {
                return;
            }

            int count = _itemList.Count;
            if (count == 0)
            {
                return;
            }

            Vector3 pos = _containerTrans.localPosition;
            bool needCheck = (pos.x != lastSnapCheckPos.x);
            lastSnapCheckPos = pos;
            if (!needCheck)
            {
                if (_leftSnapUpdateExtraCount > 0)
                {
                    _leftSnapUpdateExtraCount--;
                    needCheck = true;
                }
            }

            if (needCheck)
            {
                LoopListViewItem tViewItem0 = _itemList[0];
                tViewItem0.CachedRectTransform.GetWorldCorners(_itemWorldCorners);
                int curIndex = -1;
                float start = 0;
                float end = 0;
                float itemSnapCenter = 0;
                float curMinDist = float.MaxValue;
                float curDist = 0;
                float curDistAbs = 0;
                float snapCenter = 0;
                if (arrangeType == ListItemArrangeType.RightToLeft)
                {
                    snapCenter = -(1 - viewPortSnapPivot.x) * _viewPortRectTransform.rect.width;
                    Vector3 rightPos1 = _viewPortRectTransform.InverseTransformPoint(_itemWorldCorners[2]);
                    start = rightPos1.x;
                    end = start - tViewItem0.ItemSizeWithPadding;
                    itemSnapCenter = start - tViewItem0.ItemSize * (1 - itemSnapPivot.x);
                    for (int i = 0; i < count; ++i)
                    {
                        curDist = snapCenter - itemSnapCenter;
                        curDistAbs = Mathf.Abs(curDist);
                        if (curDistAbs < curMinDist)
                        {
                            curMinDist = curDistAbs;
                            curIndex = i;
                        }
                        else
                        {
                            break;
                        }

                        if ((i + 1) < count)
                        {
                            start = end;
                            end = end - _itemList[i + 1].ItemSizeWithPadding;
                            itemSnapCenter = start - _itemList[i + 1].ItemSize * (1 - itemSnapPivot.x);
                        }
                    }
                }
                else if (arrangeType == ListItemArrangeType.LeftToRight)
                {
                    snapCenter = viewPortSnapPivot.x * _viewPortRectTransform.rect.width;
                    Vector3 leftPos1 = _viewPortRectTransform.InverseTransformPoint(_itemWorldCorners[1]);
                    start = leftPos1.x;
                    end = start + tViewItem0.ItemSizeWithPadding;
                    itemSnapCenter = start + tViewItem0.ItemSize * itemSnapPivot.x;
                    for (int i = 0; i < count; ++i)
                    {
                        curDist = snapCenter - itemSnapCenter;
                        curDistAbs = Mathf.Abs(curDist);
                        if (curDistAbs < curMinDist)
                        {
                            curMinDist = curDistAbs;
                            curIndex = i;
                        }
                        else
                        {
                            break;
                        }

                        if ((i + 1) < count)
                        {
                            start = end;
                            end = end + _itemList[i + 1].ItemSizeWithPadding;
                            itemSnapCenter = start + _itemList[i + 1].ItemSize * itemSnapPivot.x;
                        }
                    }
                }


                if (curIndex >= 0)
                {
                    int oldNearestItemIndex = _curSnapNearestItemIndex;
                    _curSnapNearestItemIndex = _itemList[curIndex].ItemIndex;
                    if (_itemList[curIndex].ItemIndex != oldNearestItemIndex)
                    {
                        if (OnSnapNearestChanged != null)
                        {
                            OnSnapNearestChanged(this, _itemList[curIndex]);
                        }
                    }
                }
                else
                {
                    _curSnapNearestItemIndex = -1;
                }
            }

            bool canSnap = true;
            if (_scrollBarClickEventListener != null)
            {
                canSnap = !(_scrollBarClickEventListener.IsPressed);
            }

            float v = Mathf.Abs(_scrollRect.velocity.x);
            if (canSnap && !_isDraging && v < _snapVecThreshold)
            {
                UpdateCurSnapData();
                if (_curSnapData.SnapStatus != SnapStatus.SnapMoving)
                {
                    return;
                }

                if (v > 0)
                {
                    _scrollRect.StopMovement();
                }

                float old = _curSnapData.CurSnapVal;
                _curSnapData.CurSnapVal = Mathf.SmoothDamp(_curSnapData.CurSnapVal, _curSnapData.TargetSnapVal,
                    ref _smoothDumpVel, _smoothDumpRate);
                float dt = _curSnapData.CurSnapVal - old;

                if (immediate || Mathf.Abs(_curSnapData.TargetSnapVal - _curSnapData.CurSnapVal) <
                    _snapFinishThreshold)
                {
                    pos.x = pos.x + _curSnapData.TargetSnapVal - _curSnapData.CurSnapVal;
                    _curSnapData.SnapStatus = SnapStatus.SnapMoveFinish;
                    if (OnSnapItemFinished != null)
                    {
                        LoopListViewItem targetItem = GetShownItemByItemIndex(_curSnapNearestItemIndex);
                        if (targetItem != null)
                        {
                            OnSnapItemFinished(this, targetItem);
                        }
                    }
                }
                else
                {
                    pos.x = pos.x + dt;
                }

                if (arrangeType == ListItemArrangeType.LeftToRight)
                {
                    float minX = _viewPortRectLocalCorners[2].x - _containerTrans.rect.width;
                    if (pos.x >= minX && pos.x <= 0)
                    {
                        _containerTrans.localPosition = pos;
                    }
                }
                else if (arrangeType == ListItemArrangeType.RightToLeft)
                {
                    float maxX = _viewPortRectLocalCorners[1].x + _containerTrans.rect.width;
                    if (pos.x <= maxX && pos.x >= 0)
                    {
                        _containerTrans.localPosition = pos;
                    }
                }
            }
        }


        public void UpdateListView(float distanceForRecycle0, float distanceForRecycle1, float distanceForNew0,
            float distanceForNew1)
        {
            _listUpdateCheckFrameCount++;
            if (_isVertList)
            {
                bool needContinueCheck = true;
                int checkCount = 0;
                int maxCount = 9999;
                while (needContinueCheck)
                {
                    checkCount++;
                    if (checkCount >= maxCount)
                    {
                        Debug.LogError("UpdateListView Vertical while loop " + checkCount +
                                       " times! something is wrong!");
                        break;
                    }

                    needContinueCheck = UpdateForVertList(distanceForRecycle0, distanceForRecycle1, distanceForNew0,
                        distanceForNew1);
                }
            }
            else
            {
                bool needContinueCheck = true;
                int checkCount = 0;
                int maxCount = 9999;
                while (needContinueCheck)
                {
                    checkCount++;
                    if (checkCount >= maxCount)
                    {
                        Debug.LogError("UpdateListView  Horizontal while loop " + checkCount +
                                       " times! something is wrong!");
                        break;
                    }

                    needContinueCheck = UpdateForHorizontalList(distanceForRecycle0, distanceForRecycle1,
                        distanceForNew0, distanceForNew1);
                }
            }
        }


        bool UpdateForVertList(float distanceForRecycle0, float distanceForRecycle1, float distanceForNew0,
            float distanceForNew1)
        {
            if (_itemTotalCount == 0)
            {
                if (_itemList.Count > 0)
                {
                    RecycleAllItem();
                }

                return false;
            }

            if (arrangeType == ListItemArrangeType.TopToBottom)
            {
                int itemListCount = _itemList.Count;
                if (itemListCount == 0)
                {
                    float curY = _containerTrans.localPosition.y;
                    if (curY < 0)
                    {
                        curY = 0;
                    }

                    int index = 0;
                    float pos = -curY;
                    if (supportScrollBar)
                    {
                        GetPlusItemIndexAndPosAtGivenPos(curY, ref index, ref pos);
                        pos = -pos;
                    }

                    LoopListViewItem newItem = GetNewItemByIndex(index);
                    if (newItem == null)
                    {
                        return false;
                    }

                    if (supportScrollBar)
                    {
                        SetItemSize(index, newItem.CachedRectTransform.rect.height, newItem.Padding);
                    }

                    _itemList.Add(newItem);
                    newItem.CachedRectTransform.localPosition = new Vector3(newItem.StartPosOffset, pos, 0);
                    UpdateContentSize();
                    return true;
                }

                LoopListViewItem tViewItem0 = _itemList[0];
                tViewItem0.CachedRectTransform.GetWorldCorners(_itemWorldCorners);
                Vector3 topPos0 = _viewPortRectTransform.InverseTransformPoint(_itemWorldCorners[1]);
                Vector3 downPos0 = _viewPortRectTransform.InverseTransformPoint(_itemWorldCorners[0]);

                if (!_isDraging && tViewItem0.ItemCreatedCheckFrameCount != _listUpdateCheckFrameCount
                                && downPos0.y - _viewPortRectLocalCorners[1].y > distanceForRecycle0)
                {
                    _itemList.RemoveAt(0);
                    RecycleItemTmp(tViewItem0);
                    if (!supportScrollBar)
                    {
                        UpdateContentSize();
                        CheckIfNeedUpdateItemPos();
                    }

                    return true;
                }

                LoopListViewItem tViewItem1 = _itemList[_itemList.Count - 1];
                tViewItem1.CachedRectTransform.GetWorldCorners(_itemWorldCorners);
                Vector3 topPos1 = _viewPortRectTransform.InverseTransformPoint(_itemWorldCorners[1]);
                Vector3 downPos1 = _viewPortRectTransform.InverseTransformPoint(_itemWorldCorners[0]);
                if (!_isDraging && tViewItem1.ItemCreatedCheckFrameCount != _listUpdateCheckFrameCount
                                && _viewPortRectLocalCorners[0].y - topPos1.y > distanceForRecycle1)
                {
                    _itemList.RemoveAt(_itemList.Count - 1);
                    RecycleItemTmp(tViewItem1);
                    if (!supportScrollBar)
                    {
                        UpdateContentSize();
                        CheckIfNeedUpdateItemPos();
                    }

                    return true;
                }


                if (_viewPortRectLocalCorners[0].y - downPos1.y < distanceForNew1)
                {
                    if (tViewItem1.ItemIndex > _curReadyMaxItemIndex)
                    {
                        _curReadyMaxItemIndex = tViewItem1.ItemIndex;
                        _needCheckNextMaxItem = true;
                    }

                    int nIndex = tViewItem1.ItemIndex + 1;
                    if (nIndex <= _curReadyMaxItemIndex || _needCheckNextMaxItem)
                    {
                        LoopListViewItem newItem = GetNewItemByIndex(nIndex);
                        if (newItem == null)
                        {
                            _curReadyMaxItemIndex = tViewItem1.ItemIndex;
                            _needCheckNextMaxItem = false;
                            CheckIfNeedUpdateItemPos();
                        }
                        else
                        {
                            if (supportScrollBar)
                            {
                                SetItemSize(nIndex, newItem.CachedRectTransform.rect.height, newItem.Padding);
                            }

                            _itemList.Add(newItem);
                            float y = tViewItem1.CachedRectTransform.localPosition.y -
                                      tViewItem1.CachedRectTransform.rect.height - tViewItem1.Padding;
                            newItem.CachedRectTransform.localPosition = new Vector3(newItem.StartPosOffset, y, 0);
                            UpdateContentSize();
                            CheckIfNeedUpdateItemPos();

                            if (nIndex > _curReadyMaxItemIndex)
                            {
                                _curReadyMaxItemIndex = nIndex;
                            }

                            return true;
                        }
                    }
                }

                if (topPos0.y - _viewPortRectLocalCorners[1].y < distanceForNew0)
                {
                    if (tViewItem0.ItemIndex < _curReadyMinItemIndex)
                    {
                        _curReadyMinItemIndex = tViewItem0.ItemIndex;
                        _needCheckNextMinItem = true;
                    }

                    int nIndex = tViewItem0.ItemIndex - 1;
                    if (nIndex >= _curReadyMinItemIndex || _needCheckNextMinItem)
                    {
                        LoopListViewItem newItem = GetNewItemByIndex(nIndex);
                        if (newItem == null)
                        {
                            _curReadyMinItemIndex = tViewItem0.ItemIndex;
                            _needCheckNextMinItem = false;
                        }
                        else
                        {
                            if (supportScrollBar)
                            {
                                SetItemSize(nIndex, newItem.CachedRectTransform.rect.height, newItem.Padding);
                            }

                            _itemList.Insert(0, newItem);
                            float y = tViewItem0.CachedRectTransform.localPosition.y +
                                      newItem.CachedRectTransform.rect.height + newItem.Padding;
                            newItem.CachedRectTransform.localPosition = new Vector3(newItem.StartPosOffset, y, 0);
                            UpdateContentSize();
                            CheckIfNeedUpdateItemPos();
                            if (nIndex < _curReadyMinItemIndex)
                            {
                                _curReadyMinItemIndex = nIndex;
                            }

                            return true;
                        }
                    }
                }
            }
            else
            {
                if (_itemList.Count == 0)
                {
                    float curY = _containerTrans.localPosition.y;
                    if (curY > 0)
                    {
                        curY = 0;
                    }

                    int index = 0;
                    float pos = -curY;
                    if (supportScrollBar)
                    {
                        GetPlusItemIndexAndPosAtGivenPos(-curY, ref index, ref pos);
                    }

                    LoopListViewItem newItem = GetNewItemByIndex(index);
                    if (newItem == null)
                    {
                        return false;
                    }

                    if (supportScrollBar)
                    {
                        SetItemSize(index, newItem.CachedRectTransform.rect.height, newItem.Padding);
                    }

                    _itemList.Add(newItem);
                    newItem.CachedRectTransform.localPosition = new Vector3(newItem.StartPosOffset, pos, 0);
                    UpdateContentSize();
                    return true;
                }

                LoopListViewItem tViewItem0 = _itemList[0];
                tViewItem0.CachedRectTransform.GetWorldCorners(_itemWorldCorners);
                Vector3 topPos0 = _viewPortRectTransform.InverseTransformPoint(_itemWorldCorners[1]);
                Vector3 downPos0 = _viewPortRectTransform.InverseTransformPoint(_itemWorldCorners[0]);

                if (!_isDraging && tViewItem0.ItemCreatedCheckFrameCount != _listUpdateCheckFrameCount
                                && _viewPortRectLocalCorners[0].y - topPos0.y > distanceForRecycle0)
                {
                    _itemList.RemoveAt(0);
                    RecycleItemTmp(tViewItem0);
                    if (!supportScrollBar)
                    {
                        UpdateContentSize();
                        CheckIfNeedUpdateItemPos();
                    }

                    return true;
                }

                LoopListViewItem tViewItem1 = _itemList[_itemList.Count - 1];
                tViewItem1.CachedRectTransform.GetWorldCorners(_itemWorldCorners);
                Vector3 topPos1 = _viewPortRectTransform.InverseTransformPoint(_itemWorldCorners[1]);
                Vector3 downPos1 = _viewPortRectTransform.InverseTransformPoint(_itemWorldCorners[0]);
                if (!_isDraging && tViewItem1.ItemCreatedCheckFrameCount != _listUpdateCheckFrameCount
                                && downPos1.y - _viewPortRectLocalCorners[1].y > distanceForRecycle1)
                {
                    _itemList.RemoveAt(_itemList.Count - 1);
                    RecycleItemTmp(tViewItem1);
                    if (!supportScrollBar)
                    {
                        UpdateContentSize();
                        CheckIfNeedUpdateItemPos();
                    }

                    return true;
                }

                if (topPos1.y - _viewPortRectLocalCorners[1].y < distanceForNew1)
                {
                    if (tViewItem1.ItemIndex > _curReadyMaxItemIndex)
                    {
                        _curReadyMaxItemIndex = tViewItem1.ItemIndex;
                        _needCheckNextMaxItem = true;
                    }

                    int nIndex = tViewItem1.ItemIndex + 1;
                    if (nIndex <= _curReadyMaxItemIndex || _needCheckNextMaxItem)
                    {
                        LoopListViewItem newItem = GetNewItemByIndex(nIndex);
                        if (newItem == null)
                        {
                            _needCheckNextMaxItem = false;
                            CheckIfNeedUpdateItemPos();
                        }
                        else
                        {
                            if (supportScrollBar)
                            {
                                SetItemSize(nIndex, newItem.CachedRectTransform.rect.height, newItem.Padding);
                            }

                            _itemList.Add(newItem);
                            float y = tViewItem1.CachedRectTransform.localPosition.y +
                                      tViewItem1.CachedRectTransform.rect.height + tViewItem1.Padding;
                            newItem.CachedRectTransform.localPosition = new Vector3(newItem.StartPosOffset, y, 0);
                            UpdateContentSize();
                            CheckIfNeedUpdateItemPos();
                            if (nIndex > _curReadyMaxItemIndex)
                            {
                                _curReadyMaxItemIndex = nIndex;
                            }

                            return true;
                        }
                    }
                }


                if (_viewPortRectLocalCorners[0].y - downPos0.y < distanceForNew0)
                {
                    if (tViewItem0.ItemIndex < _curReadyMinItemIndex)
                    {
                        _curReadyMinItemIndex = tViewItem0.ItemIndex;
                        _needCheckNextMinItem = true;
                    }

                    int nIndex = tViewItem0.ItemIndex - 1;
                    if (nIndex >= _curReadyMinItemIndex || _needCheckNextMinItem)
                    {
                        LoopListViewItem newItem = GetNewItemByIndex(nIndex);
                        if (newItem == null)
                        {
                            _needCheckNextMinItem = false;
                            return false;
                        }
                        else
                        {
                            if (supportScrollBar)
                            {
                                SetItemSize(nIndex, newItem.CachedRectTransform.rect.height, newItem.Padding);
                            }

                            _itemList.Insert(0, newItem);
                            float y = tViewItem0.CachedRectTransform.localPosition.y -
                                      newItem.CachedRectTransform.rect.height - newItem.Padding;
                            newItem.CachedRectTransform.localPosition = new Vector3(newItem.StartPosOffset, y, 0);
                            UpdateContentSize();
                            CheckIfNeedUpdateItemPos();
                            if (nIndex < _curReadyMinItemIndex)
                            {
                                _curReadyMinItemIndex = nIndex;
                            }

                            return true;
                        }
                    }
                }
            }

            return false;
        }


        bool UpdateForHorizontalList(float distanceForRecycle0, float distanceForRecycle1, float distanceForNew0,
            float distanceForNew1)
        {
            if (_itemTotalCount == 0)
            {
                if (_itemList.Count > 0)
                {
                    RecycleAllItem();
                }

                return false;
            }

            if (arrangeType == ListItemArrangeType.LeftToRight)
            {
                if (_itemList.Count == 0)
                {
                    float curX = _containerTrans.localPosition.x;
                    if (curX > 0)
                    {
                        curX = 0;
                    }

                    int index = 0;
                    float pos = -curX;
                    if (supportScrollBar)
                    {
                        GetPlusItemIndexAndPosAtGivenPos(-curX, ref index, ref pos);
                    }

                    LoopListViewItem newItem = GetNewItemByIndex(index);
                    if (newItem == null)
                    {
                        return false;
                    }

                    if (supportScrollBar)
                    {
                        SetItemSize(index, newItem.CachedRectTransform.rect.width, newItem.Padding);
                    }

                    _itemList.Add(newItem);
                    newItem.CachedRectTransform.localPosition = new Vector3(pos, newItem.StartPosOffset, 0);
                    UpdateContentSize();
                    return true;
                }

                LoopListViewItem tViewItem0 = _itemList[0];
                tViewItem0.CachedRectTransform.GetWorldCorners(_itemWorldCorners);
                Vector3 leftPos0 = _viewPortRectTransform.InverseTransformPoint(_itemWorldCorners[1]);
                Vector3 rightPos0 = _viewPortRectTransform.InverseTransformPoint(_itemWorldCorners[2]);

                if (!_isDraging && tViewItem0.ItemCreatedCheckFrameCount != _listUpdateCheckFrameCount
                                && _viewPortRectLocalCorners[1].x - rightPos0.x > distanceForRecycle0)
                {
                    _itemList.RemoveAt(0);
                    RecycleItemTmp(tViewItem0);
                    if (!supportScrollBar)
                    {
                        UpdateContentSize();
                        CheckIfNeedUpdateItemPos();
                    }

                    return true;
                }

                LoopListViewItem tViewItem1 = _itemList[_itemList.Count - 1];
                tViewItem1.CachedRectTransform.GetWorldCorners(_itemWorldCorners);
                Vector3 leftPos1 = _viewPortRectTransform.InverseTransformPoint(_itemWorldCorners[1]);
                Vector3 rightPos1 = _viewPortRectTransform.InverseTransformPoint(_itemWorldCorners[2]);
                if (!_isDraging && tViewItem1.ItemCreatedCheckFrameCount != _listUpdateCheckFrameCount
                                && leftPos1.x - _viewPortRectLocalCorners[2].x > distanceForRecycle1)
                {
                    _itemList.RemoveAt(_itemList.Count - 1);
                    RecycleItemTmp(tViewItem1);
                    if (!supportScrollBar)
                    {
                        UpdateContentSize();
                        CheckIfNeedUpdateItemPos();
                    }

                    return true;
                }


                if (rightPos1.x - _viewPortRectLocalCorners[2].x < distanceForNew1)
                {
                    if (tViewItem1.ItemIndex > _curReadyMaxItemIndex)
                    {
                        _curReadyMaxItemIndex = tViewItem1.ItemIndex;
                        _needCheckNextMaxItem = true;
                    }

                    int nIndex = tViewItem1.ItemIndex + 1;
                    if (nIndex <= _curReadyMaxItemIndex || _needCheckNextMaxItem)
                    {
                        LoopListViewItem newItem = GetNewItemByIndex(nIndex);
                        if (newItem == null)
                        {
                            _curReadyMaxItemIndex = tViewItem1.ItemIndex;
                            _needCheckNextMaxItem = false;
                            CheckIfNeedUpdateItemPos();
                        }
                        else
                        {
                            if (supportScrollBar)
                            {
                                SetItemSize(nIndex, newItem.CachedRectTransform.rect.width, newItem.Padding);
                            }

                            _itemList.Add(newItem);
                            float x = tViewItem1.CachedRectTransform.localPosition.x +
                                      tViewItem1.CachedRectTransform.rect.width + tViewItem1.Padding;
                            newItem.CachedRectTransform.localPosition = new Vector3(x, newItem.StartPosOffset, 0);
                            UpdateContentSize();
                            CheckIfNeedUpdateItemPos();

                            if (nIndex > _curReadyMaxItemIndex)
                            {
                                _curReadyMaxItemIndex = nIndex;
                            }

                            return true;
                        }
                    }
                }

                if (_viewPortRectLocalCorners[1].x - leftPos0.x < distanceForNew0)
                {
                    if (tViewItem0.ItemIndex < _curReadyMinItemIndex)
                    {
                        _curReadyMinItemIndex = tViewItem0.ItemIndex;
                        _needCheckNextMinItem = true;
                    }

                    int nIndex = tViewItem0.ItemIndex - 1;
                    if (nIndex >= _curReadyMinItemIndex || _needCheckNextMinItem)
                    {
                        LoopListViewItem newItem = GetNewItemByIndex(nIndex);
                        if (newItem == null)
                        {
                            _curReadyMinItemIndex = tViewItem0.ItemIndex;
                            _needCheckNextMinItem = false;
                        }
                        else
                        {
                            if (supportScrollBar)
                            {
                                SetItemSize(nIndex, newItem.CachedRectTransform.rect.width, newItem.Padding);
                            }

                            _itemList.Insert(0, newItem);
                            float x = tViewItem0.CachedRectTransform.localPosition.x -
                                      newItem.CachedRectTransform.rect.width - newItem.Padding;
                            newItem.CachedRectTransform.localPosition = new Vector3(x, newItem.StartPosOffset, 0);
                            UpdateContentSize();
                            CheckIfNeedUpdateItemPos();
                            if (nIndex < _curReadyMinItemIndex)
                            {
                                _curReadyMinItemIndex = nIndex;
                            }

                            return true;
                        }
                    }
                }
            }
            else
            {
                if (_itemList.Count == 0)
                {
                    float curX = _containerTrans.localPosition.x;
                    if (curX < 0)
                    {
                        curX = 0;
                    }

                    int index = 0;
                    float pos = -curX;
                    if (supportScrollBar)
                    {
                        GetPlusItemIndexAndPosAtGivenPos(curX, ref index, ref pos);
                        pos = -pos;
                    }

                    LoopListViewItem newItem = GetNewItemByIndex(index);
                    if (newItem == null)
                    {
                        return false;
                    }

                    if (supportScrollBar)
                    {
                        SetItemSize(index, newItem.CachedRectTransform.rect.width, newItem.Padding);
                    }

                    _itemList.Add(newItem);
                    newItem.CachedRectTransform.localPosition = new Vector3(pos, newItem.StartPosOffset, 0);
                    UpdateContentSize();
                    return true;
                }

                LoopListViewItem tViewItem0 = _itemList[0];
                tViewItem0.CachedRectTransform.GetWorldCorners(_itemWorldCorners);
                Vector3 leftPos0 = _viewPortRectTransform.InverseTransformPoint(_itemWorldCorners[1]);
                Vector3 rightPos0 = _viewPortRectTransform.InverseTransformPoint(_itemWorldCorners[2]);

                if (!_isDraging && tViewItem0.ItemCreatedCheckFrameCount != _listUpdateCheckFrameCount
                                && leftPos0.x - _viewPortRectLocalCorners[2].x > distanceForRecycle0)
                {
                    _itemList.RemoveAt(0);
                    RecycleItemTmp(tViewItem0);
                    if (!supportScrollBar)
                    {
                        UpdateContentSize();
                        CheckIfNeedUpdateItemPos();
                    }

                    return true;
                }

                LoopListViewItem tViewItem1 = _itemList[_itemList.Count - 1];
                tViewItem1.CachedRectTransform.GetWorldCorners(_itemWorldCorners);
                Vector3 leftPos1 = _viewPortRectTransform.InverseTransformPoint(_itemWorldCorners[1]);
                Vector3 rightPos1 = _viewPortRectTransform.InverseTransformPoint(_itemWorldCorners[2]);
                if (!_isDraging && tViewItem1.ItemCreatedCheckFrameCount != _listUpdateCheckFrameCount
                                && _viewPortRectLocalCorners[1].x - rightPos1.x > distanceForRecycle1)
                {
                    _itemList.RemoveAt(_itemList.Count - 1);
                    RecycleItemTmp(tViewItem1);
                    if (!supportScrollBar)
                    {
                        UpdateContentSize();
                        CheckIfNeedUpdateItemPos();
                    }

                    return true;
                }


                if (_viewPortRectLocalCorners[1].x - leftPos1.x < distanceForNew1)
                {
                    if (tViewItem1.ItemIndex > _curReadyMaxItemIndex)
                    {
                        _curReadyMaxItemIndex = tViewItem1.ItemIndex;
                        _needCheckNextMaxItem = true;
                    }

                    int nIndex = tViewItem1.ItemIndex + 1;
                    if (nIndex <= _curReadyMaxItemIndex || _needCheckNextMaxItem)
                    {
                        LoopListViewItem newItem = GetNewItemByIndex(nIndex);
                        if (newItem == null)
                        {
                            _curReadyMaxItemIndex = tViewItem1.ItemIndex;
                            _needCheckNextMaxItem = false;
                            CheckIfNeedUpdateItemPos();
                        }
                        else
                        {
                            if (supportScrollBar)
                            {
                                SetItemSize(nIndex, newItem.CachedRectTransform.rect.width, newItem.Padding);
                            }

                            _itemList.Add(newItem);
                            float x = tViewItem1.CachedRectTransform.localPosition.x -
                                      tViewItem1.CachedRectTransform.rect.width - tViewItem1.Padding;
                            newItem.CachedRectTransform.localPosition = new Vector3(x, newItem.StartPosOffset, 0);
                            UpdateContentSize();
                            CheckIfNeedUpdateItemPos();

                            if (nIndex > _curReadyMaxItemIndex)
                            {
                                _curReadyMaxItemIndex = nIndex;
                            }

                            return true;
                        }
                    }
                }

                if (rightPos0.x - _viewPortRectLocalCorners[2].x < distanceForNew0)
                {
                    if (tViewItem0.ItemIndex < _curReadyMinItemIndex)
                    {
                        _curReadyMinItemIndex = tViewItem0.ItemIndex;
                        _needCheckNextMinItem = true;
                    }

                    int nIndex = tViewItem0.ItemIndex - 1;
                    if (nIndex >= _curReadyMinItemIndex || _needCheckNextMinItem)
                    {
                        LoopListViewItem newItem = GetNewItemByIndex(nIndex);
                        if (newItem == null)
                        {
                            _curReadyMinItemIndex = tViewItem0.ItemIndex;
                            _needCheckNextMinItem = false;
                        }
                        else
                        {
                            if (supportScrollBar)
                            {
                                SetItemSize(nIndex, newItem.CachedRectTransform.rect.width, newItem.Padding);
                            }

                            _itemList.Insert(0, newItem);
                            float x = tViewItem0.CachedRectTransform.localPosition.x +
                                      newItem.CachedRectTransform.rect.width + newItem.Padding;
                            newItem.CachedRectTransform.localPosition = new Vector3(x, newItem.StartPosOffset, 0);
                            UpdateContentSize();
                            CheckIfNeedUpdateItemPos();
                            if (nIndex < _curReadyMinItemIndex)
                            {
                                _curReadyMinItemIndex = nIndex;
                            }

                            return true;
                        }
                    }
                }
            }

            return false;
        }


        float GetContentPanelSize()
        {
            if (supportScrollBar)
            {
                float tTotalSize = _itemPosMgr.TotalSize > 0 ? (_itemPosMgr.TotalSize - _lastItemPadding) : 0;
                if (tTotalSize < 0)
                {
                    tTotalSize = 0;
                }

                return tTotalSize;
            }

            int count = _itemList.Count;
            if (count == 0)
            {
                return 0;
            }

            if (count == 1)
            {
                return _itemList[0].ItemSize;
            }

            if (count == 2)
            {
                return _itemList[0].ItemSizeWithPadding + _itemList[1].ItemSize;
            }

            float s = 0;
            for (int i = 0; i < count - 1; ++i)
            {
                s += _itemList[i].ItemSizeWithPadding;
            }

            s += _itemList[count - 1].ItemSize;
            return s;
        }


        private void CheckIfNeedUpdateItemPos()
        {
            int count = _itemList.Count;
            if (count == 0)
            {
                return;
            }

            if (arrangeType == ListItemArrangeType.TopToBottom)
            {
                LoopListViewItem firstItem = _itemList[0];
                LoopListViewItem lastItem = _itemList[^1];
                float viewMaxY = GetContentPanelSize();
                if (firstItem.TopY > 0 || (firstItem.ItemIndex == _curReadyMinItemIndex && firstItem.TopY != 0))
                {
                    UpdateAllShownItemsPos();
                    return;
                }

                if ((-lastItem.BottomY) > viewMaxY ||
                    (lastItem.ItemIndex == _curReadyMaxItemIndex && Math.Abs(-lastItem.BottomY - viewMaxY) > 0.001f))
                {
                    UpdateAllShownItemsPos();
                    return;
                }
            }
            else if (arrangeType == ListItemArrangeType.BottomToTop)
            {
                LoopListViewItem firstItem = _itemList[0];
                LoopListViewItem lastItem = _itemList[^1];
                float viewMaxY = GetContentPanelSize();
                if (firstItem.BottomY < 0 || (firstItem.ItemIndex == _curReadyMinItemIndex && firstItem.BottomY != 0))
                {
                    UpdateAllShownItemsPos();
                    return;
                }

                if (lastItem.TopY > viewMaxY ||
                    (lastItem.ItemIndex == _curReadyMaxItemIndex && Math.Abs(lastItem.TopY - viewMaxY) > 0.001f))
                {
                    UpdateAllShownItemsPos();
                    return;
                }
            }
            else if (arrangeType == ListItemArrangeType.LeftToRight)
            {
                LoopListViewItem firstItem = _itemList[0];
                LoopListViewItem lastItem = _itemList[^1];
                float viewMaxX = GetContentPanelSize();
                if (firstItem.LeftX < 0 || (firstItem.ItemIndex == _curReadyMinItemIndex && firstItem.LeftX != 0))
                {
                    UpdateAllShownItemsPos();
                    return;
                }

                if ((lastItem.RightX) > viewMaxX ||
                    (lastItem.ItemIndex == _curReadyMaxItemIndex && Math.Abs(lastItem.RightX - viewMaxX) > 0.001f))
                {
                    UpdateAllShownItemsPos();
                    return;
                }
            }
            else if (arrangeType == ListItemArrangeType.RightToLeft)
            {
                LoopListViewItem firstItem = _itemList[0];
                LoopListViewItem lastItem = _itemList[^1];
                float viewMaxX = GetContentPanelSize();
                if (firstItem.RightX > 0 || (firstItem.ItemIndex == _curReadyMinItemIndex && firstItem.RightX != 0))
                {
                    UpdateAllShownItemsPos();
                    return;
                }

                if ((-lastItem.LeftX) > viewMaxX ||
                    (lastItem.ItemIndex == _curReadyMaxItemIndex && Math.Abs((-lastItem.LeftX) - viewMaxX) > 0.001f))
                {
                    UpdateAllShownItemsPos();
                    return;
                }
            }
        }


        private void UpdateAllShownItemsPos()
        {
            int count = _itemList.Count;
            if (count == 0)
            {
                return;
            }

            _adjustedVec = (_containerTrans.localPosition - _lastFrameContainerPos) / Time.deltaTime;

            if (arrangeType == ListItemArrangeType.TopToBottom)
            {
                float pos = 0;
                if (supportScrollBar)
                {
                    pos = -GetItemPos(_itemList[0].ItemIndex);
                }

                float pos1 = _itemList[0].CachedRectTransform.localPosition.y;
                float d = pos - pos1;
                float curY = pos;
                for (int i = 0; i < count; ++i)
                {
                    LoopListViewItem item = _itemList[i];
                    item.CachedRectTransform.localPosition = new Vector3(item.StartPosOffset, curY, 0);
                    curY = curY - item.CachedRectTransform.rect.height - item.Padding;
                }

                if (d != 0)
                {
                    Vector2 p = _containerTrans.localPosition;
                    p.y = p.y - d;
                    _containerTrans.localPosition = p;
                }
            }
            else if (arrangeType == ListItemArrangeType.BottomToTop)
            {
                float pos = 0;
                if (supportScrollBar)
                {
                    pos = GetItemPos(_itemList[0].ItemIndex);
                }

                float pos1 = _itemList[0].CachedRectTransform.localPosition.y;
                float d = pos - pos1;
                float curY = pos;
                for (int i = 0; i < count; ++i)
                {
                    LoopListViewItem item = _itemList[i];
                    item.CachedRectTransform.localPosition = new Vector3(item.StartPosOffset, curY, 0);
                    curY = curY + item.CachedRectTransform.rect.height + item.Padding;
                }

                if (d != 0)
                {
                    Vector3 p = _containerTrans.localPosition;
                    p.y -= d;
                    _containerTrans.localPosition = p;
                }
            }
            else if (arrangeType == ListItemArrangeType.LeftToRight)
            {
                float pos = 0;
                if (supportScrollBar)
                {
                    pos = GetItemPos(_itemList[0].ItemIndex);
                }

                float pos1 = _itemList[0].CachedRectTransform.localPosition.x;
                float d = pos - pos1;
                float curX = pos;
                for (int i = 0; i < count; ++i)
                {
                    LoopListViewItem item = _itemList[i];
                    item.CachedRectTransform.localPosition = new Vector3(curX, item.StartPosOffset, 0);
                    curX = curX + item.CachedRectTransform.rect.width + item.Padding;
                }

                if (d != 0)
                {
                    Vector3 p = _containerTrans.localPosition;
                    p.x -= d;
                    _containerTrans.localPosition = p;
                }
            }
            else if (arrangeType == ListItemArrangeType.RightToLeft)
            {
                float pos = 0;
                if (supportScrollBar)
                {
                    pos = -GetItemPos(_itemList[0].ItemIndex);
                }

                float pos1 = _itemList[0].CachedRectTransform.localPosition.x;
                float d = pos - pos1;
                float curX = pos;
                for (int i = 0; i < count; ++i)
                {
                    LoopListViewItem item = _itemList[i];
                    item.CachedRectTransform.localPosition = new Vector3(curX, item.StartPosOffset, 0);
                    curX = curX - item.CachedRectTransform.rect.width - item.Padding;
                }

                if (d != 0)
                {
                    Vector3 p = _containerTrans.localPosition;
                    p.x = p.x - d;
                    _containerTrans.localPosition = p;
                }
            }

            if (_isDraging)
            {
                _scrollRect.OnBeginDrag(_pointerEventData);
                _scrollRect.Rebuild(CanvasUpdate.PostLayout);
                _scrollRect.velocity = _adjustedVec;
                _needAdjustVec = true;
            }
        }

        private void UpdateContentSize()
        {
            float size = GetContentPanelSize();
            if (_isVertList)
            {
                if (Math.Abs(_containerTrans.rect.height - size) > 0.001f)
                {
                    _containerTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
                }
            }
            else
            {
                if (Math.Abs(_containerTrans.rect.width - size) > 0.001f)
                {
                    _containerTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
                }
            }
        }

        /// <summary>
        /// 获取当前起始索引
        /// </summary>
        /// <returns></returns>
        public int GetItemStartIndex()
        {
            return _itemList is { Count: > 0 } ? _itemList[0].ItemIndex : 0;
        }
    }
}