using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using YooAsset;

namespace TEngine
{
    /// <summary>
    /// 资源句柄数据。
    /// </summary>
    public class AssetHandleData : IMemory
    {
        /// <summary>
        /// 资源操作句柄。
        /// </summary>
        private AssetOperationHandle _handle;

        /// <summary>
        /// 资源操作句柄。
        /// </summary>
        public AssetOperationHandle Handle => _handle;

        /// <summary>
        /// 标签。
        /// </summary>
        public string Tag { private set; get; }

        /// <summary>
        /// 释放资源句柄数据对象。
        /// </summary>
        public void Clear()
        {
            if (_handle != null)
            {
                _handle.Dispose();
            }

            _handle = null;

            Tag = string.Empty;
        }

        /// <summary>
        /// 从内存池中获取资源句柄数据。
        /// </summary>
        /// <param name="handle">资源操作句柄。</param>
        /// <param name="tag">标签。</param>
        /// <returns>资源句柄数据。</returns>
        public static AssetHandleData Alloc(AssetOperationHandle handle, string tag = "")
        {
            AssetHandleData assetHandleData = MemoryPool.Acquire<AssetHandleData>();
            assetHandleData._handle = handle;
            assetHandleData.Tag = tag;
            return assetHandleData;
        }

        /// <summary>
        /// 释放资源句柄数据到内存池。
        /// </summary>
        /// <param name="assetHandleData">释放资源句柄数据。</param>
        public static void Release(AssetHandleData assetHandleData)
        {
            if (assetHandleData == null)
            {
                Log.Fatal("Release AssetHandleData Failed !");
                return;
            }

            MemoryPool.Release(assetHandleData);
        }
    }

    /// <summary>
    /// 资源组数据。
    /// <remarks>DisposeGroup。</remarks>
    /// </summary>
    public class AssetGroup : IMemory
    {
        private readonly Dictionary<string,AssetHandleData> _assetHandleDataMap = new Dictionary<string,AssetHandleData>();
        
        /// <summary>
        /// 引用资源数据到资源组内。
        /// </summary>
        /// <param name="handle">资源操作句柄。</param>
        /// <param name="assetTag">资源标识。</param>
        /// <returns>是否注册成功。</returns>
        public bool Reference(AssetOperationHandle handle,string assetTag = "")
        {
            AssetHandleData handleData = AssetHandleData.Alloc(handle,assetTag);
            _assetHandleDataMap[handleData.Handle.GetAssetInfo().Address] = handleData;
            return true;
        }
        
        /// <summary>
        /// 引用资源数据到资源组内。
        /// </summary>
        /// <param name="address">资源定位地址。</param>
        /// <param name="handle">资源操作句柄。</param>
        /// <param name="assetTag">资源标识。</param>
        /// <returns>是否注册成功。</returns>
        public bool Reference(string address,AssetOperationHandle handle,string assetTag = "")
        {
            AssetHandleData handleData = AssetHandleData.Alloc(handle,assetTag);
            _assetHandleDataMap[address] = handleData;
            return true;
        }

        /// <summary>
        /// 从资源组内释放资源数据。
        /// </summary>
        /// <param name="assetTag">资源标签。</param>
        /// <returns>是否释放成功。</returns>
        public bool ReleaseByTag(string assetTag)
        {
            AssetHandleData founded = null;
            foreach (var assetHandleData in _assetHandleDataMap.Values)
            {
                if (assetHandleData.Tag == assetTag)
                {
                    founded = assetHandleData;
                    break;
                }
            }
            
            if (founded != null)
            {
                _assetHandleDataMap.Remove(founded.Handle.GetAssetInfo().Address);
                AssetHandleData.Release(founded);
                return true;
            }

            Log.Warning($"Release AssetHandleData Tag:{assetTag} Failed");
            return false;
        }
        
        /// <summary>
        /// 从资源组内释放资源数据。
        /// </summary>
        /// <param name="address">资源定位地址。</param>
        /// <returns>是否释放成功。</returns>
        public bool Release(string address)
        {
            if (_assetHandleDataMap.TryGetValue(address,out var assetHandleData))
            {
                _assetHandleDataMap.Remove(assetHandleData.Handle.GetAssetInfo().Address);
                AssetHandleData.Release(assetHandleData);
                return true;
            }
            Log.Warning($"Release AssetHandleData Address:{address} Failed");
            return false;
        }
        
