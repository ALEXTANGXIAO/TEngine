using System;
using System.Collections.Generic;

namespace TEngine
{
    public class GameEventMgr : IMemory
    {
        private List<int> m_listEventTypes;
        private List<Delegate> m_listHandles;
        private bool m_isInit = false;

        public GameEventMgr()
        {
            if (m_isInit)
            {
                return;
            }

            m_isInit = true;
            m_listEventTypes = new List<int>();
            m_listHandles = new List<Delegate>();
        }

        public void Clear()
        {
            if (!m_isInit)
            {
                return;
            }

            for (int i = 0; i < m_listEventTypes.Count; ++i)
            {
                var eventType = m_listEventTypes[i];
                var handle = m_listHandles[i];
                GameEvent.RemoveEventListener(eventType, handle);
            }

            m_listEventTypes.Clear();
            m_listHandles.Clear();
        }

        private void AddEvent(int eventType, Delegate handler)
        {
            m_listEventTypes.Add(eventType);
            m_listHandles.Add(handler);
        }

        public void AddUIEvent(int eventType, Action handler)
        {
            if (GameEvent.AddEventListener(eventType, handler))
            {
                AddEvent(eventType, handler);
            }
        }

        public void AddUIEvent<T>(int eventType, Action<T> handler)
        {
            if (GameEvent.AddEventListener(eventType, handler))
            {
                AddEvent(eventType, handler);
            }
        }

        public void AddUIEvent<T, U>(int eventType, Action<T, U> handler)
        {
            if (GameEvent.AddEventListener(eventType, handler))
            {
                AddEvent(eventType, handler);
            }
        }

        public void AddUIEvent<T, U, V>(int eventType, Action<T, U, V> handler)
        {
            if (GameEvent.AddEventListener(eventType, handler))
            {
                AddEvent(eventType, handler);
            }
        }

        public void AddUIEvent<T, U, V, W>(int eventType, Action<T, U, V, W> handler)
        {
            if (GameEvent.AddEventListener(eventType, handler))
            {
                AddEvent(eventType, handler);
            }
        }

        public void AddUIEvent<T, U, V, W, X>(int eventType, Action<T, U, V, W, X> handler)
        {
            if (GameEvent.AddEventListener(eventType, handler))
            {
                AddEvent(eventType, handler);
            }
        }
    }
}