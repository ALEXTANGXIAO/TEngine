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
        
        public static GameObject Load(string path)
        {
            if (m_ResourceHelper == null)
            {
                Log.Error("Resources helper is invalid.");
                return null;
            }
            return m_ResourceHelper.Load(path);
        }
        public static GameObject Load(string path, Transform parent)
        {
            if (m_ResourceHelper == null)
            {
                Log.Error("Resources helper is invalid.");
                return null;
            }
            return m_ResourceHelper.Load(path,parent);
        }
        public static T Load<T>(string path) where T : UnityEngine.Object
        {
            if (m_ResourceHelper == null)
            {
                Log.Error("Resources helper is invalid.");
                return null;
            }
            return m_ResourceHelper.Load<T>(path);
        }

        public static void LoadAsync(string path, Action<GameObject> callBack)
        {
            if (m_ResourceHelper == null)
            {
                Log.Error("Resources helper is invalid.");
                return;
            }
            m_ResourceHelper.LoadAsync(path,callBack);
        }
        public static void LoadAsync(string path, Action<AssetData> callBack, bool withSubAsset = false)
        {
            if (m_ResourceHelper == null)
            {
                Log.Error("Resources helper is invalid.");
                return;
            }
            m_ResourceHelper.LoadAsync(path,callBack,withSubAsset);
        }
    }
}