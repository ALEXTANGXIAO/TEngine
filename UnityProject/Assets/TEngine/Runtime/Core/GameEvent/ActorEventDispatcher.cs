using System;
using System.Collections.Generic;

#region Class Documentation
/************************************************************************************************************
Class Name:     ActorEventDispatcher.cs  局部单位事件分发器。
Type:           Actor, Event
Example:
                private ActorEventDispatcher _event;

                /// <summary>
                /// 局部事件管理器。
                /// <remark>只分发和监听这个Event内部的事件</remark>
                /// </summary>
                public ActorEventDispatcher Event => _event ??= MemoryPool.Acquire<ActorEventDispatcher>();
                
                // owner局部发送事件。
                owner.Event.Send(eventId,xxx);
                
                // owner监听自身事件并绑定持有对象为owner。 比如是组件的情况下。移除时调用RemoveAllListenerByOwner(owner)会根据持有对象（组件）移除。
                owner.Event.AddEventListener(eventId,xxx,owner);
************************************************************************************************************/
#endregion

namespace TEngine
{
    /// <summary>
    /// 局部单位事件分发器。
    /// </summary>
    public class ActorEventDispatcher : IMemory
    {
        /// <summary>
        /// 所有事件。
        /// </summary>
        private readonly Dictionary<int, List<EventRegInfo>> _allEventListenerMap;

        /// <summary>
        /// 用于标记一个事件是不是正在处理。
        /// </summary>
        private readonly List<int> _processEventList;

        /// <summary>
        /// 用于标记一个事件是不是被移除。
        /// </summary>
        private readonly List<int> _delayDeleteEventList;

        public ActorEventDispatcher()
        {
            _processEventList = new List<int>();
            _delayDeleteEventList = new List<int>();
            _allEventListenerMap = new Dictionary<int, List<EventRegInfo>>();
        }

        /// <summary>
        /// 移除所有事件监听。
        /// </summary>
        public void DestroyAllEventListener()
        {
            var itr = _allEventListenerMap.GetEnumerator();
            while (itr.MoveNext())
            {
                var kv = itr.Current;
                List<EventRegInfo> list = kv.Value;
                foreach (var eventRegInfo in list)
                {
                    EventRegInfo.Release(eventRegInfo);
                }
                kv.Value.Clear();
            }

            _processEventList.Clear();
            _delayDeleteEventList.Clear();
            itr.Dispose();
        }

        /// <summary>
        /// 延迟移除事件。
        /// </summary>
        /// <param name="eventId"></param>
        private void AddDelayDelete(int eventId)
        {
            if (!_delayDeleteEventList.Contains(eventId))
            {
                _delayDeleteEventList.Add(eventId);
                Log.Info("delay delete eventId[{0}]", eventId);
            }
        }

        /// <summary>
        /// 如果找到eventId对应的监听，删除所有标记为delete的监听。
        /// </summary>
        /// <param name="eventId">事件Id。</param>
        private void CheckDelayDelete(int eventId)
        {
            if (_delayDeleteEventList.Contains(eventId))
            {
                if (_allEventListenerMap.TryGetValue(eventId, out var listListener))
                {
                    for (int i = listListener.Count - 1; i >= 0 ; i--)
                    {
                        if (listListener[i].IsDeleted)
                        {
                            Log.Info("remove delay delete eventId[{0}]", eventId);
                            EventRegInfo.Release(listListener[i]);
                            listListener.RemoveAt(i);
                        }
                    }
                }

                _delayDeleteEventList.Remove(eventId);
            }
        }

