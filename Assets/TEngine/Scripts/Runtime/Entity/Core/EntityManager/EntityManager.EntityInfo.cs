using System;
using System.Collections.Generic;

namespace TEngine.Runtime.Entity
{
    internal sealed partial class EntityManager
    {
        /// <summary>
        /// 实体信息。
        /// </summary>
        private sealed class EntityInfo : IMemory
        {
            private IEntity m_Entity;
            private EntityStatus m_Status;
            private IEntity m_ParentEntity;
            private List<IEntity> m_ChildEntities;

            public EntityInfo()
            {
                m_Entity = null;
                m_Status = EntityStatus.Unknown;
                m_ParentEntity = null;
                m_ChildEntities = new List<IEntity>();
            }

            public IEntity Entity
            {
                get
                {
                    return m_Entity;
                }
            }

            public EntityStatus Status
            {
                get
                {
                    return m_Status;
                }
                set
                {
                    m_Status = value;
                }
            }

            public IEntity ParentEntity
            {
                get
                {
                    return m_ParentEntity;
                }
                set
                {
                    m_ParentEntity = value;
                }
            }

            public int ChildEntityCount
            {
                get
                {
                    return m_ChildEntities.Count;
                }
            }

            public static EntityInfo Create(IEntity entity)
            {
                if (entity == null)
                {
                    throw new Exception("Entity is invalid.");
                }

                EntityInfo entityInfo = MemoryPool.Acquire<EntityInfo>();
                entityInfo.m_Entity = entity;
                entityInfo.m_Status = EntityStatus.WillInit;
                return entityInfo;
            }

            public void Clear()
            {
                m_Entity = null;
                m_Status = EntityStatus.Unknown;
                m_ParentEntity = null;
                m_ChildEntities.Clear();
            }

            public IEntity GetChildEntity()
            {
                return m_ChildEntities.Count > 0 ? m_ChildEntities[0] : null;
            }

            public IEntity[] GetChildEntities()
            {
                return m_ChildEntities.ToArray();
            }

            public void GetChildEntities(List<IEntity> results)
            {
                if (results == null)
                {
                    throw new Exception("Results is invalid.");
                }

                results.Clear();
                foreach (IEntity childEntity in m_ChildEntities)
                {
                    results.Add(childEntity);
                }
            }

            public void AddChildEntity(IEntity childEntity)
            {
                if (m_ChildEntities.Contains(childEntity))
                {
                    throw new Exception("Can not add child entity which is already exist.");
                }

                m_ChildEntities.Add(childEntity);
            }

            public void RemoveChildEntity(IEntity childEntity)
            {
                if (!m_ChildEntities.Remove(childEntity))
                {
                    throw new Exception("Can not remove child entity which is not exist.");
                }
            }
        }
    }
}
