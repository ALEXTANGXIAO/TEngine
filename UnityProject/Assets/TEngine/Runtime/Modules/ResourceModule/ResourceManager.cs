using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using YooAsset;

namespace TEngine
{
    /// <summary>
    /// 资源管理模块。
    /// </summary>
    [UpdateModule]
    internal partial class ResourceManager : ModuleImp, IResourceManager
    {
        #region Propreties

        /// <summary>
        /// 资源包名称。
        /// </summary>
        public string PackageName { get; set; } = "DefaultPackage";

        /// <summary>
        /// 默认资源包。
        /// </summary>
        public ResourcePackage DefaultPackage { private set; get; }

        /// <summary>
        /// 获取当前资源适用的游戏版本号。
        /// </summary>
        // ReSharper disable once ConvertToAutoProperty
        public string ApplicableGameVersion => _applicableGameVersion;

        private string _applicableGameVersion;

        /// <summary>
        /// 获取当前内部资源版本号。
        /// </summary>
        // ReSharper disable once ConvertToAutoProperty
        public int InternalResourceVersion => _internalResourceVersion;

        private int _internalResourceVersion;

        /// <summary>
        /// 同时下载的最大数目。
        /// </summary>
        public int DownloadingMaxNum { get; set; }

        /// <summary>
        /// 失败重试最大数目。
        /// </summary>
        public int FailedTryAgain { get; set; }

        /// <summary>
        /// 获取资源只读区路径。
        /// </summary>
        public string ReadOnlyPath => _readOnlyPath;

        private string _readOnlyPath;

        /// <summary>
        /// 获取资源读写区路径。
        /// </summary>
        public string ReadWritePath => _readWritePath;

        private string _readWritePath;

        /// <summary>
        /// 资源系统运行模式。
        /// </summary>
        public EPlayMode PlayMode { get; set; }

        /// <summary>
        /// 下载文件校验等级。
        /// </summary>
        public EVerifyLevel VerifyLevel { get; set; }

        /// <summary>
        /// 设置异步系统参数，每帧执行消耗的最大时间切片（单位：毫秒）。
        /// </summary>
        public long Milliseconds { get; set; }

        /// <summary>
        /// 获取游戏框架模块优先级。
        /// </summary>
        /// <remarks>优先级较高的模块会优先轮询，并且关闭操作会后进行。</remarks>
        internal override int Priority => 4;

        /// <summary>
        /// 资源缓存表容量。
        /// </summary>
        public int ARCTableCapacity { get; set; }

        #endregion

        #region 生命周期

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
            ResourcePool.Update();
        }

        internal override void Shutdown()
        {
#if !UNITY_WEBGL
            YooAssets.Destroy();      
#endif
            ResourcePool.Destroy();
        }

        private void ReleaseAllHandle()
        {
            var iter = _releaseMaps.Values.GetEnumerator();
            while (iter.MoveNext())
            {
                AssetOperationHandle handle = iter.Current;
                if (handle is { IsValid: true })
                {
                    handle.Dispose();
                    handle = null;
                }
            }
            iter.Dispose();
            _releaseMaps.Clear();

            iter = _operationHandlesMaps.Values.GetEnumerator();
            while (iter.MoveNext())
            {
                AssetOperationHandle handle = iter.Current;
                if (handle is { IsValid: true })
                {
                    handle.Dispose();
                    handle = null;
                }
            }
            iter.Dispose();
            _operationHandlesMaps.Clear();
            
            _arcCacheTable = new ArcCacheTable<string, AssetOperationHandle>(ARCTableCapacity, OnAddAsset, OnRemoveAsset);
        }

        #endregion

        #region 设置接口

        /// <summary>
        /// 设置资源只读区路径。
        /// </summary>
        /// <param name="readOnlyPath">资源只读区路径。</param>
        public void SetReadOnlyPath(string readOnlyPath)
        {
            if (string.IsNullOrEmpty(readOnlyPath))
            {
                throw new GameFrameworkException("Read-only path is invalid.");
            }

            _readOnlyPath = readOnlyPath;
        }

