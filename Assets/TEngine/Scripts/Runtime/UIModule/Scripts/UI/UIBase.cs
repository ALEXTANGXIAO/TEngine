using System;
using System.Collections.Generic;
using UnityEngine;

namespace TEngine.Runtime.UIModule
{
    public class UIBase
    {
        protected GameObject m_go;
        protected RectTransform m_transform;
        protected string m_name;
        protected bool m_destroyed = true;

        private GameEventMgr m_eventMgr;

        protected GameEventMgr EventMgr
        {
            get
            {
                if (m_eventMgr == null)
                {
                    m_eventMgr = MemoryPool.Acquire<GameEventMgr>();
                }

                return m_eventMgr;
            }
        }

        public bool IsDestroyed
        {
            get { return m_destroyed; }
        }

        public bool IsCreated
        {
            get { return !IsDestroyed; }
        }

        public RectTransform transform
        {
            get { return m_transform; }
        }

        public GameObject gameObject
        {
            get { return m_go; }
        }

        public string name
        {
            get
            {
                if (string.IsNullOrEmpty(m_name))
                {
                    m_name = GetType().Name;
                }

                return m_name;
            }
        }

        #region Event

        protected void ClearAllRegisterEvent()
        {
            MemoryPool.Release(m_eventMgr);
        }

        protected void AddUIEvent(int eventType, Action handler)
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

        #endregion
    }
}