using System.Diagnostics;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using YooAsset;

namespace TEngine
{
    /// <summary>
    /// 资源管理器。
    /// </summary>
    internal partial class ResourceManager: GameFrameworkModule,IResourceManager
    {
        #region Propreties
        /// <summary>
        /// 资源包名称。
        /// </summary>
        public string PackageName { get; set; } = "DefaultPackage";

        /// <summary>
        /// 获取当前资源适用的游戏版本号。
        /// </summary>
        public string ApplicableGameVersion => m_ApplicableGameVersion;
        
        private string m_ApplicableGameVersion;

        /// <summary>
        /// 获取当前内部资源版本号。
        /// </summary>
        public int InternalResourceVersion => m_InternalResourceVersion;
        
        private int m_InternalResourceVersion;
        
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
        public string ReadOnlyPath => m_ReadOnlyPath;
        
        private string m_ReadOnlyPath;

        /// <summary>
        /// 获取资源读写区路径。
        /// </summary>
        public string ReadWritePath => m_ReadWritePath;
        
        private string m_ReadWritePath;
        
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
        /// The total number of frames since the start of the game (Read Only).
        /// </summary>
        private static int _lastUpdateFrame = 0;
        
        /// <summary>
        /// 获取游戏框架模块优先级。
        /// </summary>
        /// <remarks>优先级较高的模块会优先轮询，并且关闭操作会后进行。</remarks>
        internal override int Priority => 4;
        #endregion

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
            DebugCheckDuplicateDriver();
            YooAssets.Update();
        }
        
        [Conditional("DEBUG")]
        private void DebugCheckDuplicateDriver()
        {
            if (_lastUpdateFrame > 0)
            {
                if (_lastUpdateFrame == Time.frameCount)
                    YooLogger.Warning($"There are two {nameof(YooAssetsDriver)} in the scene. Please ensure there is always exactly one driver in the scene.");
            }

            _lastUpdateFrame = Time.frameCount;
        }

        internal override void Shutdown()
        {
            YooAssets.Destroy();
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

        public void Initialize()
        {
            throw new System.NotImplementedException();
        }

        public InitializationOperation InitPackage()
        {
            throw new System.NotImplementedException();
        }

        public void UnloadAsset(object asset)
        {
            throw new System.NotImplementedException();
        }

        public void UnloadUnusedAssets()
        {
            throw new System.NotImplementedException();
        }

        public void ForceUnloadAllAssets()
        {
            throw new System.NotImplementedException();
        }

        public HasAssetResult HasAsset(string assetName)
        {
            throw new System.NotImplementedException();
        }

        public T LoadAsset<T>(string assetName,out AssetOperationHandle handle) where T : Object
        {
            throw new System.NotImplementedException();
        }

        public T LoadAsset<T>(string assetName, Transform parent,out AssetOperationHandle handle) where T : Object
        {
            throw new System.NotImplementedException();
        }

        public UniTask<T> LoadAssetAsync<T>(string assetName, CancellationToken cancellationToken) where T : Object
        {
            throw new System.NotImplementedException();
        }

        public UniTask<GameObject> LoadGameObjectAsync(string assetName, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public AssetOperationHandle LoadAssetGetOperation<T>(string assetName) where T : Object
        {
            throw new System.NotImplementedException();
        }

        public AssetOperationHandle LoadAssetAsyncHandle<T>(string assetName) where T : Object
        {
            throw new System.NotImplementedException();
        }
    }
}