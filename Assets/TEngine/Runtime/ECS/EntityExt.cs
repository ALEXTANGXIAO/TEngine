using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEngine
{
    partial class Entity
    {
        public void RmvComponent<T>() where T : EcsComponent, new()
        {
            for (int i = 0; i < Components.Count; i++)
            {
                if (Components[i] is T component)
                {
                    if (component is IUpdate update)
                    {
                        Updates.Remove(update);
                    }
                    else if (component is IFixedUpdate fixedUpdate)
                    {
                        FixedUpdates.Remove(fixedUpdate);
                    }
                    System.Push(component);

                    CanUpdate = Updates.Count > 0;

                    CanFixedUpdate = FixedUpdates.Count > 0;
                }
            }

#if UNITY_EDITOR
            CheckDebugInfo();
#endif
        }

        public void RmvComponent(Type componentType)
        {
            for (int i = 0; i < Components.Count; i++)
            {
                if (Components[i].GetType() == componentType)
                {

                    if (componentType is IUpdate update)
                    {
                        Updates.Remove(update);

                        CanUpdate = Updates.Count > 0;
                    }
                    else if (componentType is IFixedUpdate fixedUpdate)
                    {
                        FixedUpdates.Remove(fixedUpdate);

                        CanFixedUpdate = FixedUpdates.Count > 0;
                    }
                    //if (componentType is EcsComponent component)
                    //{
                    //    System.Push(component);
                    //}
                }
            }
#if UNITY_EDITOR
            CheckDebugInfo();
#endif
        }

        public T AddComponent<T>() where T : EcsComponent, new()
        {
#if UNITY_EDITOR
            CheckDebugInfo();
#endif
            T component = System.Get<T>();
            component.Entity = this;
            component.System = System;
            Components.Add(component);
            component.Awake();
            if (component is IUpdate update)
            {
                Updates.Add(update);
            }
            else if (component is IFixedUpdate fixedUpdate)
            {
                FixedUpdates.Add(fixedUpdate);
            }
            CanUpdate = Updates.Count > 0;
            CanFixedUpdate = FixedUpdates.Count > 0;
            return component;
        }

        public EcsComponent AddComponent(EcsComponent component)
        {
#if UNITY_EDITOR
            CheckDebugInfo();
#endif
            component.Entity = this;
            component.System = System;
            Components.Add(component);
            component.Awake();
            if (component is IUpdate update)
            {
                Updates.Add(update);
            }
            else if (component is IFixedUpdate fixedUpdate)
            {
                FixedUpdates.Add(fixedUpdate);
            }
            CanUpdate = Updates.Count > 0;
            CanFixedUpdate = FixedUpdates.Count > 0;
            return component;
        }

        public T GetComponent<T>() where T : EcsComponent
        {
            for (int i = 0; i < Components.Count; i++)
            {
                if (Components[i] is T type)
                {
                    return type;
                }
            }

            return null;
        }

        public EcsComponent GetComponent(Type componentType)
        {
            for (int i = 0; i < Components.Count; i++)
            {
                if (Components[i].GetType() == componentType)
                {
                    return Components[i];
                }
            }

            return null;
        }

        public T[] GetComponents<T>() where T : EcsComponent
        {
            List<T> elements = new List<T>();
            for (int i = 0; i < Components.Count; i++)
            {
                if (Components[i] is T type)
                {
                    elements.Add(type);
                }
            }
            return elements.ToArray();
        }

        public List<T> GetComponentsList<T>() where T : EcsComponent
        {
            List<T> elements = new List<T>();
            for (int i = 0; i < Components.Count; i++)
            {
                if (Components[i] is T type)
                {
                    elements.Add(type);
                }
            }
            return elements;
        }

        public EcsComponent[] GetComponents(Type comType)
        {
            List<EcsComponent> elements = new List<EcsComponent>();
            for (int i = 0; i < Components.Count; i++)
            {
                {
                    if (Components[i].GetType() == comType)
                    {
                        elements.Add(Components[i]);
                    }
                }
            }
            return elements.ToArray();
        }

    }
}
