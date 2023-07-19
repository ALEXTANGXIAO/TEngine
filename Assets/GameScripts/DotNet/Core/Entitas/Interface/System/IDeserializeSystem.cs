using System;

namespace TEngine
{
    public interface IDeserializeSystem : IEntitiesSystem { }

    public abstract class DeserializeSystem<T> : IDeserializeSystem where T : Entity
    {
        public Type EntitiesType() => typeof(T);
        protected abstract void Deserialize(T self);
        public void Invoke(Entity self)
        {
            Deserialize((T) self);
        }
    }
}