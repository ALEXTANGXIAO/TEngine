using System;
using System.Collections.Generic;

namespace TEngine
{
    /// <summary>
    /// ECS架构基类Object
    /// </summary>
    public class ECSObject
    {
        internal int HashCode;

        internal ECSSystem System;

        public ECSObject()
        {
            HashCode = GetType().GetHashCode();
        }

        public virtual void Awake() { }

        public virtual void OnDestroy() { }

        /// <summary>
        /// Remove The ECSEntity or Component And Throw the ECSObject to ArrayPool When AddComponent Or Create Can Use Again
        /// </summary>
        /// <param name="ecsObject"></param>
        /// <param name="reuse">此对象是否可以复用，复用会将对象丢入System对象池中 等待再次使用，如果是Entity对象，并且不复用的话，则把Entity所使用的组件也不复用</param>
        public static void Destroy(ECSObject ecsObject, bool reuse = true)
        {
            if (ecsObject is ECSComponent ecsComponent)
            {
                ecsComponent.Entity.Components.Remove(ecsComponent);
                if (ecsComponent is IUpdate update)
                {
                    ecsComponent.Entity.Updates.Remove(update);
                }
                if (reuse)
                {
                    ecsComponent.Entity.System.Push(ecsComponent);
                }
                ecsObject.OnDestroy();
                return;
            }
            else if (ecsObject is Entity entity)
            {
                entity.System.RemoveEntity(entity);
                entity.OnDestroy();
                while (entity.Components.Count > 0)
                {
                    ECSComponent ecsComponentTemp = entity.Components[0];
                    entity.Components.RemoveAt(0);
                    ecsComponentTemp.OnDestroy();
                    if (reuse)
                    {
                        entity.System.Push(ecsComponentTemp);
                    }
                }
                entity.Updates.Clear();
                entity.CanUpdate = false;
                if (reuse)
                {
                    entity.System.Push(entity);   
                }
            }
        }

        public T FindObjectOfType<T>() where T : ECSObject
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

        public T[] FindObjectsOfType<T>() where T : ECSObject
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

