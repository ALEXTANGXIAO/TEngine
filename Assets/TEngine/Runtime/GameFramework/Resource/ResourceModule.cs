using System;
using UnityEngine;
using UnityEngine.Serialization;
using YooAsset;

namespace TEngine
{
    /// <summary>
    /// 资源模块。
    /// </summary>
    [DisallowMultipleComponent]
    public class ResourceModule : GameFrameworkModuleBase
    {
        #region Propreties
        /// <summary>
        /// 获取当前资源适用的游戏版本号。
        /// </summary>
        public string ApplicableGameVersion => m_ResourceManager?.ApplicableGameVersion ?? "Unknown";

        /// <summary>
        /// 获取当前内部资源版本号。
        /// </summary>
        public int InternalResourceVersion => m_ResourceManager?.InternalResourceVersion ?? 0;
        
        /// <summary>
        /// 默认资源加载优先级。
        /// </summary>
        public const int DefaultPriority = 0;

        /// <summary>
        /// 当前最新的包裹版本。
        /// </summary>
        public string PackageVersion { set; get; }

        /// <summary>
        /// 资源包名称。
        /// </summary>
        public string packageName = "DefaultPackage";

        /// <summary>
        /// 资源系统运行模式。
        /// </summary>
        public EPlayMode playMode = EPlayMode.EditorSimulateMode;

        /// <summary>
        /// 下载文件校验等级。
        /// </summary>
        public EVerifyLevel verifyLevel = EVerifyLevel.Middle;

        /// <summary>
        /// 资源下载器，用于下载当前资源版本所有的资源包文件。
        /// </summary>
        public ResourceDownloaderOperation Downloader { get; set; }

        [SerializeField] 
        private ReadWritePathType readWritePathType = ReadWritePathType.Unspecified;

        [SerializeField] 
        private float minUnloadUnusedAssetsInterval = 60f;

        [SerializeField] 
        private float maxUnloadUnusedAssetsInterval = 300f;

        /// <summary>
        /// 设置异步系统参数，每帧执行消耗的最大时间切片（单位：毫秒）
        /// </summary>
        public long milliseconds = 30;
        public int downloadingMaxNum = 3;
        public int failedTryAgain = 3;

        private IResourceManager m_ResourceManager;
        private AsyncOperation m_AsyncOperation = null;
        private bool m_ForceUnloadUnusedAssets = false;
        private bool m_PreorderUnloadUnusedAssets = false;
        private bool m_PerformGCCollect = false;
        private float m_LastUnloadUnusedAssetsOperationElapseSeconds = 0f;

        /// <summary>
        /// 获取或设置同时最大下载数目。
        /// </summary>
        public int DownloadingMaxNum
        {
            get => downloadingMaxNum;
            set => downloadingMaxNum = value;
        }
        
        /// <summary>
        /// 失败尝试数目。
        /// </summary>
        public int FailedTryAgain
        {
            get => failedTryAgain;
            set => failedTryAgain = value;
        }

        /// <summary>
        /// 获取资源读写路径类型。
        /// </summary>
        public ReadWritePathType ReadWritePathType => readWritePathType;

        /// <summary>
        /// 获取或设置无用资源释放的最小间隔时间，以秒为单位。
        /// </summary>
        public float MinUnloadUnusedAssetsInterval
        {
            get => minUnloadUnusedAssetsInterval;
            set => minUnloadUnusedAssetsInterval = value;
        }

        /// <summary>
        /// 获取或设置无用资源释放的最大间隔时间，以秒为单位。
        /// </summary>
        public float MaxUnloadUnusedAssetsInterval
        {
            get => maxUnloadUnusedAssetsInterval;
            set => maxUnloadUnusedAssetsInterval = value;
        }

        /// <summary>
        /// 获取无用资源释放的等待时长，以秒为单位。
        /// </summary>
        public float LastUnloadUnusedAssetsOperationElapseSeconds => m_LastUnloadUnusedAssetsOperationElapseSeconds;

        /// <summary>
        /// 获取资源只读路径。
        /// </summary>
        public string ReadOnlyPath => m_ResourceManager.ReadOnlyPath;

        /// <summary>
        /// 获取资源读写路径。
        /// </summary>
        public string ReadWritePath => m_ResourceManager.ReadWritePath;

        #endregion
        
