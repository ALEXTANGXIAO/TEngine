using System;
using System.Collections.Generic;
using TEngine.Runtime.ObjectPool;

namespace TEngine.Runtime.Entity
{
    /// <summary>
    /// 实体管理器。
    /// </summary>
    internal sealed partial class EntityManager : IEntityManager
    {
        private readonly Dictionary<int, EntityInfo> m_EntityInfos;
        private readonly Dictionary<string, EntityGroup> m_EntityGroups;
        private readonly Dictionary<int, int> m_EntitiesBeingLoaded;
        private readonly HashSet<int> m_EntitiesToReleaseOnLoad;
        private readonly Queue<EntityInfo> m_RecycleQueue;
        private readonly LoadAssetCallbacks m_LoadAssetCallbacks;
        private IObjectPoolManager m_ObjectPoolManager;
        private IEntityHelper m_EntityHelper;
        private int m_Serial;
        private bool m_IsShutdown;
        private Action<IEntity, float, object> m_ShowEntitySuccessEventHandler;
        private Action<int, string, string, string, object> m_ShowEntityFailureEventHandler;
        private Action<int, string, string, float, object> m_ShowEntityUpdateEventHandler;
        private Action<int, string, string, string, int, int, object> m_ShowEntityDependencyAssetEventHandler;
        private Action<int, string, IEntityGroup, object> m_HideEntityCompleteEventHandler;

        /// <summary>
        /// 初始化实体管理器的新实例。
        /// </summary>
        public EntityManager()
        {
            m_EntityInfos = new Dictionary<int, EntityInfo>();
            m_EntityGroups = new Dictionary<string, EntityGroup>(StringComparer.Ordinal);
            m_EntitiesBeingLoaded = new Dictionary<int, int>();
            m_EntitiesToReleaseOnLoad = new HashSet<int>();
            m_RecycleQueue = new Queue<EntityInfo>();
            m_LoadAssetCallbacks = new LoadAssetCallbacks(LoadAssetSuccessCallback, LoadAssetFailureCallback,
                LoadAssetUpdateCallback, LoadAssetDependencyAssetCallback);
            m_ObjectPoolManager = null;
            m_EntityHelper = null;
            m_Serial = 0;
            m_IsShutdown = false;
            m_ShowEntitySuccessEventHandler = null;
            m_ShowEntityFailureEventHandler = null;
            m_ShowEntityUpdateEventHandler = null;
            m_ShowEntityDependencyAssetEventHandler = null;
            m_HideEntityCompleteEventHandler = null;
        }

        /// <summary>
        /// 获取实体数量。
        /// </summary>
        public int EntityCount
        {
            get { return m_EntityInfos.Count; }
        }

        /// <summary>
        /// 获取实体组数量。
        /// </summary>
        public int EntityGroupCount
        {
            get { return m_EntityGroups.Count; }
        }

        /// <summary>
        /// 显示实体成功事件。
        /// </summary>
        public event Action<IEntity, float, object> ShowEntitySuccess
        {
            add { m_ShowEntitySuccessEventHandler += value; }
            remove { m_ShowEntitySuccessEventHandler -= value; }
        }

        /// <summary>
        /// 显示实体失败事件。
        /// </summary>
        public event Action<int, string, string, string, object> ShowEntityFailure
        {
            add { m_ShowEntityFailureEventHandler += value; }
            remove { m_ShowEntityFailureEventHandler -= value; }
        }

        /// <summary>
        /// 显示实体更新事件。
        /// </summary>
        public event Action<int, string, string, float, object> ShowEntityUpdate
        {
            add { m_ShowEntityUpdateEventHandler += value; }
            remove { m_ShowEntityUpdateEventHandler -= value; }
        }

        /// <summary>
        /// 显示实体时加载依赖资源事件。
        /// </summary>
        public event Action<int, string, string, string, int, int, object> ShowEntityDependencyAsset
        {
            add { m_ShowEntityDependencyAssetEventHandler += value; }
            remove { m_ShowEntityDependencyAssetEventHandler -= value; }
        }

        /// <summary>
        /// 隐藏实体完成事件。
        /// </summary>
        public event Action<int, string, IEntityGroup, object> HideEntityComplete
        {
            add { m_HideEntityCompleteEventHandler += value; }
            remove { m_HideEntityCompleteEventHandler -= value; }
        }

        /// <summary>
        /// 实体管理器轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        public void Update(float elapseSeconds, float realElapseSeconds)
        {
            while (m_RecycleQueue.Count > 0)
            {
                EntityInfo entityInfo = m_RecycleQueue.Dequeue();
                IEntity entity = entityInfo.Entity;
                EntityGroup entityGroup = (EntityGroup)entity.EntityGroup;
                if (entityGroup == null)
                {
                    throw new Exception("Entity group is invalid.");
                }

                entityInfo.Status = EntityStatus.WillRecycle;
                entity.OnRecycle();
                entityInfo.Status = EntityStatus.Recycled;
                entityGroup.UnspawnEntity(entity);
                MemoryPool.Release(entityInfo);
            }

            foreach (KeyValuePair<string, EntityGroup> entityGroup in m_EntityGroups)
            {
                entityGroup.Value.Update(elapseSeconds, realElapseSeconds);
            }
        }

