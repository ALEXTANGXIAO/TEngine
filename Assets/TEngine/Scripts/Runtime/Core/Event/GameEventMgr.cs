using System;

namespace TEngine.Runtime
{
    /// <summary>
    /// 总观察者 - 总事件中心系统
    /// </summary>
    public class GameEventMgr : TSingleton<GameEventMgr>
    {
        private GameEvent _gameEvent;

        public GameEventMgr()
        {
            _gameEvent = MemoryPool.Acquire<GameEvent>();
        }

        public override void Release()
        {
            MemoryPool.Release(_gameEvent);
        }

        public override void Active()
        {
            base.Active();
        }

        #region AddEventListener
        public void AddEventListener<T>(int eventId, Action<T> action)
        {
            _gameEvent.AddEventListener(eventId, action);
        }

        public void AddEventListener<T, U>(int eventId, Action<T, U> action)
        {
            _gameEvent.AddEventListener(eventId, action);
        }

        public void AddEventListener<T, U, W>(int eventId, Action<T, U, W> action)
        {
            _gameEvent.AddEventListener(eventId, action);
        }

        public void AddEventListener(int eventId, Action action)
        {
            _gameEvent.AddEventListener(eventId, action);
        }
        #endregion

        #region RemoveEventListener
        public void RemoveEventListener<T>(int eventId, Action<T> action)
        {
            _gameEvent.RemoveEventListener(eventId, action);
        }

        public void RemoveEventListener<T, U>(int eventId, Action<T, U> action)
        {
            _gameEvent.RemoveEventListener(eventId, action);
        }

        public void RemoveEventListener<T, U, W>(int eventId, Action<T, U, W> action)
        {
            _gameEvent.RemoveEventListener(eventId, action);
        }

        public void RemoveEventListener(int eventId, Action action)
        {
            _gameEvent.RemoveEventListener(eventId, action);
        }
        #endregion

        #region Send
        public void Send<T>(int eventId, T info)
        {
            _gameEvent.Send(eventId, info);
        }

        public void Send<T, U>(int eventId, T info, U info2)
        {
            _gameEvent.Send(eventId, info, info2);
        }

        public void Send<T, U, W>(int eventId, T info, U info2, W info3)
        {
            _gameEvent.Send(eventId, info, info2, info3);
        }

        public void Send(int eventId)
        {
            _gameEvent.Send(eventId);
        }
        #endregion

        #region StringEvent
        #region AddEventListener
        public void AddEventListener<T>(string eventId, Action<T> action)
        {
            _gameEvent.AddEventListener(eventId, action);
        }

        public void AddEventListener<T, U>(string eventId, Action<T, U> action)
        {
            _gameEvent.AddEventListener(eventId, action);
        }

        public void AddEventListener<T, U, W>(string eventId, Action<T, U, W> action)
        {
            _gameEvent.AddEventListener(eventId, action);
        }

        public void AddEventListener(string eventId, Action action)
        {
            _gameEvent.AddEventListener(eventId, action);
        }
        #endregion

        #region RemoveEventListener
        public void RemoveEventListener<T>(string eventId, Action<T> action)
        {
            _gameEvent.RemoveEventListener(eventId, action);
        }

        public void RemoveEventListener<T, U>(string eventId, Action<T, U> action)
        {
            _gameEvent.RemoveEventListener(eventId, action);
        }

        public void RemoveEventListener<T, U, W>(string eventId, Action<T, U, W> action)
        {
            _gameEvent.RemoveEventListener(eventId, action);
        }

        public void RemoveEventListener(string eventId, Action action)
        {
            _gameEvent.RemoveEventListener(eventId, action);
        }
        #endregion

        #region Send
        public void Send<T>(string eventId, T info)
        {
            _gameEvent.Send(eventId, info);
        }

        public void Send<T, U>(string eventId, T info, U info2)
        {
            _gameEvent.Send(eventId, info, info2);
        }

        public void Send<T, U, W>(string eventId, T info, U info2, W info3)
        {
            _gameEvent.Send(eventId, info, info2, info3);
        }

        public void Send(string eventId)
        {
            _gameEvent.Send(eventId);
        }
        #endregion
        #endregion

        #region Clear
        public void Clear()
        {
            _gameEvent.Clear();
        }
        #endregion
    }

}