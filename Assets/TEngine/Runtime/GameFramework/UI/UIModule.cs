using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YooAsset;

namespace TEngine
{
    public enum EUIGroup
    {
        Bottom,
        UI,
        Top,
        Tips,
        System,
    }

    /// <summary>
    /// UI模块。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed partial class UIModule : GameFrameworkModuleBase
    {
        private const int DefaultPriority = 0;

        [SerializeField] private Transform m_InstanceRoot = null;

        [SerializeField] private Camera m_UICamera = null;

        [SerializeField] private string m_UIWindowHelperTypeName = "TEngine.DefaultUIWindowHelper";

        [SerializeField] private UIWindowHelperBase mCustomUIWindowHelper = null;

        [SerializeField] private string m_UIGroupHelperTypeName = "TEngine.DefaultUIGroupHelper";

        [SerializeField] private UIGroupHelperBase m_CustomUIGroupHelper = null;

        [SerializeField] private UIGroup[] m_UIGroups = null;

        private readonly List<UIWindow> _stack = new List<UIWindow>(100);

        public const int GROUP_DEEP = 10000;
        public const int WINDOW_DEEP = 100;
        public const int WINDOW_HIDE_LAYER = 2; // Ignore Raycast
        public const int WINDOW_SHOW_LAYER = 5; // UI

        /// <summary>
        /// UI根节点。
        /// </summary>
        public Transform UIRoot => m_InstanceRoot;

        public static Transform UIRootStatic;

        /// <summary>
        /// UI根节点。
        /// </summary>
        public Camera UICamera => m_UICamera;

        /// <summary>
        /// 获取界面组数量。
        /// </summary>
        public int UIGroupCount => m_UIGroups?.Length ?? 0;

        private void Start()
        {
            RootModule rootModule = GameEntry.GetModule<RootModule>();
            if (rootModule == null)
            {
                Log.Fatal("Base component is invalid.");
                return;
            }

            UIWindowHelperBase uiWindowHelper = Helper.CreateHelper(m_UIWindowHelperTypeName, mCustomUIWindowHelper);
            if (uiWindowHelper == null)
            {
                Log.Error("Can not create UI form helper.");
                return;
            }

            uiWindowHelper.name = "UI Form Helper";
            Transform transform = uiWindowHelper.transform;
            transform.SetParent(this.transform);
            transform.localScale = Vector3.one;

            if (m_InstanceRoot == null)
            {
                m_InstanceRoot = new GameObject("UI Form Instances").transform;
                m_InstanceRoot.SetParent(gameObject.transform);
                m_InstanceRoot.localScale = Vector3.one;
            }

            m_InstanceRoot.gameObject.layer = LayerMask.NameToLayer("UI");
            UIRootStatic = m_InstanceRoot;

            for (int i = 0; i < m_UIGroups.Length; i++)
            {
                if (!AddUIGroup(m_UIGroups[i].Name, m_UIGroups[i].Depth))
                {
                    Log.Warning("Add UI group '{0}' failure.", m_UIGroups[i].Name);
                    continue;
                }
            }
        }

        private void OnDestroy()
        {
            CloseAll();
        }

        private void Update()
        {
            int count = _stack.Count;
            for (int i = 0; i < _stack.Count; i++)
            {
                if (_stack.Count != count)
                {
                    break;
                }

                var window = _stack[i];
                window.InternalUpdate();
            }
        }

        /// <summary>
        /// 增加界面组。
        /// </summary>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <param name="depth">界面组深度。</param>
        /// <returns>是否增加界面组成功。</returns>
        public bool AddUIGroup(string uiGroupName, int depth)
        {
            UIGroupHelperBase uiGroupHelper = Helper.CreateHelper(m_UIGroupHelperTypeName, m_CustomUIGroupHelper, m_UIGroups.Length);
            if (uiGroupHelper == null)
            {
                Log.Error("Can not create UI group helper.");
                return false;
            }

            uiGroupHelper.name = Utility.Text.Format("UI Group - {0}", uiGroupName);
            uiGroupHelper.gameObject.layer = LayerMask.NameToLayer("UI");
            Transform transform = uiGroupHelper.transform;
            transform.SetParent(m_InstanceRoot);
            transform.localScale = Vector3.one;
            transform.localPosition = Vector3.zero;

            return true;
        }

        #region 设置安全区域

        /// <summary>
        /// 设置屏幕安全区域（异形屏支持）。
        /// </summary>
        /// <param name="safeRect">安全区域</param>
        public static void ApplyScreenSafeRect(Rect safeRect)
        {
            CanvasScaler scaler = UIRootStatic.GetComponentInParent<CanvasScaler>();
            if (scaler == null)
            {
                Log.Error($"Not found {nameof(CanvasScaler)} !");
                return;
            }

            // Convert safe area rectangle from absolute pixels to UGUI coordinates
            float rateX = scaler.referenceResolution.x / Screen.width;
            float rateY = scaler.referenceResolution.y / Screen.height;
            float posX = (int)(safeRect.position.x * rateX);
            float posY = (int)(safeRect.position.y * rateY);
            float width = (int)(safeRect.size.x * rateX);
            float height = (int)(safeRect.size.y * rateY);

            float offsetMaxX = scaler.referenceResolution.x - width - posX;
            float offsetMaxY = scaler.referenceResolution.y - height - posY;

            // 注意：安全区坐标系的原点为左下角	
            var rectTrans = UIRootStatic.transform as RectTransform;
            if (rectTrans != null)
            {
                rectTrans.offsetMin = new Vector2(posX, posY); //锚框状态下的屏幕左下角偏移向量
                rectTrans.offsetMax = new Vector2(-offsetMaxX, -offsetMaxY); //锚框状态下的屏幕右上角偏移向量
            }
        }

        /// <summary>
        /// 模拟IPhoneX异形屏
        /// </summary>
        public static void SimulateIPhoneXNotchScreen()
        {
            Rect rect;
            if (Screen.height > Screen.width)
            {
                // 竖屏Portrait
                float deviceWidth = 1125;
                float deviceHeight = 2436;
                rect = new Rect(0f / deviceWidth, 102f / deviceHeight, 1125f / deviceWidth, 2202f / deviceHeight);
            }
            else
            {
                // 横屏Landscape
                float deviceWidth = 2436;
                float deviceHeight = 1125;
                rect = new Rect(132f / deviceWidth, 63f / deviceHeight, 2172f / deviceWidth, 1062f / deviceHeight);
            }

            Rect safeArea = new Rect(Screen.width * rect.x, Screen.height * rect.y, Screen.width * rect.width, Screen.height * rect.height);
            ApplyScreenSafeRect(safeArea);
        }

        #endregion

        /// <summary>
        /// 获取所有层级下顶部的窗口名称。
        /// </summary>
        public string GetTopWindow()
        {
            if (_stack.Count == 0)
            {
                return string.Empty;
            }

            UIWindow topWindow = _stack[_stack.Count - 1];
            return topWindow.WindowName;
        }

        /// <summary>
        /// 获取指定层级下顶部的窗口名称。
        /// </summary>
        public string GetTopWindow(int layer)
        {
            UIWindow lastOne = null;
            for (int i = 0; i < _stack.Count; i++)
            {
                if (_stack[i].WindowLayer == layer)
                    lastOne = _stack[i];
            }

            if (lastOne == null)
                return string.Empty;

            return lastOne.WindowName;
        }

        /// <summary>
        /// 是否有任意窗口正在加载。
        /// </summary>
        public bool IsAnyLoading()
        {
            for (int i = 0; i < _stack.Count; i++)
            {
                var window = _stack[i];
                if (window.IsLoadDone == false)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 查询窗口是否存在。
        /// </summary>
        /// <typeparam name="T">界面类型。</typeparam>
        /// <returns>是否存在。</returns>
        public bool HasWindow<T>()
        {
            return HasWindow(typeof(T));
        }

        /// <summary>
        /// 查询窗口是否存在。
        /// </summary>
        /// <param name="type">界面类型。</param>
        /// <returns>是否存在。</returns>
        public bool HasWindow(Type type)
        {
            return IsContains(type.FullName);
        }

        /// <summary>
        /// 异步打开窗口。
        /// </summary>
        /// <param name="userDatas">用户自定义数据。</param>
        /// <returns>打开窗口操作句柄。</returns>
        public OpenWindowOperation ShowUIAsync<T>(params System.Object[] userDatas) where T : UIWindow
        {
            return ShowUIAsync(typeof(T), userDatas);
        }

        /// <summary>
        /// 异步打开窗口。
        /// </summary>
        /// <param name="type">界面类型。</param>
        /// <param name="userDatas">用户自定义数据。</param>
        /// <returns>打开窗口操作句柄。</returns>
        public OpenWindowOperation ShowUIAsync(Type type, params System.Object[] userDatas)
        {
            string windowName = type.FullName;

            // 如果窗口已经存在
            if (IsContains(windowName))
            {
                UIWindow window = GetWindow(windowName);
                Pop(window); //弹出窗口
                Push(window); //重新压入
                window.TryInvoke(OnWindowPrepare, userDatas);
                var operation = new OpenWindowOperation(window.Handle);
                YooAssets.StartOperation(operation);
                return operation;
            }
            else
            {
                UIWindow window = CreateInstance(type);
                Push(window); //首次压入
                window.InternalLoad(window.AssetName, OnWindowPrepare, userDatas);
                var operation = new OpenWindowOperation(window.Handle);
                YooAssets.StartOperation(operation);
                return operation;
            }
        }

        /// <summary>
        /// 同步打开窗口。
        /// </summary>
        /// <typeparam name="T">窗口类。</typeparam>
        /// <param name="userDatas">用户自定义数据。</param>
        /// <returns>打开窗口操作句柄。</returns>
        public OpenWindowOperation ShowUI<T>(params System.Object[] userDatas) where T : UIWindow
        {
            var operation = ShowUIAsync(typeof(T), userDatas);
            operation.WaitForAsyncComplete();
            return operation;
        }

        /// <summary>
        /// 同步打开窗口。
        /// </summary>
        /// <param name="type"></param>
        /// <param name="userDatas"></param>
        /// <returns>打开窗口操作句柄。</returns>
        public OpenWindowOperation ShowUI(Type type, params System.Object[] userDatas)
        {
            var operation = ShowUIAsync(type, userDatas);
            operation.WaitForAsyncComplete();
            return operation;
        }

        /// <summary>
        /// 关闭窗口
        /// </summary>
        public void CloseWindow<T>() where T : UIWindow
        {
            CloseWindow(typeof(T));
        }

        public void CloseWindow(Type type)
        {
            string windowName = type.FullName;
            UIWindow window = GetWindow(windowName);
            if (window == null)
                return;

            window.InternalDestroy();
            Pop(window);
            OnSortWindowDepth(window.WindowLayer);
            OnSetWindowVisible();
        }

        /// <summary>
        /// 关闭所有窗口。
        /// </summary>
        public void CloseAll()
        {
            for (int i = 0; i < _stack.Count; i++)
            {
                UIWindow window = _stack[i];
                window.InternalDestroy();
            }

            _stack.Clear();
        }

        private void OnWindowPrepare(UIWindow window)
        {
            OnSortWindowDepth(window.WindowLayer);
            window.InternalCreate();
            window.InternalRefresh();
            OnSetWindowVisible();
        }

        private void OnSortWindowDepth(int layer)
        {
            int depth = layer;
            for (int i = 0; i < _stack.Count; i++)
            {
                if (_stack[i].WindowLayer == layer)
                {
                    _stack[i].Depth = depth;
                    depth += WINDOW_DEEP;
                }
            }
        }

        private void OnSetWindowVisible()
        {
            bool isHideNext = false;
            for (int i = _stack.Count - 1; i >= 0; i--)
            {
                UIWindow window = _stack[i];
                if (isHideNext == false)
                {
                    window.Visible = true;
                    if (window.IsPrepare && window.FullScreen)
                    {
                        isHideNext = true;
                    }
                }
                else
                {
                    window.Visible = false;
                }
            }
        }

        private UIWindow CreateInstance(Type type)
        {
            UIWindow window = Activator.CreateInstance(type) as UIWindow;
            WindowAttribute attribute = Attribute.GetCustomAttribute(type, typeof(WindowAttribute)) as WindowAttribute;

            if (window == null)
                throw new Exception($"Window {type.FullName} create instance failed.");
            if (attribute == null)
                throw new Exception($"Window {type.FullName} not found {nameof(WindowAttribute)} attribute.");

            string assetName = string.IsNullOrEmpty(attribute.AssetName) ? type.Name : attribute.AssetName;
            window.Init(type.FullName, attribute.WindowLayer, attribute.FullScreen, assetName);
            return window;
        }

        private UIWindow GetWindow(string windowName)
        {
            for (int i = 0; i < _stack.Count; i++)
            {
                UIWindow window = _stack[i];
                if (window.WindowName == windowName)
                {
                    return window;
                }
            }

            return null;
        }

        private bool IsContains(string windowName)
        {
            for (int i = 0; i < _stack.Count; i++)
            {
                UIWindow window = _stack[i];
                if (window.WindowName == windowName)
                {
                    return true;
                }
            }

            return false;
        }

        private void Push(UIWindow window)
        {
            // 如果已经存在
            if (IsContains(window.WindowName))
                throw new System.Exception($"Window {window.WindowName} is exist.");

            // 获取插入到所属层级的位置
            int insertIndex = -1;
            for (int i = 0; i < _stack.Count; i++)
            {
                if (window.WindowLayer == _stack[i].WindowLayer)
                {
                    insertIndex = i + 1;
                }
            }

            // 如果没有所属层级，找到相邻层级
            if (insertIndex == -1)
            {
                for (int i = 0; i < _stack.Count; i++)
                {
                    if (window.WindowLayer > _stack[i].WindowLayer)
                    {
                        insertIndex = i + 1;
                    }
                }
            }

            // 如果是空栈或没有找到插入位置
            if (insertIndex == -1)
            {
                insertIndex = 0;
            }

            // 最后插入到堆栈
            _stack.Insert(insertIndex, window);
        }

        private void Pop(UIWindow window)
        {
            // 从堆栈里移除
            _stack.Remove(window);
        }
    }
}