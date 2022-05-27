using System;
using UnityEngine;

namespace TEngine
{
    /// <summary>
    /// 通用资源加载接口
    /// </summary>
    public static class TResources
    {
        #region 同步加载
        public static GameObject Load(string path)
        {
            return ResMgr.Instance.Load(path);
        }
        public static GameObject Load(string path, Transform parent)
        {
            var obj = Load(path);

            if (obj != null && parent != null)
            {
                obj.transform.SetParent(parent);
            }

            return obj;
        }
        public static T Load<T>(string path) where T : UnityEngine.Object
        {
            return ResMgr.Instance.Load<T>(path);
        }
        #endregion

        #region 异步加载
        public static void LoadAsync(string path, Action<GameObject> callBack)
        {
            ResMgr.Instance.GetGameObjectAsync(path, callBack);
        }
        public static void LoadAsync(string path, Action<AssetData> callBack, bool withSubAsset = false)
        {
            ResMgr.Instance.GetAssetAtPathAsync(path, withSubAsset, callBack);
        }
        #endregion
    }
}