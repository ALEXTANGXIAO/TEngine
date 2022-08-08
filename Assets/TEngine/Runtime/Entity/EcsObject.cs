using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace TEngine.EntityModule
{
    /// <summary>
    /// Entity架构基类Object
    /// </summary>
    public class EcsObject
    {
        public int InstanceId
        {
            get;
            protected set;
        }

        internal int HashCode
        {
            get;
            private set;
        }

        internal EntitySystem System;


        [IgnoreDataMember]
        public bool IsDisposed => this.InstanceId == 0;

        public EcsObject()
        {
            HashCode = GetType().GetHashCode();
            InstanceId = EntitySystem.Instance.CurInstanceId;
            EntitySystem.Instance.CurInstanceId++;
        }

        public virtual void Dispose()
        {
            if (InstanceId != 0)
            {
                EntitySystem.Instance.EcsObjects.Remove(InstanceId);
            }
            else
            {
                throw new Exception($"{this.ToString()} Instance is 0 but still Dispose");
            }
            InstanceId = 0;
        }

        public virtual void Awake() { }

        public virtual void OnDestroy() { }

        /// <summary>
        /// Remove The EcsEntity or Component And Throw the EcsObject to ArrayPool When AddComponent Or Create Can Use Again
        /// </summary>
        /// <param name="ecsObject">EcsEntity/Component/System</param>
        /// <param name="reuse">此对象是否可以复用，复用会将对象丢入System对象池中 等待再次使用，如果是Entity对象，并且不复用的话，则把Entity所使用的组件也不复用</param>
        public static void Destroy(EcsObject ecsObject, bool reuse = true)
        {
            if (ecsObject is EntityComponent entityComponent)
            {
#if UNITY_EDITOR
                TLogger.LogWarning($"Destroy ID {entityComponent.InstanceId}  EntityComponent{entityComponent.ToString()},reuse {reuse}");
#endif
                entityComponent.Entity.Components.Remove(entityComponent);
                if (entityComponent is IUpdate update)
                {
                    entityComponent.Entity.Updates.Remove(update);
                }
                if (entityComponent is IFixedUpdate fixedupdate)
                {
                    entityComponent.Entity.FixedUpdates.Remove(fixedupdate);
                }
                if (entityComponent is ILateUpdate lateupdate)
                {
                    entityComponent.Entity.LateUpdates.Remove(lateupdate);
                }
                if (reuse)
                {
                    entityComponent.Entity.System.Push(entityComponent);
                }
                ecsObject.OnDestroy();
            }
            else if (ecsObject is Entity entity)
            {
#if UNITY_EDITOR
                TLogger.LogWarning($"Destroy ID {entity.InstanceId} Entity{entity.ToString()},reuse {reuse}");
#endif
                entity.System.RemoveEntity(entity);
                entity.OnDestroy();
                while (entity.Components.Count > 0)
                {
                   EntityComponent entityComponentTemp = entity.Components[0];
                    entity.Components.RemoveAt(0);
                    entityComponentTemp.OnDestroy();
                    if (reuse)
                    {
                        Destroy(entityComponentTemp);
                    }
                }
                entity.IsDispose = true;
                if (reuse)
                {
                    entity.System.Push(entity);   
                }
            }
            ecsObject.Dispose();
        }

        public T FindObjectOfType<T>() where T : EcsObject
        {
            Type type = typeof(T);
            var elements = System.Entities.ToArray();
            for (int i = 0; i < elements.Length; i++)
            {
                if (elements[i].GetType() == type)
                {
                    return elements[i] as T;
                }
                for (int n = 0; n < elements[i].Components.Count; n++)
                {
                    if (elements[i].Components[n].GetType() == type)
                    {
                        return elements[i].Components[n] as T;
                    }
                }
            }
            return null;
        }

        public T[] FindObjectsOfType<T>() where T : EcsObject
        {
            Type type = typeof(T);
            var items = System.Entities.ToArray();
            List<T> elements = new List<T>();
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i].GetType() == type)
                {
                    elements.Add(items[i] as T);
                }
                for (int n = 0; n < items[i].Components.Count; n++)
                {
                    if (items[i].Components[n].GetType() == type)
                    {
                        elements.Add(items[i].Components[n] as T);
                    }
                }
            }
            return elements.ToArray();
        }
    }
}

