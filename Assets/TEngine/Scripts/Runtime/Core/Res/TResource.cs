using System;
using UnityEngine;

namespace TEngine.Runtime
{
    /// <summary>
    /// 通用资源加载接口
    /// </summary>
    public class TResources
    {
        private static IResourceHelper m_ResourceHelper;
        public static bool Initalize => m_ResourceHelper != null;

        /// <summary>
        /// 设置游戏资源加载辅助器。
        /// </summary>
        /// <param name="resourceHelper">游戏资源加载辅助器。</param>
        public static void SetResourceHelper(IResourceHelper resourceHelper)
        {
            if (resourceHelper == null)
            {
                throw new Exception("Resources helper is invalid.");
            }

            m_ResourceHelper = resourceHelper;
        }

        /// <summary>
        /// 加载GameObject
        /// </summary>
        /// <param name="path">路径</param>
        /// <returns></returns>
        public static GameObject Load(string path)
        {
            if (m_ResourceHelper == null)
            {
                Log.Error("Resources helper is invalid.");
                return null;
            }

            return m_ResourceHelper.Load(path);
        }

        /// <summary>
        /// 加载GameObject并设置父节点
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="parent">父节点</param>
        /// <returns></returns>
        public static GameObject Load(string path, Transform parent)
        {
            if (m_ResourceHelper == null)
            {
                Log.Error("Resources helper is invalid.");
                return null;
            }

            return m_ResourceHelper.Load(path, parent);
        }

        /// <summary>
        /// 加载泛型
        /// </summary>
        /// <param name="path">路径</param>
        /// <typeparam name="T">泛型（UnityEngine.Object）</typeparam>
        /// <returns></returns>
        public static T Load<T>(string path) where T : UnityEngine.Object
        {
            if (m_ResourceHelper == null)
            {
                Log.Error("Resources helper is invalid.");
                return null;
            }

            return m_ResourceHelper.Load<T>(path);
        }

        /// <summary>
        /// 异步加载GameObject
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="callBack">回调</param>
        public static void LoadAsync(string path, Action<GameObject> callBack)
        {
            if (m_ResourceHelper == null)
            {
                Log.Error("Resources helper is invalid.");
                return;
            }

            m_ResourceHelper.LoadAsync(path, callBack);
        }

        /// <summary>
        /// 异步加载泛型
        /// </summary>
        /// <param name="path"></param>
        /// <param name="callBack"></param>
        /// <param name="withSubAsset"></param>
        /// <typeparam name="T">泛型（UnityEngine.Object）</typeparam>
        public static void LoadAsync<T>(string path, Action<T> callBack, bool withSubAsset = false)
            where T : UnityEngine.Object
        {
            if (m_ResourceHelper == null)
            {
                Log.Error("Resources helper is invalid.");
                return;
            }

            m_ResourceHelper.LoadAsync<T>(path, callBack, withSubAsset);
        }

        /// <summary>
        /// 异步加载资源。
        /// </summary>
        /// <param name="path">要加载资源的路径全名。</param>
        /// <param name="priority">加载资源的优先级。</param>
        /// <param name="loadAssetCallbacks">加载资源回调函数集。</param>
        /// <param name="userData">用户自定义数据。</param>
        public static void LoadAsset(string path, int priority, LoadAssetCallbacks loadAssetCallbacks, object userData)
        {
            if (loadAssetCallbacks == null)
            {
                Log.Error("Load asset callbacks is invalid.");
                return;
            }

            if (string.IsNullOrEmpty(path))
            {
                if (loadAssetCallbacks.LoadAssetFailureCallback != null)
                {
                    loadAssetCallbacks.LoadAssetFailureCallback(path, LoadResourceStatus.NotExist,
                        "Asset name is invalid.", userData);
                }

                return;
            }

            var timeWatcher = new GameTickWatcher();

            m_ResourceHelper.LoadAsync(path, (gameObject) =>
            {
                if (gameObject == null)
                {
                    if (loadAssetCallbacks.LoadAssetFailureCallback != null)
                    {
                        loadAssetCallbacks.LoadAssetFailureCallback(path, LoadResourceStatus.NotExist,
                            Utility.Text.Format("Asset name '{0}' is invalid.", path), userData);
                    }
                }
                else
                {
                    loadAssetCallbacks.LoadAssetSuccessCallback(path, gameObject, timeWatcher.ElapseTime(), userData);
                }
            });
        }

        /// <summary>
        /// 异步加载资源。
        /// </summary>
        /// <param name="path">要加载资源的路径全名。</param>
        /// <param name="priority">加载资源的优先级。</param>
        /// <param name="loadAssetCallbacks">加载资源回调函数集。</param>
        /// <param name="userData">用户自定义数据。</param>
        public static void LoadAsset<T>(string path, int priority, LoadAssetCallbacks loadAssetCallbacks,
            object userData) where T : UnityEngine.Object
        {
            if (loadAssetCallbacks == null)
            {
                Log.Error("Load asset callbacks is invalid.");
                return;
            }

            if (string.IsNullOrEmpty(path))
            {
                if (loadAssetCallbacks.LoadAssetFailureCallback != null)
                {
                    loadAssetCallbacks.LoadAssetFailureCallback(path, LoadResourceStatus.NotExist,
                        "Asset name is invalid.", userData);
                }

                return;
            }

            var timeWatcher = new GameTickWatcher();

            m_ResourceHelper.LoadAsync<T>(path, (obj) =>
            {
                if (obj == null)
                {
                    if (loadAssetCallbacks.LoadAssetFailureCallback != null)
                    {
                        loadAssetCallbacks.LoadAssetFailureCallback(path, LoadResourceStatus.NotExist,
                            Utility.Text.Format("Asset name '{0}' is invalid.", path), userData);
                    }
                }
                else
                {
                    loadAssetCallbacks.LoadAssetSuccessCallback(path, obj, timeWatcher.ElapseTime(), userData);
                }
            });
        }
    }
}