        /// <summary>
        /// 从资源组内释放资源数据。
        /// </summary>
        /// <param name="handle">资源操作句柄。</param>
        /// <returns>是否释放成功。</returns>
        public bool Release(AssetOperationHandle handle)
        {
            AssetHandleData founded = null;
            foreach (var assetHandleData in _assetHandleDataMap.Values)
            {
                if (assetHandleData.Handle == handle)
                {
                    founded = assetHandleData;
                    break;
                }
            }
            
            if (founded != null)
            {
                _assetHandleDataMap.Remove(founded.Handle.GetAssetInfo().Address);
                AssetHandleData.Release(founded);
                return true;
            }

            Log.Warning($"Release AssetHandleData Handle:{handle} Failed");
            return false;
        }

        public void Clear()
        {
            var etr = _assetHandleDataMap.GetEnumerator();
            while (etr.MoveNext())
            {
                AssetHandleData assetHandleData = etr.Current.Value;
                AssetHandleData.Release(assetHandleData);
            }

            etr.Dispose();
            _assetHandleDataMap.Clear();
        }

        /// <summary>
        /// 从内存池中获取资源分组数据。
        /// </summary>
        /// <returns>资源分组数据。</returns>
        public static AssetGroup Alloc()
        {
            AssetGroup assetGroup = MemoryPool.Acquire<AssetGroup>();
            return assetGroup;
        }

        /// <summary>
        /// 将内存对象归还内存池。
        /// </summary>
        /// <param name="assetGroup"></param>
        public static void Release(AssetGroup assetGroup)
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
        /// <param name="parent">父节点位置。</param>
        /// <typeparam name="T">要加载资源的类型。</typeparam>
        /// <returns>资源实例。</returns>
        public T LoadAsset<T>(string assetName, Transform parent = null) where T : Object
        {
            if (string.IsNullOrEmpty(assetName))
            {
                Log.Error("Asset name is invalid.");
                return default;
            }
            
            if (_assetHandleDataMap.TryGetValue(assetName,out var assetHandleData))
            {
                if (typeof(T) == typeof(GameObject))
                {
                    GameObject ret = assetHandleData.Handle.InstantiateSync(parent);
                    return ret as T;
                }
                else
                {
                    return assetHandleData.Handle.AssetObject as T;
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
        public T LoadAsset<T>(string assetName,out AssetOperationHandle assetOperationHandle) where T : Object
        {
            assetOperationHandle = null;
            if (string.IsNullOrEmpty(assetName))
            {
                Log.Error("Asset name is invalid.");
                return default;
            }
            
            if (_assetHandleDataMap.TryGetValue(assetName,out var assetHandleData))
            {
                if (typeof(T) == typeof(GameObject))
                {
                    GameObject ret = assetHandleData.Handle.InstantiateSync();
                    return ret as T;
                }
                else
                {
                    return assetHandleData.Handle.AssetObject as T;
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
        public T LoadAsset<T>(string assetName, Transform parent,out AssetOperationHandle assetOperationHandle) where T : Object
        {
            assetOperationHandle = null;
            if (string.IsNullOrEmpty(assetName))
            {
                Log.Error("Asset name is invalid.");
                return default;
            }

            if (_assetHandleDataMap.TryGetValue(assetName,out var assetHandleData))
            {
                if (typeof(T) == typeof(GameObject))
                {
                    GameObject ret = assetHandleData.Handle.InstantiateSync(parent);
                    return ret as T;
                }
                else
                {
                    return assetHandleData.Handle.AssetObject as T;
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
        public async UniTask<T> LoadAssetAsync<T>(string assetName, CancellationToken cancellationToken,AssetOperationHandle assetOperationHandle = null) where T : Object
        {
            if (string.IsNullOrEmpty(assetName))
            {
                Log.Error("Asset name is invalid.");
                return default;
            }
            
            if (_assetHandleDataMap.TryGetValue(assetName,out var assetHandleData))
            {
                if (typeof(T) == typeof(GameObject))
                {
                    GameObject ret = assetHandleData.Handle.InstantiateSync();
                    return ret as T;
                }
                else
                {
                    return assetHandleData.Handle.AssetObject as T;
                }
            }
            
            AssetOperationHandle handle = GameModule.Resource.LoadAssetAsyncHandle<T>(assetName);

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
            return await LoadAssetAsync<GameObject>(assetName,cancellationToken);
        }
        
        /// <summary>
        /// 异步加载游戏物体。
        /// </summary>
        /// <param name="assetName">要加载的游戏物体名称。</param>
        /// <param name="cancellationToken">取消操作Token。</param>
        /// <param name="assetOperationHandle">资源操作句柄。</param>
        /// <returns>异步游戏物体实例。</returns>
        public async UniTask<GameObject> LoadGameObjectAsync(string assetName, CancellationToken cancellationToken,AssetOperationHandle assetOperationHandle)
        {
            return await LoadAssetAsync<GameObject>(assetName,cancellationToken,assetOperationHandle);
        }
    }
}