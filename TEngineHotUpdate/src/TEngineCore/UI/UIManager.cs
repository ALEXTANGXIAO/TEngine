using System.Collections.Generic;
using TEngineCore;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TEngineCore
{
    public enum WindowStackIndex
    {
        StackNormal = 0,
        StackTop = 1,
        StackMax
    };

    public class UIWindowStack
    {
        public WindowStackIndex m_stackIndex;
        public int m_baseOrder = 0;
        public List<uint> m_windowList = new List<uint>();
        public Transform m_parentTrans;

        public int FindIndex(uint windowId)
        {
            for (int i = 0; i < m_windowList.Count; i++)
            {
                if (m_windowList[i] == windowId)
                {
                    return i;
                }
            }

            return -1;
        }
    }

    public sealed partial class UIManager : TSingleton<UIManager>
    {
        private Transform m_canvasTrans;
        public Canvas m_canvas;
        private Dictionary<uint, UIWindow> m_allWindow = new Dictionary<uint, UIWindow>();

        /// <summary>
        /// 不同显示顺序的的窗口列表
        /// </summary>
        private UIWindowStack[] m_listWindowStack = new UIWindowStack[(int)WindowStackIndex.StackMax];

        /// <summary>
        /// 类型到实例的索引
        /// </summary>
        private static Dictionary<string, UIWindow> m_typeToInst = new Dictionary<string, UIWindow>();

        /// <summary>
        /// 类型和资源的绑定关系
        /// </summary>
        private Dictionary<string, string> m_uiType2PrefabPath = new Dictionary<string, string>();

        private GameObject m_uiManagerGo;
        private Transform m_uiManagerTransform;
        private Camera m_uiCamera;
        public Camera Camera => m_uiCamera;

        public UIManager()
        {
            m_uiManagerGo = TResources.Load("UI/UIRoot.prefab");
            Object.DontDestroyOnLoad(m_uiManagerGo);
            m_uiManagerTransform = m_uiManagerGo.transform;
            m_uiCamera = UnityUtil.FindChildComponent<Camera>(m_uiManagerTransform, "Camera");
            Canvas canvas = m_uiManagerGo.GetComponentInChildren<Canvas>();
            if (canvas != null)
            {
                m_canvas = canvas;
                m_canvasTrans = canvas.transform;
            }

            var windowRoot = m_canvasTrans;

            int baseOrder = 1000;
            for (int i = 0; i < (int)WindowStackIndex.StackMax; i++)
            {
                m_listWindowStack[i] = new UIWindowStack();
                m_listWindowStack[i].m_stackIndex = (WindowStackIndex)i;
                m_listWindowStack[i].m_baseOrder = baseOrder;
                m_listWindowStack[i].m_parentTrans = windowRoot;
                baseOrder += 1000;
            }
            CalcCameraRect();
        }

        void CalcCameraRect()
        {
            CanvasScaler canvasScale = m_canvas.GetComponent<CanvasScaler>();
            if (canvasScale != null)
            {
                canvasScale.referenceResolution = new Vector2(UISys.DesginWidth, UISys.DesginHeight);
                float sceneScale = Screen.width / (float)Screen.height;
                float designScale = canvasScale.referenceResolution.x / canvasScale.referenceResolution.y;
                canvasScale.matchWidthOrHeight = sceneScale > designScale ? 1 : 0;
            }
        }


        public void Update()
        {
            var allList = GetAllWindowList();
            for (int i = 0; i < allList.Count; i++)
            {
                UIWindow window = allList[i];
                if (!window.IsDestroyed)
                {
                    window.Update();
                }
            }
        }


        private List<UIWindow> m_tmpWindowList = new List<UIWindow>();
        private bool m_tmpWindowListDirty = true;
        private List<UIWindow> GetAllWindowList()
        {
            if (m_tmpWindowListDirty)
            {
                m_tmpWindowList.Clear();
                var itr = m_allWindow.GetEnumerator();
                while (itr.MoveNext())
                {
                    var kv = itr.Current;
                    m_tmpWindowList.Add(kv.Value);
                }
                m_tmpWindowListDirty = false;
            }
            return m_tmpWindowList;
        }

        #region Methods
        public T ShowWindow<T>(bool isAsync = false) where T : UIWindow, new()
        {
            string typeName = GetWindowTypeName<T>();

            T window = GetUIWindowByType(typeName) as T;
            if (window == null)
            {
                window = new T();
                if (!CreateWindowByType(window, typeName, isAsync))
                {
                    return null;
                }
            }
            ShowWindow(window, -1);
            return window;
        }

        public string GetWindowTypeName<T>()
        {
            string typeName = typeof(T).Name;
            return typeName;
        }

        public string GetWindowTypeName(UIWindow window)
        {
            string typeName = window.GetType().Name;
            return typeName;
        }
        public void CloseWindow<T>() where T : UIWindow
        {
            string typeName = GetWindowTypeName<T>();
            CloseWindow(typeName);
        }
        public T GetWindow<T>() where T : UIWindow
        {
            string typeName = GetWindowTypeName<T>();
            UIWindow window = GetUIWindowByType(typeName);
            if (window != null)
            {
                return window as T;
            }

            return null;
        }

        public UIWindow GetUIWindowByType(string typeName)
        {
            UIWindow window;
            if (m_typeToInst.TryGetValue(typeName, out window))
            {
                return window;
            }

            return null;
        }
        #endregion

        private bool CreateWindowByType(UIWindow window, string typeName,bool async = false)
        {
            //先判断是否有缓存
            GameObject uiGo = null;

            string resPath = string.Format("{0}.prefab", GetUIResourcePath(typeName));
            if (string.IsNullOrEmpty(resPath))
            {
                Debug.LogErrorFormat("CreateWindowByType failed, typeName:{0}, cant find respath", typeName);
                return false;
            }

            UIWindowStack windowStack = GetUIWindowStack(window);

            if (async)
            {
                TResources.LoadAsync(resPath, (obj) =>
                {
                    if (obj == null)
                    {
                        Debug.LogErrorFormat("CreateWindowByType failed, typeName:{0}, load prefab failed: {1}", typeName, resPath);
                    }

                    if (obj != null && windowStack.m_parentTrans != null)
                    {
                        obj.transform.SetParent(windowStack.m_parentTrans);
                    }

                    obj.name = typeName;

                    window.AllocWindowId();

                    var rectTrans_ = obj.transform as RectTransform;
                    if (window.NeedCenterUI())
                    {
                        rectTrans_.SetMax(); //localPosition = new Vector3(0, 0, 0);
                    }

                    rectTrans_.localRotation = Quaternion.identity;
                    rectTrans_.localScale = Vector3.one;

                    if (!window.Create(this, obj))
                    {
                        Debug.LogErrorFormat("window create failed, typeName:{0}", typeName);
                        if (obj != null)
                        {
                            Object.Destroy(obj);
                            obj = null;
                        }
                    }

                    m_typeToInst[typeName] = window;
                    m_allWindow[window.WindowId] = window;
                    m_tmpWindowListDirty = true;
                });
                return true;
            }

            uiGo = TResources.Load(resPath, windowStack.m_parentTrans);
            if (uiGo == null)
            {
                Debug.LogErrorFormat("CreateWindowByType failed, typeName:{0}, load prefab failed: {1}", typeName, resPath);
                //UISys.Mgr.ShowTipMsg(TextDefine.DOWNLOAD_TIP_UI);
                //GameEvent.Get<IHomePageUI>().ShowDownloadUI();
                return false;
            }

            uiGo.name = typeName;

            window.AllocWindowId();

            RectTransform rectTrans = uiGo.transform as RectTransform;
            if (window.NeedCenterUI())
            {
                rectTrans.SetMax(); //localPosition = new Vector3(0, 0, 0);
            }

            rectTrans.localRotation = Quaternion.identity;
            rectTrans.localScale = Vector3.one;

            if (!window.Create(this, uiGo))
            {
                Debug.LogErrorFormat("window create failed, typeName:{0}", typeName);
                if (uiGo != null)
                {
                    Object.Destroy(uiGo);
                    uiGo = null;
                }
                return false;
            }

            m_typeToInst[typeName] = window;
            m_allWindow[window.WindowId] = window;
            m_tmpWindowListDirty = true;
            return true;
        }

        #region MyRegion
        private string GetUIResourcePath(string typeName)
        {
            string resPath;
            if (m_uiType2PrefabPath.TryGetValue(typeName, out resPath))
            {
                return resPath;
            }

            string path = string.Format("UI/{0}", typeName);
            m_uiType2PrefabPath.Add(typeName, path);
            return path;
        }

        private void ShowWindow(UIWindow window, int showIndex)
        {
            UIWindowStack windowStack = GetUIWindowStack(window);
            List<uint> windowList = windowStack.m_windowList;
            int resortIndex = -1;
            int findIndex = windowList.IndexOf(window.WindowId);
            if (findIndex >= 0)
            {
                windowList.RemoveAt(findIndex);
                resortIndex = findIndex;
            }

            windowList.Add(window.WindowId);

            ResortStackUI(windowStack, resortIndex);
            ShowTopUI(windowStack);
        }

        private void ResortStackUI(UIWindowStack stack, int startIdx)
        {
            if (stack.m_windowList.Count > 0)
            {
                startIdx = startIdx < 0 ? (stack.m_windowList.Count - 1) : startIdx;
                for (int i = startIdx; i < stack.m_windowList.Count; i++)
                {
                    uint windowId = stack.m_windowList[i];
                    UIWindow window = FindWindow(windowId);
                    if (window != null)
                    {
                        int order;
                        if (window.IsFixedSortingOrder)
                        {
                            order = stack.m_baseOrder + window.FixedAdditionalOrder;
                        }
                        else
                        {
                            order = stack.m_baseOrder + i * UIWindow.MaxCanvasSortingOrder;
                        }

                        window.SortingOrder = order;
                    }
                }
            }
        }

        private void ShowTopUI(UIWindowStack stack)
        {
            if (stack.m_windowList.Count > 0)
            {
                bool hasTop = false;
                for (int i = stack.m_windowList.Count - 1; i >= 0; i--)
                {
                    uint windowId = stack.m_windowList[i];
                    UIWindow window = FindWindow(windowId);
                    if (window != null)
                    {
                        if (!hasTop)
                        {
                            hasTop = window.IsFullScreen;

                            window.Show(true);
                        }
                        else
                        {
                            window.Show(false);
                        }
                    }
                }
            }
            OnWindowVisibleChanged();
        }

        private void OnWindowVisibleChanged()
        {
            bool isFullScreenMaskscene = false;
            for (int i = 0; i < m_listWindowStack.Length && !isFullScreenMaskscene; i++)
            {
                var stack = m_listWindowStack[i];
                if (stack == null)
                {
                    continue;
                }

                var listWindow = stack.m_windowList;
                for (int k = 0; k < listWindow.Count; k++)
                {
                    var winId = listWindow[k];
                    var win = FindWindow(winId);
                    if (win == null || !win.Visible)
                    {
                        continue;
                    }

                    if (win.IsFullScreenMaskScene)
                    {
                        isFullScreenMaskscene = true;
                        break;
                    }
                }
            }
            //SceneSys.Instance.CameraMgr.SetSceneCameraEnableByUI(true);
        }

        public UIWindow FindWindow(uint windowId)
        {
            UIWindow window;
            if (m_allWindow.TryGetValue(windowId, out window))
            {
                return window;
            }

            return null;
        }

        public void CloseWindow(string typeName)
        {
            UIWindow window = GetUIWindowByType(typeName);
            if (window != null)
            {
                CloseWindow(window);
            }
        }

        public void CloseWindow(UIWindow window)
        {
            if (window.IsDestroyed)
            {
                return;
            }

            //刷新窗口order，保证新创建的窗口不会出现重叠
            UIWindowStack windowStack = GetUIWindowStack(window);

            int findIndex = windowStack.FindIndex(window.WindowId);

            //window.Destroy();
            DestroyWindowObject(window);

            ResortStackUI(windowStack, findIndex);
            ShowTopUI(windowStack);
        }

        private void DestroyWindowObject(UIWindow window)
        {
            string typeName = window.GetType().Name;
            UIWindow typeWindow = null;
            if (m_typeToInst.TryGetValue(typeName, out typeWindow) && typeWindow == window)
            {
                m_typeToInst.Remove(typeName);
            }
            uint windowId = window.WindowId;
            m_allWindow.Remove(windowId);
            UIWindowStack windowStack = GetUIWindowStack(window);
            windowStack.m_windowList.Remove(windowId);
            window.Destroy();
            m_tmpWindowListDirty = true;
        }


        private int GetIndexByWindowType(UIWindowType windowType)
        {
            if (windowType == UIWindowType.WindowTop)
            {
                return (int)WindowStackIndex.StackTop;
            }

            return (int)WindowStackIndex.StackNormal;
        }

        public UIWindowStack GetUIWindowStack(UIWindow window)
        {
            int index = GetIndexByWindowType(window.GetWindowType());
            return m_listWindowStack[index];
        }

        public UIWindow GetWindowById(uint windowId)
        {
            return FindWindow(windowId);
        }

        public UIWindowStack GetUIWindowStack(UIWindowType windowType)
        {
            int index = GetIndexByWindowType(windowType);
            return m_listWindowStack[index];
        }

        #endregion

        #region 拓展
        /// <summary>
        /// 给控件添加自定义事件监听
        /// </summary>
        /// <param name="control">控件对象</param>
        /// <param name="type">事件类型</param>
        /// <param name="callback">事件的响应函数</param>
        public static void AddCustomEventListener(UIBehaviour control, EventTriggerType type, UnityAction<BaseEventData> callback)
        {
            EventTrigger trigger = control.GetComponent<EventTrigger>();

            if (trigger == null)
            {
                trigger = control.gameObject.AddComponent<EventTrigger>();
            }

            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = type;
            entry.callback.AddListener(callback);

            trigger.triggers.Add(entry);
        }

        public bool GetMouseDownUiPos(out Vector3 screenPos)
        {
            bool hadMouseDown = false;
            Vector3 mousePos = Vector3.zero;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR
            mousePos = Input.mousePosition;
            hadMouseDown = Input.GetMouseButton(0);
#else
        if (Input.touchCount > 0)
        {
            mousePos = Input.GetTouch(0).position;
            hadMouseDown = true;
        }
        else
        {
            hadMouseDown = false;
        }
#endif
            Vector2 pos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(m_canvasTrans as RectTransform, Input.mousePosition,
                m_uiCamera, out pos);
            screenPos = m_canvasTrans.TransformPoint(pos);

            return hadMouseDown;
        }


        #endregion
    }
}