        /// <summary>
        /// 设置资源读写区路径。
        /// </summary>
        /// <param name="readWritePath">资源读写区路径。</param>
        public void SetReadWritePath(string readWritePath)
        {
            if (string.IsNullOrEmpty(readWritePath))
            {
                throw new GameFrameworkException("Read-write path is invalid.");
            }

            _readWritePath = readWritePath;
        }

        #endregion

        private Dictionary<string, AssetOperationHandle> _releaseMaps;

        private Dictionary<string, AssetOperationHandle> _operationHandlesMaps;

        private ArcCacheTable<string, AssetOperationHandle> _arcCacheTable;


        private void OnAddAsset(string location, AssetOperationHandle handle)
        {
            _operationHandlesMaps[location] = handle;
            if (_releaseMaps.ContainsKey(location))
            {
                _releaseMaps.Remove(location);
            }
        }

        private void OnRemoveAsset(string location, AssetOperationHandle handle)
        {
            if (_operationHandlesMaps.ContainsKey(location))
            {
                _operationHandlesMaps.Remove(location);
            }

            _releaseMaps[location] = handle;
            GameModule.Resource.UnloadUnusedAssets(performGCCollect: false);
        }

        /// <summary>
        /// 从缓存中获取同步资源句柄。
        /// </summary>
        /// <param name="location">资源定位地址。</param>
        /// <param name="needCache">是否需要缓存。</param>
        /// <typeparam name="T">资源类型。</typeparam>
        /// <returns>资源句柄。</returns>
        private AssetOperationHandle GetHandleSync<T>(string location, bool needCache = false) where T : Object
        {
            if (!needCache)
            {
                return YooAssets.LoadAssetSync<T>(location);
            }
            AssetOperationHandle handle = null;
            // 尝试从从ARC缓存表取出对象。
            handle = _arcCacheTable.GetCache(location);

            if (handle == null)
            {
                handle = YooAssets.LoadAssetSync<T>(location);
            }

            // 对象推入ARC缓存表。
            _arcCacheTable.PutCache(location, handle);
            return handle;
        }

        /// <summary>
        /// 从缓存中获取异步资源句柄。
        /// </summary>
        /// <param name="location">资源定位地址。</param>
        /// <param name="needCache">是否需要缓存。</param>
        /// <typeparam name="T">资源类型。</typeparam>
        /// <returns>资源句柄。</returns>
        private AssetOperationHandle GetHandleAsync<T>(string location, bool needCache = false) where T : Object
        {
            if (!needCache)
            {
                return YooAssets.LoadAssetAsync<T>(location);
            }
            AssetOperationHandle handle = null;
            // 尝试从从ARC缓存表取出对象。
            handle = _arcCacheTable.GetCache(location);

            if (handle == null)
            {
                handle = YooAssets.LoadAssetAsync<T>(location);
            }

            // 对象推入ARC缓存表。
            _arcCacheTable.PutCache(location, handle);
            return handle;
        }

        /// <summary>
        /// 初始化资源模块。
        /// </summary>
        public void Initialize()
        {
            // 初始化资源系统
            YooAssets.Initialize(new AssetsLogger());
            YooAssets.SetOperationSystemMaxTimeSlice(Milliseconds);
            YooAssets.SetCacheSystemCachedFileVerifyLevel(VerifyLevel);

            // 创建默认的资源包
            string packageName = PackageName;
            var defaultPackage = YooAssets.TryGetPackage(packageName);
            if (defaultPackage == null)
            {
                defaultPackage = YooAssets.CreatePackage(packageName);
                YooAssets.SetDefaultPackage(defaultPackage);
            }

            ResourcePool.Initialize(GameModule.Get<ResourceModule>().gameObject);

            _releaseMaps ??= new Dictionary<string, AssetOperationHandle>(ARCTableCapacity);
            _operationHandlesMaps ??= new Dictionary<string, AssetOperationHandle>(ARCTableCapacity);
            _arcCacheTable ??= new ArcCacheTable<string, AssetOperationHandle>(ARCTableCapacity, OnAddAsset, OnRemoveAsset);
        }

