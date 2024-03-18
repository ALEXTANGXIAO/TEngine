using System;
using System.Collections.Generic;
using TEngine;
using UnityEngine;

namespace GameLogic
{
    /// <summary>
    /// UI列表。
    /// </summary>
    public class UILoopListWidget<TItem, TData> : UIListBase<TItem, TData> where TItem : UILoopItemWidget, new()
    {
        /// <summary>
        /// LoopRectView
        /// </summary>
        public LoopListView LoopRectView { private set; get; }

        /// <summary>
        /// Item字典
        /// <remarks>Key => GameObjectHashCode | Value => TItem.</remarks>
        /// </summary>
        private GameFrameworkDictionary<int, TItem> m_itemCache = new GameFrameworkDictionary<int, TItem>();

        /// <summary>
        /// 计算偏差后的ItemList
        /// </summary>
        private List<TItem> m_items = new List<TItem>();

        /// <summary>
        /// 计算偏差后的ItemList
        /// </summary>
        public List<TItem> items => m_items;

        protected override void BindMemberProperty()
        {
            base.BindMemberProperty();
            LoopRectView = rectTransform.GetComponent<LoopListView>();
        }

        protected override void OnCreate()
        {
            base.OnCreate();
            LoopRectView.InitListView(0, OnGetItemByIndex);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            m_itemCache.Clear();
        }

        /// <summary>
        /// Item回调函数
        /// </summary>
        protected Action<TItem, int> m_tpFuncItem;

        /// <summary>
        /// 设置显示数据
        /// </summary>
        /// <param name="n"></param>
        /// <param name="datas"></param>
        /// <param name="funcItem"></param>
        protected override void AdjustItemNum(int n, List<TData> datas = null, Action<TItem, int> funcItem = null)
        {
            base.AdjustItemNum(n, datas, funcItem);
            m_tpFuncItem = funcItem;
            LoopRectView.SetListItemCount(n);
            LoopRectView.RefreshAllShownItem();
            m_tpFuncItem = null;
            UpdateAllItemSelect();
        }

        /// <summary>
        /// 获取Item
        /// </summary>
        /// <param name="listView"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        protected LoopListViewItem OnGetItemByIndex(LoopListView listView, int index)
        {
            if (index < 0 || index >= num) return null;
            var item = itemBase == null ? CreateItem() : CreateItem(itemBase);
            if (item == null) return null;
            item.SetItemIndex(index);
            UpdateListItem(item, index, m_tpFuncItem);
            return item.LoopItem;
        }

        /// <summary>
        /// 创建Item
        /// </summary>
        /// <returns></returns>
        public TItem CreateItem()
        {
            return CreateItem(typeof(TItem).Name);
        }

        /// <summary>
        /// 创建Item
        /// </summary>
        /// <param name="itemName"></param>
        /// <returns></returns>
        public TItem CreateItem(string itemName)
        {
            TItem widget = null;
            LoopListViewItem item = LoopRectView.AllocOrNewListViewItem(itemName);
            if (item != null)
            {
                widget = CreateItem(item);
            }
            return widget;
        }

        /// <summary>
        /// 创建Item
        /// </summary>
        /// <param name="prefab"></param>
        /// <returns></returns>
        public TItem CreateItem(GameObject prefab)
        {
            TItem widget = null;
            LoopListViewItem item = LoopRectView.AllocOrNewListViewItem(prefab);
            if (item != null)
            {
                widget = CreateItem(item);
            }
            return widget;
        }

        /// <summary>
        /// 创建Item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private TItem CreateItem(LoopListViewItem item)
        {
            TItem widget;
            if (!m_itemCache.TryGetValue(item.GoId, out widget))
            {
                widget = CreateWidget<TItem>(item.gameObject);
                widget.LoopItem = item;
                m_itemCache.Add(item.GoId, widget);
            }

            return widget;
        }

       /// <summary>
       /// 获取item
       /// </summary>
       /// <param name="index"></param>
       /// <returns></returns>
        public override TItem GetItem(int index)
        {
            for (var i = 0; i < m_itemCache.Count; i++)
            {
                
                var item = m_itemCache.GetValueByIndex(i);
                if (item.GetItemIndex() == index)
                {
                    return item;
                }
            }
            return null;
        }

       /// <summary>
        /// 获取itemList
        /// </summary>
        /// <returns></returns>
        public List<TItem> GetItemList()
        {
            m_items.Clear();
            for (int i = 0; i < m_itemCache.Count; i++)
            {
                m_items.Add(m_itemCache.GetValueByIndex(i));
            }
            return m_items;
        }
       
        /// <summary>
        /// 获取当前起始索引
        /// </summary>
        /// <returns></returns>
        public int GetItemStartIndex()
        {
            return LoopRectView.GetItemStartIndex();
        }
        
        /// <summary>
        /// 获取Item。
        /// </summary>
        /// <param name="index">索引。</param>
        /// <returns>TItem。</returns>
        public TItem GetItemByIndex(int index)
        {
            return m_itemCache.GetValueByIndex(index);
        }
        
        /// <summary>
        /// 刷新所有item选中状态
        /// </summary>
        /// <returns></returns>
        public void UpdateAllItemSelect()
        {
            var index = selectIndex;
            for (var i = 0; i < m_itemCache.Count; i++)
            {
                if (m_itemCache.GetValueByIndex(i) is IListSelectItem item)
                {
                    item.SetSelected(item.GetItemIndex() == index);
                }
            }
        }

        protected override void UpdateSnapTargetItem()
        {
            base.UpdateSnapTargetItem();
            if (LoopRectView != null && LoopRectView.ItemSnapEnable)
            {
                LoopRectView.SetSnapTargetItemIndex(selectIndex);
            }
        }
    }
}
