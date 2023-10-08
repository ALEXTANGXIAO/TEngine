using System;
using System.Collections.Generic;
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
        /// </summary>
        private Dictionary<int, TItem> m_itemCache = new Dictionary<int, TItem>();

        /// <summary>
        /// 计算偏差后的ItemList
        /// </summary>
        private List<TItem> m_items = new List<TItem>();

        /// <summary>
        /// 计算偏差后的ItemList
        /// </summary>
        public List<TItem> items => m_items;

        public override void BindMemberProperty()
        {
            base.BindMemberProperty();
            LoopRectView = rectTransform.GetComponent<LoopListView>();
        }

        public override void OnCreate()
        {
            base.OnCreate();
            LoopRectView.InitListView(0, OnGetItemByIndex);
        }

        public override void OnDestroy()
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
       /// <param name="i"></param>
       /// <returns></returns>
        public override TItem GetItem(int i)
        {
            return i >= 0 && i < m_items.Count ? m_items[i] : null;
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
                m_items.Add(m_itemCache[i]);
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
    }
}