        /// <summary>
        /// 初始化资源包裹。
        /// </summary>
        /// <returns>初始化资源包裹操作句柄。</returns>
        public InitializationOperation InitPackage()
        {
            // 创建默认的资源包
            string packageName = PackageName;
            var package = YooAssets.TryGetPackage(packageName);
            if (package == null)
            {
                package = YooAssets.CreatePackage(packageName);
                YooAssets.SetDefaultPackage(package);
            }

            DefaultPackage = package;

#if UNITY_EDITOR
            //编辑器模式使用。
            EPlayMode playMode = (EPlayMode)UnityEditor.EditorPrefs.GetInt("EditorResourceMode");
            Log.Warning($"编辑器模式使用:{playMode}");
#else
            //运行时使用。
            EPlayMode playMode = PlayMode;
#endif

            // 编辑器下的模拟模式
            InitializationOperation initializationOperation = null;
            if (playMode == EPlayMode.EditorSimulateMode)
            {
                var createParameters = new EditorSimulateModeParameters();
                createParameters.SimulateManifestFilePath = EditorSimulateModeHelper.SimulateBuild(packageName);
                initializationOperation = package.InitializeAsync(createParameters);
            }

            // 单机运行模式
            if (playMode == EPlayMode.OfflinePlayMode)
            {
                var createParameters = new OfflinePlayModeParameters();
                createParameters.DecryptionServices = new GameDecryptionServices();
                initializationOperation = package.InitializeAsync(createParameters);
            }

            // 联机运行模式
            if (playMode == EPlayMode.HostPlayMode)
            {
                var createParameters = new HostPlayModeParameters();
                createParameters.DecryptionServices = new GameDecryptionServices();
                createParameters.BuildinQueryServices = new BuiltinQueryServices();
                createParameters.DeliveryQueryServices = new DefaultDeliveryQueryServices();
                createParameters.RemoteServices = new RemoteServices();
                initializationOperation = package.InitializeAsync(createParameters);
            }

            // WebGL运行模式
            if (playMode == EPlayMode.WebPlayMode)
            {
                YooAssets.SetCacheSystemDisableCacheOnWebGL();
                var createParameters = new WebPlayModeParameters();
                createParameters.DecryptionServices = new GameDecryptionServices();
                createParameters.BuildinQueryServices = new BuiltinQueryServices();
                createParameters.RemoteServices = new RemoteServices();
                initializationOperation = package.InitializeAsync(createParameters);
            }

            return initializationOperation;
        }

        /// <summary>
        /// 卸载资源。
        /// </summary>
        /// <param name="asset">要卸载的资源实例。</param>
        /// <exception cref="GameFrameworkException">游戏框架异常类 - 未实现。</exception>
        public void UnloadAsset(object asset)
        {
            throw new GameFrameworkException("System.NotImplementedException.");
        }

        /// <summary>
        /// 资源回收（卸载引用计数为零的资源）。
        /// </summary>
        public void UnloadUnusedAssets()
        {
            var iter = _releaseMaps.Values.GetEnumerator();
            while (iter.MoveNext())
            {
                AssetOperationHandle handle = iter.Current;
                if (handle is { IsValid: true })
                {
                    handle.Dispose();
                    handle = null;
                }
            }

            iter.Dispose();
            _releaseMaps.Clear();

            if (DefaultPackage == null)
            {
                throw new GameFrameworkException("Package is invalid.");
            }

            DefaultPackage.UnloadUnusedAssets();
        }

        /// <summary>
        /// 强制回收所有资源。
        /// </summary>
        public void ForceUnloadAllAssets()
        {
#if UNITY_WEBGL
			return;
#else
            if (DefaultPackage == null)
            {
                throw new GameFrameworkException("Package is invalid.");
            }

            DefaultPackage.ForceUnloadAllAssets();
#endif
        }

