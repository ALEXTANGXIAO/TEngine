using System.Collections.Generic;
using UnityEngine.Events;

namespace TEngine
{
    /// <summary>
    /// 对象池管理器
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class ObjectPoolMgr<T>
    {
        private static readonly ObjectPool<List<T>> m_ListPool = new ObjectPool<List<T>>(null, Clear);

        static void Clear(List<T> list)
        {
            list.Clear();
        }

        public static List<T> Get()
        {
            return m_ListPool.Get();
        }

        public static void Release(List<T> toRelease)
        {
            m_ListPool.Release(toRelease);
        }
    }

    public class ObjectPool<T> where T : new()
    {
        private readonly Stack<T> m_Stack = new Stack<T>();
        private readonly UnityAction<T> m_ActionGet;
        private readonly UnityAction<T> m_ActionRelease;

        public int CountAll { get; private set; }
        public int CountActive
        {
            get
            {
                return CountAll - CountInActive;
            }
        }

        public int CountInActive
        {
            get
            {
                return m_Stack.Count;
            }
        }

        public ObjectPool()
        {

        }

        public ObjectPool(UnityAction<T> actionGet, UnityAction<T> actionRelease)
        {
            m_ActionGet = actionGet;
            m_ActionRelease = actionRelease;
        }

        public T Get()
        {
            T element;
            if (m_Stack.Count <= 0)
            {
                element = new T();
                CountAll++;
            }
            else
            {
                element = m_Stack.Pop();
            }

            if (m_ActionGet != null)
            {
                m_ActionGet.Invoke(element);
            }
            return element;
        }

        public void Release(T element)
        {
            if (m_Stack.Count > 0 && ReferenceEquals(m_Stack.Peek(), element))
            {
                TLogger.LogError("Internal error. Trying to destroy object that is already released to pool.");
            }
            m_ActionRelease?.Invoke(element);
            m_Stack.Push(element);
        }
    }
}