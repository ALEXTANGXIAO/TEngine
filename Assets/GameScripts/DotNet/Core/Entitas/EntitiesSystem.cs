using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TEngine.DataStructure;
using TEngine.Core;

namespace TEngine
{
    public sealed class EntitiesSystem : Singleton<EntitiesSystem>, IUpdateSingleton
    {
        private readonly OneToManyList<int, Type> _assemblyList = new();
        private readonly Dictionary<Type, IAwakeSystem> _awakeSystems = new();
        private readonly Dictionary<Type, IUpdateSystem> _updateSystems = new();
        private readonly Dictionary<Type, IDestroySystem> _destroySystems = new();
        private readonly Dictionary<Type, IEntitiesSystem> _deserializeSystems = new();

        private readonly Queue<long> _updateQueue = new Queue<long>();

        protected override void OnLoad(int assemblyName)
        {
            foreach (var entitiesSystemType in AssemblyManager.ForEach(assemblyName, typeof(IEntitiesSystem)))
            {
                var entity = Activator.CreateInstance(entitiesSystemType);

                switch (entity)
                {
                    case IAwakeSystem iAwakeSystem:
                    {
                        _awakeSystems.Add(iAwakeSystem.EntitiesType(), iAwakeSystem);
                        break;
                    }
                    case IDestroySystem iDestroySystem:
                    {
                        _destroySystems.Add(iDestroySystem.EntitiesType(), iDestroySystem);
                        break;
                    }
                    case IDeserializeSystem iDeserializeSystem:
                    {
                        _deserializeSystems.Add(iDeserializeSystem.EntitiesType(), iDeserializeSystem);
                        break;
                    }
                    case IUpdateSystem iUpdateSystem:
                    {
                        _updateSystems.Add(iUpdateSystem.EntitiesType(), iUpdateSystem);
                        break;
                    }
                }

                _assemblyList.Add(assemblyName, entitiesSystemType);
            }
        }

        protected override void OnUnLoad(int assemblyName)
        {
            if (!_assemblyList.TryGetValue(assemblyName, out var assembly))
            {
                return;
            }

            _assemblyList.RemoveByKey(assemblyName);
            
            foreach (var type in assembly)
            {
                _awakeSystems.Remove(type);
                _updateSystems.Remove(type);
                _destroySystems.Remove(type);
                _deserializeSystems.Remove(type);
            }
        }

        public void Awake<T>(T entity) where T : Entity
        {
            var type = entity.GetType();
            
            if (!_awakeSystems.TryGetValue(type, out var awakeSystem))
            {
                return;
            }

            try
            {
                awakeSystem.Invoke(entity);
            }
            catch (Exception e)
            {
                Log.Error($"{type.Name} Error {e}");
            }
        }

        public void Destroy<T>(T entity) where T : Entity
        {
            var type = entity.GetType();

            if (!_destroySystems.TryGetValue(type, out var system))
            {
                return;
            }

            try
            {
                system.Invoke(entity);
            }
            catch (Exception e)
            {
                Log.Error($"{type.Name} Error {e}");
            }
        }
        
        public void Deserialize<T>(T entity) where T : Entity
        {
            var type = entity.GetType();
            
            if (!_deserializeSystems.TryGetValue(type, out var system))
            {
                return;
            }

            try
            {
                system.Invoke(entity);
            }
            catch (Exception e)
            {
                Log.Error($"{type.Name} Error {e}");
            }
        }

        public void StartUpdate(Entity entity)
        {
            if (!_updateSystems.ContainsKey(entity.GetType()))
            {
                return;
            }

            _updateQueue.Enqueue(entity.RuntimeId);
        }

        public void Update()
        {
            var updateQueueCount = _updateQueue.Count;

            while (updateQueueCount-- > 0)
            {
                var runtimeId = _updateQueue.Dequeue();
                var entity = Entity.GetEntity(runtimeId);

                if (entity == null || entity.IsDisposed)
                {
                    continue;
                }

                var type = entity.GetType();

                if (!_updateSystems.TryGetValue(type, out var updateSystem))
                {
                    continue;
                }

                _updateQueue.Enqueue(runtimeId);

                try
                {
                    updateSystem.Invoke(entity);
                }
                catch (Exception e)
                {
                    Log.Error($"{type} Error {e}");
                }
            }
        }

        public override void Dispose()
        {
            _assemblyList.Clear();
            _awakeSystems.Clear();
            _updateSystems.Clear();
            _destroySystems.Clear();
            _deserializeSystems.Clear();
            AssemblyManager.OnLoadAssemblyEvent -= OnLoad;
            AssemblyManager.OnUnLoadAssemblyEvent -= OnUnLoad;
            base.Dispose();
        }
    }
}