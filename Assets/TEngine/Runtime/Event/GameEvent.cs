using System;
using System.Collections.Generic;

namespace TEngine
{
    #region EventInfo
    internal interface IEventInfo
    {
        void Free();
    }

    public class EventInfo : IEventInfo, IMemPoolObject
    {
        public Action actions;
        public EventInfo()
        {

        }

        public EventInfo(Action action)
        {
            actions += action;
        }

        public void Init()
        {
            actions = null;
        }

        public void Destroy()
        {
            actions = null;
        }

        public void Free()
        {
            GameMemPool<EventInfo>.Free(this);
        }
    }


    public class EventInfo<T> : IEventInfo, IMemPoolObject
    {
        public Action<T> actions;

        public EventInfo()
        {

        }

        public EventInfo(Action<T> action)
        {
            actions += action;
        }

        public void Init()
        {
            actions = null;
        }

        public void Destroy()
        {
            actions = null;
        }

        public void Free()
        {
            GameMemPool<EventInfo<T>>.Free(this);
        }
    }

    public class EventInfo<T, U> : IEventInfo, IMemPoolObject
    {
        public Action<T, U> actions;

        public EventInfo()
        {

        }

        public EventInfo(Action<T, U> action)
        {
            actions += action;
        }

        public void Init()
        {
            actions = null;
        }

        public void Destroy()
        {
            actions = null;
        }

        public void Free()
        {
            GameMemPool<EventInfo<T, U>>.Free(this);
        }
    }

    public class EventInfo<T, U, W> : IEventInfo, IMemPoolObject
    {
        public Action<T, U, W> actions;

        public EventInfo()
        {

        }

        public EventInfo(Action<T, U, W> action)
        {
            actions += action;
        }

        public void Init()
        {
            actions = null;
        }

        public void Destroy()
        {
            actions = null;
        }

        public void Free()
        {
            GameMemPool<EventInfo<T, U, W>>.Free(this);
        }
    }
    #endregion

    public class GameEvent : IMemPoolObject
    {
        public void Init()
        {
            
        }

        public void Destroy()
        {
            Clear();
        }

        /// <summary>
        ///  Dictionary Key->Int.32 Value->EventInfo,调用频率高建议使用int事件，减少字典内String的哈希碰撞
        /// </summary>
        private Dictionary<int, IEventInfo> _eventDic = new Dictionary<int, IEventInfo>();

        /// <summary>
        ///  Dictionary Key->string Value->EventInfo,调用频率不高的时候可以使用
        /// </summary>
        private Dictionary<string, IEventInfo> m_eventStrDic = new Dictionary<string, IEventInfo>();

        #region AddEventListener
        public void AddEventListener<T>(int eventId, Action<T> action)
        {
            if (_eventDic.ContainsKey(eventId))
            {
                var eventInfo = _eventDic[eventId] as EventInfo<T>;
                if (eventInfo!= null)
                {
                    eventInfo.actions += action;
                }
                else
                {
                    throw new Exception("The Same GameEventId AddEventListener Need Same Args");
                }
            }
            else
            {
                var eventInfo = GameMemPool<EventInfo<T>>.Alloc();
                eventInfo.actions += action;
                _eventDic.Add(eventId, eventInfo);
            }
        }

        public void AddEventListener<T, U>(int eventId, Action<T, U> action)
        {
            if (_eventDic.ContainsKey(eventId))
            {
                var eventInfo = _eventDic[eventId] as EventInfo<T, U>;
                if (eventInfo != null)
                {
                    eventInfo.actions += action;
                }
                else
                {
                    throw new Exception("The Same GameEventId AddEventListener Need Same Args");
                }
            }
            else
            {
                var eventInfo = GameMemPool<EventInfo<T, U>>.Alloc();
                eventInfo.actions += action;
                _eventDic.Add(eventId, eventInfo);
            }
        }

        public void AddEventListener<T, U, W>(int eventId, Action<T, U, W> action)
        {
            if (_eventDic.ContainsKey(eventId))
            {
                var eventInfo = _eventDic[eventId] as EventInfo<T, U, W>;
                if (eventInfo != null)
                {
                    eventInfo.actions += action;
                }
                else
                {
                    throw new Exception("The Same GameEventId AddEventListener Need Same Args");
                }
            }
            else
            {
                var eventInfo = GameMemPool<EventInfo<T, U, W>>.Alloc();
                eventInfo.actions += action;
                _eventDic.Add(eventId, eventInfo);
            }
        }

