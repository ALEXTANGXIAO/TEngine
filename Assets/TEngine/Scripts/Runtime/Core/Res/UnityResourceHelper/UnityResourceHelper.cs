using System;
using System.Collections;
using UnityEngine;

namespace TEngine.Runtime
{
    public class UnityResourceHelper : ResourceHelperBase
    {
        public override GameObject Load(string path)
        {
            return Resources.Load<GameObject>(RegularPath(path));
        }

        public override GameObject Load(string path, Transform parent)
        {
            var obj = Load(RegularPath(path));
            if (obj == null)
            {
                return null;
            }

            obj = UnityEngine.Object.Instantiate(obj);

            if (parent != null)
            {
                obj.transform.SetParent(parent);
            }

            return obj;
        }

        public static string RegularPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return string.Empty;
            }
            var splits = path.Split('.');
            if (splits.Length > 1)
            {
                string ret = string.Empty;
                for (int i = 0; i < splits.Length-1; i++)
                {
                    ret += splits[i];
                }

                return ret;
            }
            else
            {
                return path;
            }
        }

        public override T Load<T>(string path)
        {
            return Resources.Load<T>(RegularPath(path));
        }

        public override void LoadAsync(string path, Action<GameObject> callBack)
        {
            MonoUtility.StartCoroutine(ReallyLoadAsync(RegularPath(path), callBack));
        }

        private IEnumerator ReallyLoadAsync<T>(string path, Action<T> callback = null) where T : UnityEngine.Object
        {
            ResourceRequest request = Resources.LoadAsync<T>(RegularPath(path));

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
            MonoUtility.StartCoroutine(ReallyLoadAsync<T>(RegularPath(path), callBack));
        }
    }
}