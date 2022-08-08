using System;

namespace TEngine
{
    /// <summary>
    /// Ecs组件物理更新接口（减少组件for循环开销）
    /// </summary>
    public interface ILateUpdate
    {
        void LateUpdate();
    }

    public interface ILateUpdateSystem : ISystemType
    {
        void Run(object o);
    }

    public abstract class LateUpdateSystem<T> : ILateUpdateSystem where T : ILateUpdate
    {
        public void Run(object o)
        {
            this.LateUpdate((T)o);
        }

        public Type Type()
        {
            return typeof(T);
        }

        public Type SystemType()
        {
            return typeof(IUpdateSystem);
        }

        public abstract void LateUpdate(T self);
    }
}