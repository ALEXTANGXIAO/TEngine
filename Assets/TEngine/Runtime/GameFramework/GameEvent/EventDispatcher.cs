using System;
using System.Collections.Generic;

namespace TEngine
{
    /// <summary>
    /// 游戏事件数据类。
    /// </summary>
    class EventDelegateData
    {
        private int m_eventType = 0;
        public List<Delegate> m_listExist = new List<Delegate>();
        private List<Delegate> m_addList = new List<Delegate>();
        private List<Delegate> m_deleteList = new List<Delegate>();
        private bool m_isExcute = false;
        private bool m_dirty = false;

        public EventDelegateData(int evnetType)
        {
            m_eventType = evnetType;
        }

        /// <summary>
        /// 添加注册委托。
        /// </summary>
        /// <param name="handler"></param>
        /// <returns></returns>
        public bool AddHandler(Delegate handler)
        {
            if (m_listExist.Contains(handler))
            {
                Log.Fatal("Repeated Add Handler");
                return false;
            }

            if (m_isExcute)
            {
                m_dirty = true;
                m_addList.Add(handler);
            }
            else
            {
                m_listExist.Add(handler);
            }

            return true;
        }

        /// <summary>
        /// 移除反注册委托。
        /// </summary>
        /// <param name="handler"></param>
        public void RmvHandler(Delegate handler)
        {
            if (m_isExcute)
            {
                m_dirty = true;
                m_deleteList.Add(handler);
            }
            else
            {
                if (!m_listExist.Remove(handler))
                {
                    Log.Fatal("Delete handle failed, not exist, EventId: {0}", StringId.HashToString(m_eventType));
                }
            }
        }

        /// <summary>
        /// 检测脏数据修正。
        /// </summary>
        private void CheckModify()
        {
            m_isExcute = false;
            if (m_dirty)
            {
                for (int i = 0; i < m_addList.Count; i++)
                {
                    m_listExist.Add(m_addList[i]);
                }

                m_addList.Clear();

                for (int i = 0; i < m_deleteList.Count; i++)
                {
                    m_listExist.Remove(m_deleteList[i]);
                }

                m_deleteList.Clear();
            }
        }

        /// <summary>
        /// 回调调用。
        /// </summary>
        public void Callback()
        {
            m_isExcute = true;
            for (var i = 0; i < m_listExist.Count; i++)
            {
                var d = m_listExist[i];
                if (d is Action action)
                {
                    action();
                }
            }

            CheckModify();
        }

        /// <summary>
        /// 回调调用。
        /// </summary>
        /// <param name="arg1"></param>
        /// <typeparam name="TArg1"></typeparam>
        public void Callback<TArg1>(TArg1 arg1)
        {
            m_isExcute = true;
            for (var i = 0; i < m_listExist.Count; i++)
            {
                var d = m_listExist[i];
                if (d is Action<TArg1> action)
                {
                    action(arg1);
                }
            }

            CheckModify();
        }

        /// <summary>
        /// 回调调用。
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <typeparam name="TArg1"></typeparam>
        /// <typeparam name="TArg2"></typeparam>
        public void Callback<TArg1, TArg2>(TArg1 arg1, TArg2 arg2)
        {
            m_isExcute = true;
            for (var i = 0; i < m_listExist.Count; i++)
            {
                var d = m_listExist[i];
                if (d is Action<TArg1, TArg2> action)
                {
                    action(arg1, arg2);
                }
            }

            CheckModify();
        }

        /// <summary>
        /// 回调调用。
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="arg3"></param>
        /// <typeparam name="TArg1"></typeparam>
        /// <typeparam name="TArg2"></typeparam>
        /// <typeparam name="TArg3"></typeparam>
        public void Callback<TArg1, TArg2, TArg3>(TArg1 arg1, TArg2 arg2, TArg3 arg3)
        {
            m_isExcute = true;
            for (var i = 0; i < m_listExist.Count; i++)
            {
                var d = m_listExist[i];
                if (d is Action<TArg1, TArg2, TArg3> action)
                {
                    action(arg1, arg2, arg3);
                }
            }

            CheckModify();
        }