        /// <summary>
        /// 检查资源是否存在。
        /// </summary>
        /// <param name="location">要检查资源的名称。</param>
        /// <returns>检查资源是否存在的结果。</returns>
        public HasAssetResult HasAsset(string location)
        {
            if (string.IsNullOrEmpty(location))
            {
                throw new GameFrameworkException("Asset name is invalid.");
            }

            AssetInfo assetInfo = YooAssets.GetAssetInfo(location);

            if (!CheckLocationValid(location))
            {
                return HasAssetResult.Valid;
            }

            if (assetInfo == null)
            {
                return HasAssetResult.NotExist;
            }

            if (IsNeedDownloadFromRemote(assetInfo))
            {
                return HasAssetResult.AssetOnline;
            }

            return HasAssetResult.AssetOnDisk;
        }

        /// <summary>
        /// 设置默认的资源包。
        /// </summary>
        public void SetDefaultPackage(ResourcePackage package)
        {
            YooAssets.SetDefaultPackage(package);
            DefaultPackage = package;
        }

        #region 资源信息

        /// <summary>
        /// 是否需要从远端更新下载。
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        public bool IsNeedDownloadFromRemote(string location)
        {
            return YooAssets.IsNeedDownloadFromRemote(location);
        }

        /// <summary>
        /// 是否需要从远端更新下载。
        /// </summary>
        /// <param name="assetInfo">资源信息。</param>
        public bool IsNeedDownloadFromRemote(AssetInfo assetInfo)
        {
            return YooAssets.IsNeedDownloadFromRemote(assetInfo);
        }

        /// <summary>
        /// 获取资源信息列表。
        /// </summary>
        /// <param name="tag">资源标签。</param>
        /// <returns>资源信息列表。</returns>
        public AssetInfo[] GetAssetInfos(string tag)
        {
            return YooAssets.GetAssetInfos(tag);
        }

        /// <summary>
        /// 获取资源信息列表。
        /// </summary>
        /// <param name="tags">资源标签列表。</param>
        /// <returns>资源信息列表。</returns>
        public AssetInfo[] GetAssetInfos(string[] tags)
        {
            return YooAssets.GetAssetInfos(tags);
        }

        /// <summary>
        /// 获取资源信息。
        /// </summary>
        /// <param name="location">资源的定位地址。</param>
        /// <returns>资源信息。</returns>
        public AssetInfo GetAssetInfo(string location)
        {
            return YooAssets.GetAssetInfo(location);
        }

        /// <summary>
        /// 检查资源定位地址是否有效。
        /// </summary>
        /// <param name="location">资源的定位地址。</param>
        public bool CheckLocationValid(string location)
        {
            return YooAssets.CheckLocationValid(location);
        }

        #endregion

        /// <summary>
        /// 同步加载资源。
        /// </summary>
        /// <param name="location">资源的定位地址。</param>
        /// <param name="needInstance">是否需要实例化。</param>
        /// <param name="needCache">是否需要缓存。</param>
        /// <typeparam name="T">要加载资源的类型。</typeparam>
        /// <returns>资源实例。</returns>
        public T LoadAsset<T>(string location, bool needInstance = true, bool needCache = false) where T : Object
        {
            if (string.IsNullOrEmpty(location))
            {
                Log.Error("Asset name is invalid.");
                return default;
            }

            AssetOperationHandle handle = GetHandleSync<T>(location, needCache);

            if (typeof(T) == typeof(GameObject))
            {
                if (needInstance)
                {
                    GameObject gameObject = handle.InstantiateSync();
                    if (!needCache)
                    {
                        AssetReference.BindAssetReference(gameObject, handle, location);
                    }
                    return gameObject as T;
                }
            }
            
            T ret = handle.AssetObject as T;
            if (!needCache)
            {
                handle.Dispose();
            }
            return ret;
        }

