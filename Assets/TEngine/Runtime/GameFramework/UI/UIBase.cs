using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using YooAsset;

namespace TEngine
{
    /// <summary>
    /// UI类型。
    /// </summary>
    public enum UIBaseType
    {
        /// <summary>
        /// 类型无。
        /// </summary>
        None,

        /// <summary>
        /// 类型Windows。
        /// </summary>
        Window,

        /// <summary>
        /// 类型Widget。
        /// </summary>
        Widget,
    }

    /// <summary>
    /// UI基类。
    /// </summary>
    public class UIBase : IUIBehaviour
    {
        /// <summary>
        /// 所属UI父节点。
        /// </summary>
        protected UIBase parent = null;

        /// <summary>
        /// UI父节点。
        /// </summary>
        public UIBase Parent => parent;

        /// <summary>
        /// 窗口的实例资源对象。
        /// </summary>
        public virtual GameObject gameObject { protected set; get; }

        /// <summary>
        /// 窗口矩阵位置组件。
        /// </summary>
        public virtual RectTransform rectTransform { protected set; get; }

        /// <summary>
        /// UI类型。
        /// </summary>
        public virtual UIBaseType BaseType => UIBaseType.None;

        /// <summary>
        /// 资源操作句柄。
        /// </summary>
        public AssetOperationHandle Handle { protected set; get; }

        /// <summary>
        /// 资源组数据。
        /// </summary>
        public AssetGroup AssetGroup { protected set; get; }

        /// <summary>
        /// 资源是否准备完毕。
        /// </summary>
        public bool IsPrepare { protected set; get; }

        /// <summary>
        /// UI子组件列表。
        /// </summary>
        public List<UIWidget> ListChild = new List<UIWidget>();

        /// <summary>
        /// 存在Update更新的UI子组件列表。
        /// </summary>
        protected List<UIWidget> m_listUpdateChild = null;

        /// <summary>
        /// 是否持有Update行为。
        /// </summary>
        protected bool m_updateListValid = false;

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
        /// 是否需要Update
        /// </summary>
        protected bool HasOverrideUpdate = true;

        /// <summary>
        /// 窗口更新
        /// </summary>
        public virtual void OnUpdate()
        {
            HasOverrideUpdate = false;
        }