        /// <summary>
        /// 回调调用。
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="arg3"></param>
        /// <param name="arg4"></param>
        /// <typeparam name="TArg1"></typeparam>
        /// <typeparam name="TArg2"></typeparam>
        /// <typeparam name="TArg3"></typeparam>
        /// <typeparam name="TArg4"></typeparam>
        public void Callback<TArg1, TArg2, TArg3, TArg4>(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
        {
            m_isExcute = true;
            for (var i = 0; i < m_listExist.Count; i++)
            {
                var d = m_listExist[i];
                if (d is Action<TArg1, TArg2, TArg3, TArg4> action)
                {
                    action(arg1, arg2, arg3, arg4);
                }
            }

            CheckModify();
        }

        /// <summary>
        /// 回调调用。
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="arg3"></param>
        /// <param name="arg4"></param>
        /// <param name="arg5"></param>
        /// <typeparam name="TArg1"></typeparam>
        /// <typeparam name="TArg2"></typeparam>
        /// <typeparam name="TArg3"></typeparam>
        /// <typeparam name="TArg4"></typeparam>
        /// <typeparam name="TArg5"></typeparam>
        public void Callback<TArg1, TArg2, TArg3, TArg4, TArg5>(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5)
        {
            m_isExcute = true;
            for (var i = 0; i < m_listExist.Count; i++)
            {
                var d = m_listExist[i];
                if (d is Action<TArg1, TArg2, TArg3, TArg4, TArg5> action)
                {
                    action(arg1, arg2, arg3, arg4, arg5);
                }
            }

            CheckModify();
        }
    }

    /// <summary>
    /// 封装消息的底层分发和注册。
    /// </summary>
    class EventDispatcher
    {
        static Dictionary<int, EventDelegateData> m_eventTable = new Dictionary<int, EventDelegateData>();

        #region 事件管理接口
        /// <summary>
        /// 增加事件监听。
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public bool AddEventListener(int eventType, Delegate handler)
        {
            if (!m_eventTable.TryGetValue(eventType, out var data))
            {
                data = new EventDelegateData(eventType);
                m_eventTable.Add(eventType, data);
            }

            return data.AddHandler(handler);
        }

        /// <summary>
        /// 移除事件监听。
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="handler"></param>
        public void RemoveEventListener(int eventType, Delegate handler)
        {
            if (m_eventTable.TryGetValue(eventType, out var data))
            {
                data.RmvHandler(handler);
            }
        }
        #endregion

        #region 事件分发接口
        /// <summary>
        /// 发送事件。
        /// </summary>
        /// <param name="eventType">事件类型。</param>
        public void Send(int eventType)
        {
            if (m_eventTable.TryGetValue(eventType, out var d))
            {
                d.Callback();
            }
        }

        /// <summary>
        /// 发送事件。
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="arg1"></param>
        /// <typeparam name="TArg1"></typeparam>
        public void Send<TArg1>(int eventType, TArg1 arg1)
        {
            if (m_eventTable.TryGetValue(eventType, out var d))
            {
                d.Callback(arg1);
            }
        }

        /// <summary>
        /// 发送事件。
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <typeparam name="TArg1"></typeparam>
        /// <typeparam name="TArg2"></typeparam>
        public void Send<TArg1, TArg2>(int eventType, TArg1 arg1, TArg2 arg2)
        {
            if (m_eventTable.TryGetValue(eventType, out var d))
            {
                d.Callback(arg1, arg2);
            }
        }

        /// <summary>
        /// 发送事件。
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="arg3"></param>
        /// <typeparam name="TArg1"></typeparam>
        /// <typeparam name="TArg2"></typeparam>
        /// <typeparam name="TArg3"></typeparam>
        public void Send<TArg1, TArg2, TArg3>(int eventType, TArg1 arg1, TArg2 arg2, TArg3 arg3)
        {
            if (m_eventTable.TryGetValue(eventType, out var d))
            {
                d.Callback(arg1, arg2, arg3);
            }
        }

        /// <summary>
        /// 发送事件。
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="arg3"></param>
        /// <param name="arg4"></param>
        /// <typeparam name="TArg1"></typeparam>
        /// <typeparam name="TArg2"></typeparam>
        /// <typeparam name="TArg3"></typeparam>
        /// <typeparam name="TArg4"></typeparam>
        public void Send<TArg1, TArg2, TArg3, TArg4>(int eventType, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
        {
            if (m_eventTable.TryGetValue(eventType, out var d))
            {
                d.Callback(arg1, arg2, arg3, arg4);
            }
        }

        /// <summary>
        /// 发送事件。
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="arg3"></param>
        /// <param name="arg4"></param>
        /// <param name="arg5"></param>
        /// <typeparam name="TArg1"></typeparam>
        /// <typeparam name="TArg2"></typeparam>
        /// <typeparam name="TArg3"></typeparam>
        /// <typeparam name="TArg4"></typeparam>
        /// <typeparam name="TArg5"></typeparam>
        public void Send<TArg1, TArg2, TArg3, TArg4, TArg5>(int eventType, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5)
        {
            if (m_eventTable.TryGetValue(eventType, out var d))
            {
                d.Callback(arg1, arg2, arg3, arg4, arg5);
            }
        }

        #endregion
    }
}