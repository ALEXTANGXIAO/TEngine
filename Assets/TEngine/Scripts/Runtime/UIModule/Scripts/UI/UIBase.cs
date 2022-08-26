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
                    m_eventMgr = GameEventMgr.Instance;
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

        private Dictionary<int, Delegate> m_eventTable = new Dictionary<int, Delegate>();

        protected void ClearAllRegisterEvent()
        {
            var element = m_eventTable.GetEnumerator();
            while (element.MoveNext())
            {
                var m_event = element.Current.Value;
                //GameEventMgr.Instance.RemoveEventListener(element.Current.Key, m_event);
            }

            m_eventTable.Clear();
        }

        protected void AddUIEvent(int eventType, Action handler)
        {
            m_eventTable.Add(eventType, handler);
            EventMgr.AddEventListener(eventType, handler);
        }

        protected void AddUIEvent<T>(int eventType, Action<T> handler)
        {
            m_eventTable.Add(eventType, handler);
            EventMgr.AddEventListener(eventType, handler);
        }

        protected void AddUIEvent<T, U>(int eventType, Action<T, U> handler)
        {
            m_eventTable.Add(eventType, handler);
            EventMgr.AddEventListener(eventType, handler);
        }

        protected void AddUIEvent<T, U, V>(int eventType, Action<T, U, V> handler)
        {
            m_eventTable.Add(eventType, handler);
            EventMgr.AddEventListener(eventType, handler);
        }

        #endregion
    }
}