using UnityEngine;

namespace GameLogic
{
    public class LoopListViewItem : MonoBehaviour
    {
        public float Padding;
        
        private int _itemIndex = -1;
        private int _itemId = -1;
        private LoopListView _parentListView = null;
        private bool _isInitHandlerCalled = false;
        private string _itemPrefabName;
        private RectTransform _cachedRectTransform;
        private float _padding;
        private float _distanceWithViewPortSnapCenter = 0;
        private int _itemCreatedCheckFrameCount = 0;
        private float _startPosOffset = 0;

        private object _userObjectData = null;
        private int _userIntData1 = 0;
        private int _userIntData2 = 0;
        private string _userStringData1 = null;
        private string _userStringData2 = null;

        private int _goId = 0;
        
        public int GoId
        {
            set => _goId = value;
            get => _goId;
        }

        public object UserObjectData
        {
            get => _userObjectData;
            set => _userObjectData = value;
        }

        public int UserIntData1
        {
            get => _userIntData1;
            set => _userIntData1 = value;
        }

        public int UserIntData2
        {
            get => _userIntData2;
            set => _userIntData2 = value;
        }

        public string UserStringData1
        {
            get => _userStringData1;
            set => _userStringData1 = value;
        }

        public string UserStringData2
        {
            get => _userStringData2;
            set => _userStringData2 = value;
        }

        public float DistanceWithViewPortSnapCenter
        {
            get => _distanceWithViewPortSnapCenter;
            set => _distanceWithViewPortSnapCenter = value;
        }

        public float StartPosOffset
        {
            get => _startPosOffset;
            set => _startPosOffset = value;
        }

        public int ItemCreatedCheckFrameCount
        {
            get => _itemCreatedCheckFrameCount;
            set => _itemCreatedCheckFrameCount = value;
        }

        public RectTransform CachedRectTransform
        {
            get
            {
                if (_cachedRectTransform == null)
                {
                    _cachedRectTransform = gameObject.GetComponent<RectTransform>();
                }

                return _cachedRectTransform;
            }
        }

        public string ItemPrefabName
        {
            get => _itemPrefabName;
            set => _itemPrefabName = value;
        }

        public int ItemIndex
        {
            get => _itemIndex;
            set => _itemIndex = value;
        }

        public int ItemId
        {
            get => _itemId;
            set => _itemId = value;
        }


        public bool IsInitHandlerCalled
        {
            get => _isInitHandlerCalled;
            set => _isInitHandlerCalled = value;
        }

        public LoopListView ParentListView
        {
            get => _parentListView;
            set => _parentListView = value;
        }

        public float TopY
        {
            get
            {
                ListItemArrangeType arrageType = ParentListView.ArrangeType;
                if (arrageType == ListItemArrangeType.TopToBottom)
                {
                    return CachedRectTransform.localPosition.y;
                }
                else if (arrageType == ListItemArrangeType.BottomToTop)
                {
                    return CachedRectTransform.localPosition.y + CachedRectTransform.rect.height;
                }

                return 0;
            }
        }

        public float BottomY
        {
            get
            {
                ListItemArrangeType arrageType = ParentListView.ArrangeType;
                if (arrageType == ListItemArrangeType.TopToBottom)
                {
                    return CachedRectTransform.localPosition.y - CachedRectTransform.rect.height;
                }
                else if (arrageType == ListItemArrangeType.BottomToTop)
                {
                    return CachedRectTransform.localPosition.y;
                }

                return 0;
            }
        }


        public float LeftX
        {
            get
            {
                ListItemArrangeType arrageType = ParentListView.ArrangeType;
                if (arrageType == ListItemArrangeType.LeftToRight)
                {
                    return CachedRectTransform.localPosition.x;
                }
                else if (arrageType == ListItemArrangeType.RightToLeft)
                {
                    return CachedRectTransform.localPosition.x - CachedRectTransform.rect.width;
                }

                return 0;
            }
        }

        public float RightX
        {
            get
            {
                ListItemArrangeType arrageType = ParentListView.ArrangeType;
                if (arrageType == ListItemArrangeType.LeftToRight)
                {
                    return CachedRectTransform.localPosition.x + CachedRectTransform.rect.width;
                }
                else if (arrageType == ListItemArrangeType.RightToLeft)
                {
                    return CachedRectTransform.localPosition.x;
                }

                return 0;
            }
        }

        public float ItemSize
        {
            get
            {
                if (ParentListView.IsVertList)
                {
                    return CachedRectTransform.rect.height;
                }
                else
                {
                    return CachedRectTransform.rect.width;
                }
            }
        }

        public float ItemSizeWithPadding => ItemSize + _padding;
    }
}