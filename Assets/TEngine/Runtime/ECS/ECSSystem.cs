using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TEngine
{
    /// <summary>
    /// Ecs系统
    /// </summary>
    [Serializable]
    public class EcsSystem : IDisposable
    {
        private static EcsSystem _instance;

        public static EcsSystem Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new EcsSystem();
                }

                return _instance;
            }
        }
        private EcsSystem()
        {
        }
        /// <summary>
        /// Key -> HashSet(type) / Value -> Stack(EcsObject)
        /// </summary>
        internal readonly Dictionary<int, Stack<EcsObject>> ObjectPool = new Dictionary<int, Stack<EcsObject>>();
        internal readonly ArrayPool<Entity> Entities = new ArrayPool<Entity>();

        public void AddEntity(Entity entity)
        {
            entity.System = this;
            entity.Awake();
            Entities.Add(entity);
        }

        public void RemoveEntity(Entity entity)
        {
            Entities.Remove(entity);
        }

        public T Get<T>() where T : EcsObject, new()
        {
            int type = typeof(T).GetHashCode();
            if (ObjectPool.TryGetValue(type, out Stack<EcsObject> stack))
            {
                if (stack.Count > 0)
                {
                    return (T)stack.Pop();
                }
                goto Instantiate;
            }
            stack = new Stack<EcsObject>();
            ObjectPool.Add(type, stack);
        Instantiate: T ecsObject = new T();
            return ecsObject;
        }

        public void Push(EcsObject ecsObject)
        {
            int type = ecsObject.HashCode;

            if (ObjectPool.TryGetValue(type, out Stack<EcsObject> stack))
            {
                stack.Push(ecsObject);
                return;
            }
            stack = new Stack<EcsObject>();
            ObjectPool.Add(type, stack);
            stack.Push(ecsObject);
        }

        public T Create<T>() where T : Entity, new()
        {
            T entity = Get<T>();
            AddEntity(entity);
            return entity;
        }

        public T Create<T>(T entity) where T : Entity, new()
        {
            AddEntity(entity);
            return entity;
        }

        /// <summary>
        /// 更新Ecs系统
        /// </summary>
        /// <param name="worker">线程池是否并行</param>
        public void Update(bool worker = false)
        {
            Run(worker);
        }

        /// <summary>
        /// 更新Ecs物理系统
        /// </summary>
        /// <param name="worker">线程池是否并行</param>
        public void FixedUpdate(bool worker = false)
        {
            RunFixed(worker);
        }

        /// <summary>
        /// 运行Ecs系统
        /// </summary>
        /// <param name="worker">线程池是否并行</param>
        public void Run(bool worker = false)
        {
            int count = Entities.Count;
            if (!worker)
            {
                for (int i = 0; i < count; i++)
                {
                    if (!Entities.Buckets[i])
                    {
                        continue;
                    }

                    if (!Entities[i].CanUpdate)
                    {
                        continue;
                    }
                    Entities[i].Execute();
                }
            }
            else
            {
                Parallel.For(0, count, i =>
                {
                    if (!Entities.Buckets[i])
                    {
                        return;
                    }

                    if (!Entities[i].CanUpdate)
                    {
                        return;
                    }
                    Entities[i].Execute();
                });
            }
        }

        public void RunFixed(bool worker = false)
        {
            int count = Entities.Count;
            if (!worker)
            {
                for (int i = 0; i < count; i++)
                {
                    if (!Entities.Buckets[i])
                    {
                        continue;
                    }

                    if (!Entities[i].CanFixedUpdate)
                    {
                        continue;
                    }
                    Entities[i].FixedUpdate();
                }
            }
            else
            {
                Parallel.For(0, count, i =>
                {
                    if (!Entities.Buckets[i])
                    {
                        return;
                    }

                    if (!Entities[i].CanFixedUpdate)
                    {
                        return;
                    }
                    Entities[i].FixedUpdate();
                });
            }
        }

        #region Dispose
        public void Dispose()
        {
            _instance = null;
        }
        #endregion

        #region FindObjectOfType
        public T FindObjectOfType<T>() where T : EcsObject
        {
            Type type = typeof(T);
            var elements = Entities.ToArray();
            for (int i = 0; i < elements.Length; i++)
            {
                if (elements[i].GetType() == type)
                {
                    return elements[i] as T;
                }

                for (int j = 0; j < elements[i].Components.Count; j++)
                {
                    if (elements[i].Components[j].GetType() == type)
                    {
                        return elements[i].Components[j] as T;
                    }
                }
            }
            return null;
        }

        public T[] FindObjectsOfType<T>() where T : EcsObject
        {
            Type type = typeof(T);
            var entities = Entities.ToArray();
            List<T> elements = new List<T>();
            for (int i = 0; i < entities.Length; i++)
            {
                if (entities[i].GetType() == type)
                {
                    elements.Add(entities[i] as T);
                }
                for (int n = 0; n < entities[i].Components.Count; n++)
                {
                    if (entities[i].Components[n].GetType() == type)
                    {
                        elements.Add(entities[i].Components[n] as T);
                    }
                }
            }
            return elements.ToArray();
        }
        #endregion
    }
}

