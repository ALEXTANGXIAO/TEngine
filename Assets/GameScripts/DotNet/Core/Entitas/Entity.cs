using System;
using System.Collections.Generic;
using TEngine.Core;
using System.Runtime.Serialization;
using TEngine.DataStructure;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
#pragma warning disable CS8618
#pragma warning disable CS8625
#pragma warning disable CS8601
#pragma warning disable CS8603
#pragma warning disable CS0436

// ReSharper disable SuspiciousTypeConversion.Global
// ReSharper disable InconsistentNaming

namespace TEngine
{
    public abstract class Entity : IDisposable
    {
        #region Entities
        
        private static readonly Dictionary<long, Entity> Entities = new Dictionary<long, Entity>();
        private static readonly OneToManyQueue<Type, Entity> Pool = new OneToManyQueue<Type, Entity>();

        public static Entity GetEntity(long runTimeId)
        {
            return Entities.TryGetValue(runTimeId, out var entity) ? entity : null;
        }

        public static bool TryGetEntity(long runTimeId, out Entity entity)
        {
            return Entities.TryGetValue(runTimeId, out entity);
        }

        public static T GetEntity<T>(long runTimeId) where T : Entity, new()
        {
            if (!Entities.TryGetValue(runTimeId, out var entity))
            {
                return default;
            }

            return (T) entity;
        }

        public static bool TryGetEntity<T>(long runTimeId, out T outEntity) where T : Entity, new()
        {
            if (!Entities.TryGetValue(runTimeId, out var entity))
            {
                outEntity = default;
                return false;
            }

            outEntity = (T) entity;
            return true;
        }

        private static T Rent<T>(Type entityType) where T : Entity, new()
        {
            if (typeof(INotSupportedPool).IsAssignableFrom(entityType))
            {
                return Activator.CreateInstance<T>();
            }

            T entity;

            if (Pool.TryDequeue(entityType, out var poolEntity))
            {
                entity = (T) poolEntity;
            }
            else
            {
                entity = Activator.CreateInstance<T>();
            }

            entity._isFromPool = true;
            return entity;
        }

        private static void Return(Entity entity)
        {
            entity.Id = 0;
            
            if (!entity._isFromPool)
            {
                return;
            }

            entity._isFromPool = false;
            Pool.Enqueue(entity.GetType(), entity);
        }

        #endregion
        
        #region Create

        public static T Create<T>(Scene scene, bool isRunEvent = true) where T : Entity, new()
        {
            var entity = Create<T>(scene.RouteId, isRunEvent);
            entity.Scene = scene;
            return entity;
        }

        public static T Create<T>(Scene scene, long id, bool isRunEvent = true) where T : Entity, new()
        {
            var entity = Create<T>(id, scene.RouteId, isRunEvent);
            entity.Scene = scene;
            return entity;
        }

        private static T Create<T>(uint routeId, bool isRunEvent = true) where T : Entity, new()
        {
            var entity = Rent<T>(typeof(T));
#if TENGINE_NET
            entity.Id = entity.RuntimeId = IdFactory.NextEntityId(routeId);
#else
            entity.Id = entity.RuntimeId = IdFactory.NextRunTimeId();
#endif
            Entities.Add(entity.RuntimeId, entity);

            if (isRunEvent)
            {
                EntitiesSystem.Instance.Awake(entity);
                EntitiesSystem.Instance.StartUpdate(entity);
            }

            return entity;
        }

        private static T Create<T>(long id, uint routeId, bool isRunEvent = true) where T : Entity, new()
        {
            return Create<T>(id, IdFactory.NextEntityId(routeId), isRunEvent);
        }

        private static T Create<T>(long id, long runtimeId, bool isRunEvent = true) where T : Entity, new()
        {
            var entity = Rent<T>(typeof(T));
            entity.Id = id;
            entity.RuntimeId = runtimeId;
            Entities.Add(entity.RuntimeId, entity);

            if (isRunEvent)
            {
                EntitiesSystem.Instance.Awake(entity);
                EntitiesSystem.Instance.StartUpdate(entity);
            }

            return entity;
        }

        protected static Scene CreateScene(long id, bool isRunEvent = true)
        {
            var entity = Create<Scene>(id, id, isRunEvent);
            entity.Scene = entity;
            return entity;
        }

        #endregion
        
        #region Members

