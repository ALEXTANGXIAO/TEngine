using System;
using System.Collections.Generic;
using TEngine;
using UnityEngine;

namespace GameLogic
{
    public class SwitchPageMgr
    {
        private event Action<int, int> SwitchTabAction;

        public delegate bool SwitchTabCondition(int selectId);

        private SwitchTabCondition _switchTabCondition;

        /// <summary>
        /// 页签Grid。
        /// </summary>
        protected Transform m_tabGrid;

        /// <summary>
        /// 子UI父节点。
        /// </summary>
        public Transform ChildPageParent;

        /// <summary>
        /// 存储子UI。
        /// </summary>
        private readonly Dictionary<int, List<string>> _switchPageDic = new Dictionary<int, List<string>>();

        /// <summary>
        /// 子页签名字。
        /// </summary>
        private readonly List<string> _childPageNames = new List<string>();

        /// <summary>
        /// 子页签字典。
        /// </summary>
        private readonly Dictionary<string, ChildPageBase> _childPageDic = new Dictionary<string, ChildPageBase>();

        /// <summary>
        /// 存储页签。
        /// </summary>
        private readonly Dictionary<int, SwitchTabItem> _tabDic = new Dictionary<int, SwitchTabItem>();

        private readonly Dictionary<int, string> _tabName = new Dictionary<int, string>();

        protected readonly List<int> IDList = new List<int>();

        private readonly UIBase _owner;

        protected int CurrentChildID = -100;

        /// <summary>
        /// 需要设置显示隐藏。
        /// </summary>
        private readonly bool _needSetActive;

        /// <summary>
        /// 子界面的共享数据。
        /// </summary>
        private readonly ChildPageSharData _shareData = new ChildPageSharData();

        public object ShareData1 => _shareData.Param1;
        public object ShareData2 => _shareData.Param2;
        public object ShareData3 => _shareData.Param3;

        public SwitchPageMgr(Transform grid, Transform childPageParent, UIBase owner, bool needSetActive = true)
        {
            m_tabGrid = grid;
            ChildPageParent = childPageParent;
            _owner = owner;
            _needSetActive = needSetActive;
        }

        public void AddSwitchAction(Action<int, int> action)
        {
            SwitchTabAction += action;
        }

        public void RemoveSwitchAction(Action<int, int> action)
        {
            SwitchTabAction -= action;
        }

        public void AddSwitchCondition(SwitchTabCondition action)
        {
            _switchTabCondition = action;
        }

        public void BindChildPage<T>(int childID) where T : ChildPageBase, new()
        {
            BindChildPage<T>(childID, string.Empty);
        }

        public void BindChildPage<T>(int childID, string tabName) where T : ChildPageBase, new()
        {
            var pageName = typeof(T).Name;

            if (IDList.IndexOf(childID) < 0)
            {
                IDList.Add(childID);
            }

            if (!_childPageDic.ContainsKey(pageName))
            {
                _childPageDic[pageName] = new T();
                _childPageNames.Add(pageName);
            }

            if (!_switchPageDic.ContainsKey(childID))
            {
                _switchPageDic[childID] = new List<string>();
            }

            if (_switchPageDic[childID].IndexOf(pageName) < 0)
            {
                _switchPageDic[childID].Add(pageName);
            }

            _tabName[childID] = tabName;
        }

        public T CreatTab<T>(int initChildID, GameObject tabTemp = null) where T : SwitchTabItem, new()
        {
            T tab = null;
            for (var index = 0; index < IDList.Count; index++)
            {
                var childID = IDList[index];
                if (!_tabDic.ContainsKey(childID))
                {
                    if (tabTemp != null)
                    {
                        tab = _owner.CreateWidgetByPrefab<T>(tabTemp, m_tabGrid);
                    }
                    else
                    {
                        tab = _owner.CreateWidgetByType<T>(m_tabGrid);
                    }

                    tab.UpdateTabName(_tabName[childID]);
                    tab.BindClickEvent(TabOnClick, childID);
                    _tabDic[childID] = tab;
                    break;
                }
            }

            SwitchPage(initChildID);
            return tab;
        }

