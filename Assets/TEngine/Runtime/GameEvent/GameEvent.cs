using System;

namespace TEngine
{
    public class GameEvent
    {
        private static EventMgr m_mgr = new EventMgr();

        public static void Init()
        {
            
        }

        #region 细分的注册接口

        public static bool AddEventListener(int eventType, Action handler)
        {
            return m_mgr.GetDispatcher().AddEventListener(eventType, handler);
        }

        public static bool AddEventListener<T>(int eventType, Action<T> handler)
        {
            return m_mgr.GetDispatcher().AddEventListener(eventType, handler);
        }

        public static bool AddEventListener<T, U>(int eventType, Action<T, U> handler)
        {
            return m_mgr.GetDispatcher().AddEventListener(eventType, handler);
        }

        public static bool AddEventListener<T, U, V>(int eventType, Action<T, U, V> handler)
        {
            return m_mgr.GetDispatcher().AddEventListener(eventType, handler);
        }

        public static bool AddEventListener<T, U, V, W>(int eventType, Action<T, U, V, W> handler)
        {
            return m_mgr.GetDispatcher().AddEventListener(eventType, handler);
        }

        public static bool AddEventListener<T, U, V, W, X>(int eventType, Action<T, U, V, W, X> handler)
        {
            return m_mgr.GetDispatcher().AddEventListener(eventType, handler);
        }

        public static void RemoveEventListener(int eventType, Action handler)
        {
            m_mgr.GetDispatcher().RemoveEventListener(eventType, handler);
        }

        public static void RemoveEventListener<T>(int eventType, Action<T> handler)
        {
            m_mgr.GetDispatcher().RemoveEventListener(eventType, handler);
        }

        public static void RemoveEventListener<T, U>(int eventType, Action<T, U> handler)
        {
            m_mgr.GetDispatcher().RemoveEventListener(eventType, handler);
        }

        public static void RemoveEventListener<T, U, V>(int eventType, Action<T, U, V> handler)
        {
            m_mgr.GetDispatcher().RemoveEventListener(eventType, handler);
        }

        public static void RemoveEventListener<T, U, V, W>(int eventType, Action<T, U, V, W> handler)
        {
            m_mgr.GetDispatcher().RemoveEventListener(eventType, handler);
        }

        public static void RemoveEventListener<T, U, V, W, X>(int eventType, Action<T, U, V, W, X> handler)
        {
            m_mgr.GetDispatcher().RemoveEventListener(eventType, handler);
        }

        public static void RemoveEventListener(int eventType, Delegate handler)
        {
            m_mgr.GetDispatcher().RemoveEventListener(eventType, handler);
        }

        //----------------------------string Event----------------------------//
        public static bool AddEventListener(string eventType, Action handler)
        {
            return m_mgr.GetDispatcher().AddEventListener(StringId.StringToHash(eventType), handler);
        }

        public static bool AddEventListener<T>(string eventType, Action<T> handler)
        {
            return m_mgr.GetDispatcher().AddEventListener(StringId.StringToHash(eventType), handler);
        }

        public static bool AddEventListener<T, U>(string eventType, Action<T, U> handler)
        {
            return m_mgr.GetDispatcher().AddEventListener(StringId.StringToHash(eventType), handler);
        }

        public static bool AddEventListener<T, U, V>(string eventType, Action<T, U, V> handler)
        {
            return m_mgr.GetDispatcher().AddEventListener(StringId.StringToHash(eventType), handler);
        }

        public static bool AddEventListener<T, U, V, W>(string eventType, Action<T, U, V, W> handler)
        {
            return m_mgr.GetDispatcher().AddEventListener(StringId.StringToHash(eventType), handler);
        }

        public static bool AddEventListener<T, U, V, W, X>(string eventType, Action<T, U, V, W, X> handler)
        {
            return m_mgr.GetDispatcher().AddEventListener(StringId.StringToHash(eventType), handler);
        }

        public static void RemoveEventListener(string eventType, Action handler)
        {
            m_mgr.GetDispatcher().RemoveEventListener(StringId.StringToHash(eventType), handler);
        }