        public void AddEventListener(int eventId, Action action)
        {
            if (_eventDic.ContainsKey(eventId))
            {
                var eventInfo = _eventDic[eventId] as EventInfo;
                if (eventInfo != null)
                {
                    eventInfo.actions += action;
                }
                else
                {
                    throw new Exception("The Same GameEventId AddEventListener Need Same Args");
                }
            }
            else
            {
                var eventInfo = GameMemPool<EventInfo>.Alloc();
                eventInfo.actions += action;
                _eventDic.Add(eventId, eventInfo);
            }
        }
        #endregion

        #region RemoveEventListener
        public void RemoveEventListener<T>(int eventId, Action<T> action)
        {
            if (action == null)
            {
                return;
            }

            if (_eventDic.ContainsKey(eventId))
            {
                var eventInfo = _eventDic[eventId] as EventInfo<T>;
                if (eventInfo != null)
                {
                    eventInfo.actions -= action;
                }
                else
                {
                    throw new Exception("The Same GameEventId RemoveEventListener Need Same Args");
                }
            }
        }

        public void RemoveEventListener<T, U>(int eventId, Action<T, U> action)
        {
            if (action == null)
            {
                return;
            }

            if (_eventDic.ContainsKey(eventId))
            {
                var eventInfo = _eventDic[eventId] as EventInfo<T, U>;
                if (eventInfo != null)
                {
                    eventInfo.actions -= action;
                }
                else
                {
                    throw new Exception("The Same GameEventId RemoveEventListener Need Same Args");
                }
            }
        }

        public void RemoveEventListener<T, U, W>(int eventId, Action<T, U, W> action)
        {
            if (action == null)
            {
                return;
            }

            if (_eventDic.ContainsKey(eventId))
            {
                var eventInfo = _eventDic[eventId] as EventInfo<T, U, W>;
                if (eventInfo != null)
                {
                    eventInfo.actions -= action;
                }
                else
                {
                    throw new Exception("The Same GameEventId RemoveEventListener Need Same Args");
                }
            }
        }

        public void RemoveEventListener(int eventId, Action action)
        {
            if (action == null)
            {
                return;
            }

            if (_eventDic.ContainsKey(eventId))
            {
                var eventInfo = _eventDic[eventId] as EventInfo;
                if (eventInfo != null)
                {
                    eventInfo.actions -= action;
                }
                else
                {
                    throw new Exception("The Same GameEventId RemoveEventListener Need Same Args");
                }
            }
        }
        #endregion

        #region Send
        public void Send<T>(int eventId, T info)
        {
            if (_eventDic.ContainsKey(eventId))
            {
                var eventInfo = _eventDic[eventId] as EventInfo<T>;
                if (eventInfo != null)
                {
                    eventInfo.actions.Invoke(info);
                }
            }
        }

        public void Send<T, U>(int eventId, T info, U info2)
        {
            if (_eventDic.ContainsKey(eventId))
            {
                var eventInfo = _eventDic[eventId] as EventInfo<T, U>;
                if (eventInfo != null)
                {
                    eventInfo.actions.Invoke(info, info2);
                }
            }
        }

