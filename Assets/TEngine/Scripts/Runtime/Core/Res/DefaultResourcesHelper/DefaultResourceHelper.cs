using System;
using UnityEngine;

namespace TEngine.Runtime
{
    public class DefaultResourceHelper : ResourceHelperBase
    {
        public override GameObject Load(string path)
        {
            return ResMgr.Instance.Load(path);
        }

        public override GameObject Load(string path, Transform parent)
        {
            var obj = Load(path);

            if (obj != null && parent != null)
            {
                obj.transform.SetParent(parent);
            }

            return obj;
        }

        public override T Load<T>(string path)
        {
            return ResMgr.Instance.Load<T>(path);
        }

        public override void LoadAsync(string path, Action<GameObject> callBack)
        {
            ResMgr.Instance.GetGameObjectAsync(path, callBack);
        }

        public override void LoadAsync<T>(string path, Action<T> callBack, bool withSubAsset = false)
        {
            ResMgr.Instance.GetAssetAtPathAsync(path, withSubAsset, callBack);
        }
    }
}