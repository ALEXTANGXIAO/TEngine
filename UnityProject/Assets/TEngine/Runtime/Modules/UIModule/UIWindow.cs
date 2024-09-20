using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace TEngine
{
    public abstract class UIWindow : UIBase
    {
        #region Propreties

        private System.Action<UIWindow> _prepareCallback;

        private bool _isCreate = false;

        private GameObject _panel;

        private Canvas _canvas;

        private Canvas[] _childCanvas;

        private GraphicRaycaster _raycaster;

        private GraphicRaycaster[] _childRaycaster;

        public override UIType Type => UIType.Window;

        /// <summary>
        /// 窗口位置组件。
        /// </summary>
        public override Transform transform => _panel.transform;
        
        /// <summary>
        /// 窗口矩阵位置组件。
        /// </summary>
        public override RectTransform rectTransform => _panel.transform as RectTransform;

        /// <summary>
        /// 窗口的实例资源对象。
        /// </summary>
        public override GameObject gameObject => _panel;

        /// <summary>
        /// 窗口名称。
        /// </summary>
        public string WindowName { private set; get; }

        /// <summary>
        /// 窗口层级。
        /// </summary>
        public int WindowLayer { private set; get; }

        /// <summary>
        /// 资源定位地址。
        /// </summary>
        public string AssetName { private set; get; }

        /// <summary>
        /// 是否为全屏窗口。
        /// </summary>
        public virtual bool FullScreen { private set; get; } = false;

        /// <summary>
        /// 是内部资源无需AB加载。
        /// </summary>
        public bool FromResources { private set; get; }
        
        /// <summary>
        /// 隐藏窗口关闭时间。
        /// </summary>
        public int HideTimeToClose { get; set; }
        
        public int HideTimerId { get; set; }

        /// <summary>
        /// 自定义数据。
        /// </summary>
        public System.Object UserData
        {
            get
            {
                if (userDatas != null && userDatas.Length >= 1)
                {
                    return userDatas[0];
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// 自定义数据集。
        /// </summary>
        public System.Object[] UserDatas => userDatas;

        /// <summary>
        /// 窗口深度值。
        /// </summary>
        public int Depth
        {
            get
            {
                if (_canvas != null)
                {
                    return _canvas.sortingOrder;
                }
                else
                {
                    return 0;
                }
            }

            set
            {
                if (_canvas != null)
                {
                    if (_canvas.sortingOrder == value)
                    {
                        return;
                    }

                    // 设置父类
                    _canvas.sortingOrder = value;

                    // 设置子类
                    int depth = value;
                    for (int i = 0; i < _childCanvas.Length; i++)
                    {
                        var canvas = _childCanvas[i];
                        if (canvas != _canvas)
                        {
                            depth += 5; //注意递增值
                            canvas.sortingOrder = depth;
                        }
                    }

                    // 虚函数
                    if (_isCreate)
                    {
                        OnSortDepth(value);
                    }
                }
            }
        }

        /// <summary>
        /// 窗口可见性
        /// </summary>
        public bool Visible
        {
            get
            {
                if (_canvas != null)
                {
                    return _canvas.gameObject.layer == UIModule.WINDOW_SHOW_LAYER;
                }
                else
                {
                    return false;
                }
            }

            set
            {
                if (_canvas != null)
                {
                    int setLayer = value ? UIModule.WINDOW_SHOW_LAYER : UIModule.WINDOW_HIDE_LAYER;
                    if (_canvas.gameObject.layer == setLayer)
                        return;

                    // 显示设置
                    _canvas.gameObject.layer = setLayer;
                    for (int i = 0; i < _childCanvas.Length; i++)
                    {
                        _childCanvas[i].gameObject.layer = setLayer;
                    }

                    // 交互设置
                    Interactable = value;

                    // 虚函数
                    if (_isCreate)
                    {
                        OnSetVisible(value);
                    }
                }
            }
        }

        /// <summary>
        /// 窗口交互性
        /// </summary>
        private bool Interactable
        {
            get
            {
                if (_raycaster != null)
                {
                    return _raycaster.enabled;
                }
                else
                {
                    return false;
                }
            }

            set
            {
                if (_raycaster != null)
                {
                    _raycaster.enabled = value;
                    for (int i = 0; i < _childRaycaster.Length; i++)
                    {
                        _childRaycaster[i].enabled = value;
                    }
                }
            }
        }

        /// <summary>
        /// 是否加载完毕。
        /// </summary>
        internal bool IsLoadDone = false;

        #endregion

        public void Init(string name, int layer, bool fullScreen, string assetName, bool fromResources, int hideTimeToClose)
        {
            WindowName = name;
            WindowLayer = layer;
            FullScreen = fullScreen;
            AssetName = assetName;
            FromResources = fromResources;
            HideTimeToClose = hideTimeToClose;
        }

        internal void TryInvoke(System.Action<UIWindow> prepareCallback, System.Object[] userDatas)
        {
            base.userDatas = userDatas;
            if (IsPrepare)
            {
                prepareCallback?.Invoke(this);
            }
            else
            {
                _prepareCallback = prepareCallback;
            }
            CancelHideToCloseTimer();
        }

        internal async UniTaskVoid InternalLoad(string location, Action<UIWindow> prepareCallback, bool isAsync, System.Object[] userDatas)
        {
            _prepareCallback = prepareCallback;
            this.userDatas = userDatas;
            if (!FromResources)
            {
                if (isAsync)
                {
                    var uiInstance = await GameModule.Resource.LoadGameObjectAsync(location, parent: GameModule.UI.UIRoot);
                    Handle_Completed(uiInstance);
                }
                else
                {
                    var uiInstance = GameModule.Resource.LoadGameObject(location, parent: GameModule.UI.UIRoot);
                    Handle_Completed(uiInstance);
                }
            }
            else
            {
                GameObject panel = Object.Instantiate(Resources.Load<GameObject>(location), GameModule.UI.UIRoot);
                Handle_Completed(panel);
            }
        }

        internal void InternalCreate()
        {
            if (_isCreate == false)
            {
                _isCreate = true;
                ScriptGenerator();
                BindMemberProperty();
                RegisterEvent();
                OnCreate();
            }
        }

        internal void InternalRefresh()
        {
            OnRefresh();
        }

        internal bool InternalUpdate()
        {
            if (!IsPrepare || !Visible)
            {
                return false;
            }

            List<UIWidget> listNextUpdateChild = null;
            if (ListChild != null && ListChild.Count > 0)
            {
                listNextUpdateChild = m_listUpdateChild;
                var updateListValid = m_updateListValid;
                List<UIWidget> listChild = null;
                if (!updateListValid)
                {
                    if (listNextUpdateChild == null)
                    {
                        listNextUpdateChild = new List<UIWidget>();
                        m_listUpdateChild = listNextUpdateChild;
                    }
                    else
                    {
                        listNextUpdateChild.Clear();
                    }

                    listChild = ListChild;
                }
                else
                {
                    listChild = listNextUpdateChild;
                }

                for (int i = 0; i < listChild.Count; i++)
                {
                    var uiWidget = listChild[i];

                    if (uiWidget == null)
                    {
                        continue;
                    }

                    TProfiler.BeginSample(uiWidget.name);
                    var needValid = uiWidget.InternalUpdate();
                    TProfiler.EndSample();

                    if (!updateListValid && needValid)
                    {
                        listNextUpdateChild.Add(uiWidget);
                    }
                }

                if (!updateListValid)
                {
                    m_updateListValid = true;
                }
            }

            TProfiler.BeginSample("OnUpdate");

            bool needUpdate = false;
            if (listNextUpdateChild == null || listNextUpdateChild.Count <= 0)
            {
                HasOverrideUpdate = true;
                OnUpdate();
                needUpdate = HasOverrideUpdate;
            }
            else
            {
                OnUpdate();
                needUpdate = true;
            }

            TProfiler.EndSample();

            return needUpdate;
        }

        internal void InternalDestroy(bool isShutDown = false)
        {
            _isCreate = false;

            RemoveAllUIEvent();

            for (int i = 0; i < ListChild.Count; i++)
            {
                var uiChild = ListChild[i];
                uiChild.CallDestroy();
                uiChild.OnDestroyWidget();
            }

            // 注销回调函数
            _prepareCallback = null;

            OnDestroy();

            // 销毁面板对象
            if (_panel != null)
            {
                Object.Destroy(_panel);
                _panel = null;
            }

            if (!isShutDown)
            {
                CancelHideToCloseTimer();
            }
        }

        /// <summary>
        /// 处理资源加载完成回调。
        /// </summary>
        /// <param name="panel">面板资源实例。</param>
        private void Handle_Completed(GameObject panel)
        {
            if (panel == null)
            {
                return;
            }

            IsLoadDone = true;
            
            panel.name = GetType().Name;
            _panel = panel;
            _panel.transform.localPosition = Vector3.zero;

            // 获取组件
            _canvas = _panel.GetComponent<Canvas>();
            if (_canvas == null)
            {
                throw new Exception($"Not found {nameof(Canvas)} in panel {WindowName}");
            }

            _canvas.overrideSorting = true;
            _canvas.sortingOrder = 0;
            _canvas.sortingLayerName = "Default";

            // 获取组件
            _raycaster = _panel.GetComponent<GraphicRaycaster>();
            _childCanvas = _panel.GetComponentsInChildren<Canvas>(true);
            _childRaycaster = _panel.GetComponentsInChildren<GraphicRaycaster>(true);

            // 通知UI管理器
            IsPrepare = true;
            _prepareCallback?.Invoke(this);
        }

        protected virtual void Close()
        {
            GameModule.UI.CloseUI(this.GetType());
        }

        internal void CancelHideToCloseTimer()
        {
            if (HideTimerId > 0)
            {
                GameModule.Timer.RemoveTimer(HideTimerId);
                HideTimerId = 0;
            }
        }
    }
}