        /// <summary>
        /// 发送事件。
        /// </summary>
        /// <param name="eventId">事件Id。</param>
        public void SendEvent(int eventId)
        {
            if (_allEventListenerMap.TryGetValue(eventId, out var listListener))
            {
                _processEventList.Add(eventId);
#if UNITY_EDITOR
                int iEventCnt = _processEventList.Count;
#endif

                var count = listListener.Count;
                for (int i = 0; i < count; i++)
                {
                    var node = listListener[i];
                    if (node.IsDeleted)
                    {
                        continue;
                    }

                    if (listListener[i].Callback is Action callBack)
                    {
                        callBack();
                    }
                    else
                    {
                        Log.Fatal("Invalid event data type: {0}", eventId);
                    }
                }


#if UNITY_EDITOR
                Log.Assert(iEventCnt == _processEventList.Count);
                Log.Assert(eventId == _processEventList[^1]);
#endif
                _processEventList.RemoveAt(_processEventList.Count - 1);

                CheckDelayDelete(eventId);
            }
        }

        /// <summary>
        /// 发送事件。
        /// </summary>
        /// <param name="eventId">事件Id。</param>
        /// <param name="arg1">事件参数。</param>
        /// <typeparam name="TArg1">事件参数类型1。</typeparam>
        public void SendEvent<TArg1>(int eventId, TArg1 arg1)
        {
            if (_allEventListenerMap.TryGetValue(eventId, out var listListener))
            {
                _processEventList.Add(eventId);
#if UNITY_EDITOR
                int iEventCnt = _processEventList.Count;
#endif

                var count = listListener.Count;
                for (int i = 0; i < count; i++)
                {
                    var node = listListener[i];
                    if (node.IsDeleted)
                    {
                        continue;
                    }

                    if (listListener[i].Callback is Action<TArg1> callBack)
                    {
                        callBack(arg1);
                    }
                    else
                    {
                        Log.Fatal("Invalid event data type: {0}", eventId);
                    }
                }


#if UNITY_EDITOR
                Log.Assert(iEventCnt == _processEventList.Count);
                Log.Assert(eventId == _processEventList[^1]);
#endif

                _processEventList.RemoveAt(_processEventList.Count - 1);

                CheckDelayDelete(eventId);
            }
        }

        /// <summary>
        /// 发送事件。
        /// </summary>
        /// <param name="eventId">事件Id。</param>
        /// <param name="arg1">事件参数1。</param>
        /// <param name="arg2">事件参数2。</param>
        /// <typeparam name="TArg1">事件参数类型1。</typeparam>
        /// <typeparam name="TArg2">事件参数类型2。</typeparam>
        public void SendEvent<TArg1, TArg2>(int eventId, TArg1 arg1, TArg2 arg2)
        {
            if (_allEventListenerMap.TryGetValue(eventId, out var listListener))
            {
                _processEventList.Add(eventId);
#if UNITY_EDITOR
                int iEventCnt = _processEventList.Count;
#endif

                var count = listListener.Count;
                for (int i = 0; i < count; i++)
                {
                    var node = listListener[i];
                    if (node.IsDeleted)
                    {
                        continue;
                    }

                    if (listListener[i].Callback is Action<TArg1, TArg2> callBack)
                    {
                        callBack(arg1, arg2);
                    }
                    else
                    {
                        Log.Fatal("Invalid event data type: {0}", eventId);
                    }
                }


#if UNITY_EDITOR
                Log.Assert(iEventCnt == _processEventList.Count);
                Log.Assert(eventId == _processEventList[^1]);
#endif
                _processEventList.RemoveAt(_processEventList.Count - 1);

                CheckDelayDelete(eventId);
            }
        }

