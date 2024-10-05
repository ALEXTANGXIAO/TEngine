using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using YooAsset;

namespace TEngine
{
    /// <summary>
    /// 资源管理器。
    /// </summary>
    internal sealed partial class ResourceManager : ModuleImp, IResourceManager
    {
        #region Propreties

        /// <summary>
        /// 资源包名称。
        /// </summary>
        public string DefaultPackageName
        {
            get => _defaultPackageName;
            set => _defaultPackageName = value;
        }

        /// <summary>
        /// 资源系统运行模式。
        /// </summary>
        public EPlayMode PlayMode { get; set; }

        /// <summary>
        /// 下载文件校验等级。
        /// </summary>
        public EVerifyLevel VerifyLevel { get; set; }

        /// <summary>
        /// 设置异步系统参数，每帧执行消耗的最大时间切片（单位：毫秒）
        /// </summary>
        public long Milliseconds { get; set; }

        /// <summary>
        /// 获取游戏框架模块优先级。
        /// </summary>
        /// <remarks>优先级较高的模块会优先轮询，并且关闭操作会后进行。</remarks>
        internal override int Priority => 4;

        /// <summary>
        /// 实例化的根节点。
        /// </summary>
        public Transform InstanceRoot { get; set; }

        /// <summary>
        /// Propagates notification that operations should be canceled.
        /// </summary>
        public CancellationToken CancellationToken { get; private set; }

        /// <summary>
        /// 资源服务器地址。
        /// </summary>
        public string HostServerURL { get; set; }

        public string FallbackHostServerURL { get; set; }

        private string m_ApplicableGameVersion;

        private int m_InternalResourceVersion;

        private string m_ReadOnlyPath;
        private string m_ReadWritePath;
        private string _defaultPackageName = "DefaultPackage";

        /// <summary>
        /// 获取资源只读区路径。
        /// </summary>
        public string ReadOnlyPath => m_ReadOnlyPath;

        /// <summary>
        /// 获取资源读写区路径。
        /// </summary>
        public string ReadWritePath => m_ReadWritePath;

        /// <summary>
        /// 获取当前资源适用的游戏版本号。
        /// </summary>
        public string ApplicableGameVersion => m_ApplicableGameVersion;

        /// <summary>
        /// 获取当前内部资源版本号。
        /// </summary>
        public int InternalResourceVersion => m_InternalResourceVersion;

        public int DownloadingMaxNum { get; set; }
        public int FailedTryAgain { get; set; }
        

        /// <summary>
        /// 默认资源包。
        /// </summary>
        internal ResourcePackage DefaultPackage { private set; get; }

        /// <summary>
        /// 资源包列表。
        /// </summary>
        private Dictionary<string, ResourcePackage> PackageMap { get; } = new Dictionary<string, ResourcePackage>();

        /// <summary>
        /// 资源信息列表。
        /// </summary>
        private readonly Dictionary<string, AssetInfo> _assetInfoMap = new Dictionary<string, AssetInfo>();

        /// <summary>
        /// 正在加载的资源列表。
        /// </summary>
        private readonly HashSet<string> _assetLoadingList = new HashSet<string>();
        #endregion

        /// <summary>
        /// 初始化资源管理器的新实例。
        /// </summary>
        public ResourceManager()
        {
        }

        public void Initialize()
        {
            // 初始化资源系统
            YooAssets.Initialize(new ResourceLogger());
            YooAssets.SetOperationSystemMaxTimeSlice(Milliseconds);

#if UNITY_WECHAT_GAME && !UNITY_EDITOR
            YooAssets.SetCacheSystemDisableCacheOnWebGL();    
#endif

            // 创建默认的资源包
            string packageName = DefaultPackageName;
            var defaultPackage = YooAssets.TryGetPackage(packageName);
            if (defaultPackage == null)
            {
                defaultPackage = YooAssets.CreatePackage(packageName);
                YooAssets.SetDefaultPackage(defaultPackage);
                DefaultPackage = defaultPackage;
            }

            CancellationToken = InstanceRoot.gameObject.GetCancellationTokenOnDestroy();

            IObjectPoolManager objectPoolManager = ModuleImpSystem.GetModule<IObjectPoolManager>();
            SetObjectPoolManager(objectPoolManager);
        }

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