        /// <summary>
        /// 关闭并清理实体管理器。
        /// </summary>
        public void ShutDown()
        {
            m_IsShutdown = true;
            HideAllLoadedEntities();
            m_EntityGroups.Clear();
            m_EntitiesBeingLoaded.Clear();
            m_EntitiesToReleaseOnLoad.Clear();
            m_RecycleQueue.Clear();
        }

        /// <summary>
        /// 设置对象池管理器。
        /// </summary>
        /// <param name="objectPoolManager">对象池管理器。</param>
        public void SetObjectPoolManager(IObjectPoolManager objectPoolManager)
        {
            if (objectPoolManager == null)
            {
                throw new Exception("Object pool manager is invalid.");
            }

            m_ObjectPoolManager = objectPoolManager;
        }
        
        /// <summary>
        /// 设置实体辅助器。
        /// </summary>
        /// <param name="entityHelper">实体辅助器。</param>
        public void SetEntityHelper(IEntityHelper entityHelper)
        {
            if (entityHelper == null)
            {
                throw new Exception("Entity helper is invalid.");
            }

            m_EntityHelper = entityHelper;
        }

        /// <summary>
        /// 是否存在实体组。
        /// </summary>
        /// <param name="entityGroupName">实体组名称。</param>
        /// <returns>是否存在实体组。</returns>
        public bool HasEntityGroup(string entityGroupName)
        {
            if (string.IsNullOrEmpty(entityGroupName))
            {
                throw new Exception("Entity group name is invalid.");
            }

            return m_EntityGroups.ContainsKey(entityGroupName);
        }

        /// <summary>
        /// 获取实体组。
        /// </summary>
        /// <param name="entityGroupName">实体组名称。</param>
        /// <returns>要获取的实体组。</returns>
        public IEntityGroup GetEntityGroup(string entityGroupName)
        {
            if (string.IsNullOrEmpty(entityGroupName))
            {
                throw new Exception("Entity group name is invalid.");
            }

            EntityGroup entityGroup = null;
            if (m_EntityGroups.TryGetValue(entityGroupName, out entityGroup))
            {
                return entityGroup;
            }

            return null;
        }

        /// <summary>
        /// 获取所有实体组。
        /// </summary>
        /// <returns>所有实体组。</returns>
        public IEntityGroup[] GetAllEntityGroups()
        {
            int index = 0;
            IEntityGroup[] results = new IEntityGroup[m_EntityGroups.Count];
            foreach (KeyValuePair<string, EntityGroup> entityGroup in m_EntityGroups)
            {
                results[index++] = entityGroup.Value;
            }

            return results;
        }

        /// <summary>
        /// 获取所有实体组。
        /// </summary>
        /// <param name="results">所有实体组。</param>
        public void GetAllEntityGroups(List<IEntityGroup> results)
        {
            if (results == null)
            {
                throw new Exception("Results is invalid.");
            }

            results.Clear();
            foreach (KeyValuePair<string, EntityGroup> entityGroup in m_EntityGroups)
            {
                results.Add(entityGroup.Value);
            }
        }

        /// <summary>
        /// 增加实体组。
        /// </summary>
        /// <param name="entityGroupName">实体组名称。</param>
        /// <param name="instanceAutoReleaseInterval">实体实例对象池自动释放可释放对象的间隔秒数。</param>
        /// <param name="instanceCapacity">实体实例对象池容量。</param>
        /// <param name="instanceExpireTime">实体实例对象池对象过期秒数。</param>
        /// <param name="instancePriority">实体实例对象池的优先级。</param>
        /// <param name="entityGroupHelper">实体组辅助器。</param>
        /// <returns>是否增加实体组成功。</returns>
        public bool AddEntityGroup(string entityGroupName, float instanceAutoReleaseInterval, int instanceCapacity,
            float instanceExpireTime, int instancePriority, IEntityGroupHelper entityGroupHelper)
        {
            if (string.IsNullOrEmpty(entityGroupName))
            {
                throw new Exception("Entity group name is invalid.");
            }

            if (entityGroupHelper == null)
            {
                throw new Exception("Entity group helper is invalid.");
            }

            if (m_ObjectPoolManager == null)
            {
                throw new Exception("You must set object pool manager first.");
            }

            if (HasEntityGroup(entityGroupName))
            {
                return false;
            }

            m_EntityGroups.Add(entityGroupName,
                new EntityGroup(entityGroupName, instanceAutoReleaseInterval, instanceCapacity, instanceExpireTime,
                    instancePriority, entityGroupHelper, m_ObjectPoolManager));

            return true;
        }

        /// <summary>
        /// 是否存在实体。
        /// </summary>
        /// <param name="entityId">实体编号。</param>
        /// <returns>是否存在实体。</returns>
        public bool HasEntity(int entityId)
        {
            return m_EntityInfos.ContainsKey(entityId);
        }

