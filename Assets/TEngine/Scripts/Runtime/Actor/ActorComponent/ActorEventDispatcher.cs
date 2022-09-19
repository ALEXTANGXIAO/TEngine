using System;
using System.Collections.Generic;

namespace TEngine.Runtime.Actor
{
    public class EventRegInfo
    {
        public Delegate callback;
        public object owner;
        public bool deleted;

        public EventRegInfo(Delegate callback, object owner)
        {
            this.callback = callback;
            this.owner = owner;
            deleted = false;
        }
    }

    public class ActorEventDispatcher
    {
        private readonly Dictionary<int, List<EventRegInfo>> _dictAllEventListener =
            new Dictionary<int, List<EventRegInfo>>();

        /// <summary>
        /// 用于标记一个事件是不是正在处理
        /// </summary>
        private readonly List<int> _processEvent = new List<int>();

        /// <summary>
        /// 用于标记一个事件是不是被移除
        /// </summary>
        private readonly List<int> _delayDeleteEvent = new List<int>();

        public void DestroyAllEventListener()
        {
            var itr = _dictAllEventListener.GetEnumerator();
            while (itr.MoveNext())
            {
                var kv = itr.Current;
                kv.Value.Clear();
            }

            _processEvent.Clear();
            _delayDeleteEvent.Clear();
            itr.Dispose();
        }

        private void AddDelayDelete(int eventId)
        {
            if (!_delayDeleteEvent.Contains(eventId))
            {
                _delayDeleteEvent.Add(eventId);
                Log.Info("delay delete eventId[{0}]", eventId);
            }
        }

        /// <summary>
        /// 如果找到eventId对应的监听，删除所有标记为delete的监听
        /// </summary>
        /// <param name="eventId"></param>
        private void CheckDelayDelete(int eventId)
        {
            if (_delayDeleteEvent.Contains(eventId))
            {
                List<EventRegInfo> listListener;
                if (_dictAllEventListener.TryGetValue(eventId, out listListener))
                {
                    for (int i = 0; i < listListener.Count; i++)
                    {
                        if (listListener[i].deleted)
                        {
                            Log.Info("remove delay delete eventId[{0}]", eventId);
                            listListener[i] = listListener[listListener.Count - 1];
                            listListener.RemoveAt(listListener.Count - 1);
                            i--;
                        }
                    }
                }

                _delayDeleteEvent.Remove(eventId);
            }
        }

        public void SendEvent(int eEventId)
        {
            int eventId = eEventId;
            List<EventRegInfo> listListener;
            if (_dictAllEventListener.TryGetValue(eventId, out listListener))
            {
                _processEvent.Add(eventId);
#if UNITY_EDITOR
                int iEventCnt = _processEvent.Count;
#endif

                var count = listListener.Count;
                for (int i = 0; i < count; i++)
                {
                    var node = listListener[i];
                    if (node.deleted)
                    {
                        continue;
                    }

                    Action callBack = listListener[i].callback as Action;
                    if (callBack != null)
                    {
                        callBack();
                    }
                    else
                    {
                        Log.Fatal("Invalid event data type: {0}", eventId);
                    }
                }


#if UNITY_EDITOR
                Log.Assert(iEventCnt == _processEvent.Count);
                Log.Assert(eventId == _processEvent[_processEvent.Count - 1]);
#endif
                _processEvent.RemoveAt(_processEvent.Count - 1);

                CheckDelayDelete(eventId);
            }
        }

        public void SendEvent<T>(int eEventId, T data)
        {
            int eventId = (int)eEventId;
            List<EventRegInfo> listListener;
            if (_dictAllEventListener.TryGetValue(eventId, out listListener))
            {
                _processEvent.Add(eventId);
#if UNITY_EDITOR
                int iEventCnt = _processEvent.Count;
#endif

                var count = listListener.Count;
                for (int i = 0; i < count; i++)
                {
                    var node = listListener[i];
                    if (node.deleted)
                    {
                        continue;
                    }

                    Action<T> callBack = listListener[i].callback as Action<T>;
                    if (callBack != null)
                    {
                        callBack(data);
                    }
                    else
                    {
                        Log.Fatal("Invalid event data type: {0}", eventId);
                    }
                }


#if UNITY_EDITOR
                Log.Assert(iEventCnt == _processEvent.Count);
                Log.Assert(eventId == _processEvent[_processEvent.Count - 1]);
#endif

                _processEvent.RemoveAt(_processEvent.Count - 1);

                CheckDelayDelete(eventId);
            }
        }

