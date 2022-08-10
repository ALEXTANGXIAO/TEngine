using System.Collections.Generic;
using System.Diagnostics;

namespace TEngine
{
    public interface IMemPoolObject
    {
        void Init();
        void Destroy();
    }

    public interface IMemPoolBase
    {
        string GetName();
        int GetPoolItemCount();
        void ClearPool();
    }

    public class MemPoolMgr : TSingleton<MemPoolMgr>
    {
        protected override void Init()
        {
            base.Init();
#if UNITY_EDITOR
            MemPoolComponent.Active();
#endif
        }

        public int Count => m_listPool.Count;

        List<IMemPoolBase> m_listPool = new List<IMemPoolBase>();

        [Conditional("UNITY_EDITOR")]
        public void ShowCount()
        {
            int totalCnt = 0;
            for (int i = 0; i < m_listPool.Count; i++)
            {
                var pool = m_listPool[i];
                totalCnt += pool.GetPoolItemCount();
                TLogger.LogInfoSuccessd("[pool][{0}] [{1}]", pool.GetName(), pool.GetPoolItemCount());
            }
            TLogger.LogInfoSuccessd("-------------------------memory pool count: {0}", totalCnt);
        }

        public void RegMemPool(IMemPoolBase pool)
        {
            m_listPool.Add(pool);
        }

        public void ClearAllPool()
        {
            for (int i = 0; i < m_listPool.Count; i++)
            {
                var pool = m_listPool[i];
                pool.ClearPool();
            }
        }

        public List<IMemPoolBase> GetAllObjectPools()
        {
            return m_listPool;
        }
    }

    public class GameMemPool<T> : TSingleton<GameMemPool<T>>, IMemPoolBase where T : IMemPoolObject, new()
    {
        private List<T> m_objPool = new List<T>();

        public static T Alloc()
        {
            return GameMemPool<T>.Instance.DoAlloc();
        }

        public static void Free(T obj)
        {
            GameMemPool<T>.Instance.DoFree(obj);
        }

        public GameMemPool()
        {
            MemPoolMgr.Instance.RegMemPool(this);
        }

        private T DoAlloc()
        {
            T newObj;
            if (m_objPool.Count > 0)
            {
                var lastIndex = m_objPool.Count - 1;
                newObj = m_objPool[lastIndex];
                m_objPool.RemoveAt(lastIndex);
            }
            else
            {
                newObj = new T();
            }

            newObj.Init();
            return newObj;
        }

        private void DoFree(T obj)
        {
            if (obj == null)
            {
                return;
            }

            obj.Destroy();
            m_objPool.Add(obj);
        }

        public void ClearPool()
        {
#if UNITY_EDITOR
            TLogger.LogInfo("clear memory[{0}] count[{1}]", GetName(), m_objPool.Count);
#endif
            m_objPool.Clear();
        }

        public string GetName()
        {
            return typeof(T).FullName;
        }

        public int GetPoolItemCount()
        {
            return m_objPool.Count;
        }
    }
}