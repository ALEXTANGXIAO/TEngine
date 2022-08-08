using System;

namespace TEngine.EntityModule
{
    public class EventCmpt :EntityComponent
    {
        private GameEvent _gameEvent;

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

        #region Clear
        public void Clear()
        {
            GameMemPool<GameEvent>.Free(_gameEvent);
        }
        #endregion

        #region 生命周期
        public override void OnDestroy()
        {
            Clear();
        }

        public override void Awake()
        {
            _gameEvent = GameMemPool<GameEvent>.Alloc();
        }
        #endregion
    }
}