        public static void RemoveEventListener<T>(string eventType, Action<T> handler)
        {
            m_mgr.GetDispatcher().RemoveEventListener(StringId.StringToHash(eventType), handler);
        }

        public static void RemoveEventListener<T, U>(string eventType, Action<T, U> handler)
        {
            m_mgr.GetDispatcher().RemoveEventListener(StringId.StringToHash(eventType), handler);
        }

        public static void RemoveEventListener<T, U, V>(string eventType, Action<T, U, V> handler)
        {
            m_mgr.GetDispatcher().RemoveEventListener(StringId.StringToHash(eventType), handler);
        }

        public static void RemoveEventListener<T, U, V, W>(string eventType, Action<T, U, V, W> handler)
        {
            m_mgr.GetDispatcher().RemoveEventListener(StringId.StringToHash(eventType), handler);
        }

        public static void RemoveEventListener<T, U, V, W, X>(string eventType, Action<T, U, V, W, X> handler)
        {
            m_mgr.GetDispatcher().RemoveEventListener(StringId.StringToHash(eventType), handler);
        }

        public static void RemoveEventListener(string eventType, Delegate handler)
        {
            m_mgr.GetDispatcher().RemoveEventListener(StringId.StringToHash(eventType), handler);
        }
        #endregion

        #region 分发消息接口

        public static T Get<T>()
        {
            return m_mgr.GetInterface<T>();
        }

        public static void Send(int eventType)
        {
            m_mgr.GetDispatcher().Send(eventType);
        }

        public static void Send<T>(int eventType, T arg1)
        {
            m_mgr.GetDispatcher().Send(eventType, arg1);
        }

        public static void Send<T, U>(int eventType, T arg1, U arg2)
        {
            m_mgr.GetDispatcher().Send(eventType, arg1, arg2);
        }

        public static void Send<T, U, V>(int eventType, T arg1, U arg2, V arg3)
        {
            m_mgr.GetDispatcher().Send(eventType, arg1, arg2, arg3);
        }

        public static void Send<T, U, V, W>(int eventType, T arg1, U arg2, V arg3, W arg4)
        {
            m_mgr.GetDispatcher().Send(eventType, arg1, arg2, arg3);
        }

        public static void Send<T, U, V, W, X>(int eventType, T arg1, U arg2, V arg3, W arg4, X arg5)
        {
            m_mgr.GetDispatcher().Send(eventType, arg1, arg2, arg3, arg4, arg5);
        }

        public static void Send(int eventType, Delegate handler)
        {
            m_mgr.GetDispatcher().Send(eventType, handler);
        }

        //-------------------------------string Send-------------------------------//
        public static void Send(string eventType)
        {
            m_mgr.GetDispatcher().Send(StringId.StringToHash(eventType));
        }

        public static void Send<T>(string eventType, T arg1)
        {
            m_mgr.GetDispatcher().Send(StringId.StringToHash(eventType), arg1);
        }

        public static void Send<T, U>(string eventType, T arg1, U arg2)
        {
            m_mgr.GetDispatcher().Send(StringId.StringToHash(eventType), arg1, arg2);
        }

        public static void Send<T, U, V>(string eventType, T arg1, U arg2, V arg3)
        {
            m_mgr.GetDispatcher().Send(StringId.StringToHash(eventType), arg1, arg2, arg3);
        }

        public static void Send<T, U, V, W>(string eventType, T arg1, U arg2, V arg3, W arg4)
        {
            m_mgr.GetDispatcher().Send(StringId.StringToHash(eventType), arg1, arg2, arg3);
        }

        public static void Send<T, U, V, W, X>(string eventType, T arg1, U arg2, V arg3, W arg4, X arg5)
        {
            m_mgr.GetDispatcher().Send(StringId.StringToHash(eventType), arg1, arg2, arg3, arg4, arg5);
        }

        public static void Send(string eventType, Delegate handler)
        {
            m_mgr.GetDispatcher().Send(StringId.StringToHash(eventType), handler);
        }
        #endregion
    }
}