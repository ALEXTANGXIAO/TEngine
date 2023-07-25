using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using YooAsset;

namespace TEngine
{
    /// <summary>
    /// 资源组数据。
    /// <remarks>DisposeGroup。</remarks>
    /// </summary>
    public class LruAssetGroup : IMemory
    {
        private const int DefaultCapacity = 48;
        private readonly LruCacheTable<string, AssetHandleData> _lruCacheTable;

        public LruAssetGroup()
        {
            _lruCacheTable = new LruCacheTable<string, AssetHandleData>(DefaultCapacity, OnAdd, OnRemove);
        }

        private void OnAdd(AssetHandleData assetHandleData)
        {
        }

        private void OnRemove(AssetHandleData assetHandleData)
        {
            MemoryPool.Release(assetHandleData);
        }

        /// <summary>
        /// 引用资源数据到资源组内。
        /// </summary>
        /// <param name="handle">资源操作句柄。</param>
        /// <param name="assetTag">资源标识。</param>
        /// <returns>是否注册成功。</returns>
        public bool Reference(AssetOperationHandle handle, string assetTag = "")
        {
            AssetHandleData handleData = AssetHandleData.Alloc(handle, assetTag);
            _lruCacheTable.Put(handleData.Handle.GetAssetInfo().Address,handleData);
            return true;
        }

        /// <summary>
        /// 从资源组内释放资源数据。
        /// </summary>
        /// <param name="handle">资源操作句柄。</param>
        /// <returns>是否释放成功。</returns>
        public bool Release(AssetOperationHandle handle)
        {
            _lruCacheTable.Remove(handle.GetAssetInfo().Address);
            return true;
        }

        public void Clear()
        {
            _lruCacheTable.Clear();
        }

        /// <summary>
        /// 从内存池中获取资源分组数据。
        /// </summary>
        /// <returns>资源分组数据。</returns>
        public static LruAssetGroup Alloc()
        {
            LruAssetGroup assetGroup = MemoryPool.Acquire<LruAssetGroup>();

            return assetGroup;
        }

        /// <summary>
        /// 将内存对象归还内存池。
        /// </summary>
        /// <param name="assetGroup"></param>
        public static void Release(LruAssetGroup assetGroup)
        {
            if (assetGroup == null)
            {
                Log.Fatal("Release AssetGroup Failed !");
                return;
            }

            MemoryPool.Release(assetGroup);
        }

        /// <summary>
        /// 同步加载资源。
        /// </summary>
        /// <param name="assetName">要加载资源的名称。</param>
        /// <typeparam name="T">要加载资源的类型。</typeparam>
        /// <returns>资源实例。</returns>
        public T LoadAsset<T>(string assetName) where T : Object
        {
            if (string.IsNullOrEmpty(assetName))
            {
                Log.Error("Asset name is invalid.");
                return default;
            }

            var data = _lruCacheTable.Get(assetName);
            if (data != null)
            {
                if (typeof(T) == typeof(GameObject))
                {
                    GameObject ret = data.Handle.InstantiateSync();
                    return ret as T;
                }
                else
                {
                    return data.Handle.AssetObject as T;
                }
            }

            AssetOperationHandle handle = GameModule.Resource.LoadAssetGetOperation<T>(assetName);

            Reference(handle);

            if (typeof(T) == typeof(GameObject))
            {
                GameObject ret = handle.InstantiateSync();
                return ret as T;
            }
            else
            {
                return handle.AssetObject as T;
            }
        }

        /// <summary>
        /// 同步加载资源。
        /// </summary>
        /// <param name="assetName">要加载资源的名称。</param>
        /// <param name="parent">父节点位置。</param>
        /// <typeparam name="T">要加载资源的类型。</typeparam>
        /// <returns>资源实例。</returns>
        public T LoadAsset<T>(string assetName, Transform parent) where T : Object
        {
            if (string.IsNullOrEmpty(assetName))
            {
                Log.Error("Asset name is invalid.");
                return default;
            }

            var data = _lruCacheTable.Get(assetName);
            if (data != null)
            {
                if (typeof(T) == typeof(GameObject))
                {
                    GameObject ret = data.Handle.InstantiateSync();
                    return ret as T;
                }
                else
                {
                    return data.Handle.AssetObject as T;
                }
            }
            
            AssetOperationHandle handle = GameModule.Resource.LoadAssetGetOperation<T>(assetName);

            Reference(handle);

            if (typeof(T) == typeof(GameObject))
            {
                GameObject ret = handle.InstantiateSync(parent);
                return ret as T;
            }
            else
            {
                return handle.AssetObject as T;
            }
        }

