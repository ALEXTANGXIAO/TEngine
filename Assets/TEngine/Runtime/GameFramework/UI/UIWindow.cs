using System;
using UnityEngine;
using UnityEngine.UI;
using YooAsset;
using Object = UnityEngine.Object;

namespace TEngine
{
    public abstract class UIWindow : UIBase, IUIBehaviour
    {
        private System.Action<UIWindow> _prepareCallback;

        private System.Object[] _userDatas;

        private bool _isCreate = false;

        private GameObject _panel;

        private Canvas _canvas;

        private Canvas[] _childCanvas;

        private GraphicRaycaster _raycaster;

        private GraphicRaycaster[] _childRaycaster;

        public override UIBaseType BaseType => UIBaseType.Window;

        /// <summary>
        /// 窗口矩阵位置组件。
        /// </summary>
        public RectTransform transform => _panel.transform as RectTransform;

        /// <summary>
        /// 窗口的实例资源对象。
        /// </summary>
        public GameObject gameObject => _panel;

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
        /// 是否为全屏窗口
        /// </summary>
        public bool FullScreen { private set; get; }

        /// <summary>
        /// 自定义数据。
        /// </summary>
        public System.Object UserData
        {
            get
            {
                if (_userDatas != null && _userDatas.Length >= 1)
                {
                    return _userDatas[0];
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
        public System.Object[] UserDatas => _userDatas;

        /// <summary>
        /// 窗口深度值
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
                    return _canvas.gameObject.layer == UIComponent.WINDOW_SHOW_LAYER;
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
                    int setLayer = value ? UIComponent.WINDOW_SHOW_LAYER : UIComponent.WINDOW_HIDE_LAYER;
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
        /// 是否加载完毕
        /// </summary>
        internal bool IsLoadDone => Handle.IsDone;

        /// <summary>
        /// 是否准备完毕
        /// </summary>
        public bool IsPrepare { private set; get; }


        public void Init(string name, int layer, bool fullScreen, string assetName)
        {
            WindowName = name;
            WindowLayer = layer;
            FullScreen = fullScreen;
            AssetName = assetName;
        }

        /// <summary>
        /// 代码自动生成绑定。
        /// </summary>
        public virtual void ScriptGenerator()
        {
        }

        /// <summary>
        /// 绑定UI成员元素。
        /// </summary>
        public virtual void BindMemberProperty()
        {
        }

        /// <summary>
        /// 注册事件。
        /// </summary>
        public virtual void RegisterEvent()
        {
        }

        /// <summary>
        /// 窗口创建。
        /// </summary>
        public virtual void OnCreate()
        {
        }

        /// <summary>
        /// 窗口刷新
        /// </summary>
        public virtual void OnRefresh()
        {
        }

        /// <summary>
        /// 窗口更新
        /// </summary>
        public virtual void OnUpdate()
        {
        }

        /// <summary>
        /// 窗口销毁
        /// </summary>
        public virtual void OnDestroy()
        {
        }

        protected virtual void Close()
        {
            GameModule.UI.CloseWindow(this.GetType());
        }

        /// <summary>
        /// 当触发窗口的层级排序
        /// </summary>
        protected virtual void OnSortDepth(int depth)
        {
        }

        /// <summary>
        /// 当因为全屏遮挡触发窗口的显隐
        /// </summary>
        protected virtual void OnSetVisible(bool visible)
        {
        }

        internal void TryInvoke(System.Action<UIWindow> prepareCallback, System.Object[] userDatas)
        {
            _userDatas = userDatas;
            if (IsPrepare)
            {
                prepareCallback?.Invoke(this);
            }
            else
            {
                _prepareCallback = prepareCallback;
            }
        }

        internal void InternalLoad(string location, System.Action<UIWindow> prepareCallback, System.Object[] userDatas)
        {
            if (Handle != null)
            {
                return;
            }

            _prepareCallback = prepareCallback;
            _userDatas = userDatas;
            Handle = YooAssets.LoadAssetAsync<GameObject>(location);
            Handle.Completed += Handle_Completed;
        }

        internal void InternalCreate()
        {
            if (_isCreate == false)
            {
                _isCreate = true;
                OnCreate();
            }
        }

        internal void InternalRefresh()
        {
            OnRefresh();
        }

        internal void InternalUpdate()
        {
            if (IsPrepare)
            {
                OnUpdate();
            }
        }

        internal void InternalDestroy()
        {
            _isCreate = false;

            RemoveAllUIEvent();

            // 注销回调函数
            _prepareCallback = null;

            // 卸载面板资源
            if (Handle != null)
            {
                Handle.Release();
                Handle = null;
            }

            // 销毁面板对象
            if (_panel != null)
            {
                OnDestroy();
                Object.Destroy(_panel);
                _panel = null;
            }
        }

        private void Handle_Completed(AssetOperationHandle handle)
        {
            if (handle.AssetObject == null)
            {
                return;
            }

            // 实例化对象
            _panel = handle.InstantiateSync(UIComponent.UIRootStatic);
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

        #region FindChildComponent

        public Transform FindChild(string path)
        {
            return DUnityUtil.FindChild(transform, path);
        }

        public Transform FindChild(Transform trans, string path)
        {
            return DUnityUtil.FindChild(trans, path);
        }

        public T FindChildComponent<T>(string path) where T : Component
        {
            return DUnityUtil.FindChildComponent<T>(transform, path);
        }

        public T FindChildComponent<T>(Transform trans, string path) where T : Component
        {
            return DUnityUtil.FindChildComponent<T>(trans, path);
        }

        #endregion

        #region UIEvent

        private GameEventMgr _eventMgr;

        protected GameEventMgr EventMgr
        {
            get
            {
                if (_eventMgr == null)
                {
                    _eventMgr = MemoryPool.Acquire<GameEventMgr>();
                }

                return _eventMgr;
            }
        }

        public void AddUIEvent(int eventType, Action handler)
        {
            EventMgr.AddUIEvent(eventType, handler);
        }

        protected void AddUIEvent<T>(int eventType, Action<T> handler)
        {
            EventMgr.AddUIEvent(eventType, handler);
        }

        protected void AddUIEvent<T, U>(int eventType, Action<T, U> handler)
        {
            EventMgr.AddUIEvent(eventType, handler);
        }

        protected void AddUIEvent<T, U, V>(int eventType, Action<T, U, V> handler)
        {
            EventMgr.AddUIEvent(eventType, handler);
        }

        protected void AddUIEvent<T, U, V, W>(int eventType, Action<T, U, V, W> handler)
        {
            EventMgr.AddUIEvent(eventType, handler);
        }

        private void RemoveAllUIEvent()
        {
            if (_eventMgr != null)
            {
                MemoryPool.Release(_eventMgr);
            }
        }

        #endregion
    }
}