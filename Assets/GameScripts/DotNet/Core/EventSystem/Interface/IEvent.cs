using System;

namespace TEngine
{
    public interface IEvent
    {
        Type EventType();
        void Invoke(object self);
    }
    
    public interface IAsyncEvent
    {
        Type EventType();
        FTask InvokeAsync(object self);
    }

    public abstract class EventSystem<T> : IEvent
    {
        private readonly Type _selfType = typeof(T);
        
        public Type EventType()
        {
            return _selfType;
        }
    
        public abstract void Handler(T self);
    
        public void Invoke(object self)
        {
            try
            {
                Handler((T) self);
            }
            catch (Exception e)
            {
                Log.Error($"{_selfType.Name} Error {e}");
            }
        }
    }
    
    public abstract class AsyncEventSystem<T> : IAsyncEvent
    {
        private readonly Type _selfType = typeof(T);

        public Type EventType()
        {
            return _selfType;
        }
    
        public abstract FTask Handler(T self);

        public async FTask InvokeAsync(object self)
        {
            try
            {
                await Handler((T) self);
            }
            catch (Exception e)
            {
                Log.Error($"{_selfType.Name} Error {e}");
            }
        }
    }
}