        private void Start()
        {
            RootModule baseComponent = GameEntry.GetModule<RootModule>();
            if (baseComponent == null)
            {
                Log.Fatal("Base component is invalid.");
                return;
            }

            m_ResourceManager = GameFrameworkEntry.GetModule<IResourceManager>();
            if (m_ResourceManager == null)
            {
                Log.Fatal("YooAssetsManager component is invalid.");
                return;
            }

            if (playMode == EPlayMode.EditorSimulateMode)
            {
                Log.Info("During this run, Game Framework will use editor resource files, which you should validate first.");
#if !UNITY_EDITOR
                playMode = EPlayMode.OfflinePlayMode;
#endif
            }
            
            m_ResourceManager.SetReadOnlyPath(Application.streamingAssetsPath);
            if (readWritePathType == ReadWritePathType.TemporaryCache)
            {
                m_ResourceManager.SetReadWritePath(Application.temporaryCachePath);
            }
            else
            {
                if (readWritePathType == ReadWritePathType.Unspecified)
                {
                    readWritePathType = ReadWritePathType.PersistentData;
                }

                m_ResourceManager.SetReadWritePath(Application.persistentDataPath);
            }
            
            m_ResourceManager.PackageName = packageName;
            m_ResourceManager.PlayMode = playMode;
            m_ResourceManager.VerifyLevel = verifyLevel;
            m_ResourceManager.Milliseconds = milliseconds;
            m_ResourceManager.Initialize();
            Log.Info($"AssetsComponent Run Mode：{playMode}");
        }
        
        /// <summary>
        /// 初始化操作。
        /// </summary>
        /// <returns></returns>
        public InitializationOperation InitPackage()
        {
            return m_ResourceManager.InitPackage();
        }
        
        /// <summary>
        /// 异步更新最新包的版本。
        /// </summary>
        /// <param name="appendTimeTicks"></param>
        /// <param name="timeout">超时时间。</param>
        /// <returns>请求远端包裹的最新版本操作句柄。</returns>
        public UpdatePackageVersionOperation UpdatePackageVersionAsync(bool appendTimeTicks = true, int timeout = 60)
        {
            var package = YooAssets.GetPackage(packageName);
            return package.UpdatePackageVersionAsync(appendTimeTicks,timeout);
        }

        /// <summary>
        /// 异步更新最新包的Manifest文件。
        /// </summary>
        /// <param name="packageVersion">包版本信息。</param>
        /// <param name="timeout">超时时间。</param>
        /// <returns>向远端请求并更新清单操作句柄。</returns>
        public UpdatePackageManifestOperation UpdatePackageManifestAsync(string packageVersion, int timeout = 60)
        {
            var package = YooAssets.GetPackage(packageName);
            return package.UpdatePackageManifestAsync(packageVersion,timeout);
        }
        
        /// <summary>
        /// 创建资源下载器，用于下载当前资源版本所有的资源包文件。
        /// </summary>
        public ResourceDownloaderOperation CreateResourceDownloader()
        {
            var package = YooAssets.GetPackage(packageName);
            Downloader = package.CreateResourceDownloader(downloadingMaxNum,failedTryAgain);
            return Downloader;
        }

        /// <summary>
        /// 清理包裹未使用的缓存文件。
        /// </summary>
        public ClearUnusedCacheFilesOperation ClearUnusedCacheFilesAsync()
        {
            var package = YooAssets.GetPackage(packageName);
            return package.ClearUnusedCacheFilesAsync();
        }
        
        /// <summary>
        /// 清理沙盒路径。
        /// </summary>
        public void ClearSandbox()
        {
            YooAssets.ClearSandbox();
        }
        
        /// <summary>
        /// 强制执行释放未被使用的资源。
        /// </summary>
        /// <param name="performGCCollect">是否使用垃圾回收。</param>
        public void ForceUnloadUnusedAssets(bool performGCCollect)
        {
            m_ForceUnloadUnusedAssets = true;
            if (performGCCollect)
            {
                m_PerformGCCollect = true;
            }
        }
        
        /// <summary>
        /// 资源模块外部轮询（释放无用资源）。
        /// </summary>
        private void Update()
        {
            m_LastUnloadUnusedAssetsOperationElapseSeconds += Time.unscaledDeltaTime;
            if (m_AsyncOperation == null && (m_ForceUnloadUnusedAssets || m_LastUnloadUnusedAssetsOperationElapseSeconds >= maxUnloadUnusedAssetsInterval || m_PreorderUnloadUnusedAssets && m_LastUnloadUnusedAssetsOperationElapseSeconds >= minUnloadUnusedAssetsInterval))
            {
                Log.Info("Unload unused assets...");
                m_ForceUnloadUnusedAssets = false;
                m_PreorderUnloadUnusedAssets = false;
                m_LastUnloadUnusedAssetsOperationElapseSeconds = 0f;
                m_AsyncOperation = Resources.UnloadUnusedAssets();
            }
            
            if (m_AsyncOperation is { isDone: true })
            {
                m_ResourceManager.UnloadUnusedAssets();
                m_AsyncOperation = null;
                if (m_PerformGCCollect)
                {
                    Log.Info("GC.Collect...");
                    m_PerformGCCollect = false;
                    GC.Collect();
                }
            }
        }
    }
}