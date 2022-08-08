using System;

namespace TEngine
{
    /// <summary>
    /// Ecs组件物理更新接口（减少组件for循环开销）
    /// </summary>
    public interface IFixedUpdate
    {
        void FixedUpdate();
    }

    public interface IFixedUpdateSystem : ISystemType
    {
        void Run(object o);
    }

    public abstract class FixedUpdateSystem<T> : IFixedUpdateSystem where T : IFixedUpdate
    {
        public void Run(object o)
        {
            this.FixedUpdate((T)o);
        }

        public Type Type()
        {
            return typeof(T);
        }

        public Type SystemType()
        {
            return typeof(IUpdateSystem);
        }

        public abstract void FixedUpdate(T self);
    }
}