        /// <summary>
        /// 发送事件。
        /// </summary>
        /// <param name="eventId">事件Id。</param>
        /// <param name="arg1">事件参数1。</param>
        /// <param name="arg2">事件参数2。</param>
        /// <param name="arg3">事件参数3。</param>
        /// <typeparam name="TArg1">事件参数类型1。</typeparam>
        /// <typeparam name="TArg2">事件参数类型2。</typeparam>
        /// <typeparam name="TArg3">事件参数类型3。</typeparam>
        public void SendEvent<TArg1, TArg2, TArg3>(int eventId, TArg1 arg1, TArg2 arg2, TArg3 arg3)
        {
            if (_allEventListenerMap.TryGetValue(eventId, out var listListener))
            {
                _processEventList.Add(eventId);
#if UNITY_EDITOR
                int iEventCnt = _processEventList.Count;
#endif

                var count = listListener.Count;
                for (int i = 0; i < count; i++)
                {
                    var node = listListener[i];
                    if (node.IsDeleted)
                    {
                        continue;
                    }

                    if (node.Callback is Action<TArg1, TArg2, TArg3> callBack)
                    {
                        callBack(arg1, arg2, arg3);
                    }
                    else
                    {
                        Log.Fatal("Invalid event data type: {0}", eventId);
                    }
                }


#if UNITY_EDITOR
                Log.Assert(iEventCnt == _processEventList.Count);
                Log.Assert(eventId == _processEventList[^1]);
#endif
                _processEventList.RemoveAt(_processEventList.Count - 1);

                CheckDelayDelete(eventId);
            }
        }

