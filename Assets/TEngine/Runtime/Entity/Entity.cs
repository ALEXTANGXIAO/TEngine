using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace TEngine.EntityModule
{
    [Flags]
    public enum EntityStatus : byte
    {
        None = 0,
        IsFromPool = 1,
        IsRegister = 1 << 1,
        IsComponent = 1 << 2,
        IsUsing = 1 << 3,
        IsDispose = 1 << 4,
    }

    public partial class Entity : EcsObject, IIndex
    {
        [IgnoreDataMember]
        private EntityStatus status = EntityStatus.None;

        #region Status
        [IgnoreDataMember]
        internal bool IsFromPool
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

        [IgnoreDataMember]
        internal bool IsDispose
        {
            get => (this.status & EntityStatus.IsDispose) == EntityStatus.IsDispose;
            set
            {
                if (value)
                {
                    this.status |= EntityStatus.IsDispose;
                    CanUpdate = false;
                    CanFixedUpdate = false;
                    CanLateUpdates = false;
                    Updates.Clear();
                    FixedUpdates.Clear();
                    LateUpdates.Clear();
                }
                else
                {
                    this.status &= ~EntityStatus.IsDispose;
                }
            }
        }
        

        #endregion

        [SerializeField]
        internal List<EntityComponent> Components = new List<EntityComponent>();
        internal List<IUpdate> Updates = new List<IUpdate>();
        internal List<IFixedUpdate> FixedUpdates = new List<IFixedUpdate>();
        internal List<ILateUpdate> LateUpdates = new List<ILateUpdate>();
        internal bool CanUpdate;
        internal bool CanFixedUpdate;
        internal bool CanLateUpdates;

        public int Index { get; set; } = -1;
        public Entity()
        {
            System = EntitySystem.Instance;
        }

        ~Entity()
        {
            
        }

        internal void Update()
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

        internal void LateUpdate()
        {
            for (int i = 0; i < LateUpdates.Count; i++)
            {
                LateUpdates[i].LateUpdate();
            }
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
            return $"{GetType().Name} InstanceId {this.InstanceId} Components: {str}";
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

        #region Static
        public static T Create<T>() where T : Entity, new()
        {
            var entity = EntitySystem.Instance.Create<T>();
#if UNITY_EDITOR
            TLogger.LogWarning($"Create ID {entity.InstanceId}  EntityComponent{entity.ToString()}");
#endif
            return entity;
        }
        #endregion
    }
}
