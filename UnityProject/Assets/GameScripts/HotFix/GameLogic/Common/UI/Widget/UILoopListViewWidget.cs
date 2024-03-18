using System.Collections.Generic;
using TEngine;
using UnityEngine;

namespace GameLogic
{
    public class UILoopListViewWidget<T> : UIWidget where T : UILoopItemWidget, new()
    {
        public LoopListView LoopRectView { private set; get; }

        private GameFrameworkDictionary<int, T> m_itemCache = new GameFrameworkDictionary<int, T>();

        protected override void BindMemberProperty()
        {
            base.BindMemberProperty();
            LoopRectView = this.rectTransform.GetComponent<LoopListView>();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            m_itemCache.Clear();
        }

        public T CreateItem()
        {
            string typeName = typeof(T).Name;
            return CreateItem(typeName);
            ;
        }

        public T CreateItem(string itemName)
        {
            T widget = null;
            var item = LoopRectView.AllocOrNewListViewItem(itemName);
            if (item != null)
            {
                widget = CreateItem(item);
            }

            return widget;
        }

        public T CreateItem(GameObject prefab)
        {
            T widget = null;
            var item = LoopRectView.AllocOrNewListViewItem(prefab);
            if (item != null)
            {
                widget = CreateItem(item);
            }

            return widget;
        }

        private T CreateItem(LoopListViewItem item)
        {
            T widget;
            if (!m_itemCache.TryGetValue(item.GoId, out widget))
            {
                widget = CreateWidget<T>(item.gameObject);
                widget.LoopItem = item;
                m_itemCache.Add(item.GoId, widget);
            }

            return widget;
        }

        public List<T> GetItemList()
        {
            List<T> list = new List<T>();
            for (int i = 0; i < m_itemCache.Count; i++)
            {
                list.Add(m_itemCache.GetValueByIndex(i));
            }
            return list;
        }

        public int GetItemCount()
        {
            return m_itemCache.Count;
        }

        /// <summary>
        /// 获取Item。
        /// </summary>
        /// <param name="index">索引。</param>
        /// <returns>TItem。</returns>
        public T GetItemByIndex(int index)
        {
            return m_itemCache.GetValueByIndex(index);
        }
    }
}