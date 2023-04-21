using System;

namespace TEngine
{
    /// <summary>
    /// 游戏事件类。
    /// </summary>
    public static class GameEvent
    {
        /// <summary>
        /// 全局事件管理器。
        /// </summary>
        private static readonly EventMgr EventMgr = new EventMgr();

        #region 细分的注册接口

        /// <summary>
        /// 增加事件监听。
        /// </summary>
        /// <param name="eventType">事件类型。</param>
        /// <param name="handler">事件Handler。</param>
        /// <returns>是否监听成功。</returns>
        public static bool AddEventListener(int eventType, Action handler)
        {
            return EventMgr.Dispatcher.AddEventListener(eventType, handler);
        }

        public static bool AddEventListener<TArg1>(int eventType, Action<TArg1> handler)
        {
            return EventMgr.Dispatcher.AddEventListener(eventType, handler);
        }

        public static bool AddEventListener<TArg1, TArg2>(int eventType, Action<TArg1, TArg2> handler)
        {
            return EventMgr.Dispatcher.AddEventListener(eventType, handler);
        }

        public static bool AddEventListener<TArg1, TArg2, TArg3>(int eventType, Action<TArg1, TArg2, TArg3> handler)
        {
            return EventMgr.Dispatcher.AddEventListener(eventType, handler);
        }

        public static bool AddEventListener<TArg1, TArg2, TArg3, TArg4>(int eventType, Action<TArg1, TArg2, TArg3, TArg4> handler)
        {
            return EventMgr.Dispatcher.AddEventListener(eventType, handler);
        }

        public static bool AddEventListener<TArg1, TArg2, TArg3, TArg4, TArg5>(int eventType, Action<TArg1, TArg2, TArg3, TArg4, TArg5> handler)
        {
            return EventMgr.Dispatcher.AddEventListener(eventType, handler);
        }

        public static void RemoveEventListener(int eventType, Action handler)
        {
            EventMgr.Dispatcher.RemoveEventListener(eventType, handler);
        }

        public static void RemoveEventListener<TArg1>(int eventType, Action<TArg1> handler)
        {
            EventMgr.Dispatcher.RemoveEventListener(eventType, handler);
        }

        public static void RemoveEventListener<TArg1, TArg2>(int eventType, Action<TArg1, TArg2> handler)
        {
            EventMgr.Dispatcher.RemoveEventListener(eventType, handler);
        }

        public static void RemoveEventListener<TArg1, TArg2, TArg3>(int eventType, Action<TArg1, TArg2, TArg3> handler)
        {
            EventMgr.Dispatcher.RemoveEventListener(eventType, handler);
        }

        public static void RemoveEventListener<TArg1, TArg2, TArg3, TArg4>(int eventType, Action<TArg1, TArg2, TArg3, TArg4> handler)
        {
            EventMgr.Dispatcher.RemoveEventListener(eventType, handler);
        }

        public static void RemoveEventListener<TArg1, TArg2, TArg3, TArg4, TArg5>(int eventType, Action<TArg1, TArg2, TArg3, TArg4, TArg5> handler)
        {
            EventMgr.Dispatcher.RemoveEventListener(eventType, handler);
        }

        public static void RemoveEventListener(int eventType, Delegate handler)
        {
            EventMgr.Dispatcher.RemoveEventListener(eventType, handler);
        }

        //----------------------------string Event----------------------------//
        public static bool AddEventListener(string eventType, Action handler)
        {
            return EventMgr.Dispatcher.AddEventListener(StringId.StringToHash(eventType), handler);
        }

        public static bool AddEventListener<TArg1>(string eventType, Action<TArg1> handler)
        {
            return EventMgr.Dispatcher.AddEventListener(StringId.StringToHash(eventType), handler);
        }

        public static bool AddEventListener<TArg1, TArg2>(string eventType, Action<TArg1, TArg2> handler)
        {
            return EventMgr.Dispatcher.AddEventListener(StringId.StringToHash(eventType), handler);
        }

        public static bool AddEventListener<TArg1, TArg2, TArg3>(string eventType, Action<TArg1, TArg2, TArg3> handler)
        {
            return EventMgr.Dispatcher.AddEventListener(StringId.StringToHash(eventType), handler);
        }

        public static bool AddEventListener<TArg1, TArg2, TArg3, TArg4>(string eventType, Action<TArg1, TArg2, TArg3, TArg4> handler)
        {
            return EventMgr.Dispatcher.AddEventListener(StringId.StringToHash(eventType), handler);
        }

        public static bool AddEventListener<TArg1, TArg2, TArg3, TArg4, TArg5>(string eventType, Action<TArg1, TArg2, TArg3, TArg4, TArg5> handler)
        {
            return EventMgr.Dispatcher.AddEventListener(StringId.StringToHash(eventType), handler);
        }

