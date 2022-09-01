using System;
using System.Collections;
using UnityEngine;

namespace TEngine.Runtime
{
    public class UnityResourceHelper : ResourceHelperBase
    {
        public override GameObject Load(string path)
        {
            return Resources.Load<GameObject>(path);
        }

        public override GameObject Load(string path, Transform parent)
        {
            var obj = Load(path);
            if (obj == null)
            {
                return null;
            }

            if (parent != null)
            {
                obj.transform.SetParent(parent);
            }

            return obj;
        }

        public override T Load<T>(string path)
        {
            return Resources.Load<T>(path);
        }

        public override void LoadAsync(string path, Action<GameObject> callBack)
        {
            MonoUtility.StartCoroutine(ReallyLoadAsync(path, callBack));
        }

        private IEnumerator ReallyLoadAsync<T>(string path, Action<T> callback = null) where T : UnityEngine.Object
        {
            ResourceRequest request = Resources.LoadAsync<T>(path);

            yield return request;

            if (request.asset is GameObject)
            {
                callback?.Invoke(GameObject.Instantiate(request.asset) as T);
            }
            else
            {
                callback?.Invoke(request.asset as T);
            }
        }

        public override void LoadAsync<T>(string path, Action<T> callBack, bool withSubAsset = false)
        {
            MonoUtility.StartCoroutine(ReallyLoadAsync<T>(path, callBack));
        }
    }
}