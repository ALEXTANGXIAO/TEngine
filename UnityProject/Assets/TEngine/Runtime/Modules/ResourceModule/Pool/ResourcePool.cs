using System;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;

namespace TEngine
{
    /// <summary>
    /// 游戏对象池系统。
    /// <remarks>用法 SpawnHandle handle = ResourcePool.SpawnAsync("Cube");</remarks>
    /// </summary>
    public static class ResourcePool
    {
        private static bool _isInitialize = false;
        private static readonly List<Spawner> Spawners = new List<Spawner>();
        private static GameObject _root;
        private static Spawner _defaultSpawner = null;

        /// <summary>
        /// 默认Package对象生成器。
        /// </summary>
        private static Spawner DefaultSpawner => _defaultSpawner ??= CreateSpawner();

        /// <summary>
        /// 初始化游戏对象池系统
        /// </summary>
        internal static void Initialize(GameObject root)
        {
            if (_isInitialize)
                throw new Exception($"{nameof(ResourcePool)} is initialized !");

            if (_isInitialize == false)
            {
                _root = root;
                _isInitialize = true;
                Log.Info($"{nameof(ResourcePool)} Initialize !");
            }
        }

        /// <summary>
        /// 销毁游戏对象池系统
        /// </summary>
        internal static void Destroy()
        {
            if (_isInitialize)
            {
                foreach (var spawner in Spawners)
                {
                    spawner.Destroy();
                }

                Spawners.Clear();

                _isInitialize = false;

                Log.Info($"{nameof(ResourcePool)} destroy all !");
            }
        }

        /// <summary>
        /// 更新游戏对象池系统
        /// </summary>
        internal static void Update()
        {
            if (_isInitialize)
            {
                foreach (var spawner in Spawners)
                {
                    spawner.Update();
                }
            }
        }

        /// <summary>
        /// 创建游戏对象生成器
        /// </summary>
        /// <param name="packageName">资源包名称</param>
        public static Spawner CreateSpawner(string packageName = "")
        {
            if (string.IsNullOrEmpty(packageName))
            {
                packageName = GameModule.Resource.packageName;
            }

            // 获取资源包
            var assetPackage = YooAssets.GetPackage(packageName);
            if (assetPackage == null)
                throw new Exception($"Not found asset package : {packageName}");

            // 检测资源包初始化状态
            if (assetPackage.InitializeStatus == EOperationStatus.None)
                throw new Exception($"Asset package {packageName} not initialize !");
            if (assetPackage.InitializeStatus == EOperationStatus.Failed)
                throw new Exception($"Asset package {packageName} initialize failed !");

            if (HasSpawner(packageName))
                return GetSpawner(packageName);

            Spawner spawner = new Spawner(_root, assetPackage);
            Spawners.Add(spawner);
            return spawner;
        }

        /// <summary>
        /// 获取游戏对象生成器。
        /// </summary>
        /// <param name="packageName">资源包名称。</param>
        public static Spawner GetSpawner(string packageName = "")
        {
            if (string.IsNullOrEmpty(packageName))
            {
                packageName = GameModule.Resource.packageName;
            }

            foreach (var spawner in Spawners)
            {
                if (spawner.PackageName == packageName)
                    return spawner;
            }

            Log.Warning($"Not found spawner : {packageName}");
            return null;
        }

        /// <summary>
        /// 检测游戏对象生成器是否存在。
        /// </summary>
        /// <param name="packageName">资源包名称。</param>
        public static bool HasSpawner(string packageName = "")
        {
            if (string.IsNullOrEmpty(packageName))
            {
                packageName = GameModule.Resource.packageName;
            }

            foreach (var spawner in Spawners)
            {
                if (spawner.PackageName == packageName)
                    return true;
            }

            return false;
        }

        #region 操作接口

        /// <summary>
        /// 销毁所有对象池及其资源。
        /// </summary>
        /// <param name="includeAll">销毁所有对象池，包括常驻对象池。</param>
        public static void DestroyAll(bool includeAll)
        {
            DefaultSpawner.DestroyAll(includeAll);
        }


        /// <summary>
        /// 异步创建指定资源的游戏对象池。
        /// </summary>
        /// <param name="location">资源定位地址。</param>
        /// <param name="dontDestroy">资源常驻不销毁。</param>
        /// <param name="initCapacity">对象池的初始容量。</param>
        /// <param name="maxCapacity">对象池的最大容量。</param>
        /// <param name="destroyTime">静默销毁时间（注意：小于零代表不主动销毁）。</param>
        public static CreatePoolOperation CreateGameObjectPoolAsync(string location, bool dontDestroy = false, int initCapacity = 0, int maxCapacity = int.MaxValue,
            float destroyTime = -1f)
        {
            return DefaultSpawner.CreateGameObjectPoolAsync(location, dontDestroy, initCapacity, maxCapacity, destroyTime);
        }