        public static void RemoveEventListener(string eventType, Action handler)
        {
            EventMgr.Dispatcher.RemoveEventListener(StringId.StringToHash(eventType), handler);
        }

        public static void RemoveEventListener<TArg1>(string eventType, Action<TArg1> handler)
        {
            EventMgr.Dispatcher.RemoveEventListener(StringId.StringToHash(eventType), handler);
        }

        public static void RemoveEventListener<TArg1, TArg2>(string eventType, Action<TArg1, TArg2> handler)
        {
            EventMgr.Dispatcher.RemoveEventListener(StringId.StringToHash(eventType), handler);
        }

        public static void RemoveEventListener<TArg1, TArg2, TArg3>(string eventType, Action<TArg1, TArg2, TArg3> handler)
        {
            EventMgr.Dispatcher.RemoveEventListener(StringId.StringToHash(eventType), handler);
        }

        public static void RemoveEventListener<TArg1, TArg2, TArg3, TArg4>(string eventType, Action<TArg1, TArg2, TArg3, TArg4> handler)
        {
            EventMgr.Dispatcher.RemoveEventListener(StringId.StringToHash(eventType), handler);
        }

        public static void RemoveEventListener<TArg1, TArg2, TArg3, TArg4, TArg5>(string eventType, Action<TArg1, TArg2, TArg3, TArg4, TArg5> handler)
        {
            EventMgr.Dispatcher.RemoveEventListener(StringId.StringToHash(eventType), handler);
        }

        public static void RemoveEventListener(string eventType, Delegate handler)
        {
            EventMgr.Dispatcher.RemoveEventListener(StringId.StringToHash(eventType), handler);
        }

        #endregion

        #region 分发消息接口

        public static TArg1 Get<TArg1>()
        {
            return EventMgr.GetInterface<TArg1>();
        }

        public static void Send(int eventType)
        {
            EventMgr.Dispatcher.Send(eventType);
        }

        public static void Send<TArg1>(int eventType, TArg1 arg1)
        {
            EventMgr.Dispatcher.Send(eventType, arg1);
        }

        public static void Send<TArg1, TArg2>(int eventType, TArg1 arg1, TArg2 arg2)
        {
            EventMgr.Dispatcher.Send(eventType, arg1, arg2);
        }

        public static void Send<TArg1, TArg2, TArg3>(int eventType, TArg1 arg1, TArg2 arg2, TArg3 arg3)
        {
            EventMgr.Dispatcher.Send(eventType, arg1, arg2, arg3);
        }

        public static void Send<TArg1, TArg2, TArg3, TArg4>(int eventType, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
        {
            EventMgr.Dispatcher.Send(eventType, arg1, arg2, arg3, arg4);
        }

        public static void Send<TArg1, TArg2, TArg3, TArg4, TArg5>(int eventType, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5)
        {
            EventMgr.Dispatcher.Send(eventType, arg1, arg2, arg3, arg4, arg5);
        }

        public static void Send(int eventType, Delegate handler)
        {
            EventMgr.Dispatcher.Send(eventType, handler);
        }

        //-------------------------------string Send-------------------------------//
        public static void Send(string eventType)
        {
            EventMgr.Dispatcher.Send(StringId.StringToHash(eventType));
        }

        public static void Send<TArg1>(string eventType, TArg1 arg1)
        {
            EventMgr.Dispatcher.Send(StringId.StringToHash(eventType), arg1);
        }

        public static void Send<TArg1, TArg2>(string eventType, TArg1 arg1, TArg2 arg2)
        {
            EventMgr.Dispatcher.Send(StringId.StringToHash(eventType), arg1, arg2);
        }

        public static void Send<TArg1, TArg2, TArg3>(string eventType, TArg1 arg1, TArg2 arg2, TArg3 arg3)
        {
            EventMgr.Dispatcher.Send(StringId.StringToHash(eventType), arg1, arg2, arg3);
        }

        public static void Send<TArg1, TArg2, TArg3, TArg4>(string eventType, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
        {
            EventMgr.Dispatcher.Send(StringId.StringToHash(eventType), arg1, arg2, arg3);
        }

        public static void Send<TArg1, TArg2, TArg3, TArg4, TArg5>(string eventType, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5)
        {
            EventMgr.Dispatcher.Send(StringId.StringToHash(eventType), arg1, arg2, arg3, arg4, arg5);
        }

        public static void Send(string eventType, Delegate handler)
        {
            EventMgr.Dispatcher.Send(StringId.StringToHash(eventType), handler);
        }

        #endregion
    }
}