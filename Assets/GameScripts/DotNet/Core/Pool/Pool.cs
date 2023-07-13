using System;
using System.Collections.Generic;

namespace TEngine
{
    /// <summary>
    /// 静态通用对象池
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class Pool<T>
    {
        private static readonly Queue<T> PoolQueue = new Queue<T>();
    
        public static int Count => PoolQueue.Count;
    
        public static T Rent()
        {
            return PoolQueue.Count == 0 ? Activator.CreateInstance<T>() : PoolQueue.Dequeue();
        }
        
        public static T Rent(Func<T> generator)
        {
            return PoolQueue.Count == 0 ? generator() : PoolQueue.Dequeue();
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

        public static void Clear()
        {
            PoolQueue.Clear();
        }
    }
}