        public void CreatTabByItem<T>(int initChildID, GameObject go) where T : SwitchTabItem, new()
        {
            if (!_tabDic.ContainsKey(initChildID))
            {
                T tab = _owner.CreateWidgetByPrefab<T>(go, m_tabGrid);
                tab.UpdateTabName(_tabName[initChildID]);
                tab.BindClickEvent(TabOnClick, initChildID);
                _tabDic[initChildID] = tab;
            }
        }

        // 设置页签的自定义点击行为
        public void SetCustomTabClickAction(int tabIdx, Action<SwitchTabItem> clickAction, object param1 = null,
            object param2 = null, object param3 = null)
        {
            _tabDic[tabIdx].BindClickEvent(clickAction, param1, param2, param3);
        }

        public int TabCount => _tabDic.Count;

        public void SetTabRedNode(int tabId, bool isShow)
        {
            if (_tabDic.TryGetValue(tabId, out var value))
            {
                value.SetRedNote(isShow);
            }
        }

        public void SetTabFontSize(int fontSize)
        {
            for (int i = 0; i < IDList.Count; i++)
            {
                var tabId = IDList[i];
                _tabDic[tabId].SetITabTextFontSize(fontSize);
            }
        }

        private void TabOnClick(SwitchTabItem tab)
        {
            var childID = (int)tab.EventParam1;
            SwitchPage(childID);
        }

        public virtual void SwitchPage(int selectID)
        {
            if (_switchTabCondition != null && !_switchTabCondition(selectID))
            {
                return;
            }

            if (CurrentChildID != selectID)
            {
                if (_switchPageDic.TryGetValue(selectID, out var pageLs))
                {
                    for (int i = 0; i < pageLs.Count; i++)
                    {
                        var pageName = pageLs[i];
                        ChildPageBase page = GetChildPageByName(pageName);
                        if (page != null && page.gameObject == null)
                        {
                            page.CreateByPath(pageName, _owner, ChildPageParent);
                            page.InitData(_shareData);
                        }
                    }

                    for (int i = 0; i < _childPageNames.Count; i++)
                    {
                        string pageName = _childPageNames[i];
                        ChildPageBase page = GetChildPageByName(pageName);
                        bool beShow = pageLs.IndexOf(pageName) >= 0;
                        if (page != null && page.gameObject != null)
                        {
                            if (_needSetActive)
                            {
                                //page.Show(beShow);
                                if (beShow)
                                {
                                    page.gameObject.SetActive(true);
                                    // page.Visible = true;
                                }
                                else
                                {
                                    page.gameObject.SetActive(false);
                                    // page.Visible = false;
                                }
                            }
                        }

                        // if (page != null && beShow)
                        if (page != null)
                        {
                            page.OnPageShowed(CurrentChildID, selectID);
                        }
                    }
                }

                for (var index = 0; index < IDList.Count; index++)
                {
                    var childID = IDList[index];
                    SwitchTabItem tab;
                    if (_tabDic.TryGetValue(childID, out tab))
                    {
                        tab.SetState(selectID == childID);
                    }
                }
            }

            var oldID = CurrentChildID;
            CurrentChildID = selectID;
            if (SwitchTabAction != null)
            {
                SwitchTabAction(oldID, selectID);
            }
        }

        public virtual void ShowPage(int selectID)
        {
            if (_switchPageDic.TryGetValue(selectID, out var pageLs))
            {
                for (int i = 0; i < pageLs.Count; i++)
                {
                    var pageName = pageLs[i];
                    ChildPageBase page = GetChildPageByName(pageName);
                    if (page != null && page.gameObject == null)
                    {
                        page.CreateByPath(pageName, _owner, ChildPageParent);
                        page.InitData(_shareData);
                    }
                }

                for (int i = 0; i < _childPageNames.Count; i++)
                {
                    string pageName = _childPageNames[i];
                    ChildPageBase page = GetChildPageByName(pageName);
                    bool beShow = pageLs.IndexOf(pageName) >= 0;
                    if (page != null && page.gameObject != null)
                    {
                        if (beShow)
                        {
                            page.gameObject.SetActive(true);
                            // page.Visible = true;
                        }
                        else
                        {
                            page.gameObject.SetActive(false);
                            // page.Visible = false;
                        }
                    }
                }
            }

            for (var index = 0; index < IDList.Count; index++)
            {
                var childID = IDList[index];
                SwitchTabItem tab;
                if (_tabDic.TryGetValue(childID, out tab))
                {
                    tab.SetState(selectID == childID);
                }
            }
        }