            m_ReadOnlyPath = readOnlyPath;
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

            m_ReadWritePath = readWritePath;
        }

        #endregion

        public async UniTask<InitializationOperation> InitPackage(string packageName)
        {
#if UNITY_EDITOR
            //编辑器模式使用。
            EPlayMode playMode = (EPlayMode)UnityEditor.EditorPrefs.GetInt("EditorPlayMode");
            Log.Warning($"Editor Module Used :{playMode}");
#else
            //运行时使用。
            EPlayMode playMode = (EPlayMode)PlayMode;
#endif

            if (PackageMap.ContainsKey(packageName))
            {
                Log.Error($"ResourceSystem has already init package : {packageName}");
                return null;
            }

            // 创建资源包裹类
            var package = YooAssets.TryGetPackage(packageName);
            if (package == null)
            {
                package = YooAssets.CreatePackage(packageName);
            }

            PackageMap[packageName] = package;
            
            // 编辑器下的模拟模式
            InitializationOperation initializationOperation = null;
            if (playMode == EPlayMode.EditorSimulateMode)
            {
                var createParameters = new EditorSimulateModeParameters();
                createParameters.CacheBootVerifyLevel = VerifyLevel;
                createParameters.SimulateManifestFilePath = EditorSimulateModeHelper.SimulateBuild(EDefaultBuildPipeline.BuiltinBuildPipeline, packageName);
                initializationOperation = package.InitializeAsync(createParameters);
            }

            // 单机运行模式
            if (playMode == EPlayMode.OfflinePlayMode)
            {
                var createParameters = new OfflinePlayModeParameters();
                createParameters.CacheBootVerifyLevel = VerifyLevel;
                createParameters.DecryptionServices = new FileStreamDecryption();
                initializationOperation = package.InitializeAsync(createParameters);
            }

            // 联机运行模式
            if (playMode == EPlayMode.HostPlayMode)
            {
                string defaultHostServer = HostServerURL;
                string fallbackHostServer = FallbackHostServerURL;
                var createParameters = new HostPlayModeParameters();
                createParameters.CacheBootVerifyLevel = VerifyLevel;
                createParameters.DecryptionServices = new FileStreamDecryption();
                createParameters.BuildinQueryServices = new GameQueryServices();
                createParameters.RemoteServices = new RemoteServices(defaultHostServer, fallbackHostServer);
                initializationOperation = package.InitializeAsync(createParameters);
            }

            // WebGL运行模式
            if (playMode == EPlayMode.WebPlayMode)
            {
                string defaultHostServer = HostServerURL;
                string fallbackHostServer = FallbackHostServerURL;
                var createParameters = new WebPlayModeParameters();
                createParameters.CacheBootVerifyLevel = VerifyLevel;
                createParameters.DecryptionServices = new FileStreamDecryption();
                createParameters.BuildinQueryServices = new GameQueryServices();
                createParameters.RemoteServices = new RemoteServices(defaultHostServer, fallbackHostServer);
                initializationOperation = package.InitializeAsync(createParameters);
            }

            await initializationOperation.ToUniTask();

            Log.Info($"Init resource package version : {initializationOperation?.PackageVersion}");

            return initializationOperation;
        }

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
        }

        internal override void Shutdown()
        {
            PackageMap.Clear();
            m_AssetPool = null;
            _assetLoadingList.Clear();
            _assetInfoMap.Clear();
#if !UNITY_WEBGL
            YooAssets.Destroy();
#endif
        }

        #region Public Methods

        #region 获取资源信息

        /// <summary>
        /// 是否需要从远端更新下载。
        /// </summary>
        /// <param name="location">资源的定位地址。</param>
        /// <param name="packageName">资源包名称。</param>
        public bool IsNeedDownloadFromRemote(string location, string packageName = "")
        {
            if (string.IsNullOrEmpty(packageName))
            {
                return YooAssets.IsNeedDownloadFromRemote(location);
            }
            else
            {
                var package = YooAssets.GetPackage(packageName);
                return package.IsNeedDownloadFromRemote(location);
            }
        }

        /// <summary>
        /// 是否需要从远端更新下载。
        /// </summary>
        /// <param name="assetInfo">资源信息。</param>
        /// <param name="packageName">资源包名称。</param>
        public bool IsNeedDownloadFromRemote(AssetInfo assetInfo, string packageName = "")
        {
            if (string.IsNullOrEmpty(packageName))
            {
                return YooAssets.IsNeedDownloadFromRemote(assetInfo);
            }
            else
            {
                var package = YooAssets.GetPackage(packageName);
                return package.IsNeedDownloadFromRemote(assetInfo);
            }
        }

        /// <summary>
        /// 获取资源信息列表。
        /// </summary>
        /// <param name="tag">资源标签。</param>
        /// <param name="packageName">资源包名称。</param>
        /// <returns>资源信息列表。</returns>
        public AssetInfo[] GetAssetInfos(string tag, string packageName = "")
        {
            if (string.IsNullOrEmpty(packageName))
            {
                return YooAssets.GetAssetInfos(tag);
            }
            else
            {
                var package = YooAssets.GetPackage(packageName);
                return package.GetAssetInfos(tag);
            }
        }

        /// <summary>
        /// 获取资源信息列表。
        /// </summary>
        /// <param name="tags">资源标签列表。</param>
        /// <param name="packageName">资源包名称。</param>
        /// <returns>资源信息列表。</returns>
        public AssetInfo[] GetAssetInfos(string[] tags, string packageName = "")
        {
            if (string.IsNullOrEmpty(packageName))
            {
                return YooAssets.GetAssetInfos(tags);
            }
            else
            {
                var package = YooAssets.GetPackage(packageName);
                return package.GetAssetInfos(tags);
            }
        }

        /// <summary>
        /// 获取资源信息。
        /// </summary>
        /// <param name="location">资源的定位地址。</param>
        /// <param name="packageName">资源包名称。</param>
        /// <returns>资源信息。</returns>
        public AssetInfo GetAssetInfo(string location, string packageName = "")
        {
            if (string.IsNullOrEmpty(location))
            {
                throw new GameFrameworkException("Asset name is invalid.");
            }

            if (string.IsNullOrEmpty(packageName))
            {
                if (_assetInfoMap.TryGetValue(location, out AssetInfo assetInfo))
                {
                    return assetInfo;
                }

                assetInfo = YooAssets.GetAssetInfo(location);
                _assetInfoMap[location] = assetInfo;
                return assetInfo;
            }
            else
            {
                string key = $"{packageName}/{location}";
                if (_assetInfoMap.TryGetValue(key, out AssetInfo assetInfo))
                {
                    return assetInfo;
                }

                var package = YooAssets.GetPackage(packageName);
                if (package == null)
                {
                    throw new GameFrameworkException($"The package does not exist. Package Name :{packageName}");
                }

                assetInfo = package.GetAssetInfo(location);
                _assetInfoMap[key] = assetInfo;
                return assetInfo;
            }
        }

        /// <summary>
        /// 检查资源是否存在。
        /// </summary>
        /// <param name="location">资源定位地址。</param>
        /// <param name="packageName">资源包名称。</param>
        /// <returns>检查资源是否存在的结果。</returns>
        public HasAssetResult HasAsset(string location, string packageName = "")
        {
            if (string.IsNullOrEmpty(location))
            {
                throw new GameFrameworkException("Asset name is invalid.");
            }

            AssetInfo assetInfo = GetAssetInfo(location, packageName);

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
        /// 检查资源定位地址是否有效。
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        /// <param name="packageName">资源包名称。</param>
        public bool CheckLocationValid(string location, string packageName = "")
        {
            if (string.IsNullOrEmpty(packageName))
            {
                return YooAssets.CheckLocationValid(location);
            }
            else
            {
                var package = YooAssets.GetPackage(packageName);
                return package.CheckLocationValid(location);
            }
        }

        #endregion

        #region 资源加载

        #region 获取资源句柄
        /// <summary>
        /// 获取同步资源句柄。
        /// </summary>
        /// <param name="location">资源定位地址。</param>
        /// <param name="packageName">指定资源包的名称。不传使用默认资源包</param>
        /// <typeparam name="T">资源类型。</typeparam>
        /// <returns>资源句柄。</returns>
        private AssetHandle GetHandleSync<T>(string location, string packageName = "") where T : UnityEngine.Object
        {
            return GetHandleSync(location,typeof(T), packageName);
        }
        
        private AssetHandle GetHandleSync(string location, Type assetType, string packageName = "")
        {
            if (string.IsNullOrEmpty(packageName))
            {
                return YooAssets.LoadAssetSync(location, assetType);
            }

            var package = YooAssets.GetPackage(packageName);
            return package.LoadAssetSync(location, assetType);
        }

        /// <summary>
        /// 获取异步资源句柄。
        /// </summary>
        /// <param name="location">资源定位地址。</param>
        /// <param name="packageName">指定资源包的名称。不传使用默认资源包</param>
        /// <typeparam name="T">资源类型。</typeparam>
        /// <returns>资源句柄。</returns>
        private AssetHandle GetHandleAsync<T>(string location, string packageName = "") where T : UnityEngine.Object
        {
            return GetHandleAsync(location, typeof(T), packageName);
        }
        
        private AssetHandle GetHandleAsync(string location, Type assetType, string packageName = "")
        {
            if (string.IsNullOrEmpty(packageName))
            {
                return YooAssets.LoadAssetAsync(location, assetType);
            }

            var package = YooAssets.GetPackage(packageName);
            return package.LoadAssetAsync(location, assetType);
        }
        #endregion
        
        /// <summary>
        /// 获取资源定位地址的缓存Key。
        /// </summary>
        /// <param name="location">资源定位地址。</param>
        /// <param name="packageName">资源包名称。</param>
        /// <returns>资源定位地址的缓存Key。</returns>
        private string GetCacheKey(string location, string packageName = "")
        {
            if (string.IsNullOrEmpty(packageName) || packageName.Equals(DefaultPackageName))
            {
                return location;
            }
            return $"{packageName}/{location}";
        }
        
        public T LoadAsset<T>(string location, string packageName = "") where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(location))
            {
                throw new GameFrameworkException("Asset name is invalid.");
            }

            string assetObjectKey = GetCacheKey(location, packageName);
            AssetObject assetObject = m_AssetPool.Spawn(assetObjectKey);
            if (assetObject != null)
            {
                return assetObject.Target as T;
            }
            
            AssetHandle handle = GetHandleSync<T>(location, packageName: packageName);

            T ret = handle.AssetObject as T;
                
            assetObject = AssetObject.Create(assetObjectKey, handle.AssetObject, handle,this);
            m_AssetPool.Register(assetObject, true);

            return ret;
        }

        public GameObject LoadGameObject(string location, Transform parent = null, string packageName = "")
        {
            if (string.IsNullOrEmpty(location))
            {
                throw new GameFrameworkException("Asset name is invalid.");
            }
            
            string assetObjectKey = GetCacheKey(location, packageName);
            AssetObject assetObject = m_AssetPool.Spawn(assetObjectKey);
            if (assetObject != null)
            {
                return AssetsReference.Instantiate(assetObject.Target as GameObject, parent, this).gameObject;
            }
            
            AssetHandle handle = GetHandleSync<GameObject>(location, packageName: packageName);

            GameObject gameObject = AssetsReference.Instantiate(handle.AssetObject as GameObject, parent, this).gameObject;

            assetObject = AssetObject.Create(assetObjectKey, handle.AssetObject, handle,this);
            m_AssetPool.Register(assetObject, true);
            
            return gameObject;
        }
        
        /// <summary>
        /// 异步加载资源。
        /// </summary>
        /// <param name="location">资源的定位地址。</param>
        /// <param name="callback">回调函数。</param>
        /// <param name="packageName">指定资源包的名称。不传使用默认资源包</param>
        /// <typeparam name="T">要加载资源的类型。</typeparam>
        public async UniTaskVoid LoadAsset<T>(string location, Action<T> callback, string packageName = "") where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(location))
            {
                Log.Error("Asset name is invalid.");
                return;
            }
            
            if (string.IsNullOrEmpty(location))
            {
                throw new GameFrameworkException("Asset name is invalid.");
            }
            
            string assetObjectKey = GetCacheKey(location, packageName);
            
            await TryWaitingLoading(assetObjectKey);

            AssetObject assetObject = m_AssetPool.Spawn(assetObjectKey);
            if (assetObject != null)
            {
                await UniTask.Yield();
                callback?.Invoke(assetObject.Target as T);
                return;
            }
            
            _assetLoadingList.Add(assetObjectKey);
            
            AssetHandle handle = GetHandleAsync<T>(location, packageName: packageName);

            handle.Completed += assetHandle =>
            {
                _assetLoadingList.Remove(assetObjectKey);
                
                if (assetHandle.AssetObject != null)
                {
                    assetObject = AssetObject.Create(assetObjectKey, handle.AssetObject, handle,this);
                    m_AssetPool.Register(assetObject, true);
            
                    callback?.Invoke(assetObject.Target as T);
                }
                else
                {
                    callback?.Invoke(null);
                }
            };
        }

        public TObject[] LoadSubAssetsSync<TObject>(string location, string packageName = "") where TObject : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(location))
            {
                throw new GameFrameworkException("Asset name is invalid.");
            }
            throw new NotImplementedException();
        }

        public UniTask<TObject[]> LoadSubAssetsAsync<TObject>(string location, string packageName = "") where TObject : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(location))
            {
                throw new GameFrameworkException("Asset name is invalid.");
            }
            throw new NotImplementedException();
        }

        public async UniTask<T> LoadAssetAsync<T>(string location, CancellationToken cancellationToken = default, string packageName = "") where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(location))
            {
                throw new GameFrameworkException("Asset name is invalid.");
            }
            
            string assetObjectKey = GetCacheKey(location, packageName);

            await TryWaitingLoading(assetObjectKey);
            
            AssetObject assetObject = m_AssetPool.Spawn(assetObjectKey);
            if (assetObject != null)
            {
                await UniTask.Yield();
                return assetObject.Target as T;
            }
            
            _assetLoadingList.Add(assetObjectKey);
 
            AssetHandle handle = GetHandleAsync<T>(location, packageName: packageName);

            bool cancelOrFailed = await handle.ToUniTask().AttachExternalCancellation(cancellationToken).SuppressCancellationThrow();

            if (cancelOrFailed)
            {
                _assetLoadingList.Remove(assetObjectKey);
                return null;
            }
            
            assetObject = AssetObject.Create(assetObjectKey, handle.AssetObject, handle,this);
            m_AssetPool.Register(assetObject, true);

            _assetLoadingList.Remove(assetObjectKey);
            
            return handle.AssetObject as T;
        }

        public async UniTask<GameObject> LoadGameObjectAsync(string location, Transform parent = null, CancellationToken cancellationToken = default, string packageName = "")
        {
            if (string.IsNullOrEmpty(location))
            {
                throw new GameFrameworkException("Asset name is invalid.");
            }
            
            string assetObjectKey = GetCacheKey(location, packageName);
            
            await TryWaitingLoading(assetObjectKey);
            
            AssetObject assetObject = m_AssetPool.Spawn(assetObjectKey);
            if (assetObject != null)
            {
                await UniTask.Yield();
                return AssetsReference.Instantiate(assetObject.Target as GameObject, parent, this).gameObject;
            }
            
            _assetLoadingList.Add(assetObjectKey);

            AssetHandle handle = GetHandleAsync<GameObject>(location, packageName: packageName);

            bool cancelOrFailed = await handle.ToUniTask().AttachExternalCancellation(cancellationToken).SuppressCancellationThrow();

            if (cancelOrFailed)
            {
                _assetLoadingList.Remove(assetObjectKey);
                return null;
            }

            GameObject gameObject = AssetsReference.Instantiate(handle.AssetObject as GameObject, parent, this).gameObject;
            
            assetObject = AssetObject.Create(assetObjectKey, handle.AssetObject, handle,this);
            m_AssetPool.Register(assetObject, true);

            _assetLoadingList.Remove(assetObjectKey);
            
            return gameObject;
        }

        #endregion

        /// <summary>
        /// 异步加载资源。
        /// </summary>
        /// <param name="location">资源的定位地址。</param>
        /// <param name="assetType">要加载资源的类型。</param>
        /// <param name="priority">加载资源的优先级。</param>
        /// <param name="loadAssetCallbacks">加载资源回调函数集。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <param name="packageName">指定资源包的名称。不传使用默认资源包。</param>
        public async void LoadAssetAsync(string location, Type assetType, int priority, LoadAssetCallbacks loadAssetCallbacks, object userData, string packageName = "")
        {
            if (string.IsNullOrEmpty(location))
            {
                throw new GameFrameworkException("Asset name is invalid.");
            }

            if (loadAssetCallbacks == null)
            {
                throw new GameFrameworkException("Load asset callbacks is invalid.");
            }
            
            string assetObjectKey = GetCacheKey(location, packageName);
            
            await TryWaitingLoading(assetObjectKey);
            
            float duration = Time.time;
            
            AssetObject assetObject = m_AssetPool.Spawn(assetObjectKey);
            if (assetObject != null)
            {
                await UniTask.Yield();
                loadAssetCallbacks.LoadAssetSuccessCallback(location, assetObject.Target, Time.time - duration, userData);
                return;
            }
            
            _assetLoadingList.Add(assetObjectKey);
            
            AssetInfo assetInfo = GetAssetInfo(location, packageName);

            if (!string.IsNullOrEmpty(assetInfo.Error))
            {
                _assetLoadingList.Remove(assetObjectKey);
                
                string errorMessage = Utility.Text.Format("Can not load asset '{0}' because :'{1}'.", location, assetInfo.Error);
                if (loadAssetCallbacks.LoadAssetFailureCallback != null)
                {
                    loadAssetCallbacks.LoadAssetFailureCallback(location, LoadResourceStatus.NotExist, errorMessage, userData);
                    return;
                }

                throw new GameFrameworkException(errorMessage);
            }

            AssetHandle handle = GetHandleAsync(location, assetType, packageName: packageName);

            if (loadAssetCallbacks.LoadAssetUpdateCallback != null)
            {
                InvokeProgress(location, handle, loadAssetCallbacks.LoadAssetUpdateCallback, userData).Forget();
            }
            
            await handle.ToUniTask();

            if (handle.AssetObject == null || handle.Status == EOperationStatus.Failed)
            {
                _assetLoadingList.Remove(assetObjectKey);
                
                string errorMessage = Utility.Text.Format("Can not load asset '{0}'.", location);
                if (loadAssetCallbacks.LoadAssetFailureCallback != null)
                {
                    loadAssetCallbacks.LoadAssetFailureCallback(location, LoadResourceStatus.NotReady, errorMessage, userData);
                    return;
                }

                throw new GameFrameworkException(errorMessage);
            }
            else
            {
                assetObject = AssetObject.Create(assetObjectKey, handle.AssetObject, handle,this);
                m_AssetPool.Register(assetObject, true);
                
                _assetLoadingList.Remove(assetObjectKey);
                
                if (loadAssetCallbacks.LoadAssetSuccessCallback != null)
                {
                    duration = Time.time - duration;
                    
                    loadAssetCallbacks.LoadAssetSuccessCallback(location, handle.AssetObject, duration, userData);
                }
            }
        }

        /// <summary>
        /// 异步加载资源。
        /// </summary>
        /// <param name="location">资源的定位地址。</param>
        /// <param name="priority">加载资源的优先级。</param>
        /// <param name="loadAssetCallbacks">加载资源回调函数集。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <param name="packageName">指定资源包的名称。不传使用默认资源包。</param>
        public async void LoadAssetAsync(string location, int priority, LoadAssetCallbacks loadAssetCallbacks, object userData, string packageName = "")
        {
            if (string.IsNullOrEmpty(location))
            {
                throw new GameFrameworkException("Asset name is invalid.");
            }

            if (loadAssetCallbacks == null)
            {
                throw new GameFrameworkException("Load asset callbacks is invalid.");
            }
            
            string assetObjectKey = GetCacheKey(location, packageName);
            
            await TryWaitingLoading(assetObjectKey);
            
            float duration = Time.time;
            
            AssetObject assetObject = m_AssetPool.Spawn(assetObjectKey);
            if (assetObject != null)
            {
                await UniTask.Yield();
                loadAssetCallbacks.LoadAssetSuccessCallback(location, assetObject.Target, Time.time - duration, userData);
                return;
            }
            
            _assetLoadingList.Add(assetObjectKey);

            AssetInfo assetInfo = GetAssetInfo(location, packageName);

            if (!string.IsNullOrEmpty(assetInfo.Error))
            {
                _assetLoadingList.Remove(assetObjectKey);
                
                string errorMessage = Utility.Text.Format("Can not load asset '{0}' because :'{1}'.", location, assetInfo.Error);
                if (loadAssetCallbacks.LoadAssetFailureCallback != null)
                {
                    loadAssetCallbacks.LoadAssetFailureCallback(location, LoadResourceStatus.NotExist, errorMessage, userData);
                    return;
                }

                throw new GameFrameworkException(errorMessage);
            }
            
            AssetHandle handle = GetHandleAsync(location, assetInfo.AssetType, packageName: packageName);

            if (loadAssetCallbacks.LoadAssetUpdateCallback != null)
            {
                InvokeProgress(location, handle, loadAssetCallbacks.LoadAssetUpdateCallback, userData).Forget();
            }
            
            await handle.ToUniTask();

            if (handle.AssetObject == null || handle.Status == EOperationStatus.Failed)
            {
                _assetLoadingList.Remove(assetObjectKey);
                
                string errorMessage = Utility.Text.Format("Can not load asset '{0}'.", location);
                if (loadAssetCallbacks.LoadAssetFailureCallback != null)
                {
                    loadAssetCallbacks.LoadAssetFailureCallback(location, LoadResourceStatus.NotReady, errorMessage, userData);
                    return;
                }

                throw new GameFrameworkException(errorMessage);
            }
            else
            {
                assetObject = AssetObject.Create(assetObjectKey, handle.AssetObject, handle,this);
                m_AssetPool.Register(assetObject, true);
                
                _assetLoadingList.Remove(assetObjectKey);

                if (loadAssetCallbacks.LoadAssetSuccessCallback != null)
                {
                    duration = Time.time - duration;

                    loadAssetCallbacks.LoadAssetSuccessCallback(location, handle.AssetObject, duration, userData);
                }
            }
        }

        private async UniTaskVoid InvokeProgress(string location, AssetHandle assetHandle, LoadAssetUpdateCallback loadAssetUpdateCallback, object userData)
        {
            if (string.IsNullOrEmpty(location))
            {
                throw new GameFrameworkException("Asset name is invalid.");
            }
            
            if (loadAssetUpdateCallback != null)
            {
                while (assetHandle is { IsValid: true, IsDone: false })
                {
                    await UniTask.Yield();
                
                    loadAssetUpdateCallback.Invoke(location, assetHandle.Progress, userData);
                }
            }
        }
        
        private readonly TimeoutController _timeoutController = new TimeoutController();
        
        private async UniTask TryWaitingLoading(string assetObjectKey)
        {
            if (_assetLoadingList.Contains(assetObjectKey))
            {
                try
                {
                    await UniTask.WaitUntil(
                        () => !_assetLoadingList.Contains(assetObjectKey), 
                        cancellationToken:CancellationToken)
#if UNITY_EDITOR
                        .AttachExternalCancellation(_timeoutController.Timeout(TimeSpan.FromSeconds(60)));
                    _timeoutController.Reset();
#else
                    ;
#endif
                
                }
                catch (OperationCanceledException ex)
                {
                    if (_timeoutController.IsTimeout())
                    {
                        Log.Error($"LoadAssetAsync Waiting {assetObjectKey} timeout. reason:{ex.Message}");
                    }
                }
            }
        }
        #endregion

        #region 资源回收
        public void UnloadUnusedAssets()
        {
            m_AssetPool.ReleaseAllUnused();
            foreach (var package in PackageMap.Values)
            {
                if (package is { InitializeStatus: EOperationStatus.Succeed })
                {
                    package.UnloadUnusedAssets();
                }
            }
        }

        public void ForceUnloadAllAssets()
        {
#if UNITY_WEBGL
            Log.Warning($"WebGL not support invoke {nameof(ForceUnloadAllAssets)}");
			return;
#else
            
            foreach (var package in PackageMap.Values)
            {
                if (package is { InitializeStatus: EOperationStatus.Succeed })
                {
                    package.ForceUnloadAllAssets();
                }
            }
#endif
        }
        #endregion
    }
}