        [BsonId]
        [BsonElement]
        [BsonIgnoreIfDefault]
        [BsonDefaultValue(0L)]
        public long Id { get; private set; }
        
        [BsonIgnore] 
        [IgnoreDataMember]
        public long RuntimeId { get; private set; }

        [BsonIgnore] 
        [JsonIgnore]
        [IgnoreDataMember] 
        public bool IsDisposed => RuntimeId == 0;
        
        [BsonIgnore]
        [JsonIgnore]
        [IgnoreDataMember]
        public Scene Scene { get; private set; }
        
        [BsonIgnore] 
        [JsonIgnore]
        [IgnoreDataMember]
        public Entity Parent { get; private set; }

        [BsonElement("t")] 
        [BsonIgnoreIfNull] 
        private ListPool<Entity> _treeDb;
        
        [BsonIgnore] 
        [IgnoreDataMember] 
        private DictionaryPool<Type, Entity> _tree;
        
        [BsonElement("m")] 
        [BsonIgnoreIfNull] 
        private ListPool<Entity> _multiDb;
        
        [BsonIgnore] 
        [IgnoreDataMember] 
        private DictionaryPool<long, ISupportedMultiEntity> _multi;

        [BsonIgnore] 
        [IgnoreDataMember] 
        private bool _isFromPool;

        #endregion

        #region AddComponent

        public T AddComponent<T>() where T : Entity, new()
        {
            var entity = Create<T>(Scene.RouteId, false);
            AddComponent(entity);
            EntitiesSystem.Instance.Awake(entity);
            EntitiesSystem.Instance.StartUpdate(entity);
            return entity;
        }

        public T AddComponent<T>(long id) where T : Entity, new()
        {
            var entity = Create<T>(id, Scene.RouteId, false);
            AddComponent(entity);
            EntitiesSystem.Instance.Awake(entity);
            EntitiesSystem.Instance.StartUpdate(entity);
            return entity;
        }

        public void AddComponent(Entity component)
        {
            if (this == component)
            {
                Log.Error("Cannot add oneself to one's own components");
                return;
            }
            
            if (component.IsDisposed)
            {
                Log.Error($"component is Disposed {component.GetType().FullName}");
                return;
            }
            
            var type = component.GetType();
            component.Parent?.RemoveComponent(component, false);
            
            if (component is ISupportedMultiEntity multiEntity)
            {
                _multi ??= DictionaryPool<long, ISupportedMultiEntity>.Create();
                _multi.Add(component.Id, multiEntity);

                if (component is ISupportedDataBase)
                {
                    _multiDb ??= ListPool<Entity>.Create();
                    _multiDb.Add(component);
                }
            }
            else
            {
                if (_tree == null)
                {
                    _tree = DictionaryPool<Type, Entity>.Create();
                }
                else if(_tree.ContainsKey(type))
                {
                    Log.Error($"type:{type.FullName} If you want to add multiple components of the same type, please implement IMultiEntity");
                    return;
                }
                
                _tree.Add(type, component);

                if (component is ISupportedDataBase)
                {
                    _treeDb ??= ListPool<Entity>.Create();
                    _treeDb.Add(component);
                }
            }

            component.Parent = this;
            component.Scene = Scene;
        }
        
        #endregion

        #region GetComponent

        public T GetComponent<T>() where T : Entity, new()
        {
            return GetComponent(typeof(T)) as T;
        }
        
        public Entity GetComponent(Type componentType)
        {
            if (_tree == null)
            {
                return default;
            }

            return _tree.TryGetValue(componentType, out var component) ? component : default;
        }
        
        public T GetComponent<T>(long id) where T : ISupportedMultiEntity, new()
        {
            if (_multi == null)
            {
                return default;
            }

            return _multi.TryGetValue(id, out var entity) ? (T) entity : default;
        }

        #endregion

        #region RemoveComponent
        
        public void RemoveComponent<T>(bool isDispose = true) where T : Entity, new()
        {
            if (_tree == null || !_tree.TryGetValue(typeof(T), out var component))
            {
                return;
            }

            RemoveComponent(component, isDispose);
        }

        public void RemoveComponent<T>(long id, bool isDispose = true) where T : ISupportedMultiEntity, new()
        {
            if (_multi == null || !_multi.TryGetValue(id, out var component))
            {
                return;
            }

            RemoveComponent((Entity)component, isDispose);
        }

