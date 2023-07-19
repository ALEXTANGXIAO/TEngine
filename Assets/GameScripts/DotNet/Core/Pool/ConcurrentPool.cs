using System;
using System.Collections.Concurrent;

namespace TEngine
{
    /// <summary>
    /// 线程安全的静态通用对象池
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class ConcurrentPool<T>
    {
        private static readonly ConcurrentQueue<T> PoolQueue = new ConcurrentQueue<T>();
    
        public static int Count => PoolQueue.Count;
    
        public static T Rent()
        {
            return PoolQueue.TryDequeue(out var t) ? t : Activator.CreateInstance<T>();
        }
        
        public static T Rent(Func<T> generator)
        {
            return PoolQueue.TryDequeue(out var t) ? t : generator();
        }
    
        public static void Return(T t)
        {
            if (t == null)
            {
                return;
            }
    
            PoolQueue.Enqueue(t);
        }

        public static void Return(T t, Action<T> reset)
        {
            if (t == null)
            {
                return;
            }

            reset(t);
            PoolQueue.Enqueue(t);
        }
    }
}