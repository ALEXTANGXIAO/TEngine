using System;

namespace TEngine
{
    public interface IAwake
    {
    }

    public interface IAwake<T>
    {
    }

    public interface IAwake<T, U>
    {
    }

    public interface IAwake<T, U, V>
    {
    }

    public interface IAwake<T, U, V, W>
    {
    }

    public interface IAwakeSystem : ISystemType
    {
        void Run(object o);
    }

    public interface IAwakeSystem<T> : ISystemType
    {
        void Run(object o, T t);
    }

    public interface IAwakeSystem<T, U> : ISystemType
    {
        void Run(object o, T t, U u);
    }

    public interface IAwakeSystem<T, U, V> : ISystemType
    {
        void Run(object o, T t, U u, V v);
    }

    public interface IAwakeSystem<T, U, V, W> : ISystemType
    {
        void Run(object o, T t, U u, V v, W w);
    }

    [ObjectSystem]
    public abstract class AwakeSystem<T> : IAwakeSystem where T : IAwake
    {
        public Type Type()
        {
            return typeof(T);
        }

        public Type SystemType()
        {
            return typeof(IAwakeSystem);
        }

        public void Run(object o)
        {
            this.Awake((T)o);
        }

        public abstract void Awake(T self);
    }

    [ObjectSystem]
    public abstract class AwakeSystem<T, A> : IAwakeSystem<A> where T : IAwake<A>
    {
        public Type Type()
        {
            return typeof(T);
        }

        public Type SystemType()
        {
            return typeof(IAwakeSystem<A>);
        }

        public void Run(object o, A a)
        {
            this.Awake((T)o, a);
        }

        public abstract void Awake(T self, A a);
    }

    [ObjectSystem]
    public abstract class AwakeSystem<T, A, B> : IAwakeSystem<A, B> where T : IAwake<A, B>
    {
        public Type Type()
        {
            return typeof(T);
        }

        public Type SystemType()
        {
            return typeof(IAwakeSystem<A, B>);
        }

        public void Run(object o, A a, B b)
        {
            this.Awake((T)o, a, b);
        }

        public abstract void Awake(T self, A a, B b);
    }

    [ObjectSystem]
    public abstract class AwakeSystem<T, A, B, C> : IAwakeSystem<A, B, C> where T : IAwake<A, B, C>
    {
        public Type Type()
        {
            return typeof(T);
        }

        public Type SystemType()
        {
            return typeof(IAwakeSystem<A, B, C>);
        }

        public void Run(object o, A a, B b, C c)
        {
            this.Awake((T)o, a, b, c);
        }

        public abstract void Awake(T self, A a, B b, C c);
    }

    [ObjectSystem]
    public abstract class AwakeSystem<T, A, B, C, D> : IAwakeSystem<A, B, C, D> where T : IAwake<A, B, C, D>
    {
        public Type Type()
        {
            return typeof(T);
        }

        public Type SystemType()
        {
            return typeof(IAwakeSystem<A, B, C, D>);
        }

        public void Run(object o, A a, B b, C c, D d)
        {
            this.Awake((T)o, a, b, c, d);
        }

        public abstract void Awake(T self, A a, B b, C c, D d);
    }
}