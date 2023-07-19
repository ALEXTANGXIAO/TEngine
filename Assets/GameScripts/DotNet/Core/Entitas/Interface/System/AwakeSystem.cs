using System;

namespace TEngine
{
    public interface IAwakeSystem : IEntitiesSystem { }
    
    public abstract class AwakeSystem<T> : IAwakeSystem where T : Entity
    {
        public Type EntitiesType() => typeof(T);

        protected abstract void Awake(T self);

        public void Invoke(Entity self)
        {
            Awake((T) self);
        }
    }
}