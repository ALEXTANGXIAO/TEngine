using System;
using System.Collections.Generic;
using TEngine;
using UnityEngine;
using UnityEngine.UI;

namespace GameLogic
{
    /// <summary>
    /// UI列表Item
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    public interface IListDataItem<in TData>
    {
        void SetItemData(TData d);
    }

    /// <summary>
    /// UI列表Item
    /// </summary>
    public interface IListSelectItem
    {
        /// <summary>
        /// 获取索引。
        /// </summary>
        /// <returns></returns>
        int GetItemIndex();

        /// <summary>
        /// 设置索引。
        /// </summary>
        /// <param name="i"></param>
        void SetItemIndex(int i);

        /// <summary>
        /// 是否被选中。
        /// </summary>
        /// <returns></returns>
        bool IsSelected();

        /// <summary>
        /// 设置是否选中。
        /// </summary>
        /// <param name="v"></param>
        void SetSelected(bool v);
    }

    public class SelectItemBase : UIEventItem<SelectItemBase>, IListSelectItem
    {
        /// <summary>
        /// 索引。
        /// </summary>
        protected int m_itemIndex;

        public int GetItemIndex()
        {
            return m_itemIndex;
        }

        public void SetItemIndex(int i)
        {
            m_itemIndex = i;
        }

        /// <summary>
        /// 是否被选中。
        /// </summary>
        protected bool m_isSelected;

        public virtual bool IsSelected()
        {
            return m_isSelected;
        }

        public virtual void SetSelected(bool v)
        {
            if (m_isSelected == v) return;
            m_isSelected = v;
            UpdateSelect();
        }

        /// <summary>
        /// 刷新选中状态。
        /// </summary>
        public virtual void UpdateSelect()
        {
        }

        protected override void RegisterEvent()
        {
            base.RegisterEvent();
            AddSelectEvt();
        }

        /// <summary>
        /// 监听选中事件。
        /// </summary>
        protected virtual void AddSelectEvt()
        {
            if (Parent == null || !(Parent is IUISelectList)) return;
            var btn = gameObject.GetOrAddComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(OnSelectClick);
            }
        }

        /// <summary>
        /// 选中点击
        /// </summary>
        protected virtual void OnSelectClick()
        {
            var p = Parent as IUISelectList;
            if (p != null)
            {
                p.OnItemClick(this, GetItemIndex());
            }
        }
    }

    interface IUISelectList
    {
        void OnItemClick(object item, int i);
    }

    /// <summary>
    /// UI列表
    /// </summary>
    public class UIListBase<ItemT, DataT> : UIWidget, IUISelectList where ItemT : UIWidget, new()
    {
        /// <summary>
        /// item模板
        /// </summary>
        public GameObject itemBase;

        /// <summary>
        /// 数据列表
        /// </summary>
        protected List<DataT> m_datas;

        /// <summary>
        /// 数据列表
        /// </summary>
        public List<DataT> datas => m_datas;

        /// <summary>
        /// 数据数量
        /// </summary>
        public int DataNum => m_datas?.Count ?? 0;

        /// <summary>
        /// 数量
        /// </summary>
        protected int m_num;

        /// <summary>
        /// 数量
        /// </summary>
        public int num => m_num;

        /// <summary>
        /// 设置数据数量
        /// </summary>
        /// <param name="n"></param>
        /// <param name="funcItem"></param>
        public void SetDataNum(int n, Action<ItemT, int> funcItem = null)
        {
            AdjustItemNum(n, null, funcItem);
        }

        /// <summary>
        /// 数据起始索引
        /// </summary>
        public int dataStartOffset = 0;

        /// <summary>
        /// 设置数据
        /// </summary>
        /// <param name="dataList"></param>
        /// <param name="n"></param>
        public void SetDatas(List<DataT> dataList, int n = -1)
        {
            AdjustItemNum(Mathf.Max(0, n >= 0 ? n : (dataList == null ? 0 : (dataList.Count - dataStartOffset))), dataList);
        }

        /// <summary>
        /// 设置显示数据
        /// </summary>
        /// <param name="n"></param>
        /// <param name="datas"></param>
        /// <param name="funcItem"></param>
        protected virtual void AdjustItemNum(int n, List<DataT> datas = null, Action<ItemT, int> funcItem = null)
        {
            m_num = n;
            m_datas = datas;
            if (itemBase != null)
            {
                itemBase.SetActive(false);
            }
        }

        /// <summary>
        /// 刷新列表ITEM
        /// </summary>
        /// <param name="item"></param>
        /// <param name="i"></param>
        /// <param name="func"></param>
        protected virtual void UpdateListItem(ItemT item, int i, Action<ItemT, int> func)
        {
            if (item == null) return;
            if (func != null)
            {
                func.Invoke(item, i);
                return;
            }

            if (item is IListDataItem<DataT> listDataItem)
            {
                listDataItem.SetItemData(GetData(i));
            }
        }

        /// <summary>
        /// 选中索引
        /// </summary>
        protected int m_selectIndex = -1;

        /// <summary>
        /// Item点击
        /// </summary>
        public Action<int> funcOnItemClick;

        /// <summary>
        /// 选中变化回调函数
        /// </summary>
        public Action funcOnSelectChange;

        /// <summary>
        /// 点击无选中变化回调
        /// </summary>
        public Action funcNoSelectChange;

        /// <summary>
        /// 选中索引
        /// </summary>
        public int selectIndex
        {
            get => m_selectIndex;
            set => SetSelectIndex(value);
        }

        /// <summary>
        /// 设置选中索引
        /// </summary>
        /// <param name="i"></param>
        /// <param name="forceUpdate"></param>
        /// <param name="triggerEvt"></param>
        public void SetSelectIndex(int i, bool forceUpdate = false, bool triggerEvt = true)
        {
            if (!forceUpdate && m_selectIndex == i)
            {
                if (funcNoSelectChange != null)
                {
                    funcNoSelectChange.Invoke();
                }

                return;
            }

            var preIndex = selectIndex;
            m_selectIndex = i;
            if (GetItem(preIndex) is IListSelectItem item)
            {
                item.SetSelected(false);
            }

            item = GetItem(selectIndex) as IListSelectItem;
            if (item != null)
            {
                item.SetSelected(true);
            }
            UpdateSnapTargetItem();
            if (triggerEvt && funcOnSelectChange != null)
            {
                funcOnSelectChange.Invoke();
            }
        }
        
        /// <summary>
        /// 刷新Snap
        /// </summary>
        protected virtual void UpdateSnapTargetItem()
        {
        }

        /// <summary>
        /// 获取当前选中的数据
        /// </summary>
        /// <returns></returns>
        public DataT GetSelectData()
        {
            return GetData(selectIndex);
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public DataT GetData(int i)
        {
            i += dataStartOffset;
            return m_datas == null || i < 0 || i >= m_datas.Count ? default(DataT) : m_datas[i];
        }

        /// <summary>
        /// 获取item
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public virtual ItemT GetItem(int i)
        {
            return null;
        }
        
        /// <summary>
        /// 点击选择
        /// </summary>
        public bool SelectByClick = true;

        /// <summary>
        /// item被点击
        /// </summary>
        /// <param name="item"></param>
        /// <param name="i"></param>
        public void OnItemClick(object item, int i)
        {
            if (funcOnItemClick != null)
            {
                funcOnItemClick.Invoke(i);
            }

            if (SelectByClick)
            {
                selectIndex = i;
            }
        }
    }
}