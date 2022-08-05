using System;

namespace TEngine
{
    /// <summary>
    /// Ecs组件更新接口（减少组件for循环开销）
    /// </summary>
    public interface IUpdate
    {
        void Update();
    }

    public interface IUpdateSystem : ISystemType
    {
        void Run(object o);
    }

    public abstract class UpdateSystem<T> : IUpdateSystem where T : IUpdate
    {
        public void Run(object o)
        {
            this.Update((T)o);
        }

        public Type Type()
        {
            return typeof(T);
        }

        public Type SystemType()
        {
            return typeof(IUpdateSystem);
        }

        public abstract void Update(T self);
    }
}