using System;
using System.Collections.Generic;
using UnityEngine;

namespace TEngine.Runtime.Actor
{
    /// <summary>
    /// Actor 组件基类
    /// </summary>
    public abstract class ActorComponent
    {
        internal bool m_callStart = false;
        internal bool m_calledOnDestroy = false;
        protected GameActor m_actor;

        private List<RegisterAttrChangeData> m_registAttrChanged = new List<RegisterAttrChangeData>();

        public bool m_destroy;
        public ActorComponent m_prev;
        public ActorComponent m_next;

        public GameTimer m_updateTimer;
        public GameTimer m_lateUpdateTimer;
        public bool m_needOnceUpdate = false;
        public bool m_needLoopUpdate = false;
        public bool m_needLoopLateUpdate = false;

        protected void RequestOnceUpdate()
        {
            if (!m_callStart)
            {
                m_needOnceUpdate = true;
                return;
            }

            if (GameTimer.IsNull(m_updateTimer))
            {
                ActorComponentPool.Instance.RequestOnceUpdate(this);
            }
        }

        protected void RequestLoopUpdate(string detail = null)
        {
            if (!m_callStart)
            {
                m_needLoopUpdate = true;
                return;
            }

            if (GameTimer.IsNull(m_updateTimer))
            {
                ActorComponentPool.Instance.RequestLoopUpdate(this, detail);
            }
        }

        protected void RequestLoopLateUpdate()
        {
            if (!m_callStart)
            {
                m_needLoopLateUpdate = true;
                return;
            }

            if (GameTimer.IsNull(m_lateUpdateTimer))
            {
                ActorComponentPool.Instance.RequestLoopLateUpdate(this);
            }
        }

        protected void StopLoopUpdate()
        {
            if (!m_callStart)
            {
                m_needLoopUpdate = false;
                return;
            }

            if (!GameTimer.IsNull(m_updateTimer))
            {
                ActorComponentPool.Instance.StopLoopUpdate(this);
            }
        }

        private class RegisterAttrChangeData
        {
            public int AttrId;
            public Delegate Handler;

            public RegisterAttrChangeData(int attrId, Delegate handler)
            {
                AttrId = attrId;
                Handler = handler;
            }
        }

        public GameActor OwnActor
        {
            get
            {
                if (m_actor != null && m_actor.IsDestroyed)
                {
                    m_actor = null;
                    return null;
                }

                return m_actor;
            }
        }


        public Vector3 Position
        {
            get { return m_actor.Position; }
        }

        #region 扩展接口

        protected virtual void Awake()
        {
        }

        protected virtual void Start()
        {
        }

        public virtual void LateUpdate()
        {
        }

        public virtual void Update()
        {
        }

        protected virtual void OnDestroy()
        {
        }

        /// <summary>
        /// 不显示的时候是否需要update
        /// </summary>
        /// <returns></returns>
        public virtual bool IsInvisibleNeedUpdate()
        {
            return true;
        }

        #endregion

        #region 操作接口

        /// <summary>
        /// 只有添加到对象上，才触发下面的初始化逻辑
        /// </summary>
        /// <param name="actor"></param>
        /// <returns></returns>
        public bool BeforeAttachToActor(GameActor actor)
        {
            m_actor = actor;
            m_callStart = false;
            Awake();

            AddDebug();
            return true;
        }

        public void BeforeDestroy()
        {
            if (m_calledOnDestroy)
            {
                return;
            }

            RmvDebug();

            m_calledOnDestroy = true;
            if (m_actor != null)
            {
                ClearAllEventChangeHandler();
                OnDestroy();
            }
        }

        public void CallStart()
        {
            Log.Assert(!m_callStart);
            Start();
            m_callStart = true;
        }


        public void AfterOnStartAction(ActorTimerMgr timerMgr)
        {
            if (m_needLoopUpdate)
            {
                Log.Assert(GameTimer.IsNull(m_updateTimer));

                timerMgr.CreateLoopFrameTimer(ref m_updateTimer, GetType().FullName, Update);
            }
            else if (m_needOnceUpdate)
            {
                Log.Assert(GameTimer.IsNull(m_updateTimer));

                if (GameTimer.IsNull(m_updateTimer))
                {
                    m_updateTimer = timerMgr.CreateOnceFrameTimer(GetType().FullName, Update);
                }
            }

            if (m_needLoopLateUpdate)
            {
                Log.Assert(GameTimer.IsNull(m_lateUpdateTimer));

                timerMgr.CreateLoopFrameLateTimer(ref m_lateUpdateTimer, GetType().FullName, LateUpdate);
            }
        }

        #endregion

        #region DebugBehaviour

        protected void AddDebug()
        {
#if UNITY_EDITOR
            var debugData = UnityUtil.AddMonoBehaviour<ActorDebugerBehaviour>(OwnActor.gameObject);
            debugData.AddDebugComponent(GetType().Name);
#endif
        }

        protected void RmvDebug()
        {
#if UNITY_EDITOR
            var debugData = UnityUtil.AddMonoBehaviour<ActorDebugerBehaviour>(OwnActor.gameObject);
            debugData.RmvDebugComponent(GetType().Name);
#endif
        }

        protected void SetDebugInfo(string key, string val)
        {
#if UNITY_EDITOR
            var debugData = UnityUtil.AddMonoBehaviour<ActorDebugerBehaviour>(OwnActor.gameObject);
            debugData.SetDebugInfo(GetType().Name, key, val);
#endif
        }

        protected void RemoveAllDebugInfo()
        {
#if UNITY_EDITOR
            var debugData = UnityUtil.AddMonoBehaviour<ActorDebugerBehaviour>(OwnActor.gameObject);
            debugData.RemoveAllDebugInfo(GetType().Name);
#endif
        }

        #endregion

        #region EventBehaviour

        public void AddEventListener(int eventId, Action eventCallback)
        {
            m_actor.Event.AddEventListener(eventId, eventCallback, this);
        }

        public void AddEventListener<T>(int eventId, Action<T> eventCallback)
        {
            m_actor.Event.AddEventListener(eventId, eventCallback, this);
        }

        public void AddEventListener<T, TU>(int eventId, Action<T, TU> eventCallback)
        {
            m_actor.Event.AddEventListener(eventId, eventCallback, this);
        }

        public void AddEventListener<T, TU, TV>(int eventId, Action<T, TU, TV> eventCallback)
        {
            m_actor.Event.AddEventListener(eventId, eventCallback, this);
        }

        public void AddEventListener<T, TU, TV, TS>(int eventId, Action<T, TU, TV, TS> eventCallback)
        {
            m_actor.Event.AddEventListener(eventId, eventCallback, this);
        }

        public void RegAttrChangeEvent<T>(int attrId, Action<T, T> handler)
        {
            m_registAttrChanged.Add(new RegisterAttrChangeData(attrId, handler));
            OwnActor.Attr.RegAttrChangeEvent((int)attrId, handler);
        }

        private void ClearAllEventChangeHandler()
        {
            var attr = OwnActor.Attr;
            for (int i = 0; i < m_registAttrChanged.Count; i++)
            {
                var data = m_registAttrChanged[i];
                attr.UnRegAttrChangeEvent(data.AttrId, data.Handler);
            }
        }

        #endregion
    }
}