        /// <summary>
        /// 同步加载资源。
        /// </summary>
        /// <param name="location">资源的定位地址。</param>
        /// <param name="parent">父节点位置。</param>
        /// <param name="needInstance">是否需要实例化。</param>
        /// <param name="needCache">是否需要缓存。</param>
        /// <typeparam name="T">要加载资源的类型。</typeparam>
        /// <returns>资源实例。</returns>
        public T LoadAsset<T>(string location, Transform parent, bool needInstance = true, bool needCache = false) where T : Object
        {
            if (string.IsNullOrEmpty(location))
            {
                Log.Error("Asset name is invalid.");
                return default;
            }

            AssetOperationHandle handle = GetHandleSync<T>(location, needCache);

            if (typeof(T) == typeof(GameObject))
            {
                if (needInstance)
                {
                    GameObject gameObject = handle.InstantiateSync(parent);
                    if (!needCache)
                    {
                        AssetReference.BindAssetReference(gameObject, handle, location);
                    }
                    return gameObject as T;
                }
            }
            
            T ret = handle.AssetObject as T;
            if (!needCache)
            {
                handle.Dispose();
            }
            return ret;
        }

        /// <summary>
        /// 同步加载资源。
        /// </summary>
        /// <param name="handle">资源操作句柄。</param>
        /// <param name="location">资源的定位地址。</param>
        /// <param name="needCache">是否需要缓存。</param>
        /// <typeparam name="T">要加载资源的类型。</typeparam>
        /// <returns>资源实例。</returns>
        public T LoadAsset<T>(string location, out AssetOperationHandle handle, bool needCache = false) where T : Object
        {
            handle = GetHandleSync<T>(location, needCache);
            if (string.IsNullOrEmpty(location))
            {
                Log.Error("Asset name is invalid.");
                return default;
            }

            if (typeof(T) == typeof(GameObject))
            {
                GameObject gameObject = handle.InstantiateSync();
                if (!needCache)
                {
                    AssetReference.BindAssetReference(gameObject, handle, location);
                }
                return gameObject as T;
            }
            
            T ret = handle.AssetObject as T;
            if (!needCache)
            {
                handle.Dispose();
            }
            return ret;
        }

        /// <summary>
        /// 同步加载资源。
        /// </summary>
        /// <param name="location">资源的定位地址。</param>
        /// <param name="handle">资源操作句柄。</param>
        /// <param name="needCache">是否需要缓存。</param>
        /// <param name="parent">父节点位置。</param>
        /// <typeparam name="T">要加载资源的类型。</typeparam>
        /// <returns>资源实例。</returns>
        public T LoadAsset<T>(string location, Transform parent, out AssetOperationHandle handle, bool needCache = false) where T : Object
        {
            handle = GetHandleSync<T>(location, needCache);

            if (string.IsNullOrEmpty(location))
            {
                Log.Error("Asset name is invalid.");
                return default;
            }

            if (typeof(T) == typeof(GameObject))
            {
                GameObject gameObject = handle.InstantiateSync(parent);
                if (!needCache)
                {
                    AssetReference.BindAssetReference(gameObject, handle, location);
                }
                return gameObject as T;
            }
            
            T ret = handle.AssetObject as T;
            if (!needCache)
            {
                handle.Dispose();
            }
            return ret;
        }

        /// <summary>
        /// 同步加载资源并获取句柄。
        /// </summary>
        /// <param name="location">资源的定位地址。</param>
        /// <param name="needCache">是否需要缓存。</param>
        /// <typeparam name="T">要加载资源的类型。</typeparam>
        /// <returns>同步加载资源句柄。</returns>
        public AssetOperationHandle LoadAssetGetOperation<T>(string location, bool needCache = false) where T : Object
        {
            return GetHandleSync<T>(location, needCache);
        }

