using System;
using System.Collections.Generic;
using UnityEngine;

namespace TEngineCore
{
    public class Entity : ECSObject, IIndex
    {
        [SerializeField]
        internal List<ECSComponent> Components = new List<ECSComponent>();
        internal List<IUpdate> Updates = new List<IUpdate>();
        internal List<IFixedUpdate> FixedUpdates = new List<IFixedUpdate>();
        internal bool InActive;
        internal bool CanUpdate;
        internal bool CanFixedUpdate;
        public int Index { get; set; } = -1;
        public ECSEventCmpt Event { get; set; }
        public Entity()
        {
            InActive = true;
            System = ECSSystem.Instance;
        }

        ~Entity()
        {
            InActive = false;
        }

        internal void Execute()
        {
            for (int i = 0; i < Updates.Count; i++)
            {
                Updates[i].Update();
            }
        }

        internal void FixedUpdate()
        {
            for (int i = 0; i < FixedUpdates.Count; i++)
            {
                FixedUpdates[i].FixedUpdate();
            }
        }

        public void RmvComponent<T>() where T : ECSComponent, new()
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
                    //if (componentType is ECSComponent component)
                    //{
                    //    System.Push(component);
                    //}
                }
            }
#if UNITY_EDITOR
            CheckDebugInfo();
#endif
        }

        public T AddComponent<T>() where T : ECSComponent, new()
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

        public ECSComponent AddComponent(ECSComponent component)
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

        public T GetComponent<T>() where T : ECSComponent
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

        public ECSComponent GetComponent(Type componentType)
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

        public T[] GetComponents<T>() where T : ECSComponent
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

        public List<T> GetComponentsList<T>() where T : ECSComponent
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

        public ECSComponent[] GetComponents(Type comType)
        {
            List<ECSComponent> elements = new List<ECSComponent>();
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


        public override string ToString()
        {
            string str = "[";
            for (int i = 0; i < Components.Count; i++)
            {
                str += Components[i].GetType().Name + ",";
            }
            str = str.TrimEnd(',');
            str += "]";
            return $"{GetType().Name} Components: {str}";
        }


        public void CheckDebugInfo(GameObject gameObject)
        {
#if UNITY_EDITOR
            if (gameObject == null)
            {
                return;
            }

            var debugBehaviour = UnityUtil.AddMonoBehaviour<ECSDebugBehaviour>(gameObject);
            debugBehaviour.m_ECSInfo.Clear();
            for (int i = 0; i < this.Components.Count; i++)
            {
                var component = this.Components[i];
                var cmptName = component.GetType().Name;
                debugBehaviour.SetDebugInfo(cmptName, "", "");
            }
#endif
        }
        public void CheckDebugInfo()
        {
#if UNITY_EDITOR
            //var actorEntity = this as Entity;

            //if (actorEntity == null)
            //{
            //    return;
            //}

            //if (actorEntity.gameObject == null)
            //{
            //    return;
            //}

            //var debugBehaviour = UnityUtil.AddMonoBehaviour<ECSDebugBehaviour>(actorEntity.gameObject);
            //debugBehaviour.m_ECSInfo.Clear();
            //for (int i = 0; i < this.Components.Count; i++)
            //{
            //    var component = this.Components[i];
            //    var cmptName = component.GetType().Name;
            //    debugBehaviour.SetDebugInfo(cmptName, "", "");
            //}
#endif
        }
    }
}