        public void SendEvent<T, U>(int eEventId, T dataT, U dataU)
        {
            int eventId = (int)eEventId;
            List<EventRegInfo> listListener;
            if (_dictAllEventListener.TryGetValue(eventId, out listListener))
            {
                _processEvent.Add(eventId);
#if UNITY_EDITOR
                int iEventCnt = _processEvent.Count;
#endif

                var count = listListener.Count;
                for (int i = 0; i < count; i++)
                {
                    var node = listListener[i];
                    if (node.deleted)
                    {
                        continue;
                    }

                    Action<T, U> callBack = listListener[i].callback as Action<T, U>;
                    if (callBack != null)
                    {
                        callBack(dataT, dataU);
                    }
                    else
                    {
                        Log.Fatal("Invalid event data type: {0}", eventId);
                    }
                }


#if UNITY_EDITOR
                Log.Assert(iEventCnt == _processEvent.Count);
                Log.Assert(eventId == _processEvent[_processEvent.Count - 1]);
#endif
                _processEvent.RemoveAt(_processEvent.Count - 1);

                CheckDelayDelete(eventId);
            }
        }

        public void SendEvent<T, U, V>(int eEventId, T dataT, U dataU, V dataV)
        {
            int eventId = (int)eEventId;
            List<EventRegInfo> listListener;
            if (_dictAllEventListener.TryGetValue(eventId, out listListener))
            {
                _processEvent.Add(eventId);
#if UNITY_EDITOR
                int iEventCnt = _processEvent.Count;
#endif

                var count = listListener.Count;
                for (int i = 0; i < count; i++)
                {
                    var node = listListener[i];
                    if (node.deleted)
                    {
                        continue;
                    }

                    Action<T, U, V> callBack = node.callback as Action<T, U, V>;
                    if (callBack != null)
                    {
                        callBack(dataT, dataU, dataV);
                    }
                    else
                    {
                        Log.Fatal("Invalid event data type: {0}", eventId);
                    }
                }


#if UNITY_EDITOR
                Log.Assert(iEventCnt == _processEvent.Count);
                Log.Assert(eventId == _processEvent[_processEvent.Count - 1]);
#endif
                _processEvent.RemoveAt(_processEvent.Count - 1);

                CheckDelayDelete(eventId);
            }
        }

        public void SendEvent<T, U, V, S>(int eEventId, T dataT, U dataU, V dataV, S dataS)
        {
            int eventId = (int)eEventId;

            List<EventRegInfo> listListener;
            if (_dictAllEventListener.TryGetValue(eventId, out listListener))
            {
                _processEvent.Add(eventId);
#if UNITY_EDITOR
                int iEventCnt = _processEvent.Count;
#endif


                var count = listListener.Count;
                for (int i = 0; i < count; i++)
                {
                    var node = listListener[i];
                    if (node.deleted)
                    {
                        continue;
                    }

                    Action<T, U, V, S> callBack = listListener[i].callback as Action<T, U, V, S>;
                    if (callBack != null)
                    {
                        callBack(dataT, dataU, dataV, dataS);
                    }
                    else
                    {
                        Log.Fatal("Invalid event data type: {0}", eventId);
                    }
                }


#if UNITY_EDITOR
                Log.Assert(iEventCnt == _processEvent.Count);
                Log.Assert(eventId == _processEvent[_processEvent.Count - 1]);
#endif
                _processEvent.RemoveAt(_processEvent.Count - 1);

                CheckDelayDelete(eventId);
            }
        }

        public void AddEventListener(int eventHashId, Action eventCallback, object owner)
        {
            AddEventListenerImp(eventHashId, eventCallback, owner);
        }