        /// <summary>
        /// 窗口销毁
        /// </summary>
        public virtual void OnDestroy()
        {
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


        #region FindChildComponent

        public Transform FindChild(string path)
        {
            return DUnityUtil.FindChild(rectTransform, path);
        }

        public Transform FindChild(Transform trans, string path)
        {
            return DUnityUtil.FindChild(trans, path);
        }

        public T FindChildComponent<T>(string path) where T : Component
        {
            return DUnityUtil.FindChildComponent<T>(rectTransform, path);
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

        protected void RemoveAllUIEvent()
        {
            if (_eventMgr != null)
            {
                MemoryPool.Release(_eventMgr);
            }
        }

        #endregion

        #region UIWidget

        public T CreateWidget<T>(string goPath, bool visible = true) where T : UIWidget, new()
        {
            var goRootTrans = FindChild(goPath);

            if (goRootTrans != null)
            {
                return CreateWidget<T>(goRootTrans.gameObject, visible);
            }

            return null;
        }

        public T CreateWidget<T>(GameObject goRoot, bool visible = true) where T : UIWidget, new()
        {
            var widget = new T();
            if (widget.Create(this, goRoot, visible))
            {
                return widget;
            }

            return null;
        }

        public T CreateWidget<T>(Transform parentTrans, string goPath, bool visible = true) where T : UIWidget, new()
        {
            var goRootTrans = FindChild(parentTrans, goPath);
            if (goRootTrans != null)
            {
                return CreateWidget<T>(goRootTrans.gameObject, visible);
            }

            return null;
        }

        public T CreateWidgetByPath<T>(Transform parentTrans, string assetPath, bool visible = true) where T : UIWidget, new()
        {
            if (AssetGroup == null)
            {
                AssetGroup = AssetGroup.Alloc();
            }

            GameObject goInst = AssetGroup.LoadAsset<GameObject>(assetPath, parentTrans);
            return CreateWidget<T>(goInst, visible);
        }

        public T CreateWidgetByPrefab<T>(GameObject goPrefab, Transform parentTrans = null, bool visible = true) where T : UIWidget, new()
        {
            var widget = new T();
            if (!widget.CreateByPrefab(this, goPrefab, parentTrans, visible))
            {
                return null;
            }

            return widget;
        }

        public T CreateWidgetByType<T>(Transform parentTrans, bool visible = true) where T : UIWidget, new()
        {
            return CreateWidgetByPath<T>(parentTrans, typeof(T).Name, visible);
        }

        /// <summary>
        /// 调整图标数量。
        /// </summary>
        public void AdjustIconNum<T>(List<T> listIcon, int tarNum, Transform parent, GameObject prefab = null, string assetPath = "") where T : UIWidget, new()
        {
            if (listIcon == null)
            {
                listIcon = new List<T>();
            }

            if (listIcon.Count < tarNum)
            {
                int needNum = tarNum - listIcon.Count;
                for (int iconIdx = 0; iconIdx < needNum; iconIdx++)
                {
                    T tmpT = prefab == null ? CreateWidgetByType<T>(parent) : CreateWidgetByPrefab<T>(prefab, parent);
                    listIcon.Add(tmpT);
                }
            }
            else if (listIcon.Count > tarNum)
            {
                RemoveUnUseItem<T>(listIcon, tarNum);
            }
        }

        /// <summary>
        /// 异步调整图标数量。
        /// </summary>
        /// <param name="listIcon"></param>
        /// <param name="tarNum"></param>
        /// <param name="parentTrans"></param>
        /// <param name="prefab"></param>
        /// <param name="assetPath"></param>
        /// <param name="maxNumPerFrame"></param>
        /// <param name="updateAction"></param>
        /// <typeparam name="T"></typeparam>
        public void AsyncAdjustIconNum<T>(List<T> listIcon, int tarNum, Transform parentTrans, GameObject prefab = null,
            string assetPath = "", int maxNumPerFrame = 5,
            Action<T, int> updateAction = null) where T : UIWidget, new()
        {
            AsyncAdjustIconNumInternal(listIcon, tarNum, parentTrans, maxNumPerFrame, updateAction, prefab, assetPath).Forget();
        }

        /// <summary>
        /// 异步创建接口。
        /// </summary>
        private async UniTaskVoid AsyncAdjustIconNumInternal<T>(List<T> listIcon, int tarNum, Transform parentTrans, int maxNumPerFrame,
            Action<T, int> updateAction, GameObject prefab, string assetPath) where T : UIWidget, new()
        {
            if (listIcon == null)
            {
                listIcon = new List<T>();
            }

            int createCnt = 0;

            for (int i = 0; i < tarNum; i++)
            {
                T tmpT = null;
                if (i < listIcon.Count)
                {
                    tmpT = listIcon[i];
                }
                else
                {
                    if (prefab == null)
                    {
                        tmpT = CreateWidgetByPath<T>(parentTrans, assetPath);
                    }
                    else
                    {
                        tmpT = CreateWidgetByPrefab<T>(prefab, parentTrans);
                    }

                    listIcon.Add(tmpT);
                }

                int index = i;
                if (updateAction != null)
                {
                    updateAction(tmpT, index);
                }

                createCnt++;
                if (createCnt >= maxNumPerFrame)
                {
                    createCnt = 0;
                    await UniTask.Yield();
                }
            }

            if (listIcon.Count > tarNum)
            {
                RemoveUnUseItem(listIcon, tarNum);
            }
        }

        private void RemoveUnUseItem<T>(List<T> listIcon, int tarNum) where T : UIWidget
        {
            var removeIcon = new List<T>();
            for (int iconIdx = 0; iconIdx < listIcon.Count; iconIdx++)
            {
                var icon = listIcon[iconIdx];
                if (iconIdx >= tarNum)
                {
                    removeIcon.Add(icon);
                }
            }

            for (var index = 0; index < removeIcon.Count; index++)
            {
                var icon = removeIcon[index];
                listIcon.Remove(icon);
                icon.OnDestroy();
                ListChild.Remove(icon);
                UnityEngine.Object.Destroy(icon.gameObject);
            }
        }

        #endregion
    }
}