        /// <summary>
        /// 是否存在实体。
        /// </summary>
        /// <param name="entityAssetName">实体资源名称。</param>
        /// <returns>是否存在实体。</returns>
        public bool HasEntity(string entityAssetName)
        {
            if (string.IsNullOrEmpty(entityAssetName))
            {
                throw new Exception("Entity asset name is invalid.");
            }

            foreach (KeyValuePair<int, EntityInfo> entityInfo in m_EntityInfos)
            {
                if (entityInfo.Value.Entity.EntityAssetName == entityAssetName)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 获取实体。
        /// </summary>
        /// <param name="entityId">实体编号。</param>
        /// <returns>要获取的实体。</returns>
        public IEntity GetEntity(int entityId)
        {
            EntityInfo entityInfo = GetEntityInfo(entityId);
            if (entityInfo == null)
            {
                return null;
            }

            return entityInfo.Entity;
        }

        /// <summary>
        /// 获取实体。
        /// </summary>
        /// <param name="entityAssetName">实体资源名称。</param>
        /// <returns>要获取的实体。</returns>
        public IEntity GetEntity(string entityAssetName)
        {
            if (string.IsNullOrEmpty(entityAssetName))
            {
                throw new Exception("Entity asset name is invalid.");
            }

            foreach (KeyValuePair<int, EntityInfo> entityInfo in m_EntityInfos)
            {
                if (entityInfo.Value.Entity.EntityAssetName == entityAssetName)
                {
                    return entityInfo.Value.Entity;
                }
            }

            return null;
        }

        /// <summary>
        /// 获取实体。
        /// </summary>
        /// <param name="entityAssetName">实体资源名称。</param>
        /// <returns>要获取的实体。</returns>
        public IEntity[] GetEntities(string entityAssetName)
        {
            if (string.IsNullOrEmpty(entityAssetName))
            {
                throw new Exception("Entity asset name is invalid.");
            }

            List<IEntity> results = new List<IEntity>();
            foreach (KeyValuePair<int, EntityInfo> entityInfo in m_EntityInfos)
            {
                if (entityInfo.Value.Entity.EntityAssetName == entityAssetName)
                {
                    results.Add(entityInfo.Value.Entity);
                }
            }

            return results.ToArray();
        }

        /// <summary>
        /// 获取实体。
        /// </summary>
        /// <param name="entityAssetName">实体资源名称。</param>
        /// <param name="results">要获取的实体。</param>
        public void GetEntities(string entityAssetName, List<IEntity> results)
        {
            if (string.IsNullOrEmpty(entityAssetName))
            {
                throw new Exception("Entity asset name is invalid.");
            }

            if (results == null)
            {
                throw new Exception("Results is invalid.");
            }

            results.Clear();
            foreach (KeyValuePair<int, EntityInfo> entityInfo in m_EntityInfos)
            {
                if (entityInfo.Value.Entity.EntityAssetName == entityAssetName)
                {
                    results.Add(entityInfo.Value.Entity);
                }
            }
        }

        /// <summary>
        /// 获取所有已加载的实体。
        /// </summary>
        /// <returns>所有已加载的实体。</returns>
        public IEntity[] GetAllLoadedEntities()
        {
            int index = 0;
            IEntity[] results = new IEntity[m_EntityInfos.Count];
            foreach (KeyValuePair<int, EntityInfo> entityInfo in m_EntityInfos)
            {
                results[index++] = entityInfo.Value.Entity;
            }

            return results;
        }

        /// <summary>
        /// 获取所有已加载的实体。
        /// </summary>
        /// <param name="results">所有已加载的实体。</param>
        public void GetAllLoadedEntities(List<IEntity> results)
        {
            if (results == null)
            {
                throw new Exception("Results is invalid.");
            }

            results.Clear();
            foreach (KeyValuePair<int, EntityInfo> entityInfo in m_EntityInfos)
            {
                results.Add(entityInfo.Value.Entity);
            }
        }

        /// <summary>
        /// 获取所有正在加载实体的编号。
        /// </summary>
        /// <returns>所有正在加载实体的编号。</returns>
        public int[] GetAllLoadingEntityIds()
        {
            int index = 0;
            int[] results = new int[m_EntitiesBeingLoaded.Count];
            foreach (KeyValuePair<int, int> entityBeingLoaded in m_EntitiesBeingLoaded)
            {
                results[index++] = entityBeingLoaded.Key;
            }

            return results;
        }

        /// <summary>
        /// 获取所有正在加载实体的编号。
        /// </summary>
        /// <param name="results">所有正在加载实体的编号。</param>
        public void GetAllLoadingEntityIds(List<int> results)
        {
            if (results == null)
            {
                throw new Exception("Results is invalid.");
            }

            results.Clear();
            foreach (KeyValuePair<int, int> entityBeingLoaded in m_EntitiesBeingLoaded)
            {
                results.Add(entityBeingLoaded.Key);
            }
        }

        /// <summary>
        /// 是否正在加载实体。
        /// </summary>
        /// <param name="entityId">实体编号。</param>
        /// <returns>是否正在加载实体。</returns>
        public bool IsLoadingEntity(int entityId)
        {
            return m_EntitiesBeingLoaded.ContainsKey(entityId);
        }

        /// <summary>
        /// 是否是合法的实体。
        /// </summary>
        /// <param name="entity">实体。</param>
        /// <returns>实体是否合法。</returns>
        public bool IsValidEntity(IEntity entity)
        {
            if (entity == null)
            {
                return false;
            }

            return HasEntity(entity.Id);
        }

        /// <summary>
        /// 显示实体。
        /// </summary>
        /// <param name="entityId">实体编号。</param>
        /// <param name="entityAssetName">实体资源名称。</param>
        /// <param name="entityGroupName">实体组名称。</param>
        public void ShowEntity(int entityId, string entityAssetName, string entityGroupName)
        {
            ShowEntity(entityId, entityAssetName, entityGroupName, Constant.DefaultPriority, null);
        }

        /// <summary>
        /// 显示实体。
        /// </summary>
        /// <param name="entityId">实体编号。</param>
        /// <param name="entityAssetName">实体资源名称。</param>
        /// <param name="entityGroupName">实体组名称。</param>
        /// <param name="priority">加载实体资源的优先级。</param>
        public void ShowEntity(int entityId, string entityAssetName, string entityGroupName, int priority)
        {
            ShowEntity(entityId, entityAssetName, entityGroupName, priority, null);
        }

        /// <summary>
        /// 显示实体。
        /// </summary>
        /// <param name="entityId">实体编号。</param>
        /// <param name="entityAssetName">实体资源名称。</param>
        /// <param name="entityGroupName">实体组名称。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void ShowEntity(int entityId, string entityAssetName, string entityGroupName, object userData)
        {
            ShowEntity(entityId, entityAssetName, entityGroupName, Constant.DefaultPriority, userData);
        }

        /// <summary>
        /// 显示实体。
        /// </summary>
        /// <param name="entityId">实体编号。</param>
        /// <param name="entityAssetName">实体资源名称。</param>
        /// <param name="entityGroupName">实体组名称。</param>
        /// <param name="priority">加载实体资源的优先级。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void ShowEntity(int entityId, string entityAssetName, string entityGroupName, int priority,
            object userData)
        {
            if (m_EntityHelper == null)
            {
                throw new Exception("You must set entity helper first.");
            }

            if (string.IsNullOrEmpty(entityAssetName))
            {
                throw new Exception("Entity asset name is invalid.");
            }

            if (string.IsNullOrEmpty(entityGroupName))
            {
                throw new Exception("Entity group name is invalid.");
            }

            if (HasEntity(entityId))
            {
                throw new Exception(Utility.Text.Format("Entity id '{0}' is already exist.", entityId));
            }

            if (IsLoadingEntity(entityId))
            {
                throw new Exception(Utility.Text.Format("Entity '{0}' is already being loaded.",
                    entityId));
            }

            EntityGroup entityGroup = (EntityGroup)GetEntityGroup(entityGroupName);
            if (entityGroup == null)
            {
                throw new Exception(Utility.Text.Format("Entity group '{0}' is not exist.",
                    entityGroupName));
            }

            EntityInstanceObject entityInstanceObject = entityGroup.SpawnEntityInstanceObject(entityAssetName);
            if (entityInstanceObject == null)
            {
                int serialId = ++m_Serial;
                m_EntitiesBeingLoaded.Add(entityId, serialId);
                TResources.LoadAsset(entityAssetName, priority, m_LoadAssetCallbacks,
                    ShowEntityInfo.Create(serialId, entityId, entityGroup, userData));
                return;
            }

            InternalShowEntity(entityId, entityAssetName, entityGroup, entityInstanceObject.Target, false, 0f,
                userData);
        }

        /// <summary>
        /// 隐藏实体。
        /// </summary>
        /// <param name="entityId">实体编号。</param>
        public void HideEntity(int entityId)
        {
            HideEntity(entityId, null);
        }

        /// <summary>
        /// 隐藏实体。
        /// </summary>
        /// <param name="entityId">实体编号。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void HideEntity(int entityId, object userData)
        {
            if (IsLoadingEntity(entityId))
            {
                m_EntitiesToReleaseOnLoad.Add(m_EntitiesBeingLoaded[entityId]);
                m_EntitiesBeingLoaded.Remove(entityId);
                return;
            }

            EntityInfo entityInfo = GetEntityInfo(entityId);
            if (entityInfo == null)
            {
                throw new Exception(Utility.Text.Format("Can not find entity '{0}'.", entityId));
            }

            InternalHideEntity(entityInfo, userData);
        }

        /// <summary>
        /// 隐藏实体。
        /// </summary>
        /// <param name="entity">实体。</param>
        public void HideEntity(IEntity entity)
        {
            HideEntity(entity, null);
        }

        /// <summary>
        /// 隐藏实体。
        /// </summary>
        /// <param name="entity">实体。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void HideEntity(IEntity entity, object userData)
        {
            if (entity == null)
            {
                throw new Exception("Entity is invalid.");
            }

            HideEntity(entity.Id, userData);
        }

        /// <summary>
        /// 隐藏所有已加载的实体。
        /// </summary>
        public void HideAllLoadedEntities()
        {
            HideAllLoadedEntities(null);
        }

        /// <summary>
        /// 隐藏所有已加载的实体。
        /// </summary>
        /// <param name="userData">用户自定义数据。</param>
        public void HideAllLoadedEntities(object userData)
        {
            while (m_EntityInfos.Count > 0)
            {
                foreach (KeyValuePair<int, EntityInfo> entityInfo in m_EntityInfos)
                {
                    InternalHideEntity(entityInfo.Value, userData);
                    break;
                }
            }
        }

        /// <summary>
        /// 隐藏所有正在加载的实体。
        /// </summary>
        public void HideAllLoadingEntities()
        {
            foreach (KeyValuePair<int, int> entityBeingLoaded in m_EntitiesBeingLoaded)
            {
                m_EntitiesToReleaseOnLoad.Add(entityBeingLoaded.Value);
            }

            m_EntitiesBeingLoaded.Clear();
        }

        /// <summary>
        /// 获取父实体。
        /// </summary>
        /// <param name="childEntityId">要获取父实体的子实体的实体编号。</param>
        /// <returns>子实体的父实体。</returns>
        public IEntity GetParentEntity(int childEntityId)
        {
            EntityInfo childEntityInfo = GetEntityInfo(childEntityId);
            if (childEntityInfo == null)
            {
                throw new Exception(Utility.Text.Format("Can not find child entity '{0}'.",
                    childEntityId));
            }

            return childEntityInfo.ParentEntity;
        }

        /// <summary>
        /// 获取父实体。
        /// </summary>
        /// <param name="childEntity">要获取父实体的子实体。</param>
        /// <returns>子实体的父实体。</returns>
        public IEntity GetParentEntity(IEntity childEntity)
        {
            if (childEntity == null)
            {
                throw new Exception("Child entity is invalid.");
            }

            return GetParentEntity(childEntity.Id);
        }

        /// <summary>
        /// 获取子实体数量。
        /// </summary>
        /// <param name="parentEntityId">要获取子实体数量的父实体的实体编号。</param>
        /// <returns>子实体数量。</returns>
        public int GetChildEntityCount(int parentEntityId)
        {
            EntityInfo parentEntityInfo = GetEntityInfo(parentEntityId);
            if (parentEntityInfo == null)
            {
                throw new Exception(Utility.Text.Format("Can not find parent entity '{0}'.",
                    parentEntityId));
            }

            return parentEntityInfo.ChildEntityCount;
        }

        /// <summary>
        /// 获取子实体。
        /// </summary>
        /// <param name="parentEntityId">要获取子实体的父实体的实体编号。</param>
        /// <returns>子实体。</returns>
        public IEntity GetChildEntity(int parentEntityId)
        {
            EntityInfo parentEntityInfo = GetEntityInfo(parentEntityId);
            if (parentEntityInfo == null)
            {
                throw new Exception(Utility.Text.Format("Can not find parent entity '{0}'.",
                    parentEntityId));
            }

            return parentEntityInfo.GetChildEntity();
        }

        /// <summary>
        /// 获取子实体。
        /// </summary>
        /// <param name="parentEntity">要获取子实体的父实体。</param>
        /// <returns>子实体。</returns>
        public IEntity GetChildEntity(IEntity parentEntity)
        {
            if (parentEntity == null)
            {
                throw new Exception("Parent entity is invalid.");
            }

            return GetChildEntity(parentEntity.Id);
        }

        /// <summary>
        /// 获取所有子实体。
        /// </summary>
        /// <param name="parentEntityId">要获取所有子实体的父实体的实体编号。</param>
        /// <returns>所有子实体。</returns>
        public IEntity[] GetChildEntities(int parentEntityId)
        {
            EntityInfo parentEntityInfo = GetEntityInfo(parentEntityId);
            if (parentEntityInfo == null)
            {
                throw new Exception(Utility.Text.Format("Can not find parent entity '{0}'.",
                    parentEntityId));
            }

            return parentEntityInfo.GetChildEntities();
        }

        /// <summary>
        /// 获取所有子实体。
        /// </summary>
        /// <param name="parentEntityId">要获取所有子实体的父实体的实体编号。</param>
        /// <param name="results">所有子实体。</param>
        public void GetChildEntities(int parentEntityId, List<IEntity> results)
        {
            EntityInfo parentEntityInfo = GetEntityInfo(parentEntityId);
            if (parentEntityInfo == null)
            {
                throw new Exception(Utility.Text.Format("Can not find parent entity '{0}'.",
                    parentEntityId));
            }

            parentEntityInfo.GetChildEntities(results);
        }

        /// <summary>
        /// 获取所有子实体。
        /// </summary>
        /// <param name="parentEntity">要获取所有子实体的父实体。</param>
        /// <returns>所有子实体。</returns>
        public IEntity[] GetChildEntities(IEntity parentEntity)
        {
            if (parentEntity == null)
            {
                throw new Exception("Parent entity is invalid.");
            }

            return GetChildEntities(parentEntity.Id);
        }

        /// <summary>
        /// 获取所有子实体。
        /// </summary>
        /// <param name="parentEntity">要获取所有子实体的父实体。</param>
        /// <param name="results">所有子实体。</param>
        public void GetChildEntities(IEntity parentEntity, List<IEntity> results)
        {
            if (parentEntity == null)
            {
                throw new Exception("Parent entity is invalid.");
            }

            GetChildEntities(parentEntity.Id, results);
        }

        /// <summary>
        /// 附加子实体。
        /// </summary>
        /// <param name="childEntityId">要附加的子实体的实体编号。</param>
        /// <param name="parentEntityId">被附加的父实体的实体编号。</param>
        public void AttachEntity(int childEntityId, int parentEntityId)
        {
            AttachEntity(childEntityId, parentEntityId, null);
        }

        /// <summary>
        /// 附加子实体。
        /// </summary>
        /// <param name="childEntityId">要附加的子实体的实体编号。</param>
        /// <param name="parentEntityId">被附加的父实体的实体编号。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void AttachEntity(int childEntityId, int parentEntityId, object userData)
        {
            if (childEntityId == parentEntityId)
            {
                throw new Exception(Utility.Text.Format(
                    "Can not attach entity when child entity id equals to parent entity id '{0}'.", parentEntityId));
            }

            EntityInfo childEntityInfo = GetEntityInfo(childEntityId);
            if (childEntityInfo == null)
            {
                throw new Exception(Utility.Text.Format("Can not find child entity '{0}'.",
                    childEntityId));
            }

            if (childEntityInfo.Status >= EntityStatus.WillHide)
            {
                throw new Exception(
                    Utility.Text.Format("Can not attach entity when child entity status is '{0}'.",
                        childEntityInfo.Status));
            }

            EntityInfo parentEntityInfo = GetEntityInfo(parentEntityId);
            if (parentEntityInfo == null)
            {
                throw new Exception(Utility.Text.Format("Can not find parent entity '{0}'.",
                    parentEntityId));
            }

            if (parentEntityInfo.Status >= EntityStatus.WillHide)
            {
                throw new Exception(Utility.Text.Format(
                    "Can not attach entity when parent entity status is '{0}'.", parentEntityInfo.Status));
            }

            IEntity childEntity = childEntityInfo.Entity;
            IEntity parentEntity = parentEntityInfo.Entity;
            DetachEntity(childEntity.Id, userData);
            childEntityInfo.ParentEntity = parentEntity;
            parentEntityInfo.AddChildEntity(childEntity);
            parentEntity.OnAttached(childEntity, userData);
            childEntity.OnAttachTo(parentEntity, userData);
        }

        /// <summary>
        /// 附加子实体。
        /// </summary>
        /// <param name="childEntityId">要附加的子实体的实体编号。</param>
        /// <param name="parentEntity">被附加的父实体。</param>
        public void AttachEntity(int childEntityId, IEntity parentEntity)
        {
            AttachEntity(childEntityId, parentEntity, null);
        }

        /// <summary>
        /// 附加子实体。
        /// </summary>
        /// <param name="childEntityId">要附加的子实体的实体编号。</param>
        /// <param name="parentEntity">被附加的父实体。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void AttachEntity(int childEntityId, IEntity parentEntity, object userData)
        {
            if (parentEntity == null)
            {
                throw new Exception("Parent entity is invalid.");
            }

            AttachEntity(childEntityId, parentEntity.Id, userData);
        }

        /// <summary>
        /// 附加子实体。
        /// </summary>
        /// <param name="childEntity">要附加的子实体。</param>
        /// <param name="parentEntityId">被附加的父实体的实体编号。</param>
        public void AttachEntity(IEntity childEntity, int parentEntityId)
        {
            AttachEntity(childEntity, parentEntityId, null);
        }

        /// <summary>
        /// 附加子实体。
        /// </summary>
        /// <param name="childEntity">要附加的子实体。</param>
        /// <param name="parentEntityId">被附加的父实体的实体编号。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void AttachEntity(IEntity childEntity, int parentEntityId, object userData)
        {
            if (childEntity == null)
            {
                throw new Exception("Child entity is invalid.");
            }

            AttachEntity(childEntity.Id, parentEntityId, userData);
        }

        /// <summary>
        /// 附加子实体。
        /// </summary>
        /// <param name="childEntity">要附加的子实体。</param>
        /// <param name="parentEntity">被附加的父实体。</param>
        public void AttachEntity(IEntity childEntity, IEntity parentEntity)
        {
            AttachEntity(childEntity, parentEntity, null);
        }

        /// <summary>
        /// 附加子实体。
        /// </summary>
        /// <param name="childEntity">要附加的子实体。</param>
        /// <param name="parentEntity">被附加的父实体。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void AttachEntity(IEntity childEntity, IEntity parentEntity, object userData)
        {
            if (childEntity == null)
            {
                throw new Exception("Child entity is invalid.");
            }

            if (parentEntity == null)
            {
                throw new Exception("Parent entity is invalid.");
            }

            AttachEntity(childEntity.Id, parentEntity.Id, userData);
        }

        /// <summary>
        /// 解除子实体。
        /// </summary>
        /// <param name="childEntityId">要解除的子实体的实体编号。</param>
        public void DetachEntity(int childEntityId)
        {
            DetachEntity(childEntityId, null);
        }

        /// <summary>
        /// 解除子实体。
        /// </summary>
        /// <param name="childEntityId">要解除的子实体的实体编号。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void DetachEntity(int childEntityId, object userData)
        {
            EntityInfo childEntityInfo = GetEntityInfo(childEntityId);
            if (childEntityInfo == null)
            {
                throw new Exception(Utility.Text.Format("Can not find child entity '{0}'.",
                    childEntityId));
            }

            IEntity parentEntity = childEntityInfo.ParentEntity;
            if (parentEntity == null)
            {
                return;
            }

            EntityInfo parentEntityInfo = GetEntityInfo(parentEntity.Id);
            if (parentEntityInfo == null)
            {
                throw new Exception(Utility.Text.Format("Can not find parent entity '{0}'.",
                    parentEntity.Id));
            }

            IEntity childEntity = childEntityInfo.Entity;
            childEntityInfo.ParentEntity = null;
            parentEntityInfo.RemoveChildEntity(childEntity);
            parentEntity.OnDetached(childEntity, userData);
            childEntity.OnDetachFrom(parentEntity, userData);
        }

        /// <summary>
        /// 解除子实体。
        /// </summary>
        /// <param name="childEntity">要解除的子实体。</param>
        public void DetachEntity(IEntity childEntity)
        {
            DetachEntity(childEntity, null);
        }

        /// <summary>
        /// 解除子实体。
        /// </summary>
        /// <param name="childEntity">要解除的子实体。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void DetachEntity(IEntity childEntity, object userData)
        {
            if (childEntity == null)
            {
                throw new Exception("Child entity is invalid.");
            }

            DetachEntity(childEntity.Id, userData);
        }

        /// <summary>
        /// 解除所有子实体。
        /// </summary>
        /// <param name="parentEntityId">被解除的父实体的实体编号。</param>
        public void DetachChildEntities(int parentEntityId)
        {
            DetachChildEntities(parentEntityId, null);
        }

        /// <summary>
        /// 解除所有子实体。
        /// </summary>
        /// <param name="parentEntityId">被解除的父实体的实体编号。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void DetachChildEntities(int parentEntityId, object userData)
        {
            EntityInfo parentEntityInfo = GetEntityInfo(parentEntityId);
            if (parentEntityInfo == null)
            {
                throw new Exception(Utility.Text.Format("Can not find parent entity '{0}'.",
                    parentEntityId));
            }

            while (parentEntityInfo.ChildEntityCount > 0)
            {
                IEntity childEntity = parentEntityInfo.GetChildEntity();
                DetachEntity(childEntity.Id, userData);
            }
        }

        /// <summary>
        /// 解除所有子实体。
        /// </summary>
        /// <param name="parentEntity">被解除的父实体。</param>
        public void DetachChildEntities(IEntity parentEntity)
        {
            DetachChildEntities(parentEntity, null);
        }

        /// <summary>
        /// 解除所有子实体。
        /// </summary>
        /// <param name="parentEntity">被解除的父实体。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void DetachChildEntities(IEntity parentEntity, object userData)
        {
            if (parentEntity == null)
            {
                throw new Exception("Parent entity is invalid.");
            }

            DetachChildEntities(parentEntity.Id, userData);
        }

        /// <summary>
        /// 获取实体信息。
        /// </summary>
        /// <param name="entityId">实体编号。</param>
        /// <returns>实体信息。</returns>
        private EntityInfo GetEntityInfo(int entityId)
        {
            EntityInfo entityInfo = null;
            if (m_EntityInfos.TryGetValue(entityId, out entityInfo))
            {
                return entityInfo;
            }

            return null;
        }

        private void InternalShowEntity(int entityId, string entityAssetName, EntityGroup entityGroup,
            object entityInstance, bool isNewInstance, float duration, object userData)
        {
            try
            {
                IEntity entity = m_EntityHelper.CreateEntity(entityInstance, entityGroup, userData);
                if (entity == null)
                {
                    throw new Exception("Can not create entity in entity helper.");
                }

                EntityInfo entityInfo = EntityInfo.Create(entity);
                m_EntityInfos.Add(entityId, entityInfo);
                entityInfo.Status = EntityStatus.WillInit;
                entity.OnInit(entityId, entityAssetName, entityGroup, isNewInstance, userData);
                entityInfo.Status = EntityStatus.Inited;
                entityGroup.AddEntity(entity);
                entityInfo.Status = EntityStatus.WillShow;
                entity.OnShow(userData);
                entityInfo.Status = EntityStatus.Showed;

                if (m_ShowEntitySuccessEventHandler != null)
                {
                    m_ShowEntitySuccessEventHandler(entity, duration, userData);
                }
            }
            catch (Exception exception)
            {
                if (m_ShowEntityFailureEventHandler != null)
                {
                    m_ShowEntityFailureEventHandler(entityId,
                        entityAssetName, entityGroup.Name, exception.ToString(), userData);
                    return;
                }

                throw;
            }
        }

        private void InternalHideEntity(EntityInfo entityInfo, object userData)
        {
            while (entityInfo.ChildEntityCount > 0)
            {
                IEntity childEntity = entityInfo.GetChildEntity();
                HideEntity(childEntity.Id, userData);
            }

            if (entityInfo.Status == EntityStatus.Hidden)
            {
                return;
            }

            IEntity entity = entityInfo.Entity;
            DetachEntity(entity.Id, userData);
            entityInfo.Status = EntityStatus.WillHide;
            entity.OnHide(m_IsShutdown, userData);
            entityInfo.Status = EntityStatus.Hidden;

            EntityGroup entityGroup = (EntityGroup)entity.EntityGroup;
            if (entityGroup == null)
            {
                throw new Exception("Entity group is invalid.");
            }

            entityGroup.RemoveEntity(entity);
            if (!m_EntityInfos.Remove(entity.Id))
            {
                throw new Exception("Entity info is unmanaged.");
            }

            if (m_HideEntityCompleteEventHandler != null)
            {
                m_HideEntityCompleteEventHandler(entity.Id, entity.EntityAssetName, entityGroup, userData);
            }

            m_RecycleQueue.Enqueue(entityInfo);
        }

        private void LoadAssetSuccessCallback(string entityAssetName, object entityAsset, float duration,
            object userData)
        {
            ShowEntityInfo showEntityInfo = (ShowEntityInfo)userData;
            if (showEntityInfo == null)
            {
                throw new Exception("Show entity info is invalid.");
            }

            if (m_EntitiesToReleaseOnLoad.Contains(showEntityInfo.SerialId))
            {
                m_EntitiesToReleaseOnLoad.Remove(showEntityInfo.SerialId);
                MemoryPool.Release(showEntityInfo);
                m_EntityHelper.ReleaseEntity(entityAsset, null);
                return;
            }

            m_EntitiesBeingLoaded.Remove(showEntityInfo.EntityId);
            EntityInstanceObject entityInstanceObject = EntityInstanceObject.Create(entityAssetName, entityAsset,
                m_EntityHelper.InstantiateEntity(entityAsset), m_EntityHelper);
            showEntityInfo.EntityGroup.RegisterEntityInstanceObject(entityInstanceObject, true);

            InternalShowEntity(showEntityInfo.EntityId, entityAssetName, showEntityInfo.EntityGroup,
                entityInstanceObject.Target, true, duration, showEntityInfo.UserData);
            MemoryPool.Release(showEntityInfo);
        }

        private void LoadAssetFailureCallback(string entityAssetName, LoadResourceStatus status, string errorMessage,
            object userData)
        {
            ShowEntityInfo showEntityInfo = (ShowEntityInfo)userData;
            if (showEntityInfo == null)
            {
                throw new Exception("Show entity info is invalid.");
            }

            if (m_EntitiesToReleaseOnLoad.Contains(showEntityInfo.SerialId))
            {
                m_EntitiesToReleaseOnLoad.Remove(showEntityInfo.SerialId);
                return;
            }

            m_EntitiesBeingLoaded.Remove(showEntityInfo.EntityId);
            string appendErrorMessage =
                Utility.Text.Format("Load entity failure, asset name '{0}', status '{1}', error message '{2}'.",
                    entityAssetName, status, errorMessage);
            if (m_ShowEntityFailureEventHandler != null)
            {
                m_ShowEntityFailureEventHandler(showEntityInfo.EntityId, entityAssetName,
                    showEntityInfo.EntityGroup.Name, appendErrorMessage, showEntityInfo.UserData);
                return;
            }

            throw new Exception(appendErrorMessage);
        }

        private void LoadAssetUpdateCallback(string entityAssetName, float progress, object userData)
        {
            ShowEntityInfo showEntityInfo = (ShowEntityInfo)userData;
            if (showEntityInfo == null)
            {
                throw new Exception("Show entity info is invalid.");
            }

            if (m_ShowEntityUpdateEventHandler != null)
            {
                m_ShowEntityUpdateEventHandler(showEntityInfo.EntityId, entityAssetName,
                    showEntityInfo.EntityGroup.Name, progress, showEntityInfo.UserData);
            }
        }

        private void LoadAssetDependencyAssetCallback(string entityAssetName, string dependencyAssetName,
            int loadedCount, int totalCount, object userData)
        {
            ShowEntityInfo showEntityInfo = (ShowEntityInfo)userData;
            if (showEntityInfo == null)
            {
                throw new Exception("Show entity info is invalid.");
            }

            if (m_ShowEntityDependencyAssetEventHandler != null)
            {
                m_ShowEntityDependencyAssetEventHandler(showEntityInfo.EntityId, entityAssetName,
                    showEntityInfo.EntityGroup.Name, dependencyAssetName, loadedCount, totalCount,
                    showEntityInfo.UserData);
            }
        }
    }
}