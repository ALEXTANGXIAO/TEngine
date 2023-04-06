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
        public static AssetHandleData Alloc(AssetOperationHandle handle, string tag)
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
    /// 资源分组数据。
    /// <remarks>DisposeGroup。</remarks>
    /// </summary>
    public class AssetGroup : IMemory
    {
        private Dictionary<string, AssetHandleData> _handles = new Dictionary<string, AssetHandleData>(16);

        /// <summary>
        /// 注册资源数据到资源组内。
        /// </summary>
        /// <param name="handle">资源操作句柄。</param>
        /// <param name="tag">资源标识。</param>
        /// <returns>是否注册成功。</returns>
        public bool Register(AssetOperationHandle handle, string tag = "ROOT")
        {
            if (_handles.TryGetValue(tag, out var handleData))
            {
                _handles.Remove(tag);
                AssetHandleData.Release(handleData);
                handleData = null;
            }

            handleData = AssetHandleData.Alloc(handle, tag);
            _handles.Add(tag, handleData);
            return true;
        }

        /// <summary>
        /// 从资源组内反注册资源数据。
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public bool UnRegister(string tag)
        {
            if (_handles.TryGetValue(tag, out var handleData))
            {
                _handles.Remove(tag);
                AssetHandleData.Release(handleData);
                return true;
            }

            Log.Warning($"UnRegister AssetHandleData Tag:{tag} Failed");
            return false;
        }

        public void Clear()
        {
            var etr = _handles.GetEnumerator();
            while (etr.MoveNext())
            {
                AssetHandleData assetHandleData = etr.Current.Value;
                AssetHandleData.Release(assetHandleData);
            }

            etr.Dispose();
            _handles.Clear();
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
        /// <param name="tag">要加载资源的标签名称。</param>
        /// <typeparam name="T">要加载资源的类型。</typeparam>
        /// <returns>资源实例。</returns>
        public T LoadAsset<T>(string assetName, string tag) where T : Object
        {
            if (string.IsNullOrEmpty(assetName))
            {
                Log.Error("Asset name is invalid.");
                return default;
            }

            AssetOperationHandle handle = YooAssets.LoadAssetSync<T>(assetName);

            Register(handle, tag);

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
        /// <param name="tag">要加载资源的标签名称。</param>
        /// <typeparam name="T">要加载资源的类型。</typeparam>
        /// <returns>资源实例。</returns>
        public T LoadAsset<T>(string assetName, Transform parent, string tag) where T : Object
        {
            if (string.IsNullOrEmpty(assetName))
            {
                Log.Error("Asset name is invalid.");
                return default;
            }

            AssetOperationHandle handle = YooAssets.LoadAssetSync<T>(assetName);

            Register(handle, tag);

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
        /// 异步加载资源实例。
        /// </summary>
        /// <param name="assetName">要加载的实例名称。</param>
        /// <param name="tag">要加载资源的标签名称。</param>
        /// <param name="cancellationToken">取消操作Token。</param>
        /// <returns>资源实实例。</returns>
        public async UniTask<T> LoadAssetAsync<T>(string assetName, string tag, CancellationToken cancellationToken) where T : Object
        {
            AssetOperationHandle handle = GameModule.Resource.LoadAssetAsyncHandle<GameObject>(assetName);

            Register(handle, tag);

            bool cancelOrFailed = await handle.ToUniTask(cancellationToken: cancellationToken).SuppressCancellationThrow();

            if (cancelOrFailed)
            {
                UnRegister(tag);

                return null;
            }

            return handle.AssetObject as T;
        }

        /// <summary>
        /// 异步加载游戏物体。
        /// </summary>
        /// <param name="assetName">要加载的游戏物体名称。</param>
        /// <param name="tag">要加载资源的标签名称。</param>
        /// <param name="cancellationToken">取消操作Token。</param>
        /// <returns>异步游戏物体实例。</returns>
        public async UniTask<GameObject> LoadGameObjectAsync(string assetName, string tag, CancellationToken cancellationToken)
        {
            AssetOperationHandle handle = GameModule.Resource.LoadAssetAsyncHandle<GameObject>(assetName);

            Register(handle, tag);

            bool cancelOrFailed = await handle.ToUniTask(cancellationToken: cancellationToken).SuppressCancellationThrow();

            if (cancelOrFailed)
            {
                UnRegister(tag);

                return null;
            }

            GameObject ret = handle.InstantiateSync();

            return ret;
        }
    }
}