        /// <summary>
        /// 同步创建指定资源的游戏对象池。
        /// </summary>
        /// <param name="location">资源定位地址。</param>
        /// <param name="dontDestroy">资源常驻不销毁。</param>
        /// <param name="initCapacity">对象池的初始容量。</param>
        /// <param name="maxCapacity">对象池的最大容量。</param>
        /// <param name="destroyTime">静默销毁时间（注意：小于零代表不主动销毁）。</param>
        public static CreatePoolOperation CreateGameObjectPoolSync(string location, bool dontDestroy = false, int initCapacity = 0, int maxCapacity = int.MaxValue,
            float destroyTime = -1f)
        {
            return DefaultSpawner.CreateGameObjectPoolSync(location, dontDestroy, initCapacity, maxCapacity, destroyTime);
        }

        /// <summary>
        /// 异步实例化一个游戏对象。
        /// </summary>
        /// <param name="location">资源定位地址。</param>
        /// <param name="forceClone">强制克隆游戏对象，忽略缓存池里的对象。</param>
        /// <param name="userDatas">用户自定义数据。</param>
        public static SpawnHandle SpawnAsync(string location, bool forceClone = false, params System.Object[] userDatas)
        {
            return DefaultSpawner.SpawnAsync(location, null, Vector3.zero, Quaternion.identity, forceClone, userDatas);
        }

        /// <summary>
        /// 异步实例化一个游戏对象。
        /// </summary>
        /// <param name="location">资源定位地址。</param>
        /// <param name="parent">父物体。</param>
        /// <param name="forceClone">强制克隆游戏对象，忽略缓存池里的对象。</param>
        /// <param name="userDatas">用户自定义数据。</param>
        public static SpawnHandle SpawnAsync(string location, Transform parent, bool forceClone = false, params System.Object[] userDatas)
        {
            return DefaultSpawner.SpawnAsync(location, parent, Vector3.zero, Quaternion.identity, forceClone, userDatas);
        }

        /// <summary>
        /// 异步实例化一个游戏对象。
        /// </summary>
        /// <param name="location">资源定位地址。</param>
        /// <param name="parent">父物体。</param>
        /// <param name="position">世界坐标。</param>
        /// <param name="rotation">世界角度。</param>
        /// <param name="forceClone">强制克隆游戏对象，忽略缓存池里的对象。</param>
        /// <param name="userDatas">用户自定义数据。</param>
        public static SpawnHandle SpawnAsync(string location, Transform parent, Vector3 position, Quaternion rotation, bool forceClone = false, params System.Object[] userDatas)
        {
            return DefaultSpawner.SpawnAsync(location, parent, position, rotation, forceClone, userDatas);
        }

        /// <summary>
        /// 同步实例化一个游戏对象。
        /// </summary>
        /// <param name="location">资源定位地址。</param>
        /// <param name="forceClone">强制克隆游戏对象，忽略缓存池里的对象。</param>
        /// <param name="userDatas">用户自定义数据。</param>
        public static SpawnHandle SpawnSync(string location, bool forceClone = false, params System.Object[] userDatas)
        {
            return DefaultSpawner.SpawnSync(location, null, Vector3.zero, Quaternion.identity, forceClone, userDatas);
        }

        /// <summary>
        /// 同步实例化一个游戏对象。
        /// </summary>
        /// <param name="location">资源定位地址。</param>
        /// <param name="parent">父物体</param>
        /// <param name="forceClone">强制克隆游戏对象，忽略缓存池里的对象。</param>
        /// <param name="userDatas">用户自定义数据。</param>
        public static SpawnHandle SpawnSync(string location, Transform parent, bool forceClone = false, params System.Object[] userDatas)
        {
            return DefaultSpawner.SpawnAsync(location, parent, forceClone, userDatas);
        }

        /// <summary>
        /// 同步实例化一个游戏对象。
        /// </summary>
        /// <param name="location">资源定位地址。</param>
        /// <param name="parent">父物体。</param>
        /// <param name="position">世界坐标。</param>
        /// <param name="rotation">世界角度。</param>
        /// <param name="forceClone">强制克隆游戏对象，忽略缓存池里的对象。</param>
        /// <param name="userDatas">用户自定义数据。</param>
        public static SpawnHandle SpawnSync(string location, Transform parent, Vector3 position, Quaternion rotation, bool forceClone = false, params System.Object[] userDatas)
        {
            return DefaultSpawner.SpawnSync(location, parent, position, rotation, forceClone, userDatas);
        }

        #endregion
    }
}