using System;
using System.Collections.Generic;

namespace TEngine.EntityModule
{
    partial class Entity
    {
        public void RmvComponent<T>(T component) where T :EntityComponent, new()
        {
            if (component is IUpdate update)
            {
                Updates.Remove(update);
            }
            else if (component is IFixedUpdate fixedUpdate)
            {
                FixedUpdates.Remove(fixedUpdate);
            }

            CanUpdate = Updates.Count > 0;

            Destroy(component);
        }

        public T AddComponent<T>() where T :EntityComponent, new()
        {
            T component = EntityComponent.Create<T>();
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

            return component;
        }

        public EntityComponent AddComponent(EntityComponent component)
        {
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
            return component;
        }

        public T GetComponent<T>() where T :EntityComponent
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

        public EntityComponent GetComponent(Type componentType)
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

        public T[] GetComponents<T>() where T :EntityComponent
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

        public List<T> GetComponentsList<T>() where T :EntityComponent
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

        public EntityComponent[] GetComponents(Type comType)
        {
            List<EntityComponent> elements = new List<EntityComponent>();
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