        public void AddEventListener<T>(int eventHashId, Action<T> eventCallback, object owner)
        {
            AddEventListenerImp(eventHashId, eventCallback, owner);
        }

        public void AddEventListener<T, U>(int eventHashId, Action<T, U> eventCallback, object owner)
        {
            AddEventListenerImp(eventHashId, eventCallback, owner);
        }

        public void AddEventListener<T, U, V>(int eventHashId, Action<T, U, V> eventCallback, object owner)
        {
            AddEventListenerImp(eventHashId, eventCallback, owner);
        }

        public void AddEventListener<T, U, V, S>(int eventHashId, Action<T, U, V, S> eventCallback,
            object owner)
        {
            AddEventListenerImp(eventHashId, eventCallback, owner);
        }

        private void AddEventListenerImp(int eventHashId, Delegate listener, object owner)
        {
            List<EventRegInfo> listListener;
            if (!_dictAllEventListener.TryGetValue((int)eventHashId, out listListener))
            {
                listListener = new List<EventRegInfo>();
                _dictAllEventListener.Add((int)eventHashId, listListener);
            }

            var existNode = listListener.Find((node) => { return node.callback == listener; });
            if (existNode != null)
            {
                if (existNode.deleted)
                {
                    existNode.deleted = false;
                    Log.Warning("AddEvent hashId deleted, repeat add: {0}", eventHashId);
                    return;
                }

                Log.Fatal("AddEvent hashId repeated: {0}", eventHashId);
                return;
            }

            listListener.Add(new EventRegInfo(listener, owner));
        }

        public void RemoveAllListenerByOwner(object owner)
        {
            var itr = _dictAllEventListener.GetEnumerator();
            while (itr.MoveNext())
            {
                var kv = itr.Current;
                var list = kv.Value;

                int eventId = kv.Key;
                bool isProcessing = _processEvent.Contains(eventId);
                bool delayDeleted = false;

                for (int i = 0; i < list.Count; i++)
                {
                    var regInfo = list[i];
                    if (regInfo.owner == owner)
                    {
                        if (isProcessing)
                        {
                            regInfo.deleted = true;
                            delayDeleted = true;
                        }
                        else
                        {
                            list[i] = list[list.Count - 1];
                            list.RemoveAt(list.Count - 1);
                            i--;
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

        public void RemoveEventListener(int eventHashId, Action eventCallback)
        {
            RemoveEventListenerImp(eventHashId, eventCallback);
        }

        public void RemoveEventListener<T>(int eventHashId, Action<T> eventCallback)
        {
            RemoveEventListenerImp(eventHashId, eventCallback);
        }

        public void RemoveEventListener<T, U>(int eventHashId, Action<T, U> eventCallback)
        {
            RemoveEventListenerImp(eventHashId, eventCallback);
        }

        public void RemoveEventListener<T, U, V>(int eventHashId, Action<T, U, V> eventCallback)
        {
            RemoveEventListenerImp(eventHashId, eventCallback);
        }

        public void RemoveEventListener<T, U, V, S>(int eventHashId, Action<T, U, V, S> eventCallback)
        {
            RemoveEventListenerImp(eventHashId, eventCallback);
        }

        /// <summary>
        /// 删除监听，如果是正在处理的监听则标记为删除
        /// </summary>
        /// <param name="eventHashId"></param>
        /// <param name="listener"></param>
        protected void RemoveEventListenerImp(int eventHashId, Delegate listener)
        {
            List<EventRegInfo> listListener;
            if (_dictAllEventListener.TryGetValue(eventHashId, out listListener))
            {
                bool isProcessing = _processEvent.Contains(eventHashId);
                if (!isProcessing)
                {
                    listListener.RemoveAll((node) => { return node.callback == listener; });
                }
                else
                {
                    int listenCnt = listListener.Count;
                    for (int i = 0; i < listenCnt; i++)
                    {
                        var node = listListener[i];
                        if (node.callback == listener)
                        {
                            node.deleted = true;
                            AddDelayDelete(eventHashId);
                            break;
                        }
                    }
                }
            }
        }
    }
}