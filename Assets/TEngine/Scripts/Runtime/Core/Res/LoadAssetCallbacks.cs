using System;

namespace TEngine.Runtime
{
    /// <summary>
    /// 加载资源成功回调函数。
    /// </summary>
    /// <param name="assetName">要加载的资源名称。</param>
    /// <param name="asset">已加载的资源。</param>
    /// <param name="duration">加载持续时间。</param>
    /// <param name="userData">用户自定义数据。</param>
    public delegate void LoadAssetSuccessCallback(string assetName, object asset, float duration, object userData);
    
    /// <summary>
    /// 加载资源失败回调函数。
    /// </summary>
    /// <param name="assetName">要加载的资源名称。</param>
    /// <param name="status">加载资源状态。</param>
    /// <param name="errorMessage">错误信息。</param>
    /// <param name="userData">用户自定义数据。</param>
    public delegate void LoadAssetFailureCallback(string assetName, LoadResourceStatus status, string errorMessage, object userData);
    
    /// <summary>
    /// 加载资源更新回调函数。
    /// </summary>
    /// <param name="assetName">要加载的资源名称。</param>
    /// <param name="progress">加载资源进度。</param>
    /// <param name="userData">用户自定义数据。</param>
    public delegate void LoadAssetUpdateCallback(string assetName, float progress, object userData);
    
    /// <summary>
    /// 加载资源时加载依赖资源回调函数。
    /// </summary>
    /// <param name="assetName">要加载的资源名称。</param>
    /// <param name="dependencyAssetName">被加载的依赖资源名称。</param>
    /// <param name="loadedCount">当前已加载依赖资源数量。</param>
    /// <param name="totalCount">总共加载依赖资源数量。</param>
    /// <param name="userData">用户自定义数据。</param>
    public delegate void LoadAssetDependencyAssetCallback(string assetName, string dependencyAssetName, int loadedCount, int totalCount, object userData);
    
    /// <summary>
    /// 加载资源回调函数集。
    /// </summary>
    public sealed class LoadAssetCallbacks
    {
        private readonly LoadAssetSuccessCallback m_LoadAssetSuccessCallback;
        private readonly LoadAssetFailureCallback m_LoadAssetFailureCallback;
        private readonly LoadAssetUpdateCallback m_LoadAssetUpdateCallback;
        private readonly LoadAssetDependencyAssetCallback m_LoadAssetDependencyAssetCallback;

        /// <summary>
        /// 初始化加载资源回调函数集的新实例。
        /// </summary>
        /// <param name="loadAssetSuccessCallback">加载资源成功回调函数。</param>
        public LoadAssetCallbacks(LoadAssetSuccessCallback loadAssetSuccessCallback)
            : this(loadAssetSuccessCallback, null, null, null)
        {
        }

        /// <summary>
        /// 初始化加载资源回调函数集的新实例。
        /// </summary>
        /// <param name="loadAssetSuccessCallback">加载资源成功回调函数。</param>
        /// <param name="loadAssetFailureCallback">加载资源失败回调函数。</param>
        public LoadAssetCallbacks(LoadAssetSuccessCallback loadAssetSuccessCallback, LoadAssetFailureCallback loadAssetFailureCallback)
            : this(loadAssetSuccessCallback, loadAssetFailureCallback, null, null)
        {
        }

        /// <summary>
        /// 初始化加载资源回调函数集的新实例。
        /// </summary>
        /// <param name="loadAssetSuccessCallback">加载资源成功回调函数。</param>
        /// <param name="loadAssetUpdateCallback">加载资源更新回调函数。</param>
        public LoadAssetCallbacks(LoadAssetSuccessCallback loadAssetSuccessCallback, LoadAssetUpdateCallback loadAssetUpdateCallback)
            : this(loadAssetSuccessCallback, null, loadAssetUpdateCallback, null)
        {
        }

