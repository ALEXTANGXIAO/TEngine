using System;
using System.Collections.Generic;
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
    public class UIBase:IUIBehaviour
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
        public virtual RectTransform rectTransform  { protected set; get; }
        
        /// <summary>
        /// UI类型。
        /// </summary>
        public virtual UIBaseType BaseType => UIBaseType.None;
        
        /// <summary>
        /// 资源操作句柄。
        /// </summary>
        public AssetOperationHandle Handle { protected set; get; }
        
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
        public T CreateWidget<T>(string goPath, bool visible = true) where T : UIWidget,new()
        {
            var goRootTrans = FindChild(goPath);

            if (goRootTrans != null)
            {
                return CreateWidget<T>(goRootTrans.gameObject, visible);
            }

            return null;
        }
        
        public T CreateWidget<T>(GameObject goRoot, bool visible = true) where T : UIWidget,new()
        {
            var widget = new T();
            if (widget.Create(this, goRoot, visible))
            {
                return widget;
            }

            return null;
        }

        #endregion
    }
}