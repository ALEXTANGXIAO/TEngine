using System;
using System.Collections.Generic;
using TEngine;

namespace TEngine
{
    #region EventInfo
    internal interface IEventInfo
    {

    }

    public class EventInfo : IEventInfo
    {
        public Action actions;

        public EventInfo(Action action)
        {
            actions += action;
        }
    }


    public class EventInfo<T> : IEventInfo
    {
        public Action<T> actions;

        public EventInfo(Action<T> action)
        {
            actions += action;
        }
    }

    public class EventInfo<T, U> : IEventInfo
    {
        public Action<T, U> actions;

        public EventInfo(Action<T, U> action)
        {
            actions += action;
        }
    }

    public class EventInfo<T, U, W> : IEventInfo
    {
        public Action<T, U, W> actions;

        public EventInfo(Action<T, U, W> action)
        {
            actions += action;
        }
    }
    #endregion
    /// <summary>
    /// 总观察者 - 总事件中心系统
    /// </summary>
    public class GameEventMgr : TSingleton<GameEventMgr>
    {
        /// <summary>
        ///  Dictionary<int, IEventInfo> Key->Int.32 Value->EventInfo,调用频率高建议使用int事件，减少字典内String的哈希碰撞
        /// </summary>
        private Dictionary<int, IEventInfo> m_eventDic = new Dictionary<int, IEventInfo>();

        /// <summary>
        ///  Dictionary<string, IEventInfo> Key->string Value->EventInfo,调用频率不高的时候可以使用
        /// </summary>
        private Dictionary<string, IEventInfo> m_eventStrDic = new Dictionary<string, IEventInfo>();

        #region AddEventListener
        public void AddEventListener<T>(int eventid, Action<T> action)
        {
            if (m_eventDic.ContainsKey(eventid))
            {
                (m_eventDic[eventid] as EventInfo<T>).actions += action;
            }
            else
            {
                m_eventDic.Add(eventid, new EventInfo<T>(action));
            }
        }

        public void AddEventListener<T, U>(int eventid, Action<T, U> action)
        {
            if (m_eventDic.ContainsKey(eventid))
            {
                (m_eventDic[eventid] as EventInfo<T, U>).actions += action;
            }
            else
            {
                m_eventDic.Add(eventid, new EventInfo<T, U>(action));
            }
        }

        public void AddEventListener<T, U, W>(int eventid, Action<T, U, W> action)
        {
            if (m_eventDic.ContainsKey(eventid))
            {
                (m_eventDic[eventid] as EventInfo<T, U, W>).actions += action;
            }
            else
            {
                m_eventDic.Add(eventid, new EventInfo<T, U, W>(action));
            }
        }

        public void AddEventListener(int eventid, Action action)
        {
            if (m_eventDic.ContainsKey(eventid))
            {
                (m_eventDic[eventid] as EventInfo).actions += action;
            }
            else
            {
                m_eventDic.Add(eventid, new EventInfo(action));
            }
        }
        #endregion

        #region RemoveEventListener
        public void RemoveEventListener<T>(int eventid, Action<T> action)
        {
            if (action == null)
            {
                return;
            }

            if (m_eventDic.ContainsKey(eventid))
            {
                (m_eventDic[eventid] as EventInfo<T>).actions -= action;
            }
        }

        public void RemoveEventListener<T, U>(int eventid, Action<T, U> action)
        {
            if (action == null)
            {
                return;
            }

            if (m_eventDic.ContainsKey(eventid))
            {
                (m_eventDic[eventid] as EventInfo<T, U>).actions -= action;
            }
        }

        public void RemoveEventListener<T, U, W>(int eventid, Action<T, U, W> action)
        {
            if (action == null)
            {
                return;
            }

            if (m_eventDic.ContainsKey(eventid))
            {
                (m_eventDic[eventid] as EventInfo<T, U, W>).actions -= action;
            }
        }

        public void RemoveEventListener(int eventid, Action action)
        {
            if (action == null)
            {
                return;
            }

            if (m_eventDic.ContainsKey(eventid))
            {
                (m_eventDic[eventid] as EventInfo).actions -= action;
            }
        }
        #endregion

        #region Send
        public void Send<T>(int eventid, T info)
        {
            if (m_eventDic.ContainsKey(eventid))
            {
                var eventInfo = (m_eventDic[eventid] as EventInfo<T>);
                if (eventInfo != null)
                {
                    eventInfo.actions.Invoke(info);
                }
            }
        }

        public void Send<T, U>(int eventid, T info, U info2)
        {
            if (m_eventDic.ContainsKey(eventid))
            {
                var eventInfo = (m_eventDic[eventid] as EventInfo<T, U>);
                if (eventInfo != null)
                {
                    eventInfo.actions.Invoke(info, info2);
                }
            }
        }