        /// <summary>
        /// 初始化加载资源回调函数集的新实例。
        /// </summary>
        /// <param name="loadAssetSuccessCallback">加载资源成功回调函数。</param>
        /// <param name="loadAssetDependencyAssetCallback">加载资源时加载依赖资源回调函数。</param>
        public LoadAssetCallbacks(LoadAssetSuccessCallback loadAssetSuccessCallback, LoadAssetDependencyAssetCallback loadAssetDependencyAssetCallback)
            : this(loadAssetSuccessCallback, null, null, loadAssetDependencyAssetCallback)
        {
        }

        /// <summary>
        /// 初始化加载资源回调函数集的新实例。
        /// </summary>
        /// <param name="loadAssetSuccessCallback">加载资源成功回调函数。</param>
        /// <param name="loadAssetFailureCallback">加载资源失败回调函数。</param>
        /// <param name="loadAssetUpdateCallback">加载资源更新回调函数。</param>
        public LoadAssetCallbacks(LoadAssetSuccessCallback loadAssetSuccessCallback, LoadAssetFailureCallback loadAssetFailureCallback, LoadAssetUpdateCallback loadAssetUpdateCallback)
            : this(loadAssetSuccessCallback, loadAssetFailureCallback, loadAssetUpdateCallback, null)
        {
        }

        /// <summary>
        /// 初始化加载资源回调函数集的新实例。
        /// </summary>
        /// <param name="loadAssetSuccessCallback">加载资源成功回调函数。</param>
        /// <param name="loadAssetFailureCallback">加载资源失败回调函数。</param>
        /// <param name="loadAssetDependencyAssetCallback">加载资源时加载依赖资源回调函数。</param>
        public LoadAssetCallbacks(LoadAssetSuccessCallback loadAssetSuccessCallback, LoadAssetFailureCallback loadAssetFailureCallback, LoadAssetDependencyAssetCallback loadAssetDependencyAssetCallback)
            : this(loadAssetSuccessCallback, loadAssetFailureCallback, null, loadAssetDependencyAssetCallback)
        {
        }

        /// <summary>
        /// 初始化加载资源回调函数集的新实例。
        /// </summary>
        /// <param name="loadAssetSuccessCallback">加载资源成功回调函数。</param>
        /// <param name="loadAssetFailureCallback">加载资源失败回调函数。</param>
        /// <param name="loadAssetUpdateCallback">加载资源更新回调函数。</param>
        /// <param name="loadAssetDependencyAssetCallback">加载资源时加载依赖资源回调函数。</param>
        public LoadAssetCallbacks(LoadAssetSuccessCallback loadAssetSuccessCallback, LoadAssetFailureCallback loadAssetFailureCallback, LoadAssetUpdateCallback loadAssetUpdateCallback, LoadAssetDependencyAssetCallback loadAssetDependencyAssetCallback)
        {
            if (loadAssetSuccessCallback == null)
            {
                throw new Exception("Load asset success callback is invalid.");
            }

            m_LoadAssetSuccessCallback = loadAssetSuccessCallback;
            m_LoadAssetFailureCallback = loadAssetFailureCallback;
            m_LoadAssetUpdateCallback = loadAssetUpdateCallback;
            m_LoadAssetDependencyAssetCallback = loadAssetDependencyAssetCallback;
        }

        /// <summary>
        /// 获取加载资源成功回调函数。
        /// </summary>
        public LoadAssetSuccessCallback LoadAssetSuccessCallback
        {
            get
            {
                return m_LoadAssetSuccessCallback;
            }
        }

        /// <summary>
        /// 获取加载资源失败回调函数。
        /// </summary>
        public LoadAssetFailureCallback LoadAssetFailureCallback
        {
            get
            {
                return m_LoadAssetFailureCallback;
            }
        }

        /// <summary>
        /// 获取加载资源更新回调函数。
        /// </summary>
        public LoadAssetUpdateCallback LoadAssetUpdateCallback
        {
            get
            {
                return m_LoadAssetUpdateCallback;
            }
        }

        /// <summary>
        /// 获取加载资源时加载依赖资源回调函数。
        /// </summary>
        public LoadAssetDependencyAssetCallback LoadAssetDependencyAssetCallback
        {
            get
            {
                return m_LoadAssetDependencyAssetCallback;
            }
        }
    }
}