        /// <summary>
        /// 同步加载资源。
        /// </summary>
        /// <param name="assetName">要加载资源的名称。</param>
        /// <typeparam name="T">要加载资源的类型。</typeparam>
        /// <param name="assetOperationHandle">资源操作句柄。</param>
        /// <returns>资源实例。</returns>
        public T LoadAsset<T>(string assetName, out AssetOperationHandle assetOperationHandle) where T : Object
        {
            assetOperationHandle = null;
            if (string.IsNullOrEmpty(assetName))
            {
                Log.Error("Asset name is invalid.");
                return default;
            }

            var data = _lruCacheTable.Get(assetName);
            if (data != null)
            {
                if (typeof(T) == typeof(GameObject))
                {
                    GameObject ret = data.Handle.InstantiateSync();
                    return ret as T;
                }
                else
                {
                    return data.Handle.AssetObject as T;
                }
            }
            

            assetOperationHandle = GameModule.Resource.LoadAssetGetOperation<T>(assetName);

            Reference(assetOperationHandle);

            if (typeof(T) == typeof(GameObject))
            {
                GameObject ret = assetOperationHandle.InstantiateSync();
                return ret as T;
            }
            else
            {
                return assetOperationHandle.AssetObject as T;
            }
        }

        /// <summary>
        /// 同步加载资源。
        /// </summary>
        /// <param name="assetName">要加载资源的名称。</param>
        /// <param name="parent">父节点位置。</param>
        /// <param name="assetOperationHandle">资源操作句柄。</param>
        /// <typeparam name="T">要加载资源的类型。</typeparam>
        /// <returns>资源实例。</returns>
        public T LoadAsset<T>(string assetName, Transform parent, out AssetOperationHandle assetOperationHandle)
            where T : Object
        {
            assetOperationHandle = null;
            if (string.IsNullOrEmpty(assetName))
            {
                Log.Error("Asset name is invalid.");
                return default;
            }
            
            var data = _lruCacheTable.Get(assetName);
            if (data != null)
            {
                if (typeof(T) == typeof(GameObject))
                {
                    GameObject ret = data.Handle.InstantiateSync();
                    return ret as T;
                }
                else
                {
                    return data.Handle.AssetObject as T;
                }
            }

            assetOperationHandle = GameModule.Resource.LoadAssetGetOperation<T>(assetName);

            Reference(assetOperationHandle);

            if (typeof(T) == typeof(GameObject))
            {
                GameObject ret = assetOperationHandle.InstantiateSync(parent);
                return ret as T;
            }
            else
            {
                return assetOperationHandle.AssetObject as T;
            }
        }

        /// <summary>
        /// 异步加载资源实例。
        /// </summary>
        /// <param name="assetName">要加载的实例名称。</param>
        /// <param name="cancellationToken">取消操作Token。</param>
        /// <param name="assetOperationHandle">资源操作句柄。</param>
        /// <returns>资源实实例。</returns>
        // ReSharper disable once UnusedParameter.Global
        // ReSharper disable once RedundantAssignment
        public async UniTask<T> LoadAssetAsync<T>(string assetName, CancellationToken cancellationToken,
            AssetOperationHandle assetOperationHandle = null) where T : Object
        {
            
            if (string.IsNullOrEmpty(assetName))
            {
                Log.Error("Asset name is invalid.");
                return default;
            }
            
            var data = _lruCacheTable.Get(assetName);
            if (data != null)
            {
                if (typeof(T) == typeof(GameObject))
                {
                    GameObject ret = data.Handle.InstantiateSync();
                    return ret as T;
                }
                else
                {
                    return data.Handle.AssetObject as T;
                }
            }
            
            AssetOperationHandle handle = GameModule.Resource.LoadAssetAsyncHandle<GameObject>(assetName);

            Reference(handle);

            bool cancelOrFailed = await handle.ToUniTask().AttachExternalCancellation(cancellationToken).SuppressCancellationThrow();

            if (cancelOrFailed)
            {
                Release(handle);

                return null;
            }

            assetOperationHandle = handle;

            if (typeof(T) == typeof(GameObject))
            {
                GameObject ret = handle.InstantiateSync();
                return ret as T;
            }
            else
            {
                return handle.AssetObject as T;
            }
        }

        /// <summary>
        /// 异步加载游戏物体。
        /// </summary>
        /// <param name="assetName">要加载的游戏物体名称。</param>
        /// <param name="cancellationToken">取消操作Token。</param>
        /// <returns>异步游戏物体实例。</returns>
        public async UniTask<GameObject> LoadGameObjectAsync(string assetName, CancellationToken cancellationToken)
        {
            return await LoadAssetAsync<GameObject>(assetName, cancellationToken);
        }

        /// <summary>
        /// 异步加载游戏物体。
        /// </summary>
        /// <param name="assetName">要加载的游戏物体名称。</param>
        /// <param name="cancellationToken">取消操作Token。</param>
        /// <param name="assetOperationHandle">资源操作句柄。</param>
        /// <returns>异步游戏物体实例。</returns>
        public async UniTask<GameObject> LoadGameObjectAsync(string assetName, CancellationToken cancellationToken,
            AssetOperationHandle assetOperationHandle)
        {
            return await LoadAssetAsync<GameObject>(assetName, cancellationToken, assetOperationHandle);
        }
    }
}