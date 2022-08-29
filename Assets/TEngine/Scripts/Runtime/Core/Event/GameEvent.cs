using System;
using System.Collections.Generic;

namespace TEngine.Runtime
{
    #region EventInfo
    internal interface IEventInfo
    {
        void Free();
    }

    public class EventInfo : IEventInfo, IMemory
    {
        private Action _actions;

        public Action Actions => _actions;

        public int EventCount
        {
            set;
            get;
        }

        public void AddAction(Action action)
        {
            _actions += action;
            EventCount++;
        }

        public void RmvAction(Action action)
        {
            _actions -= action;
            EventCount--;
        }

        public void Clear()
        {
            EventCount = 0;
            _actions = null;
        }

        public void Free()
        {
            MemoryPool.Release(this);
        }
    }


    public class EventInfo<T> : IEventInfo, IMemory
    {
        private Action<T> _actions;

        public Action<T> Actions => _actions;

        public int EventCount
        {
            set;
            get;
        }

        public void AddAction(Action<T> action)
        {
            _actions += action;
            EventCount++;
        }

        public void RmvAction(Action<T> action)
        {
            _actions -= action;
            EventCount--;
        }

        public void Clear()
        {
            EventCount = 0;
            _actions = null;
        }

        public void Free()
        {
            MemoryPool.Release(this);
        }
    }

    public class EventInfo<T, U> : IEventInfo, IMemory
    {
        private Action<T, U> _actions;

        public Action<T, U> Actions => _actions;

        public int EventCount
        {
            set;
            get;
        }

        public void AddAction(Action<T, U> action)
        {
            _actions += action;
            EventCount++;
        }

        public void RmvAction(Action<T, U> action)
        {
            _actions -= action;
            EventCount--;
        }

        public void Clear()
        {
            EventCount = 0;
            _actions = null;
        }

        public void Free()
        {
            MemoryPool.Release(this);
        }
    }

    public class EventInfo<T, U, W> : IEventInfo, IMemory
    {
        private Action<T, U, W> _actions;

        public Action<T, U, W> Actions => _actions;

        public int EventCount
        {
            set;
            get;
        }

        public void AddAction(Action<T, U, W> action)
        {
            _actions += action;
            EventCount++;
        }

        public void RmvAction(Action<T, U, W> action)
        {
            _actions -= action;
            EventCount--;
        }

        public void Clear()
        {
            EventCount = 0;
            _actions = null;
        }

        public void Free()
        {
            MemoryPool.Release(this);
        }
    }
    #endregion

