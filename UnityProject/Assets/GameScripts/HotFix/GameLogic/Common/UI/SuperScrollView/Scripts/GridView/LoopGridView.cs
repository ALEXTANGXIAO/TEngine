using System.Collections.Generic;
using TEngine;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace GameLogic
{
    [System.Serializable]
    public class GridViewItemPrefabConfData
    {
        public GameObject itemPrefab = null;
        public int initCreateCount = 0;
    }


    public class LoopGridViewInitParam
    {
        // all the default values
        public float SmoothDumpRate = 0.3f;
        public float SnapFinishThreshold = 0.01f;
        public float SnapVecThreshold = 145;

        public static LoopGridViewInitParam CopyDefaultInitParam()
        {
            return new LoopGridViewInitParam();
        }
    }


    public class LoopGridViewSettingParam
    {
        public object ItemSize = null;
        public object Padding = null;
        public object ItemPadding = null;
        public object GridFixedType = null;
        public object FixedRowOrColumnCount = null;
    }


    public class LoopGridView : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        class SnapData
        {
            public SnapStatus SnapStatus = SnapStatus.NoTargetSet;
            public RowColumnPair SnapTarget;
            public Vector2 SnapNeedMoveDir;
            public float TargetSnapVal = 0;
            public float CurSnapVal = 0;
            public bool IsForceSnapTo = false;

            public void Clear()
            {
                SnapStatus = SnapStatus.NoTargetSet;
                IsForceSnapTo = false;
            }
        }

        class ItemRangeData
        {
            public int MaxRow;
            public int MinRow;
            public int MaxColumn;
            public int MinColumn;
            public Vector2 CheckedPosition;
        }

        private readonly Dictionary<string, GridItemPool> _itemPoolDict = new Dictionary<string, GridItemPool>();
        private readonly List<GridItemPool> _itemPoolList = new List<GridItemPool>();
        [SerializeField] private List<GridViewItemPrefabConfData> mItemPrefabDataList = new List<GridViewItemPrefabConfData>();

        [SerializeField] private GridItemArrangeType mArrangeType = GridItemArrangeType.TopLeftToBottomRight;

        public GridItemArrangeType ArrangeType
        {
            get => mArrangeType;
            set => mArrangeType = value;
        }

        private RectTransform _containerTrans;
        private ScrollRect _scrollRect = null;
        private RectTransform _scrollRectTransform = null;
        private RectTransform _viewPortRectTransform = null;
        private int _itemTotalCount = 0;
        [SerializeField] int mFixedRowOrColumnCount = 0;
        [SerializeField] RectOffset mPadding = new RectOffset();
        [SerializeField] Vector2 mItemPadding = Vector2.zero;
        [SerializeField] Vector2 mItemSize = Vector2.zero;
        [SerializeField] Vector2 mItemRecycleDistance = new Vector2(50, 50);
        private Vector2 _itemSizeWithPadding = Vector2.zero;
        private Vector2 _startPadding;
        private Vector2 _endPadding;
        private System.Func<LoopGridView, int, int, int, LoopGridViewItem> _onGetItemByRowColumn;
        private readonly List<GridItemGroup> _itemGroupObjPool = new List<GridItemGroup>();

        //if GridFixedType is GridFixedType.ColumnCountFixed, then the GridItemGroup is one row of the GridView
        //if GridFixedType is GridFixedType.RowCountFixed, then the GridItemGroup is one column of the GridView
        //so mItemGroupList is current all shown rows or columns
        private readonly List<GridItemGroup> _itemGroupList = new List<GridItemGroup>();

        private bool _isDragging = false;
        private int _rowCount = 0;
        private int _columnCount = 0;
        public System.Action<PointerEventData> OnBeginDragAction = null;
        public System.Action<PointerEventData> OnDragingAction = null;
        public System.Action<PointerEventData> OnEndDragAction = null;
        private float _smoothDumpVel = 0;
        private float _smoothDumpRate = 0.3f;
        private float _snapFinishThreshold = 0.1f;
        private float _snapVecThreshold = 145;
        [SerializeField] bool mItemSnapEnable = false;
        [SerializeField] GridFixedType mGridFixedType = GridFixedType.ColumnCountFixed;

        public System.Action<LoopGridView, LoopGridViewItem> OnSnapItemFinished = null;

        //in this callback, use CurSnapNearestItemRowColumn to get cur snaped item row column.
        public System.Action<LoopGridView> OnSnapNearestChanged = null;
        private int _leftSnapUpdateExtraCount = 1;
        [SerializeField] Vector2 mViewPortSnapPivot = Vector2.zero;
        [SerializeField] Vector2 mItemSnapPivot = Vector2.zero;
        private SnapData _curSnapData = new SnapData();
        private Vector3 _lastSnapCheckPos = Vector3.zero;
        private bool _listViewInited = false;
        private int _listUpdateCheckFrameCount = 0;
        private ItemRangeData _curFrameItemRangeData = new ItemRangeData();
        private int _needCheckContentPosLeftCount = 1;
        private ClickEventListener _scrollBarClickEventListener1 = null;
        private ClickEventListener _scrollBarClickEventListener2 = null;

        private RowColumnPair _curSnapNearestItemRowColumn;

        public List<GridViewItemPrefabConfData> ItemPrefabDataList => mItemPrefabDataList;

        public int ItemTotalCount => _itemTotalCount;

        public RectTransform ContainerTrans => _containerTrans;

        public float ViewPortWidth => _viewPortRectTransform.rect.width;

        public float ViewPortHeight => _viewPortRectTransform.rect.height;

        public ScrollRect ScrollRect => _scrollRect;

        public bool IsDragging => _isDragging;

        public bool ItemSnapEnable
        {
            get => mItemSnapEnable;
            set => mItemSnapEnable = value;
        }

        public Vector2 ItemSize
        {
            get => mItemSize;
            set => SetItemSize(value);
        }

        public Vector2 ItemPadding
        {
            get => mItemPadding;
            set => SetItemPadding(value);
        }

        public Vector2 ItemSizeWithPadding => _itemSizeWithPadding;

        public RectOffset Padding
        {
            get => mPadding;
            set => SetPadding(value);
        }


        public GridViewItemPrefabConfData GetItemPrefabConfData(string prefabName)
        {
            foreach (GridViewItemPrefabConfData data in mItemPrefabDataList)
            {
                if (data.itemPrefab == null)
                {
                    Debug.LogError("A item prefab is null ");
                    continue;
                }

                if (prefabName == data.itemPrefab.name)
                {
                    return data;
                }
            }

            return null;
        }

        /*
        LoopGridView method is to initiate the LoopGridView component. There are 4 parameters:
        itemTotalCount: the total item count in the GridView, this parameter must be set a value >=0 , then the ItemIndex can be from 0 to itemTotalCount -1.
        onGetItemByRowColumn: when a item is getting in the ScrollRect viewport, and this Action will be called with the item' index and the row and column index as the parameters, to let you create the item and update its content.
        settingParam: You can use this parameter to override the values in the Inspector Setting
        */
        public void InitGridView(int itemTotalCount,
            System.Func<LoopGridView, int, int, int, LoopGridViewItem> onGetItemByRowColumn,
            LoopGridViewSettingParam settingParam = null,
            LoopGridViewInitParam initParam = null)
        {
            if (_listViewInited == true)
            {
                Debug.LogError("LoopGridView.InitListView method can be called only once.");
                return;
            }

            _listViewInited = true;
            if (itemTotalCount < 0)
            {
                Debug.LogError("itemTotalCount is  < 0");
                itemTotalCount = 0;
            }

            if (settingParam != null)
            {
                UpdateFromSettingParam(settingParam);
            }

            if (initParam != null)
            {
                _smoothDumpRate = initParam.SmoothDumpRate;
                _snapFinishThreshold = initParam.SnapFinishThreshold;
                _snapVecThreshold = initParam.SnapVecThreshold;
            }

            _scrollRect = gameObject.GetComponent<ScrollRect>();
            if (_scrollRect == null)
            {
                Debug.LogError("ListView Init Failed! ScrollRect component not found!");
                return;
            }

            _curSnapData.Clear();
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

            if (_scrollRect.verticalScrollbarVisibility == ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport && _scrollRect.verticalScrollbar != null)
            {
                Debug.LogError("ScrollRect.verticalScrollbarVisibility cannot be set to AutoHideAndExpandViewport");
            }

            SetScrollbarListener();
            AdjustViewPortPivot();
            AdjustContainerAnchorAndPivot();
            InitItemPool();
            _onGetItemByRowColumn = onGetItemByRowColumn;
            _needCheckContentPosLeftCount = 4;
            _curSnapData.Clear();
            _itemTotalCount = itemTotalCount;
            UpdateAllGridSetting();
        }


        /*
        This method may use to set the item total count of the GridView at runtime.
        this parameter must be set a value >=0 , and the ItemIndex can be from 0 to itemCount -1.
        If resetPos is set false, then the ScrollRect’s content position will not changed after this method finished.
        */
        public void SetListItemCount(int itemCount, bool resetPos = true)
        {
            if (itemCount < 0)
            {
                return;
            }

            if (itemCount == _itemTotalCount)
            {
                return;
            }

            _curSnapData.Clear();
            _itemTotalCount = itemCount;
            UpdateColumnRowCount();
            UpdateContentSize();
            ForceToCheckContentPos();
            if (_itemTotalCount == 0)
            {
                RecycleAllItem();
                ClearAllTmpRecycledItem();
                return;
            }

            VaildAndSetContainerPos();
            UpdateGridViewContent();
            ClearAllTmpRecycledItem();
            if (resetPos)
            {
                MovePanelToItemByRowColumn(0, 0);
                return;
            }
        }

        //fetch or create a new item form the item pool.
        public LoopGridViewItem NewListViewItem(string itemPrefabName)
        {
            GridItemPool pool = null;
            if (_itemPoolDict.TryGetValue(itemPrefabName, out pool) == false)
            {
                return null;
            }

            LoopGridViewItem item = pool.GetItem();
            RectTransform rf = item.GetComponent<RectTransform>();
            rf.SetParent(_containerTrans);
            rf.localScale = Vector3.one;
            rf.anchoredPosition3D = Vector3.zero;
            rf.localEulerAngles = Vector3.zero;
            item.ParentGridView = this;
            return item;
        }
        
        public LoopGridViewItem AllocOrNewListViewItem(string itemPrefabName)
        {
            GridItemPool pool = null;
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
        
        public LoopGridViewItem AllocOrNewListViewItem(GameObject prefabGo)
        {
            GridItemPool pool = null;
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
        
        private LoopGridViewItem AllocOrNewListViewItem(GridItemPool pool)
        {
            LoopGridViewItem item = pool.GetItem();
            RectTransform rf = item.GetComponent<RectTransform>();
            rf.SetParent(_containerTrans);
            rf.localScale = Vector3.one;
            rf.localPosition = Vector3.zero;
            rf.localEulerAngles = Vector3.zero;
            item.ParentGridView = this;
            return item;
        }

        private GridItemPool TryCreateItemPool(string itemPrefabName)
        {
            string resPath = itemPrefabName;
            GameObject go = GameModule.Resource.LoadGameObject(resPath, parent: _containerTrans);
            if (go != null)
            {
                go.SetActive(false);
                go.name = itemPrefabName;
                GridItemPool pool = new GridItemPool();
                pool.Init(go, 0, _containerTrans);
                _itemPoolDict.Add(itemPrefabName, pool);
                _itemPoolList.Add(pool);
                return pool;
            }

            return null;
        }

        private GridItemPool TryCreateItemPool(GameObject itemPrefab)
        {
            if (itemPrefab != null)
            {
                itemPrefab.SetActive(false);
                GridItemPool pool = new GridItemPool();
                pool.Init(itemPrefab, 0,  _containerTrans);
                _itemPoolDict.Add(itemPrefab.name, pool);
                _itemPoolList.Add(pool);
                return pool;
            }

            return null;
        }


        /*
        To update a item by itemIndex.if the itemIndex-th item is not visible, then this method will do nothing.
        Otherwise this method will call RefreshItemByRowColumn to do real work.
        */
        public void RefreshItemByItemIndex(int itemIndex)
        {
            if (itemIndex < 0 || itemIndex >= ItemTotalCount)
            {
                return;
            }

            int count = _itemGroupList.Count;
            if (count == 0)
            {
                return;
            }

            RowColumnPair val = GetRowColumnByItemIndex(itemIndex);
            RefreshItemByRowColumn(val.mRow, val.mColumn);
        }


        /*
        To update a item by (row,column).if the item is not visible, then this method will do nothing.
        Otherwise this method will call mOnGetItemByRowColumn(row,column) to get a new updated item.
        */
        public void RefreshItemByRowColumn(int row, int column)
        {
            int count = _itemGroupList.Count;
            if (count == 0)
            {
                return;
            }

            if (mGridFixedType == GridFixedType.ColumnCountFixed)
            {
                GridItemGroup group = GetShownGroup(row);
                if (group == null)
                {
                    return;
                }

                LoopGridViewItem curItem = group.GetItemByColumn(column);
                if (curItem == null)
                {
                    return;
                }

                LoopGridViewItem newItem = GetNewItemByRowColumn(row, column);
                if (newItem == null)
                {
                    return;
                }

                Vector3 pos = curItem.CachedRectTransform.anchoredPosition3D;
                group.ReplaceItem(curItem, newItem);
                RecycleItemTmp(curItem);
                newItem.CachedRectTransform.anchoredPosition3D = pos;
                ClearAllTmpRecycledItem();
            }
            else
            {
                GridItemGroup group = GetShownGroup(column);
                if (group == null)
                {
                    return;
                }

                LoopGridViewItem curItem = group.GetItemByRow(row);
                if (curItem == null)
                {
                    return;
                }

                LoopGridViewItem newItem = GetNewItemByRowColumn(row, column);
                if (newItem == null)
                {
                    return;
                }

                Vector3 pos = curItem.CachedRectTransform.anchoredPosition3D;
                group.ReplaceItem(curItem, newItem);
                RecycleItemTmp(curItem);
                newItem.CachedRectTransform.anchoredPosition3D = pos;
                ClearAllTmpRecycledItem();
            }
        }

        //Clear current snap target and then the GridView will auto snap to the CurSnapNearestItem.
        public void ClearSnapData()
        {
            _curSnapData.Clear();
        }

        //set cur snap target
        public void SetSnapTargetItemRowColumn(int row, int column)
        {
            if (row < 0)
            {
                row = 0;
            }

            if (column < 0)
            {
                column = 0;
            }

            _curSnapData.SnapTarget.mRow = row;
            _curSnapData.SnapTarget.mColumn = column;
            _curSnapData.SnapStatus = SnapStatus.TargetHasSet;
            _curSnapData.IsForceSnapTo = true;
        }

        //Get the nearest item row and column with the viewport snap point.
        public RowColumnPair CurSnapNearestItemRowColumn => _curSnapNearestItemRowColumn;


        //force to update the mCurSnapNearestItemRowColumn value
        public void ForceSnapUpdateCheck()
        {
            if (_leftSnapUpdateExtraCount <= 0)
            {
                _leftSnapUpdateExtraCount = 1;
            }
        }

        //force to refresh the mCurFrameItemRangeData that what items should be shown in viewport.
        public void ForceToCheckContentPos()
        {
            if (_needCheckContentPosLeftCount <= 0)
            {
                _needCheckContentPosLeftCount = 1;
            }
        }

        /*
        This method will move the panel's position to ( the position of itemIndex'th item + offset ).
        */
        public void MovePanelToItemByIndex(int itemIndex, float offsetX = 0, float offsetY = 0)
        {
            if (ItemTotalCount == 0)
            {
                return;
            }

            if (itemIndex >= ItemTotalCount)
            {
                itemIndex = ItemTotalCount - 1;
            }

            if (itemIndex < 0)
            {
                itemIndex = 0;
            }

            RowColumnPair val = GetRowColumnByItemIndex(itemIndex);
            MovePanelToItemByRowColumn(val.mRow, val.mColumn, offsetX, offsetY);
        }

        /*
        This method will move the panel's position to ( the position of (row,column) item + offset ).
        */
        public void MovePanelToItemByRowColumn(int row, int column, float offsetX = 0, float offsetY = 0)
        {
            _scrollRect.StopMovement();
            _curSnapData.Clear();
            if (_itemTotalCount == 0)
            {
                return;
            }

            Vector2 itemPos = GetItemPos(row, column);
            Vector3 pos = _containerTrans.anchoredPosition3D;
            if (_scrollRect.horizontal)
            {
                float maxCanMoveX = Mathf.Max(ContainerTrans.rect.width - ViewPortWidth, 0);
                if (maxCanMoveX > 0)
                {
                    float x = -itemPos.x + offsetX;
                    x = Mathf.Min(Mathf.Abs(x), maxCanMoveX) * Mathf.Sign(x);
                    pos.x = x;
                }
            }

            if (_scrollRect.vertical)
            {
                float maxCanMoveY = Mathf.Max(ContainerTrans.rect.height - ViewPortHeight, 0);
                if (maxCanMoveY > 0)
                {
                    float y = -itemPos.y + offsetY;
                    y = Mathf.Min(Mathf.Abs(y), maxCanMoveY) * Mathf.Sign(y);
                    pos.y = y;
                }
            }

            if (pos != _containerTrans.anchoredPosition3D)
            {
                _containerTrans.anchoredPosition3D = pos;
            }

            VaildAndSetContainerPos();
            ForceToCheckContentPos();
        }

        //update all visible items.
        public void RefreshAllShownItem()
        {
            int count = _itemGroupList.Count;
            if (count == 0)
            {
                return;
            }

            ForceToCheckContentPos();
            RecycleAllItem();
            UpdateGridViewContent();
        }


        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            _curSnapData.Clear();
            _isDragging = true;
            if (OnBeginDragAction != null)
            {
                OnBeginDragAction(eventData);
            }
        }

        public virtual void OnEndDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            _isDragging = false;
            ForceSnapUpdateCheck();
            if (OnEndDragAction != null)
            {
                OnEndDragAction(eventData);
            }
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            if (OnDragingAction != null)
            {
                OnDragingAction(eventData);
            }
        }


        public int GetItemIndexByRowColumn(int row, int column)
        {
            if (mGridFixedType == GridFixedType.ColumnCountFixed)
            {
                return row * mFixedRowOrColumnCount + column;
            }
            else
            {
                return column * mFixedRowOrColumnCount + row;
            }
        }


        public RowColumnPair GetRowColumnByItemIndex(int itemIndex)
        {
            if (itemIndex < 0)
            {
                itemIndex = 0;
            }

            if (mGridFixedType == GridFixedType.ColumnCountFixed)
            {
                int row = itemIndex / mFixedRowOrColumnCount;
                int column = itemIndex % mFixedRowOrColumnCount;
                return new RowColumnPair(row, column);
            }
            else
            {
                int column = itemIndex / mFixedRowOrColumnCount;
                int row = itemIndex % mFixedRowOrColumnCount;
                return new RowColumnPair(row, column);
            }
        }


        public Vector2 GetItemAbsPos(int row, int column)
        {
            float x = _startPadding.x + column * _itemSizeWithPadding.x;
            float y = _startPadding.y + row * _itemSizeWithPadding.y;
            return new Vector2(x, y);
        }


        public Vector2 GetItemPos(int row, int column)
        {
            Vector2 absPos = GetItemAbsPos(row, column);
            float x = absPos.x;
            float y = absPos.y;
            if (ArrangeType == GridItemArrangeType.TopLeftToBottomRight)
            {
                return new Vector2(x, -y);
            }
            else if (ArrangeType == GridItemArrangeType.BottomLeftToTopRight)
            {
                return new Vector2(x, y);
            }
            else if (ArrangeType == GridItemArrangeType.TopRightToBottomLeft)
            {
                return new Vector2(-x, -y);
            }
            else if (ArrangeType == GridItemArrangeType.BottomRightToTopLeft)
            {
                return new Vector2(-x, y);
            }

            return Vector2.zero;
        }

        //get the shown item of itemIndex, if this item is not shown,then return null.
        public LoopGridViewItem GetShownItemByItemIndex(int itemIndex)
        {
            if (itemIndex < 0 || itemIndex >= ItemTotalCount)
            {
                return null;
            }

            if (_itemGroupList.Count == 0)
            {
                return null;
            }

            RowColumnPair val = GetRowColumnByItemIndex(itemIndex);
            return GetShownItemByRowColumn(val.mRow, val.mColumn);
        }

        //get the shown item of (row, column), if this item is not shown,then return null.
        public LoopGridViewItem GetShownItemByRowColumn(int row, int column)
        {
            if (_itemGroupList.Count == 0)
            {
                return null;
            }

            if (mGridFixedType == GridFixedType.ColumnCountFixed)
            {
                GridItemGroup group = GetShownGroup(row);
                if (group == null)
                {
                    return null;
                }

                return group.GetItemByColumn(column);
            }
            else
            {
                GridItemGroup group = GetShownGroup(column);
                if (group == null)
                {
                    return null;
                }

                return group.GetItemByRow(row);
            }
        }

        public void UpdateAllGridSetting()
        {
            UpdateStartEndPadding();
            UpdateItemSize();
            UpdateColumnRowCount();
            UpdateContentSize();
            ForceSnapUpdateCheck();
            ForceToCheckContentPos();
        }

        //set mGridFixedType and mFixedRowOrColumnCount at runtime
        public void SetGridFixedGroupCount(GridFixedType fixedType, int count)
        {
            if (mGridFixedType == fixedType && mFixedRowOrColumnCount == count)
            {
                return;
            }

            mGridFixedType = fixedType;
            mFixedRowOrColumnCount = count;
            UpdateColumnRowCount();
            UpdateContentSize();
            if (_itemGroupList.Count == 0)
            {
                return;
            }

            RecycleAllItem();
            ForceSnapUpdateCheck();
            ForceToCheckContentPos();
        }

        //change item size at runtime
        public void SetItemSize(Vector2 newSize)
        {
            if (newSize == mItemSize)
            {
                return;
            }

            mItemSize = newSize;
            UpdateItemSize();
            UpdateContentSize();
            if (_itemGroupList.Count == 0)
            {
                return;
            }

            RecycleAllItem();
            ForceSnapUpdateCheck();
            ForceToCheckContentPos();
        }

        //change item padding at runtime
        public void SetItemPadding(Vector2 newPadding)
        {
            if (newPadding == mItemPadding)
            {
                return;
            }

            mItemPadding = newPadding;
            UpdateItemSize();
            UpdateContentSize();
            if (_itemGroupList.Count == 0)
            {
                return;
            }

            RecycleAllItem();
            ForceSnapUpdateCheck();
            ForceToCheckContentPos();
        }

        //change padding at runtime
        public void SetPadding(RectOffset newPadding)
        {
            if (newPadding == mPadding)
            {
                return;
            }

            mPadding = newPadding;
            UpdateStartEndPadding();
            UpdateContentSize();
            if (_itemGroupList.Count == 0)
            {
                return;
            }

            RecycleAllItem();
            ForceSnapUpdateCheck();
            ForceToCheckContentPos();
        }


        public void UpdateContentSize()
        {
            float width = _startPadding.x + _columnCount * _itemSizeWithPadding.x - mItemPadding.x + _endPadding.x;
            float height = _startPadding.y + _rowCount * _itemSizeWithPadding.y - mItemPadding.y + _endPadding.y;
            if (_containerTrans.rect.height != height)
            {
                _containerTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            }

            if (_containerTrans.rect.width != width)
            {
                _containerTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            }
        }


        public void VaildAndSetContainerPos()
        {
            Vector3 pos = _containerTrans.anchoredPosition3D;
            _containerTrans.anchoredPosition3D = GetContainerVaildPos(pos.x, pos.y);
        }

        public void ClearAllTmpRecycledItem()
        {
            int count = _itemPoolList.Count;
            for (int i = 0; i < count; ++i)
            {
                _itemPoolList[i].ClearTmpRecycledItem();
            }
        }


        public void RecycleAllItem()
        {
            foreach (GridItemGroup group in _itemGroupList)
            {
                RecycleItemGroupTmp(group);
            }

            _itemGroupList.Clear();
        }

        public void UpdateGridViewContent()
        {
            _listUpdateCheckFrameCount++;
            if (_itemTotalCount == 0)
            {
                if (_itemGroupList.Count > 0)
                {
                    RecycleAllItem();
                }

                return;
            }

            UpdateCurFrameItemRangeData();
            if (mGridFixedType == GridFixedType.ColumnCountFixed)
            {
                int groupCount = _itemGroupList.Count;
                int minRow = _curFrameItemRangeData.MinRow;
                int maxRow = _curFrameItemRangeData.MaxRow;
                for (int i = groupCount - 1; i >= 0; --i)
                {
                    GridItemGroup group = _itemGroupList[i];
                    if (group.GroupIndex < minRow || group.GroupIndex > maxRow)
                    {
                        RecycleItemGroupTmp(group);
                        _itemGroupList.RemoveAt(i);
                    }
                }

                if (_itemGroupList.Count == 0)
                {
                    GridItemGroup group = CreateItemGroup(minRow);
                    _itemGroupList.Add(group);
                }

                while (_itemGroupList[0].GroupIndex > minRow)
                {
                    GridItemGroup group = CreateItemGroup(_itemGroupList[0].GroupIndex - 1);
                    _itemGroupList.Insert(0, group);
                }

                while (_itemGroupList[_itemGroupList.Count - 1].GroupIndex < maxRow)
                {
                    GridItemGroup group = CreateItemGroup(_itemGroupList[_itemGroupList.Count - 1].GroupIndex + 1);
                    _itemGroupList.Add(group);
                }

                int count = _itemGroupList.Count;
                for (int i = 0; i < count; ++i)
                {
                    UpdateRowItemGroupForRecycleAndNew(_itemGroupList[i]);
                }
            }
            else
            {
                int groupCount = _itemGroupList.Count;
                int minColumn = _curFrameItemRangeData.MinColumn;
                int maxColumn = _curFrameItemRangeData.MaxColumn;
                for (int i = groupCount - 1; i >= 0; --i)
                {
                    GridItemGroup group = _itemGroupList[i];
                    if (group.GroupIndex < minColumn || group.GroupIndex > maxColumn)
                    {
                        RecycleItemGroupTmp(group);
                        _itemGroupList.RemoveAt(i);
                    }
                }

                if (_itemGroupList.Count == 0)
                {
                    GridItemGroup group = CreateItemGroup(minColumn);
                    _itemGroupList.Add(group);
                }

                while (_itemGroupList[0].GroupIndex > minColumn)
                {
                    GridItemGroup group = CreateItemGroup(_itemGroupList[0].GroupIndex - 1);
                    _itemGroupList.Insert(0, group);
                }

                while (_itemGroupList[_itemGroupList.Count - 1].GroupIndex < maxColumn)
                {
                    GridItemGroup group = CreateItemGroup(_itemGroupList[_itemGroupList.Count - 1].GroupIndex + 1);
                    _itemGroupList.Add(group);
                }

                int count = _itemGroupList.Count;
                for (int i = 0; i < count; ++i)
                {
                    UpdateColumnItemGroupForRecycleAndNew(_itemGroupList[i]);
                }
            }
        }

        public void UpdateStartEndPadding()
        {
            if (ArrangeType == GridItemArrangeType.TopLeftToBottomRight)
            {
                _startPadding.x = mPadding.left;
                _startPadding.y = mPadding.top;
                _endPadding.x = mPadding.right;
                _endPadding.y = mPadding.bottom;
            }
            else if (ArrangeType == GridItemArrangeType.BottomLeftToTopRight)
            {
                _startPadding.x = mPadding.left;
                _startPadding.y = mPadding.bottom;
                _endPadding.x = mPadding.right;
                _endPadding.y = mPadding.top;
            }
            else if (ArrangeType == GridItemArrangeType.TopRightToBottomLeft)
            {
                _startPadding.x = mPadding.right;
                _startPadding.y = mPadding.top;
                _endPadding.x = mPadding.left;
                _endPadding.y = mPadding.bottom;
            }
            else if (ArrangeType == GridItemArrangeType.BottomRightToTopLeft)
            {
                _startPadding.x = mPadding.right;
                _startPadding.y = mPadding.bottom;
                _endPadding.x = mPadding.left;
                _endPadding.y = mPadding.top;
            }
        }


        public void UpdateItemSize()
        {
            if (mItemSize.x > 0f && mItemSize.y > 0f)
            {
                _itemSizeWithPadding = mItemSize + mItemPadding;
                return;
            }

            do
            {
                if (mItemPrefabDataList.Count == 0)
                {
                    break;
                }

                GameObject obj = mItemPrefabDataList[0].itemPrefab;
                if (obj == null)
                {
                    break;
                }

                RectTransform rtf = obj.GetComponent<RectTransform>();
                if (rtf == null)
                {
                    break;
                }

                mItemSize = rtf.rect.size;
                _itemSizeWithPadding = mItemSize + mItemPadding;
            } while (false);

            if (mItemSize.x <= 0 || mItemSize.y <= 0)
            {
                Debug.LogError("Error, ItemSize is invaild.");
            }
        }

        public void UpdateColumnRowCount()
        {
            if (mGridFixedType == GridFixedType.ColumnCountFixed)
            {
                _columnCount = mFixedRowOrColumnCount;
                _rowCount = _itemTotalCount / _columnCount;
                if (_itemTotalCount % _columnCount > 0)
                {
                    _rowCount++;
                }

                if (_itemTotalCount <= _columnCount)
                {
                    _columnCount = _itemTotalCount;
                }
            }
            else
            {
                _rowCount = mFixedRowOrColumnCount;
                _columnCount = _itemTotalCount / _rowCount;
                if (_itemTotalCount % _rowCount > 0)
                {
                    _columnCount++;
                }

                if (_itemTotalCount <= _rowCount)
                {
                    _rowCount = _itemTotalCount;
                }
            }
        }


        /// ///////////////////////////////////////////////////////////////////////////////////////////////////////
        /// </summary>
        bool IsContainerTransCanMove()
        {
            if (_itemTotalCount == 0)
            {
                return false;
            }

            if (_scrollRect.horizontal && ContainerTrans.rect.width > ViewPortWidth)
            {
                return true;
            }

            if (_scrollRect.vertical && ContainerTrans.rect.height > ViewPortHeight)
            {
                return true;
            }

            return false;
        }


        void RecycleItemGroupTmp(GridItemGroup group)
        {
            if (group == null)
            {
                return;
            }

            while (group.First != null)
            {
                LoopGridViewItem item = group.RemoveFirst();
                RecycleItemTmp(item);
            }

            group.Clear();
            RecycleOneItemGroupObj(group);
        }


        void RecycleItemTmp(LoopGridViewItem item)
        {
            if (item == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(item.ItemPrefabName))
            {
                return;
            }

            GridItemPool pool = null;
            if (_itemPoolDict.TryGetValue(item.ItemPrefabName, out pool) == false)
            {
                return;
            }

            pool.RecycleItem(item);
        }


        void AdjustViewPortPivot()
        {
            RectTransform rtf = _viewPortRectTransform;
            if (ArrangeType == GridItemArrangeType.TopLeftToBottomRight)
            {
                rtf.pivot = new Vector2(0, 1);
            }
            else if (ArrangeType == GridItemArrangeType.BottomLeftToTopRight)
            {
                rtf.pivot = new Vector2(0, 0);
            }
            else if (ArrangeType == GridItemArrangeType.TopRightToBottomLeft)
            {
                rtf.pivot = new Vector2(1, 1);
            }
            else if (ArrangeType == GridItemArrangeType.BottomRightToTopLeft)
            {
                rtf.pivot = new Vector2(1, 0);
            }
        }

        void AdjustContainerAnchorAndPivot()
        {
            RectTransform rtf = ContainerTrans;

            if (ArrangeType == GridItemArrangeType.TopLeftToBottomRight)
            {
                rtf.anchorMin = new Vector2(0, 1);
                rtf.anchorMax = new Vector2(0, 1);
                rtf.pivot = new Vector2(0, 1);
            }
            else if (ArrangeType == GridItemArrangeType.BottomLeftToTopRight)
            {
                rtf.anchorMin = new Vector2(0, 0);
                rtf.anchorMax = new Vector2(0, 0);
                rtf.pivot = new Vector2(0, 0);
            }
            else if (ArrangeType == GridItemArrangeType.TopRightToBottomLeft)
            {
                rtf.anchorMin = new Vector2(1, 1);
                rtf.anchorMax = new Vector2(1, 1);
                rtf.pivot = new Vector2(1, 1);
            }
            else if (ArrangeType == GridItemArrangeType.BottomRightToTopLeft)
            {
                rtf.anchorMin = new Vector2(1, 0);
                rtf.anchorMax = new Vector2(1, 0);
                rtf.pivot = new Vector2(1, 0);
            }
        }

        void AdjustItemAnchorAndPivot(RectTransform rtf)
        {
            if (ArrangeType == GridItemArrangeType.TopLeftToBottomRight)
            {
                rtf.anchorMin = new Vector2(0, 1);
                rtf.anchorMax = new Vector2(0, 1);
                rtf.pivot = new Vector2(0, 1);
            }
            else if (ArrangeType == GridItemArrangeType.BottomLeftToTopRight)
            {
                rtf.anchorMin = new Vector2(0, 0);
                rtf.anchorMax = new Vector2(0, 0);
                rtf.pivot = new Vector2(0, 0);
            }
            else if (ArrangeType == GridItemArrangeType.TopRightToBottomLeft)
            {
                rtf.anchorMin = new Vector2(1, 1);
                rtf.anchorMax = new Vector2(1, 1);
                rtf.pivot = new Vector2(1, 1);
            }
            else if (ArrangeType == GridItemArrangeType.BottomRightToTopLeft)
            {
                rtf.anchorMin = new Vector2(1, 0);
                rtf.anchorMax = new Vector2(1, 0);
                rtf.pivot = new Vector2(1, 0);
            }
        }


        void InitItemPool()
        {
            foreach (GridViewItemPrefabConfData data in mItemPrefabDataList)
            {
                if (data.itemPrefab == null)
                {
                    Debug.LogError("A item prefab is null ");
                    continue;
                }

                string prefabName = data.itemPrefab.name;
                if (_itemPoolDict.ContainsKey(prefabName))
                {
                    Debug.LogError("A item prefab with name " + prefabName + " has existed!");
                    continue;
                }

                RectTransform rtf = data.itemPrefab.GetComponent<RectTransform>();
                if (rtf == null)
                {
                    Debug.LogError("RectTransform component is not found in the prefab " + prefabName);
                    continue;
                }

                AdjustItemAnchorAndPivot(rtf);
                LoopGridViewItem tItem = data.itemPrefab.GetComponent<LoopGridViewItem>();
                if (tItem == null)
                {
                    data.itemPrefab.AddComponent<LoopGridViewItem>();
                }

                GridItemPool pool = new GridItemPool();
                pool.Init(data.itemPrefab, data.initCreateCount, _containerTrans);
                _itemPoolDict.Add(prefabName, pool);
                _itemPoolList.Add(pool);
            }
        }


        LoopGridViewItem GetNewItemByRowColumn(int row, int column)
        {
            int itemIndex = GetItemIndexByRowColumn(row, column);
            if (itemIndex < 0 || itemIndex >= ItemTotalCount)
            {
                return null;
            }

            LoopGridViewItem newItem = _onGetItemByRowColumn(this, itemIndex, row, column);
            if (newItem == null)
            {
                return null;
            }

            newItem.NextItem = null;
            newItem.PrevItem = null;
            newItem.Row = row;
            newItem.Column = column;
            newItem.ItemIndex = itemIndex;
            newItem.ItemCreatedCheckFrameCount = _listUpdateCheckFrameCount;
            return newItem;
        }


        RowColumnPair GetCeilItemRowColumnAtGivenAbsPos(float ax, float ay)
        {
            ax = Mathf.Abs(ax);
            ay = Mathf.Abs(ay);
            int row = Mathf.CeilToInt((ay - _startPadding.y) / _itemSizeWithPadding.y) - 1;
            int column = Mathf.CeilToInt((ax - _startPadding.x) / _itemSizeWithPadding.x) - 1;
            if (row < 0)
            {
                row = 0;
            }

            if (row >= _rowCount)
            {
                row = _rowCount - 1;
            }

            if (column < 0)
            {
                column = 0;
            }

            if (column >= _columnCount)
            {
                column = _columnCount - 1;
            }

            return new RowColumnPair(row, column);
        }

        void Update()
        {
            if (_listViewInited == false)
            {
                return;
            }

            UpdateSnapMove();
            UpdateGridViewContent();
            ClearAllTmpRecycledItem();
        }


        GridItemGroup CreateItemGroup(int groupIndex)
        {
            GridItemGroup ret = GetOneItemGroupObj();
            ret.GroupIndex = groupIndex;
            return ret;
        }

        Vector2 GetContainerMovedDistance()
        {
            Vector2 pos = GetContainerVaildPos(ContainerTrans.anchoredPosition3D.x, ContainerTrans.anchoredPosition3D.y);
            return new Vector2(Mathf.Abs(pos.x), Mathf.Abs(pos.y));
        }


        Vector2 GetContainerVaildPos(float curX, float curY)
        {
            float maxCanMoveX = Mathf.Max(ContainerTrans.rect.width - ViewPortWidth, 0);
            float maxCanMoveY = Mathf.Max(ContainerTrans.rect.height - ViewPortHeight, 0);
            if (mArrangeType == GridItemArrangeType.TopLeftToBottomRight)
            {
                curX = Mathf.Clamp(curX, -maxCanMoveX, 0);
                curY = Mathf.Clamp(curY, 0, maxCanMoveY);
            }
            else if (mArrangeType == GridItemArrangeType.BottomLeftToTopRight)
            {
                curX = Mathf.Clamp(curX, -maxCanMoveX, 0);
                curY = Mathf.Clamp(curY, -maxCanMoveY, 0);
            }
            else if (mArrangeType == GridItemArrangeType.BottomRightToTopLeft)
            {
                curX = Mathf.Clamp(curX, 0, maxCanMoveX);
                curY = Mathf.Clamp(curY, -maxCanMoveY, 0);
            }
            else if (mArrangeType == GridItemArrangeType.TopRightToBottomLeft)
            {
                curX = Mathf.Clamp(curX, 0, maxCanMoveX);
                curY = Mathf.Clamp(curY, 0, maxCanMoveY);
            }

            return new Vector2(curX, curY);
        }


        void UpdateCurFrameItemRangeData()
        {
            Vector2 distVector2 = GetContainerMovedDistance();
            if (_needCheckContentPosLeftCount <= 0 && _curFrameItemRangeData.CheckedPosition == distVector2)
            {
                return;
            }

            if (_needCheckContentPosLeftCount > 0)
            {
                _needCheckContentPosLeftCount--;
            }

            float distX = distVector2.x - mItemRecycleDistance.x;
            float distY = distVector2.y - mItemRecycleDistance.y;
            if (distX < 0)
            {
                distX = 0;
            }

            if (distY < 0)
            {
                distY = 0;
            }

            RowColumnPair val = GetCeilItemRowColumnAtGivenAbsPos(distX, distY);
            _curFrameItemRangeData.MinColumn = val.mColumn;
            _curFrameItemRangeData.MinRow = val.mRow;
            distX = distVector2.x + mItemRecycleDistance.x + ViewPortWidth;
            distY = distVector2.y + mItemRecycleDistance.y + ViewPortHeight;
            val = GetCeilItemRowColumnAtGivenAbsPos(distX, distY);
            _curFrameItemRangeData.MaxColumn = val.mColumn;
            _curFrameItemRangeData.MaxRow = val.mRow;
            _curFrameItemRangeData.CheckedPosition = distVector2;
        }


        void UpdateRowItemGroupForRecycleAndNew(GridItemGroup group)
        {
            int minColumn = _curFrameItemRangeData.MinColumn;
            int maxColumn = _curFrameItemRangeData.MaxColumn;
            int row = group.GroupIndex;
            while (group.First != null && group.First.Column < minColumn)
            {
                RecycleItemTmp(group.RemoveFirst());
            }

            while (group.Last != null && ((group.Last.Column > maxColumn) || (group.Last.ItemIndex >= ItemTotalCount)))
            {
                RecycleItemTmp(group.RemoveLast());
            }

            if (group.First == null)
            {
                LoopGridViewItem item = GetNewItemByRowColumn(row, minColumn);
                if (item == null)
                {
                    return;
                }

                item.CachedRectTransform.anchoredPosition3D = GetItemPos(item.Row, item.Column);
                group.AddFirst(item);
            }

            while (group.First.Column > minColumn)
            {
                LoopGridViewItem item = GetNewItemByRowColumn(row, group.First.Column - 1);
                if (item == null)
                {
                    break;
                }

                item.CachedRectTransform.anchoredPosition3D = GetItemPos(item.Row, item.Column);

                group.AddFirst(item);
            }

            while (group.Last.Column < maxColumn)
            {
                LoopGridViewItem item = GetNewItemByRowColumn(row, group.Last.Column + 1);
                if (item == null)
                {
                    break;
                }

                item.CachedRectTransform.anchoredPosition3D = GetItemPos(item.Row, item.Column);

                group.AddLast(item);
            }
        }

        void UpdateColumnItemGroupForRecycleAndNew(GridItemGroup group)
        {
            int minRow = _curFrameItemRangeData.MinRow;
            int maxRow = _curFrameItemRangeData.MaxRow;
            int column = group.GroupIndex;
            while (group.First != null && group.First.Row < minRow)
            {
                RecycleItemTmp(group.RemoveFirst());
            }

            while (group.Last != null && ((group.Last.Row > maxRow) || (group.Last.ItemIndex >= ItemTotalCount)))
            {
                RecycleItemTmp(group.RemoveLast());
            }

            if (group.First == null)
            {
                LoopGridViewItem item = GetNewItemByRowColumn(minRow, column);
                if (item == null)
                {
                    return;
                }

                item.CachedRectTransform.anchoredPosition3D = GetItemPos(item.Row, item.Column);
                group.AddFirst(item);
            }

            while (group.First.Row > minRow)
            {
                LoopGridViewItem item = GetNewItemByRowColumn(group.First.Row - 1, column);
                if (item == null)
                {
                    break;
                }

                item.CachedRectTransform.anchoredPosition3D = GetItemPos(item.Row, item.Column);

                group.AddFirst(item);
            }

            while (group.Last.Row < maxRow)
            {
                LoopGridViewItem item = GetNewItemByRowColumn(group.Last.Row + 1, column);
                if (item == null)
                {
                    break;
                }

                item.CachedRectTransform.anchoredPosition3D = GetItemPos(item.Row, item.Column);

                group.AddLast(item);
            }
        }


        void SetScrollbarListener()
        {
            if (ItemSnapEnable == false)
            {
                return;
            }

            _scrollBarClickEventListener1 = null;
            _scrollBarClickEventListener2 = null;
            Scrollbar curScrollBar1 = null;
            Scrollbar curScrollBar2 = null;
            if (_scrollRect.vertical && _scrollRect.verticalScrollbar != null)
            {
                curScrollBar1 = _scrollRect.verticalScrollbar;
            }

            if (_scrollRect.horizontal && _scrollRect.horizontalScrollbar != null)
            {
                curScrollBar2 = _scrollRect.horizontalScrollbar;
            }

            if (curScrollBar1 != null)
            {
                ClickEventListener listener = ClickEventListener.Get(curScrollBar1.gameObject);
                _scrollBarClickEventListener1 = listener;
                listener.SetPointerUpHandler(OnPointerUpInScrollBar);
                listener.SetPointerDownHandler(OnPointerDownInScrollBar);
            }

            if (curScrollBar2 != null)
            {
                ClickEventListener listener = ClickEventListener.Get(curScrollBar2.gameObject);
                _scrollBarClickEventListener2 = listener;
                listener.SetPointerUpHandler(OnPointerUpInScrollBar);
                listener.SetPointerDownHandler(OnPointerDownInScrollBar);
            }
        }

        void OnPointerDownInScrollBar(GameObject obj)
        {
            _curSnapData.Clear();
        }

        void OnPointerUpInScrollBar(GameObject obj)
        {
            ForceSnapUpdateCheck();
        }

        RowColumnPair FindNearestItemWithLocalPos(float x, float y)
        {
            Vector2 targetPos = new Vector2(x, y);
            RowColumnPair val = GetCeilItemRowColumnAtGivenAbsPos(targetPos.x, targetPos.y);
            int row = val.mRow;
            int column = val.mColumn;
            float distance = 0;
            RowColumnPair ret = new RowColumnPair(-1, -1);
            Vector2 pos = Vector2.zero;
            float minDistance = float.MaxValue;
            for (int r = row - 1; r <= row + 1; ++r)
            {
                for (int c = column - 1; c <= column + 1; ++c)
                {
                    if (r >= 0 && r < _rowCount && c >= 0 && c < _columnCount)
                    {
                        pos = GetItemSnapPivotLocalPos(r, c);
                        distance = (pos - targetPos).sqrMagnitude;
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            ret.mRow = r;
                            ret.mColumn = c;
                        }
                    }
                }
            }

            return ret;
        }

        Vector2 GetItemSnapPivotLocalPos(int row, int column)
        {
            Vector2 absPos = GetItemAbsPos(row, column);
            if (mArrangeType == GridItemArrangeType.TopLeftToBottomRight)
            {
                float x = absPos.x + mItemSize.x * mItemSnapPivot.x;
                float y = -absPos.y - mItemSize.y * (1 - mItemSnapPivot.y);
                return new Vector2(x, y);
            }
            else if (mArrangeType == GridItemArrangeType.BottomLeftToTopRight)
            {
                float x = absPos.x + mItemSize.x * mItemSnapPivot.x;
                float y = absPos.y + mItemSize.y * mItemSnapPivot.y;
                return new Vector2(x, y);
            }
            else if (mArrangeType == GridItemArrangeType.TopRightToBottomLeft)
            {
                float x = -absPos.x - mItemSize.x * (1 - mItemSnapPivot.x);
                float y = -absPos.y - mItemSize.y * (1 - mItemSnapPivot.y);
                return new Vector2(x, y);
            }
            else if (mArrangeType == GridItemArrangeType.BottomRightToTopLeft)
            {
                float x = -absPos.x - mItemSize.x * (1 - mItemSnapPivot.x);
                float y = absPos.y + mItemSize.y * mItemSnapPivot.y;
                return new Vector2(x, y);
            }

            return Vector2.zero;
        }

        Vector2 GetViewPortSnapPivotLocalPos(Vector2 pos)
        {
            float pivotLocalPosX = 0;
            float pivotLocalPosY = 0;
            if (mArrangeType == GridItemArrangeType.TopLeftToBottomRight)
            {
                pivotLocalPosX = -pos.x + ViewPortWidth * mViewPortSnapPivot.x;
                pivotLocalPosY = -pos.y - ViewPortHeight * (1 - mViewPortSnapPivot.y);
            }
            else if (mArrangeType == GridItemArrangeType.BottomLeftToTopRight)
            {
                pivotLocalPosX = -pos.x + ViewPortWidth * mViewPortSnapPivot.x;
                pivotLocalPosY = -pos.y + ViewPortHeight * mViewPortSnapPivot.y;
            }
            else if (mArrangeType == GridItemArrangeType.TopRightToBottomLeft)
            {
                pivotLocalPosX = -pos.x - ViewPortWidth * (1 - mViewPortSnapPivot.x);
                pivotLocalPosY = -pos.y - ViewPortHeight * (1 - mViewPortSnapPivot.y);
            }
            else if (mArrangeType == GridItemArrangeType.BottomRightToTopLeft)
            {
                pivotLocalPosX = -pos.x - ViewPortWidth * (1 - mViewPortSnapPivot.x);
                pivotLocalPosY = -pos.y + ViewPortHeight * mViewPortSnapPivot.y;
            }

            return new Vector2(pivotLocalPosX, pivotLocalPosY);
        }

        void UpdateNearestSnapItem(bool forceSendEvent)
        {
            if (mItemSnapEnable == false)
            {
                return;
            }

            int count = _itemGroupList.Count;
            if (count == 0)
            {
                return;
            }

            if (IsContainerTransCanMove() == false)
            {
                return;
            }

            Vector2 pos = GetContainerVaildPos(ContainerTrans.anchoredPosition3D.x, ContainerTrans.anchoredPosition3D.y);
            bool needCheck = (pos.y != _lastSnapCheckPos.y || pos.x != _lastSnapCheckPos.x);
            _lastSnapCheckPos = pos;
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
                RowColumnPair curVal = new RowColumnPair(-1, -1);
                Vector2 snapTartetPos = GetViewPortSnapPivotLocalPos(pos);
                curVal = FindNearestItemWithLocalPos(snapTartetPos.x, snapTartetPos.y);
                if (curVal.mRow >= 0)
                {
                    RowColumnPair oldNearestItem = _curSnapNearestItemRowColumn;
                    _curSnapNearestItemRowColumn = curVal;
                    if (forceSendEvent || oldNearestItem != _curSnapNearestItemRowColumn)
                    {
                        if (OnSnapNearestChanged != null)
                        {
                            OnSnapNearestChanged(this);
                        }
                    }
                }
                else
                {
                    _curSnapNearestItemRowColumn.mRow = -1;
                    _curSnapNearestItemRowColumn.mColumn = -1;
                }
            }
        }

        void UpdateFromSettingParam(LoopGridViewSettingParam param)
        {
            if (param == null)
            {
                return;
            }

            if (param.ItemSize != null)
            {
                mItemSize = (Vector2)(param.ItemSize);
            }

            if (param.ItemPadding != null)
            {
                mItemPadding = (Vector2)(param.ItemPadding);
            }

            if (param.Padding != null)
            {
                mPadding = (RectOffset)(param.Padding);
            }

            if (param.GridFixedType != null)
            {
                mGridFixedType = (GridFixedType)(param.GridFixedType);
            }

            if (param.FixedRowOrColumnCount != null)
            {
                mFixedRowOrColumnCount = (int)(param.FixedRowOrColumnCount);
            }
        }

        //snap move will finish at once.
        public void FinishSnapImmediately()
        {
            UpdateSnapMove(true);
        }

        //update snap move. if immediate is set true, then the snap move will finish at once.
        void UpdateSnapMove(bool immediate = false, bool forceSendEvent = false)
        {
            if (mItemSnapEnable == false)
            {
                return;
            }

            UpdateNearestSnapItem(false);
            Vector2 pos = _containerTrans.anchoredPosition3D;
            if (CanSnap() == false)
            {
                ClearSnapData();
                return;
            }

            UpdateCurSnapData();
            if (_curSnapData.SnapStatus != SnapStatus.SnapMoving)
            {
                return;
            }

            float v = Mathf.Abs(_scrollRect.velocity.x) + Mathf.Abs(_scrollRect.velocity.y);
            if (v > 0)
            {
                _scrollRect.StopMovement();
            }

            float old = _curSnapData.CurSnapVal;
            _curSnapData.CurSnapVal = Mathf.SmoothDamp(_curSnapData.CurSnapVal, _curSnapData.TargetSnapVal, ref _smoothDumpVel, _smoothDumpRate);
            float dt = _curSnapData.CurSnapVal - old;

            if (immediate || Mathf.Abs(_curSnapData.TargetSnapVal - _curSnapData.CurSnapVal) < _snapFinishThreshold)
            {
                pos = pos + (_curSnapData.TargetSnapVal - old) * _curSnapData.SnapNeedMoveDir;
                _curSnapData.SnapStatus = SnapStatus.SnapMoveFinish;
                if (OnSnapItemFinished != null)
                {
                    LoopGridViewItem targetItem = GetShownItemByRowColumn(_curSnapNearestItemRowColumn.mRow, _curSnapNearestItemRowColumn.mColumn);
                    if (targetItem != null)
                    {
                        OnSnapItemFinished(this, targetItem);
                    }
                }
            }
            else
            {
                pos = pos + dt * _curSnapData.SnapNeedMoveDir;
            }

            _containerTrans.anchoredPosition3D = GetContainerVaildPos(pos.x, pos.y);
        }

        GridItemGroup GetShownGroup(int groupIndex)
        {
            if (groupIndex < 0)
            {
                return null;
            }

            int count = _itemGroupList.Count;
            if (count == 0)
            {
                return null;
            }

            if (groupIndex < _itemGroupList[0].GroupIndex || groupIndex > _itemGroupList[count - 1].GroupIndex)
            {
                return null;
            }

            int i = groupIndex - _itemGroupList[0].GroupIndex;
            return _itemGroupList[i];
        }


        void FillCurSnapData(int row, int column)
        {
            Vector2 itemSnapPivotLocalPos = GetItemSnapPivotLocalPos(row, column);
            Vector2 containerPos = GetContainerVaildPos(ContainerTrans.anchoredPosition3D.x, ContainerTrans.anchoredPosition3D.y);
            Vector2 snapTartetPos = GetViewPortSnapPivotLocalPos(containerPos);
            Vector2 dir = snapTartetPos - itemSnapPivotLocalPos;
            if (_scrollRect.horizontal == false)
            {
                dir.x = 0;
            }

            if (_scrollRect.vertical == false)
            {
                dir.y = 0;
            }

            _curSnapData.TargetSnapVal = dir.magnitude;
            _curSnapData.CurSnapVal = 0;
            _curSnapData.SnapNeedMoveDir = dir.normalized;
        }


        void UpdateCurSnapData()
        {
            int count = _itemGroupList.Count;
            if (count == 0)
            {
                _curSnapData.Clear();
                return;
            }

            if (_curSnapData.SnapStatus == SnapStatus.SnapMoveFinish)
            {
                if (_curSnapData.SnapTarget == _curSnapNearestItemRowColumn)
                {
                    return;
                }

                _curSnapData.SnapStatus = SnapStatus.NoTargetSet;
            }

            if (_curSnapData.SnapStatus == SnapStatus.SnapMoving)
            {
                if ((_curSnapData.SnapTarget == _curSnapNearestItemRowColumn) || _curSnapData.IsForceSnapTo)
                {
                    return;
                }

                _curSnapData.SnapStatus = SnapStatus.NoTargetSet;
            }

            if (_curSnapData.SnapStatus == SnapStatus.NoTargetSet)
            {
                LoopGridViewItem nearestItem = GetShownItemByRowColumn(_curSnapNearestItemRowColumn.mRow, _curSnapNearestItemRowColumn.mColumn);
                if (nearestItem == null)
                {
                    return;
                }

                _curSnapData.SnapTarget = _curSnapNearestItemRowColumn;
                _curSnapData.SnapStatus = SnapStatus.TargetHasSet;
                _curSnapData.IsForceSnapTo = false;
            }

            if (_curSnapData.SnapStatus == SnapStatus.TargetHasSet)
            {
                LoopGridViewItem targetItem = GetShownItemByRowColumn(_curSnapData.SnapTarget.mRow, _curSnapData.SnapTarget.mColumn);
                if (targetItem == null)
                {
                    _curSnapData.Clear();
                    return;
                }

                FillCurSnapData(targetItem.Row, targetItem.Column);
                _curSnapData.SnapStatus = SnapStatus.SnapMoving;
            }
        }


        bool CanSnap()
        {
            if (_isDragging)
            {
                return false;
            }

            if (_scrollBarClickEventListener1 != null)
            {
                if (_scrollBarClickEventListener1.IsPressed)
                {
                    return false;
                }
            }

            if (_scrollBarClickEventListener2 != null)
            {
                if (_scrollBarClickEventListener2.IsPressed)
                {
                    return false;
                }
            }

            if (IsContainerTransCanMove() == false)
            {
                return false;
            }

            if (Mathf.Abs(_scrollRect.velocity.x) > _snapVecThreshold)
            {
                return false;
            }

            if (Mathf.Abs(_scrollRect.velocity.y) > _snapVecThreshold)
            {
                return false;
            }

            Vector3 pos = _containerTrans.anchoredPosition3D;
            Vector2 vPos = GetContainerVaildPos(pos.x, pos.y);
            if (Mathf.Abs(pos.x - vPos.x) > 3)
            {
                return false;
            }

            if (Mathf.Abs(pos.y - vPos.y) > 3)
            {
                return false;
            }

            return true;
        }

        GridItemGroup GetOneItemGroupObj()
        {
            int count = _itemGroupObjPool.Count;
            if (count == 0)
            {
                return new GridItemGroup();
            }

            GridItemGroup ret = _itemGroupObjPool[count - 1];
            _itemGroupObjPool.RemoveAt(count - 1);
            return ret;
        }

        void RecycleOneItemGroupObj(GridItemGroup obj)
        {
            _itemGroupObjPool.Add(obj);
        }
    }
}