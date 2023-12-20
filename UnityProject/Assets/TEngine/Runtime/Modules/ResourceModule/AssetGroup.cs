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
        /// 自定义资源标签。
        /// <remarks>不同于AssetTag。</remarks>
        /// </summary>
        public string Tag { private set; get; }

        /// <summary>
        /// 释放资源句柄数据对象。
        /// </summary>
        public void Clear()
        {
            if (_handle is { IsValid: true })
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

        #region 引用资源数据
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
        #endregion

        #region 释放资源数据
        private readonly List<AssetHandleData> _tempResult = new List<AssetHandleData>();

        /// <summary>
        /// 从资源组内释放资源数据。
        /// </summary>
        /// <param name="tag">自定义资源标签。</param>
        /// <returns>是否释放成功。</returns>
        public bool ReleaseByTag(string tag)
        {
            _tempResult.Clear();
            foreach (var assetHandleData in _assetHandleDataMap.Values)
            {
                if (assetHandleData.Tag == tag)
                {
                    _tempResult.Add(assetHandleData);
                }
            }
            
            if (_tempResult.Count > 0)
            {
                foreach (var founded in _tempResult)
                {
                    _assetHandleDataMap.Remove(founded.Handle.GetAssetInfo().Address);
                    AssetHandleData.Release(founded);
                }
                return true;
            }

            Log.Warning($"Release AssetHandleData Tag:{tag} Failed");
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
        #endregion

        #region 内存池接口
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
        #endregion

        /// <summary>
        /// 同步加载资源。
        /// </summary>
        /// <param name="location">资源的定位地址。</param>
        /// <param name="parent">父节点位置。</param>
        /// <param name="needInstance">是否需要实例化。</param>
        /// <param name="packageName">指定资源包的名称。不传使用默认资源包</param>
        /// <typeparam name="T">要加载资源的类型。</typeparam>
        /// <returns>资源实例。</returns>
        public T LoadAsset<T>(string location, bool needInstance = true,string packageName = "",Transform parent = null) where T : Object
        {
            if (string.IsNullOrEmpty(location))
            {
                Log.Error("Asset name is invalid.");
                return default;
            }
            
            if (_assetHandleDataMap.TryGetValue(location,out var assetHandleData))
            {
                if (typeof(T) == typeof(GameObject) && needInstance)
                {
                    GameObject ret = assetHandleData.Handle.InstantiateSync(parent);
                    return ret as T;
                }
                else
                {
                    return assetHandleData.Handle.AssetObject as T;
                }
            }

            AssetOperationHandle handle;
            if (string.IsNullOrEmpty(packageName))
            {
                handle = YooAssets.LoadAssetSync<T>(location);
            }
            else
            {
                var package = YooAssets.GetPackage(packageName);
                handle = package.LoadAssetSync<T>(location);
            }

            Reference(handle);

            if (typeof(T) == typeof(GameObject) && needInstance)
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
        /// 异步加载资源。
        /// </summary>
        /// <param name="location">资源的定位地址。</param>
        /// <param name="cancellationToken">取消操作Token。</param>
        /// <param name="needInstance">是否需要实例化。</param>
        /// <param name="packageName">指定资源包的名称。不传使用默认资源包。</param>
        /// <param name="parent">资源实例父节点。</param>
        /// <typeparam name="T">要加载资源的类型。</typeparam>
        /// <returns>异步资源实例。</returns>
        public async UniTask<T> LoadAssetAsync<T>(string location, CancellationToken cancellationToken = default,
            bool needInstance = true, string packageName = "", Transform parent = null) where T : Object
        {
            if (string.IsNullOrEmpty(location))
            {
                Log.Error("Asset name is invalid.");
                return default;
            }
            
            if (_assetHandleDataMap.TryGetValue(location,out var assetHandleData))
            {
                if (typeof(T) == typeof(GameObject) && needInstance)
                {
                    GameObject ret = assetHandleData.Handle.InstantiateSync();
                    return ret as T;
                }
                else
                {
                    return assetHandleData.Handle.AssetObject as T;
                }
            }
            
            AssetOperationHandle handle;
            if (string.IsNullOrEmpty(packageName))
            {
                handle = YooAssets.LoadAssetSync<T>(location);
            }
            else
            {
                var package = YooAssets.GetPackage(packageName);
                handle = package.LoadAssetSync<T>(location);
            }

            Reference(handle);

            bool cancelOrFailed = await handle.ToUniTask().AttachExternalCancellation(cancellationToken).SuppressCancellationThrow();

            if (cancelOrFailed)
            {
                Release(handle);

                return null;
            }
            
            if (typeof(T) == typeof(GameObject) && needInstance)
            {
                GameObject ret = handle.InstantiateSync(parent);
                return ret as T;
            }
            else
            {
                return handle.AssetObject as T;
            }
        }
    }
}