    public class GameEvent : IMemory
    {
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
                if (eventInfo != null)
                {
                    eventInfo.AddAction(action);
                }
                else
                {
                    throw new Exception("The Same GameEventId AddEventListener Need Same Args");
                }
            }
            else
            {
                var eventInfo = MemoryPool.Acquire<EventInfo<T>>();
                eventInfo.AddAction(action);
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
                    eventInfo.AddAction(action);
                }
                else
                {
                    throw new Exception("The Same GameEventId AddEventListener Need Same Args");
                }
            }
            else
            {
                var eventInfo = MemoryPool.Acquire<EventInfo<T, U>>();
                eventInfo.AddAction(action);
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
                    eventInfo.AddAction(action);
                }
                else
                {
                    throw new Exception("The Same GameEventId AddEventListener Need Same Args");
                }
            }
            else
            {
                var eventInfo = MemoryPool.Acquire<EventInfo<T, U, W>>();
                eventInfo.AddAction(action);
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
                    eventInfo.AddAction(action);
                }
                else
                {
                    throw new Exception("The Same GameEventId AddEventListener Need Same Args");
                }
            }
            else
            {
                var eventInfo = MemoryPool.Acquire<EventInfo>();
                eventInfo.AddAction(action);
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
                    eventInfo.RmvAction(action);
                    if (eventInfo.EventCount <= 0)
                    {
                        _eventDic.Remove(eventId);
                        MemoryPool.Release(eventInfo);
                    }
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
                    eventInfo.RmvAction(action);
                    if (eventInfo.EventCount <= 0)
                    {
                        _eventDic.Remove(eventId);
                        MemoryPool.Release(eventInfo);
                    }
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
                    eventInfo.RmvAction(action);
                    if (eventInfo.EventCount <= 0)
                    {
                        _eventDic.Remove(eventId);
                        MemoryPool.Release(eventInfo);
                    }
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
                    eventInfo.RmvAction(action);
                    if (eventInfo.EventCount <= 0)
                    {
                        _eventDic.Remove(eventId);
                        MemoryPool.Release(eventInfo);
                    }
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
                    eventInfo.Actions?.Invoke(info);
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
                    eventInfo.Actions?.Invoke(info, info2);
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
                    eventInfo.Actions?.Invoke(info, info2, info3);
                }
            }
        }

        public void Send(int eventId)
        {
            if (_eventDic.ContainsKey(eventId))
            {
                var eventInfo = _eventDic[eventId] as EventInfo;
                if (eventInfo != null)
                {
                    eventInfo.Actions?.Invoke();
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
                    eventInfo.AddAction(action);
                }
                else
                {
                    throw new Exception("The Same GameEventId AddEventListener Need Same Args");
                }
            }
            else
            {
                var eventInfo = MemoryPool.Acquire<EventInfo<T>>();
                eventInfo.AddAction(action);
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
                    eventInfo.AddAction(action);
                }
                else
                {
                    throw new Exception("The Same GameEventId AddEventListener Need Same Args");
                }
            }
            else
            {
                var eventInfo = MemoryPool.Acquire<EventInfo<T, U>>();
                eventInfo.AddAction(action);
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
                    eventInfo.AddAction(action);
                }
                else
                {
                    throw new Exception("The Same GameEventId AddEventListener Need Same Args");
                }
            }
            else
            {
                var eventInfo = MemoryPool.Acquire<EventInfo<T, U, W>>();
                eventInfo.AddAction(action);
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
                    eventInfo.AddAction(action);
                }
                else
                {
                    throw new Exception("The Same GameEventId AddEventListener Need Same Args");
                }
            }
            else
            {
                var eventInfo = MemoryPool.Acquire<EventInfo>();
                eventInfo.AddAction(action);
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
                var eventInfo = m_eventStrDic[eventId] as EventInfo<T>;
                if (eventInfo != null)
                {
                    eventInfo.RmvAction(action);
                    if (eventInfo.EventCount <= 0)
                    {
                        m_eventStrDic.Remove(eventId);
                        MemoryPool.Release(eventInfo);
                    }
                }
                else
                {
                    throw new Exception("The Same GameEventId RemoveEventListener Need Same Args");
                }
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
                var eventInfo = m_eventStrDic[eventId] as EventInfo<T, U>;
                if (eventInfo != null)
                {
                    eventInfo.RmvAction(action);
                    if (eventInfo.EventCount <= 0)
                    {
                        m_eventStrDic.Remove(eventId);
                        MemoryPool.Release(eventInfo);
                    }
                }
                else
                {
                    throw new Exception("The Same GameEventId RemoveEventListener Need Same Args");
                }
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
                var eventInfo = m_eventStrDic[eventId] as EventInfo<T, U, W>;
                if (eventInfo != null)
                {
                    eventInfo.RmvAction(action);
                    if (eventInfo.EventCount <= 0)
                    {
                        m_eventStrDic.Remove(eventId);
                        MemoryPool.Release(eventInfo);
                    }
                }
                else
                {
                    throw new Exception("The Same GameEventId RemoveEventListener Need Same Args");
                }
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
                var eventInfo = m_eventStrDic[eventId] as EventInfo;
                if (eventInfo != null)
                {
                    eventInfo.RmvAction(action);
                    if (eventInfo.EventCount <= 0)
                    {
                        m_eventStrDic.Remove(eventId);
                        MemoryPool.Release(eventInfo);
                    }
                }
                else
                {
                    throw new Exception("The Same GameEventId RemoveEventListener Need Same Args");
                }
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
                    eventInfo.Actions?.Invoke(info);
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
                    eventInfo.Actions?.Invoke(info, info2);
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
                    eventInfo.Actions?.Invoke(info, info2, info3);
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
                    eventInfo.Actions?.Invoke();
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