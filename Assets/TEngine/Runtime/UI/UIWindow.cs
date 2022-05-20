using TEngine;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public enum UIWindowType
    {
        /// <summary> 普通窗口 </summary>
        WindowNormal,

        /// <summary> 置顶窗口 </summary>
        WindowTop,

        /// <summary> 模态窗口 </summary>
        WindowModel
    }


    public class UIWindow : UIWindowBase
    {
        #region 属性
        private bool m_isClosed = false;
        private bool m_isCreating = false;
        private Image m_modalImage;
        private float m_modalAlpha = 0.86f;

        /// <summary>
        /// 是否固定SortingOrder
        /// </summary>
        public virtual bool IsFixedSortingOrder
        {
            get { return false; }
        }

        public virtual bool NeedCenterUI()
        {
            return true;
        }

        /// <summary>
        /// 窗口Id
        /// </summary>
        private uint m_windowId = 0;

        /// <summary>
        /// 窗口Id
        /// </summary>
        public uint WindowId
        {
            get { return m_windowId; }
        }

        private static uint m_nextWindowId = 0;

        public virtual bool IsFullScreen
        {
            get { return false; }
        }

        public virtual bool IsFullScreenMaskScene
        {
            get { return false; }
        }

        /// <summary>
        /// 一个界面中最大sortOrder值
        /// </summary>
        public const int MaxCanvasSortingOrder = 50;

        /// <summary>
        /// SortingOrder = stack.m_baseOrder + FixedAdditionalOrder
        /// </summary>
        public virtual int FixedAdditionalOrder
        {
            get { return UIWindow.MaxCanvasSortingOrder; }
        }

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
        #endregion


        public void AllocWindowId()
        {
            if (m_nextWindowId == 0)
            {
                m_nextWindowId++;
            }

            m_windowId = m_nextWindowId++;
        }

        #region virtual function

        public virtual UIWindowType GetWindowType()
        {
            return UIWindowType.WindowNormal;
        }

        #endregion

        #region Call From UIManager
        public bool Create(UIManager uiMgr, GameObject uiGo)
        {
            if (IsCreated)
            {
                return true;
            }

            m_isClosed = false;
            if (!CreateBase(uiGo, true))
            {
                return false;
            }

            if (m_canvas == null)
            {
                Debug.LogErrorFormat("{0} have not a canvas!!", this.ToString());
                Destroy();
                return false;
            }

            m_ownUIManager = uiMgr;
            m_firstVisible = false;

            if (m_canvas != null)
            {
                m_canvas.overrideSorting = true;
            }

            ScriptGenerator();
            BindMemberProperty();
            RegisterEvent();

            m_isCreating = true;
            OnCreate();
            m_isCreating = false;

            if (m_isClosed)
            {
                Destroy();
                return false;
            }

            SetModalState(GetModalType());
            return true;
        }

        public virtual ModalType GetModalType()
        {
            if (IsFullScreen || GetWindowType() == UIWindowType.WindowTop)
            {
                return ModalType.TransparentType;
            }
            return ModalType.NormalType;
        }

        private void SetModalState(ModalType type)
        {
            var canClose = false;
            switch (type)
            {
                case ModalType.NormalType:
                    {
                        m_modalAlpha = 0f;
                        break;
                    }
                case ModalType.NormalHaveClose:
                    {
                        canClose = true;
                        break;
                    }
                case ModalType.TransparentType:
                    {
                        m_modalAlpha = 0.01f;
                        break;
                    }
                case ModalType.TransparentHaveClose:
                    {
                        m_modalAlpha = 0.01f;
                        canClose = true;
                        break;
                    }
                default:
                    {
                        m_modalAlpha = 0f;
                        break;
                    }
            }
            if (m_modalAlpha > 0)
            {
                string path = "UI/ModalSprite.prefab";
                GameObject modal = TResources.Load(path);
                modal.transform.SetParent(transform);
                modal.transform.SetAsFirstSibling();
                modal.transform.localScale = Vector3.one;
                modal.transform.localPosition = Vector3.zero;
                if (canClose)
                {
                    var button = UnityUtil.AddMonoBehaviour<Button>(modal);
                    button.onClick.AddListener(Close);
                }
                m_modalImage = UnityUtil.AddMonoBehaviour<Image>(modal);
                m_modalImage.color = new Color(0, 0, 0, m_modalAlpha);
            }
        }

        public virtual void Close()
        {
            if (m_isCreating)
            {
                m_isClosed = true;
                return;
            }

            var mgr = UIMgr;
            if (mgr != null)
            {
                mgr.CloseWindow(this);
            }
        }
        #endregion
    }

    public enum ModalType
    {
        /// <summary> 普通模态 </summary>
        NormalType,

        /// <summary> 透明模态 </summary>
        TransparentType,

        /// <summary> 普通状态且有关闭功能 </summary>
        NormalHaveClose,

        /// <summary> 透明状态且有关闭功能 </summary>
        TransparentHaveClose,

        /// <summary> 非模态 </summary>
        NoneType,
    }

    public enum UIWindowBaseType
    {
        None,
        Window,
        Widget,
    }

}