        public void Send<T, U, W>(int eventId, T info, U info2, W info3)
        {
            if (_eventDic.ContainsKey(eventId))
            {
                var eventInfo = _eventDic[eventId] as EventInfo<T, U, W>;
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
        public void Send(int eventId)
        {
            if (_eventDic.ContainsKey(eventId))
            {
                var eventInfo = _eventDic[eventId] as EventInfo;
                if (eventInfo != null)
                {
                    eventInfo.actions.Invoke();
                }
            }
        }
        #endregion

        #region StringEvent
        #region AddEventListener
        public void AddEventListener<T>(string eventId, Action<T> action)
        {
            if (m_eventStrDic.ContainsKey(eventId))
            {
                var eventInfo = m_eventStrDic[eventId] as EventInfo<T>;
                if (eventInfo != null)
                {
                    eventInfo.actions += action;
                }
                else
                {
                    throw new Exception("The Same GameEventId AddEventListener Need Same Args");
                }
            }
            else
            {
                var eventInfo = GameMemPool<EventInfo<T>>.Alloc();
                eventInfo.actions += action;
                m_eventStrDic.Add(eventId, eventInfo);
            }
        }

        public void AddEventListener<T, U>(string eventId, Action<T, U> action)
        {
            if (m_eventStrDic.ContainsKey(eventId))
            {
                var eventInfo = m_eventStrDic[eventId] as EventInfo<T, U>;
                if (eventInfo != null)
                {
                    eventInfo.actions += action;
                }
                else
                {
                    throw new Exception("The Same GameEventId AddEventListener Need Same Args");
                }
            }
            else
            {
                var eventInfo = GameMemPool<EventInfo<T, U>>.Alloc();
                eventInfo.actions += action;
                m_eventStrDic.Add(eventId, eventInfo);
            }
        }

        public void AddEventListener<T, U, W>(string eventId, Action<T, U, W> action)
        {
            if (m_eventStrDic.ContainsKey(eventId))
            {
                var eventInfo = m_eventStrDic[eventId] as EventInfo<T, U, W>;
                if (eventInfo != null)
                {
                    eventInfo.actions += action;
                }
                else
                {
                    throw new Exception("The Same GameEventId AddEventListener Need Same Args");
                }
            }
            else
            {
                var eventInfo = GameMemPool<EventInfo<T, U, W>>.Alloc();
                eventInfo.actions += action;
                m_eventStrDic.Add(eventId, eventInfo);
            }
        }

        public void AddEventListener(string eventId, Action action)
        {
            if (m_eventStrDic.ContainsKey(eventId))
            {
                var eventInfo = m_eventStrDic[eventId] as EventInfo;
                if (eventInfo != null)
                {
                    eventInfo.actions += action;
                }
                else
                {
                    throw new Exception("The Same GameEventId AddEventListener Need Same Args");
                }
            }
            else
            {
                var eventInfo = GameMemPool<EventInfo>.Alloc();
                eventInfo.actions += action;
                m_eventStrDic.Add(eventId, eventInfo);
            }
        }
        #endregion

        #region RemoveEventListener
        public void RemoveEventListener<T>(string eventId, Action<T> action)
        {
            if (action == null)
            {
                return;
            }

            if (m_eventStrDic.ContainsKey(eventId))
            {
                (m_eventStrDic[eventId] as EventInfo<T>).actions -= action;
            }
        }

        public void RemoveEventListener<T, U>(string eventId, Action<T, U> action)
        {
            if (action == null)
            {
                return;
            }

            if (m_eventStrDic.ContainsKey(eventId))
            {
                (m_eventStrDic[eventId] as EventInfo<T, U>).actions -= action;
            }
        }

        public void RemoveEventListener<T, U, W>(string eventId, Action<T, U, W> action)
        {
            if (action == null)
            {
                return;
            }

            if (m_eventStrDic.ContainsKey(eventId))
            {
                (m_eventStrDic[eventId] as EventInfo<T, U, W>).actions -= action;
            }
        }

        public void RemoveEventListener(string eventId, Action action)
        {
            if (action == null)
            {
                return;
            }

            if (m_eventStrDic.ContainsKey(eventId))
            {
                (m_eventStrDic[eventId] as EventInfo).actions -= action;
            }
        }
        #endregion

        #region Send
        public void Send<T>(string eventId, T info)
        {
            if (m_eventStrDic.ContainsKey(eventId))
            {
                var eventInfo = (m_eventStrDic[eventId] as EventInfo<T>);
                if (eventInfo != null)
                {
                    eventInfo.actions.Invoke(info);
                }
            }
        }

        public void Send<T, U>(string eventId, T info, U info2)
        {
            if (m_eventStrDic.ContainsKey(eventId))
            {
                var eventInfo = (m_eventStrDic[eventId] as EventInfo<T, U>);
                if (eventInfo != null)
                {
                    eventInfo.actions.Invoke(info, info2);
                }
            }
        }

        public void Send<T, U, W>(string eventId, T info, U info2, W info3)
        {
            if (m_eventStrDic.ContainsKey(eventId))
            {
                var eventInfo = (m_eventStrDic[eventId] as EventInfo<T, U, W>);
                if (eventInfo != null)
                {
                    eventInfo.actions.Invoke(info, info2, info3);
                }
            }
        }

        public void Send(string eventId)
        {
            if (m_eventStrDic.ContainsKey(eventId))
            {
                var eventInfo = (m_eventStrDic[eventId] as EventInfo);
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
            var etr = _eventDic.GetEnumerator();
            while (etr.MoveNext())
            {
                var eventInfo = etr.Current.Value;
                eventInfo.Free();
            }
            etr.Dispose();

            var etrStr = m_eventStrDic.GetEnumerator();
            while (etrStr.MoveNext())
            {
                var eventInfo = etrStr.Current.Value;
                eventInfo.Free();
            }
            etrStr.Dispose();

            _eventDic.Clear();
            m_eventStrDic.Clear();
        }
        #endregion
    }

}