        public void RemoveComponent(Entity component, bool isDispose = true)
        {
            if (this == component)
            {
                return;
            }
            
            if (component is ISupportedMultiEntity)
            {
                if (_multi != null)
                {
#if TENGINE_NET
                    if (component is ISupportedDataBase && _multiDb != null)
                    {
                        _multiDb.Remove(component);

                        if (_multiDb.Count == 0)
                        {
                            _multiDb.Dispose();
                            _multiDb = null;
                        }
                    }
#endif
                    _multi.Remove(component.Id);

                    if (_multi.Count == 0)
                    {
                        _multi.Dispose();
                        _multi = null;
                    }
                }
            }
            else if (_tree != null)
            {
#if TENGINE_NET
                if (component is ISupportedDataBase && _treeDb != null)
                {
                    _treeDb.Remove(component);
            
                    if (_treeDb.Count == 0)
                    {
                        _treeDb.Dispose();
                        _treeDb = null;
                    }
                }
#endif
                _tree.Remove(component.GetType());
                
                if (_tree.Count == 0)
                {
                    _tree.Dispose();
                    _tree = null;
                }
            }

            if (isDispose)
            {
                component.Dispose();
            }
        }

        #endregion

        #region Deserialize

        public void Deserialize(Scene scene, bool resetId = false)
        {
            if (IsDisposed)
            {
                Log.Error($"component is Disposed {this.GetType().FullName}");
                return;
            }
            
            if (RuntimeId != 0)
            {
                return;
            }

            try
            {
#if TENGINE_NET
                RuntimeId = IdFactory.NextEntityId(scene.RouteId);
#else
                RuntimeId = IdFactory.NextRunTimeId();
#endif
                if (resetId)
                {
                    Id = RuntimeId;
                }

                Entities.Add(RuntimeId, this);

                if (_treeDb != null && _treeDb.Count > 0)
                {
                    _tree = DictionaryPool<Type, Entity>.Create();
                    foreach (var entity in _treeDb)
                    {
                        entity.Parent = this;
                        entity.Scene = scene;
                        entity.Deserialize(scene, resetId);
                        _tree.Add(entity.GetType(), entity);
                    }
                }

                if (_multiDb != null && _multiDb.Count > 0)
                {
                    _multi = DictionaryPool<long, ISupportedMultiEntity>.Create();
                    foreach (var entity in _multiDb)
                    {
                        entity.Parent = this;
                        entity.Scene = scene;
                        entity.Deserialize(scene, resetId);
                        _multi.Add(entity.Id, (ISupportedMultiEntity)entity);
                    }
                }
            }
            catch (Exception e)
            {
                if (RuntimeId != 0)
                {
                    Entities.Remove(RuntimeId);
                }

                Log.Error(e);
            }
        }

        #endregion

        #region Clone

        public Entity Clone()
        {
#if TENGINE_NET
            var entity = MongoHelper.Instance.Clone(this);
            entity.Deserialize(Scene, true);
            return entity;
#elif TENGINE_UNITY
            var entity = ProtoBufHelper.Clone(this);
            entity.Deserialize(Scene, true);
            return entity;
#endif
        }

        #endregion

        #region Dispose

        public virtual void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }

            var runtimeId = RuntimeId;
            RuntimeId = 0;

            if (_tree != null)
            {
                foreach (var (_, entity) in _tree)
                {
                    entity.Dispose();
                }

                _tree.Dispose();
                _tree = null;
            }
            
            if (_multi != null)
            {
                foreach (var (_, entity) in _multi)
                {
                    entity.Dispose();
                }
                
                _multi.Dispose();
                _multi = null;
            }
            
#if TENGINE_NET
            if (_treeDb != null)
            {
                foreach (var entity in _treeDb)
                {
                    entity.Dispose();
                }
                
                _treeDb.Dispose();
                _treeDb = null;
            }
            
            if (_multiDb != null)
            {
                foreach (var entity in _multiDb)
                {
                    entity.Dispose();
                }
                
                _multiDb.Dispose();
                _multiDb = null;
            }
#endif
            EntitiesSystem.Instance?.Destroy(this);

            if (Parent != null && Parent != this && !Parent.IsDisposed)
            {
                Parent.RemoveComponent(this, false);
                Parent = null;
            }

            Entities.Remove(runtimeId);
            Scene = null;
            Return(this);
        }

        #endregion
    }
}