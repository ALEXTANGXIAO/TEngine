using System;
using System.Collections.Generic;

namespace TEngineCore
{
    public class ECSEventCmpt : ECSComponent
    {
        private Dictionary<int, IEcsEcsEventInfo> m_eventDic = new Dictionary<int, IEcsEcsEventInfo>();

        #region AddEventListener
        public void AddEventListener<T>(int eventid, Action<T> action)
        {
            if (m_eventDic.ContainsKey(eventid))
            {
                (m_eventDic[eventid] as EcsEventInfo<T>).actions += action;
            }
            else
            {
                m_eventDic.Add(eventid, new EcsEventInfo<T>(action));
            }
        }

        public void AddEventListener<T, U>(int eventid, Action<T, U> action)
        {
            if (m_eventDic.ContainsKey(eventid))
            {
                (m_eventDic[eventid] as EcsEventInfo<T, U>).actions += action;
            }
            else
            {
                m_eventDic.Add(eventid, new EcsEventInfo<T, U>(action));
            }
        }

        public void AddEventListener<T, U, W>(int eventid, Action<T, U, W> action)
        {
            if (m_eventDic.ContainsKey(eventid))
            {
                (m_eventDic[eventid] as EcsEventInfo<T, U, W>).actions += action;
            }
            else
            {
                m_eventDic.Add(eventid, new EcsEventInfo<T, U, W>(action));
            }
        }

        public void AddEventListener(int eventid, Action action)
        {
            if (m_eventDic.ContainsKey(eventid))
            {
                (m_eventDic[eventid] as EcsEventInfo).actions += action;
            }
            else
            {
                m_eventDic.Add(eventid, new EcsEventInfo(action));
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
                (m_eventDic[eventid] as EcsEventInfo<T>).actions -= action;
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
                (m_eventDic[eventid] as EcsEventInfo<T, U>).actions -= action;
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
                (m_eventDic[eventid] as EcsEventInfo<T, U, W>).actions -= action;
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
                (m_eventDic[eventid] as EcsEventInfo).actions -= action;
            }
        }
        #endregion

        #region Send
        public void Send<T>(int eventid, T info)
        {
            if (m_eventDic.ContainsKey(eventid))
            {
                var EcsEventInfo = (m_eventDic[eventid] as EcsEventInfo<T>);
                if (EcsEventInfo != null)
                {
                    EcsEventInfo.actions.Invoke(info);
                }
            }
        }

        public void Send<T, U>(int eventid, T info, U info2)
        {
            if (m_eventDic.ContainsKey(eventid))
            {
                var EcsEventInfo = (m_eventDic[eventid] as EcsEventInfo<T, U>);
                if (EcsEventInfo != null)
                {
                    EcsEventInfo.actions.Invoke(info, info2);
                }
            }
        }

        public void Send<T, U, W>(int eventid, T info, U info2, W info3)
        {
            if (m_eventDic.ContainsKey(eventid))
            {
                var EcsEventInfo = (m_eventDic[eventid] as EcsEventInfo<T, U, W>);
                if (EcsEventInfo != null)
                {
                    EcsEventInfo.actions.Invoke(info, info2, info3);
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
                var EcsEventInfo = (m_eventDic[eventid] as EcsEventInfo);
                if (EcsEventInfo != null)
                {
                    EcsEventInfo.actions.Invoke();
                }
            }
        }
        #endregion

        #region Clear
        public void Clear()
        {
            m_eventDic.Clear();
        }
        #endregion

        #region 生命周期
        public override void OnDestroy()
        {
            Clear();
        }

        public override void Awake()
        {
            Entity.Event = this;
        }
        #endregion
    }

    #region EcsEventInfo
    internal interface IEcsEcsEventInfo
    {

    }

    public class EcsEventInfo : IEcsEcsEventInfo
    {
        public Action actions;

        public EcsEventInfo(Action action)
        {
            actions += action;
        }
    }


    public class EcsEventInfo<T> : IEcsEcsEventInfo
    {
        public Action<T> actions;

        public EcsEventInfo(Action<T> action)
        {
            actions += action;
        }
    }

    public class EcsEventInfo<T, U> : IEcsEcsEventInfo
    {
        public Action<T, U> actions;

        public EcsEventInfo(Action<T, U> action)
        {
            actions += action;
        }
    }

    public class EcsEventInfo<T, U, W> : IEcsEcsEventInfo
    {
        public Action<T, U, W> actions;

        public EcsEventInfo(Action<T, U, W> action)
        {
            actions += action;
        }
    }
    #endregion

}