        /// <summary>
        /// 异步加载资源并获取句柄。
        /// </summary>
        /// <param name="location">资源的定位地址。</param>
        /// <param name="needCache">是否需要缓存。</param>
        /// <typeparam name="T">要加载资源的类型。</typeparam>
        /// <returns>异步加载资源句柄。</returns>
        public AssetOperationHandle LoadAssetAsyncHandle<T>(string location, bool needCache = false) where T : Object
        {
            return GetHandleAsync<T>(location, needCache);
        }

        /// <summary>
        /// 同步加载子资源对象
        /// </summary>
        /// <typeparam name="TObject">资源类型。</typeparam>
        /// <param name="location">资源的定位地址。</param>
        public SubAssetsOperationHandle LoadSubAssetsSync<TObject>(string location) where TObject : Object
        {
            return YooAssets.LoadSubAssetsSync<TObject>(location: location);
        }

        /// <summary>
        /// 异步加载子资源对象
        /// </summary>
        /// <typeparam name="TObject">资源类型。</typeparam>
        /// <param name="location">资源的定位地址。</param>
        public SubAssetsOperationHandle LoadSubAssetsAsync<TObject>(string location) where TObject : Object
        {
            return YooAssets.LoadSubAssetsAsync<TObject>(location: location);
        }

        /// <summary>
        /// 同步加载子资源对象
        /// </summary>
        /// <param name="assetInfo">资源信息。</param>
        public SubAssetsOperationHandle LoadSubAssetsSync(AssetInfo assetInfo)
        {
            return YooAssets.LoadSubAssetsSync(assetInfo);
        }

        /// <summary>
        /// 通过Tag加载资源对象集合。
        /// </summary>
        /// <param name="assetTag">资源标识。</param>
        /// <typeparam name="T">资源类型。</typeparam>
        /// <returns>资源对象集合。</returns>
        public async UniTask<List<T>> LoadAssetsByTagAsync<T>(string assetTag) where T : UnityEngine.Object
        {
            LoadAssetsByTagOperation<T> operation = new LoadAssetsByTagOperation<T>(assetTag);
            YooAssets.StartOperation(operation);
            await operation.ToUniTask();
            List<T> assetObjects = operation.AssetObjects;
            operation.ReleaseHandle();
            return assetObjects;
        }

        /// <summary>
        /// 异步加载资源实例。
        /// </summary>
        /// <param name="location">要加载的实例名称。</param>
        /// <param name="cancellationToken">取消操作Token。</param>
        /// <param name="needInstance">是否需要实例化。</param>
        /// <param name="needCache">是否需要缓存。</param>
        /// <returns>资源实实例。</returns>
        public async UniTask<T> LoadAssetAsync<T>(string location, CancellationToken cancellationToken = default, bool needInstance = true, bool needCache = false) where T : Object
        {
            AssetOperationHandle handle = LoadAssetAsyncHandle<T>(location, needCache);

            bool cancelOrFailed = await handle.ToUniTask().AttachExternalCancellation(cancellationToken).SuppressCancellationThrow();

            if (cancelOrFailed)
            {
                return null;
            }

            if (typeof(T) == typeof(GameObject))
            {
                if (needInstance)
                {
                    GameObject gameObject = handle.InstantiateSync();
                    if (!needCache)
                    {
                        AssetReference.BindAssetReference(gameObject, handle, location);
                    }
                    return gameObject as T;
                }
            }
            
            T ret = handle.AssetObject as T;
            if (!needCache)
            {
                handle.Dispose();
            }
            return ret;
        }

        /// <summary>
        /// 异步加载游戏物体。
        /// </summary>
        /// <param name="location">要加载的游戏物体名称。</param>
        /// <param name="cancellationToken">取消操作Token。</param>
        /// <param name="needCache">是否需要缓存。</param>
        /// <returns>异步游戏物体实例。</returns>
        public async UniTask<GameObject> LoadGameObjectAsync(string location, CancellationToken cancellationToken = default, bool needCache = false)
        {
            AssetOperationHandle handle = LoadAssetAsyncHandle<GameObject>(location, needCache);

            bool cancelOrFailed = await handle.ToUniTask().AttachExternalCancellation(cancellationToken).SuppressCancellationThrow();

            if (cancelOrFailed)
            {
                return null;
            }

            GameObject gameObject = handle.InstantiateSync();
            if (!needCache)
            {
                AssetReference.BindAssetReference(gameObject, handle, location);
            }
            return gameObject;
        }

