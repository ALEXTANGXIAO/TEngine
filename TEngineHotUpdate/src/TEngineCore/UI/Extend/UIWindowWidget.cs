using TEngineCore;
using UnityEngine;

namespace TEngineCore
{
    /// <summary>
    /// 用来封装各个界面里子模块用
    /// </summary>
    public class UIWindowWidget : UIWindowBase
    {
        public int SortingOrder
        {
            get
            {
                if (m_canvas != null)
                {
                    return m_canvas.sortingOrder;
                }

                return 0;
            }

            set
            {
                if (m_canvas != null)
                {
                    int oldOrder = m_canvas.sortingOrder;
                    if (oldOrder != value)
                    {
                        var listCanvas = gameObject.GetComponentsInChildren<Canvas>(true);
                        for (int i = 0; i < listCanvas.Length; i++)
                        {
                            var childCanvas = listCanvas[i];
                            childCanvas.sortingOrder = value + (childCanvas.sortingOrder - oldOrder);
                        }
                        m_canvas.sortingOrder = value;
                        _OnSortingOrderChg();
                    }
                }
            }
        }
        /// <summary>
        /// 所属的窗口
        /// </summary>
        public UIWindow OwnerWindow
        {
            get
            {
                var parent = m_parent;
                while (parent != null)
                {
                    if (parent.BaseType == UIWindowBaseType.Window)
                    {
                        return parent as UIWindow;
                    }

                    parent = parent.Parent;
                }

                return null;
            }
        }

        public override UIWindowBaseType BaseType
        {
            get { return UIWindowBaseType.Widget; }
        }

        /// <summary> 根据类型创建 </summary>
        public bool CreateByType<T>(UIWindowBase parent, Transform parentTrans = null) where T : UIWindowWidget
        {
            string resPath = string.Format("UI/{0}.prefab", typeof(T).Name);
            return CreateByPath(resPath, parent, parentTrans);
        }

        /// <summary> 根据资源名创建 </summary>
        public bool CreateByPath(string resPath, UIWindowBase parent, Transform parentTrans = null, bool visible = true)
        {
            GameObject goInst = TResources.Load(resPath, parentTrans);
            if (goInst == null)
            {
                return false;
            }
            if (!Create(parent, goInst, visible))
            {
                return false;
            }
            goInst.transform.localScale = Vector3.one;
            goInst.transform.localPosition = Vector3.zero;
            return true;
        }

        /**
         * 根据prefab或者模版来创建新的 widget
         */
        public bool CreateByPrefab(UIWindowBase parent, GameObject goPrefab, Transform parentTrans, bool visible = true)
        {
            if (parentTrans == null)
            {
                parentTrans = parent.transform;
            }

            var widgetRoot = GameObject.Instantiate(goPrefab, parentTrans);
            return CreateImp(parent, widgetRoot, true, visible);
        }

        /**
         * 创建窗口内嵌的界面
         */
        public bool Create(UIWindowBase parent, GameObject widgetRoot, bool visible = true)
        {
            return CreateImp(parent, widgetRoot, false, visible);
        }

        #region 私有的函数

        private bool CreateImp(UIWindowBase parent, GameObject widgetRoot, bool bindGo, bool visible = true)
        {
            if (!CreateBase(widgetRoot, bindGo))
            {
                return false;
            }
            RestChildCanvas(parent);
            m_parent = parent;
            if (m_parent != null)
            {
                m_parent.AddChild(this);
            }

            if (m_canvas != null)
            {
                m_canvas.overrideSorting = true;
            }
            ScriptGenerator();
            BindMemberProperty();
            RegisterEvent();

            OnCreate();

            if (visible)
            {
                Show(true);
            }
            else
            {
                widgetRoot.SetActive(false);
            }

            return true;
        }

        private void RestChildCanvas(UIWindowBase parent)
        {
            if (gameObject == null)
            {
                return;
            }
            if (parent == null || parent.gameObject == null)
            {
                return;
            }
            Canvas parentCanvas = parent.gameObject.GetComponentInParent<Canvas>();
            if (parentCanvas == null)
            {
                return;
            }
            var listCanvas = gameObject.GetComponentsInChildren<Canvas>(true);
            for (var index = 0; index < listCanvas.Length; index++)
            {
                var childCanvas = listCanvas[index];
                childCanvas.sortingOrder = parentCanvas.sortingOrder + childCanvas.sortingOrder % UIWindow.MaxCanvasSortingOrder;
            }
        }

        #endregion
    }

}