        public void Send<T, U, W>(int eventid, T info, U info2, W info3)
        {
            if (m_eventDic.ContainsKey(eventid))
            {
                var eventInfo = (m_eventDic[eventid] as EventInfo<T, U, W>);
                if (eventInfo != null)
                {
                    eventInfo.actions.Invoke(info, info2, info3);
                }
            }
        }

        /// <summary>
        /// 事件触发 无参
        /// </summary>
        /// <param name="name"></param>
        public void Send(int eventid)
        {
            if (m_eventDic.ContainsKey(eventid))
            {
                var eventInfo = (m_eventDic[eventid] as EventInfo);
                if (eventInfo != null)
                {
                    eventInfo.actions.Invoke();
                }
            }
        }
        #endregion

        #region StringEvent
        #region AddEventListener
        public void AddEventListener<T>(string eventid, Action<T> action)
        {
            if (m_eventStrDic.ContainsKey(eventid))
            {
                (m_eventStrDic[eventid] as EventInfo<T>).actions += action;
            }
            else
            {
                m_eventStrDic.Add(eventid, new EventInfo<T>(action));
            }
        }

        public void AddEventListener<T, U>(string eventid, Action<T, U> action)
        {
            if (m_eventStrDic.ContainsKey(eventid))
            {
                (m_eventStrDic[eventid] as EventInfo<T, U>).actions += action;
            }
            else
            {
                m_eventStrDic.Add(eventid, new EventInfo<T, U>(action));
            }
        }

        public void AddEventListener<T, U, W>(string eventid, Action<T, U, W> action)
        {
            if (m_eventStrDic.ContainsKey(eventid))
            {
                (m_eventStrDic[eventid] as EventInfo<T, U, W>).actions += action;
            }
            else
            {
                m_eventStrDic.Add(eventid, new EventInfo<T, U, W>(action));
            }
        }

        public void AddEventListener(string eventid, Action action)
        {
            if (m_eventStrDic.ContainsKey(eventid))
            {
                (m_eventStrDic[eventid] as EventInfo).actions += action;
            }
            else
            {
                m_eventStrDic.Add(eventid, new EventInfo(action));
            }
        }
        #endregion

        #region RemoveEventListener
        public void RemoveEventListener<T>(string eventid, Action<T> action)
        {
            if (action == null)
            {
                return;
            }

            if (m_eventStrDic.ContainsKey(eventid))
            {
                (m_eventStrDic[eventid] as EventInfo<T>).actions -= action;
            }
        }

        public void RemoveEventListener<T, U>(string eventid, Action<T, U> action)
        {
            if (action == null)
            {
                return;
            }

            if (m_eventStrDic.ContainsKey(eventid))
            {
                (m_eventStrDic[eventid] as EventInfo<T, U>).actions -= action;
            }
        }

        public void RemoveEventListener<T, U, W>(string eventid, Action<T, U, W> action)
        {
            if (action == null)
            {
                return;
            }

            if (m_eventStrDic.ContainsKey(eventid))
            {
                (m_eventStrDic[eventid] as EventInfo<T, U, W>).actions -= action;
            }
        }

        public void RemoveEventListener(string eventid, Action action)
        {
            if (action == null)
            {
                return;
            }

            if (m_eventStrDic.ContainsKey(eventid))
            {
                (m_eventStrDic[eventid] as EventInfo).actions -= action;
            }
        }
        #endregion

        #region Send
        public void Send<T>(string eventid, T info)
        {
            if (m_eventStrDic.ContainsKey(eventid))
            {
                var eventInfo = (m_eventStrDic[eventid] as EventInfo<T>);
                if (eventInfo != null)
                {
                    eventInfo.actions.Invoke(info);
                }
            }
        }

        public void Send<T, U>(string eventid, T info, U info2)
        {
            if (m_eventStrDic.ContainsKey(eventid))
            {
                var eventInfo = (m_eventStrDic[eventid] as EventInfo<T, U>);
                if (eventInfo != null)
                {
                    eventInfo.actions.Invoke(info, info2);
                }
            }
        }

        public void Send<T, U, W>(string eventid, T info, U info2, W info3)
        {
            if (m_eventStrDic.ContainsKey(eventid))
            {
                var eventInfo = (m_eventStrDic[eventid] as EventInfo<T, U, W>);
                if (eventInfo != null)
                {
                    eventInfo.actions.Invoke(info, info2, info3);
                }
            }
        }

        public void Send(string eventid)
        {
            if (m_eventStrDic.ContainsKey(eventid))
            {
                var eventInfo = (m_eventStrDic[eventid] as EventInfo);
                if (eventInfo != null)
                {
                    eventInfo.actions.Invoke();
                }
            }
        }
        #endregion
        #endregion

        #region Clear
        public void Clear()
        {
            m_eventDic.Clear();
            m_eventStrDic.Clear();
        }
        #endregion
    }

}