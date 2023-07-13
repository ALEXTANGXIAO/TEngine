using System;

namespace TEngine
{
    public interface IEntitiesSystem
    {
        public Type EntitiesType();
        void Invoke(Entity entity);
    }
}