        /// <summary>
        /// 发送事件。
        /// </summary>
        /// <param name="eventId">事件Id。</param>
        /// <param name="arg1">事件参数1。</param>
        /// <param name="arg2">事件参数2。</param>
        /// <param name="arg3">事件参数3。</param>
        /// <param name="arg4">事件参数4。</param>
        /// <typeparam name="TArg1">事件参数类型1。</typeparam>
        /// <typeparam name="TArg2">事件参数类型2。</typeparam>
        /// <typeparam name="TArg3">事件参数类型3。</typeparam>
        /// <typeparam name="TArg4">事件参数类型4。</typeparam>
        public void SendEvent<TArg1, TArg2, TArg3, TArg4>(int eventId, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
        {
            if (_allEventListenerMap.TryGetValue(eventId, out var listListener))
            {
                _processEventList.Add(eventId);
#if UNITY_EDITOR
                int iEventCnt = _processEventList.Count;
#endif


                var count = listListener.Count;
                for (int i = 0; i < count; i++)
                {
                    var node = listListener[i];
                    if (node.IsDeleted)
                    {
                        continue;
                    }

                    if (listListener[i].Callback is Action<TArg1, TArg2, TArg3, TArg4> callBack)
                    {
                        callBack(arg1, arg2, arg3, arg4);
                    }
                    else
                    {
                        Log.Fatal("Invalid event data type: {0}", eventId);
                    }
                }


#if UNITY_EDITOR
                Log.Assert(iEventCnt == _processEventList.Count);
                Log.Assert(eventId == _processEventList[^1]);
#endif
                _processEventList.RemoveAt(_processEventList.Count - 1);

                CheckDelayDelete(eventId);
            }
        }

        /// <summary>
        /// 增加事件监听。
        /// </summary>
        /// <param name="eventId">事件Id。</param>
        /// <param name="eventCallback">事件回调。</param>
        /// <param name="owner">持有者Tag。</param>
        public void AddEventListener(int eventId, Action eventCallback, object owner)
        {
            AddEventListenerImp(eventId, eventCallback, owner);
        }

        /// <summary>
        /// 增加事件监听。
        /// </summary>
        /// <param name="eventId">事件Id。</param>
        /// <param name="eventCallback">事件回调。</param>
        /// <param name="owner">持有者Tag。</param>
        /// <typeparam name="TArg1">事件参数类型1。</typeparam>
        public void AddEventListener<TArg1>(int eventId, Action<TArg1> eventCallback, object owner)
        {
            AddEventListenerImp(eventId, eventCallback, owner);
        }

        /// <summary>
        /// 增加事件监听。
        /// </summary>
        /// <param name="eventId">事件Id。</param>
        /// <param name="eventCallback">事件回调。</param>
        /// <param name="owner">持有者Tag。</param>
        /// <typeparam name="TArg1">事件参数类型1。</typeparam>
        /// <typeparam name="TArg2">事件参数类型2。</typeparam>
        public void AddEventListener<TArg1, TArg2>(int eventId, Action<TArg1, TArg2> eventCallback, object owner)
        {
            AddEventListenerImp(eventId, eventCallback, owner);
        }

        /// <summary>
        /// 增加事件监听。
        /// </summary>
        /// <param name="eventId">事件Id。</param>
        /// <param name="eventCallback">事件回调。</param>
        /// <param name="owner">持有者Tag。</param>
        /// <typeparam name="TArg1">事件参数类型1。</typeparam>
        /// <typeparam name="TArg2">事件参数类型2。</typeparam>
        /// <typeparam name="TArg3">事件参数类型3。</typeparam>
        public void AddEventListener<TArg1, TArg2, TArg3>(int eventId, Action<TArg1, TArg2, TArg3> eventCallback, object owner)
        {
            AddEventListenerImp(eventId, eventCallback, owner);
        }

        /// <summary>
        /// 增加事件监听。
        /// </summary>
        /// <param name="eventId">事件Id。</param>
        /// <param name="eventCallback">事件回调。</param>
        /// <param name="owner">持有者Tag。</param>
        /// <typeparam name="TArg1">事件参数类型1。</typeparam>
        /// <typeparam name="TArg2">事件参数类型2。</typeparam>
        /// <typeparam name="TArg3">事件参数类型3。</typeparam>
        /// <typeparam name="TArg4">事件参数类型4。</typeparam>
        public void AddEventListener<TArg1, TArg2, TArg3, TArg4>(int eventId, Action<TArg1, TArg2, TArg3, TArg4> eventCallback, object owner)
        {
            AddEventListenerImp(eventId, eventCallback, owner);
        }

        /// <summary>
        /// 增加事件监听具体实现。
        /// </summary>
        /// <param name="eventId">事件Id。</param>
        /// <param name="listener">事件回调。</param>
        /// <param name="owner">持有者Tag。</param>
        private void AddEventListenerImp(int eventId, Delegate listener, object owner)
        {
            if (!_allEventListenerMap.TryGetValue(eventId, out var listListener))
            {
                listListener = new List<EventRegInfo>();
                _allEventListenerMap.Add(eventId, listListener);
            }

            var existNode = listListener.Find((node) => node.Callback == listener);
            if (existNode != null)
            {
                if (existNode.IsDeleted)
                {
                    existNode.IsDeleted = false;
                    Log.Warning("AddEvent hashId deleted, repeat add: {0}", eventId);
                    return;
                }

                Log.Fatal("AddEvent hashId repeated: {0}", eventId);
                return;
            }

            listListener.Add(EventRegInfo.Alloc(listener, owner));
        }

        /// <summary>
        /// 通过持有者Tag移除监听。
        /// </summary>
        /// <param name="owner">持有者Tag。</param>
        public void RemoveAllListenerByOwner(object owner)
        {
            var itr = _allEventListenerMap.GetEnumerator();
            while (itr.MoveNext())
            {
                var kv = itr.Current;
                var list = kv.Value;

                int eventId = kv.Key;
                bool isProcessing = _processEventList.Contains(eventId);
                bool delayDeleted = false;

                for (int i = list.Count - 1; i >= 0 ; i--)
                {
                    var regInfo = list[i];
                    if (regInfo.Owner == owner)
                    {
                        if (isProcessing)
                        {
                            regInfo.IsDeleted = true;
                            delayDeleted = true;
                        }
                        else
                        {
                            EventRegInfo.Release(list[i]);
                            list.RemoveAt(i);
                        }
                    }
                }

                if (delayDeleted)
                {
                    AddDelayDelete(eventId);
                }
            }

            itr.Dispose();
        }

        /// <summary>
        /// 移除事件监听。
        /// </summary>
        /// <param name="eventId">事件Id。</param>
        /// <param name="eventCallback">消息回调。</param>
        public void RemoveEventListener(int eventId, Action eventCallback)
        {
            RemoveEventListenerImp(eventId, eventCallback);
        }

        /// <summary>
        /// 移除事件监听。
        /// </summary>
        /// <param name="eventId">事件Id。</param>
        /// <param name="eventCallback">消息回调。</param>
        /// <typeparam name="TArg1">参数类型1。</typeparam>
        public void RemoveEventListener<TArg1>(int eventId, Action<TArg1> eventCallback)
        {
            RemoveEventListenerImp(eventId, eventCallback);
        }

        /// <summary>
        /// 移除事件监听。
        /// </summary>
        /// <param name="eventId">事件Id。</param>
        /// <param name="eventCallback">消息回调。</param>
        /// <typeparam name="TArg1">参数类型1。</typeparam>
        /// <typeparam name="TArg2">参数类型2。</typeparam>
        public void RemoveEventListener<TArg1, TArg2>(int eventId, Action<TArg1, TArg2> eventCallback)
        {
            RemoveEventListenerImp(eventId, eventCallback);
        }

        /// <summary>
        /// 移除事件监听。
        /// </summary>
        /// <param name="eventId">事件Id。</param>
        /// <param name="eventCallback">消息回调。</param>
        /// <typeparam name="TArg1">参数类型1。</typeparam>
        /// <typeparam name="TArg2">参数类型2。</typeparam>
        /// <typeparam name="TArg3">参数类型3。</typeparam>
        public void RemoveEventListener<TArg1, TArg2, TArg3>(int eventId, Action<TArg1, TArg2, TArg3> eventCallback)
        {
            RemoveEventListenerImp(eventId, eventCallback);
        }

        /// <summary>
        /// 移除事件监听。
        /// </summary>
        /// <param name="eventId">事件Id。</param>
        /// <param name="eventCallback">消息回调。</param>
        /// <typeparam name="TArg1">参数类型1。</typeparam>
        /// <typeparam name="TArg2">参数类型2。</typeparam>
        /// <typeparam name="TArg3">参数类型3。</typeparam>
        /// <typeparam name="TArg4">参数类型4。</typeparam>
        public void RemoveEventListener<TArg1, TArg2, TArg3, TArg4>(int eventId, Action<TArg1, TArg2, TArg3, TArg4> eventCallback)
        {
            RemoveEventListenerImp(eventId, eventCallback);
        }

        /// <summary>
        /// 删除监听，如果是正在处理的监听则标记为删除
        /// </summary>
        /// <param name="eventId">事件Id。</param>
        /// <param name="listener">事件监听。</param>
        protected void RemoveEventListenerImp(int eventId, Delegate listener)
        {
            if (_allEventListenerMap.TryGetValue(eventId, out var listListener))
            {
                bool isProcessing = _processEventList.Contains(eventId);
                if (!isProcessing)
                {
                    listListener.RemoveAll(node => node.Callback == listener);
                }
                else
                {
                    int listenCnt = listListener.Count;
                    for (int i = 0; i < listenCnt; i++)
                    {
                        var node = listListener[i];
                        if (node.Callback == listener)
                        {
                            node.IsDeleted = true;
                            AddDelayDelete(eventId);
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 清除回收接口。
        /// </summary>
        public void Clear()
        {
            DestroyAllEventListener();
        }
    }
    
    /// <summary>
    /// 事件注册信息。
    /// </summary>
    public class EventRegInfo : IMemory
    {
        /// <summary>
        /// 事件回调。
        /// </summary>
        public Delegate Callback;

        /// <summary>
        /// 事件持有者。
        /// </summary>
        public object Owner;

        /// <summary>
        /// 事件是否删除。
        /// </summary>
        public bool IsDeleted;

        public EventRegInfo(Delegate callback, object owner)
        {
            this.Callback = callback;
            Owner = owner;
            IsDeleted = false;
        }
        
        public EventRegInfo() { }

        public void Clear()
        {
            Callback = null;
            Owner = null;
            IsDeleted = false;
        }

        public static EventRegInfo Alloc(Delegate callback, object owner)
        {
            EventRegInfo ret = MemoryPool.Acquire<EventRegInfo>();
            ret.Callback = callback;
            ret.Owner = owner;
            ret.IsDeleted = false;
            return ret;
        }
        
        public static void Release(EventRegInfo eventRegInfo)
        {
            MemoryPool.Release(eventRegInfo);
        }
    }
}