        public void RefreshCurrentChildPage()
        {
            if (_switchPageDic.TryGetValue(CurrentChildID, out var pageNames))
            {
                for (int i = 0; i < pageNames.Count; i++)
                {
                    ChildPageBase page = GetChildPageByName(pageNames[i]);
                    if (page != null && page.gameObject != null)
                    {
                        page.RefreshPage();
                    }
                }
            }
        }

        public void RefreshChildPage(int childID)
        {
            if (_switchPageDic.TryGetValue(childID, out var pageNames))
            {
                for (int i = 0; i < pageNames.Count; i++)
                {
                    ChildPageBase page = GetChildPageByName(pageNames[i]);
                    if (page != null && page.gameObject != null)
                    {
                        page.RefreshPage();
                    }
                }
            }
        }

        public bool TryGetChildPage<T>(out T t) where T : ChildPageBase
        {
            t = GetChildPage<T>();
            return t != null;
        }

        public int GetCurShowType()
        {
            return CurrentChildID;
        }

        public void ReductionShowType()
        {
            CurrentChildID = -100;
        }

        #region 没有ChildPage，可通过m_switchTabAction来处理切页事件(也可以光展示用)

        public SwitchTabItem GetTabItem(int childId)
        {
            _tabDic.TryGetValue(childId, out var item);
            return item;
        }

        public void CreateJumpTab<T>(int initChildID, string tabName, GameObject tabTemp = null)
            where T : SwitchTabItem, new()
        {
            if (IDList.IndexOf(initChildID) < 0)
            {
                IDList.Add(initChildID);
            }

            _tabName[initChildID] = tabName;

            for (var index = 0; index < IDList.Count; index++)
            {
                var childID = IDList[index];
                if (!_tabDic.ContainsKey(childID))
                {
                    T tab;
                    if (tabTemp != null)
                    {
                        tab = _owner.CreateWidgetByPrefab<T>(tabTemp, m_tabGrid);
                    }
                    else
                    {
                        tab = _owner.CreateWidgetByType<T>(m_tabGrid);
                    }

                    tab.UpdateTabName(_tabName[childID]);
                    tab.BindClickEvent(TabOnClick, childID);
                    _tabDic[childID] = tab;
                }
            }

            SwitchPage(initChildID);
        }

        #endregion

        /// <summary>
        /// 设置共享参数。
        /// </summary>
        /// <param name="paramIdx"></param>
        /// <param name="param"></param>
        public void SetShareParam(int paramIdx, object param)
        {
            _shareData.SetParam(paramIdx, param);
        }

        public T GetChildPage<T>() where T : ChildPageBase
        {
            var pageName = typeof(T).Name;
            _childPageDic.TryGetValue(pageName, out var page);
            return page as T;
        }

        public ChildPageBase GetChildPageByName(string pageName)
        {
            _childPageDic.TryGetValue(pageName, out var page);
            return page;
        }

        public void BindChildPage<T, U>(int childID, string tabName)
            where T : ChildPageBase, new()
            where U : ChildPageBase, new()
        {
            BindChildPage<T>(childID, tabName);
            BindChildPage<U>(childID, tabName);
        }

        public void BindChildPage<T, U, V>(int childID, string tabName)
            where T : ChildPageBase, new()
            where U : ChildPageBase, new()
            where V : ChildPageBase, new()
        {
            BindChildPage<T>(childID, tabName);
            BindChildPage<U>(childID, tabName);
            BindChildPage<V>(childID, tabName);
        }

        public void BindChildPage<T, U, V, W>(int childID, string tabName)
            where T : ChildPageBase, new()
            where U : ChildPageBase, new()
            where V : ChildPageBase, new()
            where W : ChildPageBase, new()
        {
            BindChildPage<T>(childID, tabName);
            BindChildPage<U>(childID, tabName);
            BindChildPage<V>(childID, tabName);
            BindChildPage<W>(childID, tabName);
        }
    }
}