        /// <summary>
        /// 异步加载游戏物体。
        /// </summary>
        /// <param name="location">资源定位地址。</param>
        /// <param name="parent">父节点位置。</param>
        /// <param name="cancellationToken">取消操作Token。</param>
        /// <param name="needCache">是否需要缓存。</param>
        /// <returns>异步游戏物体实例。</returns>
        public async UniTask<GameObject> LoadGameObjectAsync(string location, Transform parent, CancellationToken cancellationToken = default, bool needCache = false)
        {
            GameObject gameObject = await LoadGameObjectAsync(location, cancellationToken, needCache);
            if (parent != null)
            {
                gameObject.transform.SetParent(parent);
            }
            else
            {
                Log.Error("Set Parent Failed");
            }

            return gameObject;
        }

        /// <summary>
        /// 异步加载原生文件。
        /// </summary>
        /// <param name="location">资源定位地址。</param>
        /// <param name="cancellationToken">取消操作Token。</param>
        /// <returns>原生文件资源实例操作句柄。</returns>
        /// <remarks>需要自行释放资源句柄(RawFileOperationHandle)。</remarks>
        public async UniTask<RawFileOperationHandle> LoadRawAssetAsync(string location, CancellationToken cancellationToken = default)
        {
            RawFileOperationHandle handle = YooAssets.LoadRawFileAsync(location);

            bool cancelOrFailed = await handle.ToUniTask().AttachExternalCancellation(cancellationToken).SuppressCancellationThrow();

            return cancelOrFailed ? null : handle;
        }

        /// <summary>
        /// 异步加载子文件。
        /// </summary>
        /// <param name="location">资源定位地址。</param>
        /// <param name="assetName">子资源名称。</param>
        /// <param name="cancellationToken">取消操作Token。</param>
        /// <typeparam name="T">资源实例类型。</typeparam>
        /// <returns>原生文件资源实例。</returns>
        public async UniTask<T> LoadSubAssetAsync<T>(string location, string assetName, CancellationToken cancellationToken = default) where T : Object
        {
            var assetInfo = GetAssetInfo(location);
            if (assetInfo == null)
            {
                Log.Fatal($"AssetsInfo is null");
                return null;
            }

            SubAssetsOperationHandle handle = YooAssets.LoadSubAssetsAsync(assetInfo);

            bool cancelOrFailed = await handle.ToUniTask().AttachExternalCancellation(cancellationToken).SuppressCancellationThrow();

            handle.Dispose();

            return cancelOrFailed ? null : handle.GetSubAssetObject<T>(assetName);
        }

        /// <summary>
        /// 异步加载子文件。
        /// </summary>
        /// <param name="location">资源定位地址。</param>
        /// <param name="cancellationToken">取消操作Token。</param>
        /// <typeparam name="T">资源实例类型。</typeparam>
        /// <returns>原生文件资源实例。</returns>
        public async UniTask<T[]> LoadAllSubAssetAsync<T>(string location, CancellationToken cancellationToken = default) where T : Object
        {
            var assetInfo = GetAssetInfo(location);
            if (assetInfo == null)
            {
                Log.Fatal($"AssetsInfo is null");
                return null;
            }

            SubAssetsOperationHandle handle = YooAssets.LoadSubAssetsAsync(assetInfo);

            bool cancelOrFailed = await handle.ToUniTask().AttachExternalCancellation(cancellationToken).SuppressCancellationThrow();

            handle.Dispose();

            return cancelOrFailed ? null : handle.GetSubAssetObjects<T>();
        }
    }
}