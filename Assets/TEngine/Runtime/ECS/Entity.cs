using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace TEngine
{
    [Flags]
    public enum EntityStatus : byte
    {
        None = 0,
        IsFromPool = 1,
        IsRegister = 1 << 1,
        IsComponent = 1 << 2,
        IsCreated = 1 << 3,
        IsNew = 1 << 4,
    }

    public class Entity : EcsObject, IIndex
    {
        [IgnoreDataMember]
        private EntityStatus status = EntityStatus.None;

        #region Status
        [IgnoreDataMember]
        private bool IsFromPool
        {
            get => (this.status & EntityStatus.IsFromPool) == EntityStatus.IsFromPool;
            set
            {
                if (value)
                {
                    this.status |= EntityStatus.IsFromPool;
                }
                else
                {
                    this.status &= ~EntityStatus.IsFromPool;
                }
            }
        }


        #endregion

        [SerializeField]
        internal List<EcsComponent> Components = new List<EcsComponent>();
        internal List<IUpdate> Updates = new List<IUpdate>();
        internal List<IFixedUpdate> FixedUpdates = new List<IFixedUpdate>();
        internal bool InActive;
        internal bool CanUpdate;
        internal bool CanFixedUpdate;
        public int Index { get; set; } = -1;
        public EcsEventCmpt Event { get; set; }
        public Entity()
        {
            InActive = true;
            System = EcsSystem.Instance;
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

            var debugBehaviour = UnityUtil.AddMonoBehaviour<EcsDebugBehaviour>(gameObject);
            debugBehaviour.m_EcsInfo.Clear();
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

            //var debugBehaviour = UnityUtil.AddMonoBehaviour<EcsDebugBehaviour>(actorEntity.gameObject);
            //debugBehaviour.m_EcsInfo.Clear();
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
