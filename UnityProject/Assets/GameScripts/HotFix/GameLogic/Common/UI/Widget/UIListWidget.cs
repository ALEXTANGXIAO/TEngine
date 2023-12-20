using System;
using System.Collections.Generic;
using TEngine;

namespace GameLogic
{
    /// <summary>
    /// 普通UI列表。
    /// </summary>
    public class UIListWidget<TItem, TData> : UIListBase<TItem, TData> where TItem : UIWidget, new()
    {
        /// <summary>
        /// item列表。
        /// </summary>
        protected List<TItem> m_items = new List<TItem>();

        /// <summary>
        /// item列表。
        /// </summary>
        public List<TItem> items => m_items;

        /// <summary>
        /// 设置显示数据。
        /// </summary>
        /// <param name="n"></param>
        /// <param name="datas"></param>
        /// <param name="funcItem"></param>
        protected override void AdjustItemNum(int n, List<TData> datas = null, Action<TItem, int> funcItem = null)
        {
            base.AdjustItemNum(n, datas, funcItem);
            AdjustIconNum(m_items, n, gameObject.transform, itemBase);
            UpdateList(funcItem);
        }

        /// <summary>
        /// 刷新列表。
        /// </summary>
        /// <param name="funcItem"></param>
        protected void UpdateList(Action<TItem, int> funcItem = null)
        {
            for (var i = 0; i < m_items.Count; i++)
            {
                UpdateListItem(m_items[i], i, funcItem);
            }
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
    }
}