using System;
using YooAsset;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace TEngine
{
    public static class Resource
    {
        private static readonly Dictionary<UnityEngine.Object, OperationHandleBase> ObjectHandlesMap = new Dictionary<UnityEngine.Object, OperationHandleBase>();

        private static readonly Dictionary<GameObject, UnityEngine.Object> GameObjectsMap = new Dictionary<GameObject, UnityEngine.Object>();

        public static async UniTask<GameObject> InstantiateAsync(string location, Transform parent = null, bool stayWorldSpace = false)
        {
            var handle = YooAssets.LoadAssetAsync<GameObject>(location);

            await handle.ToUniTask();

            if (!handle.IsValid)
            {
                throw new Exception($"[Resource] InstantiateAsync Failed to load asset: {location}");
            }

            ObjectHandlesMap.Add(handle.AssetObject, handle);

            GameObject go = UnityEngine.Object.Instantiate(handle.AssetObject, parent, stayWorldSpace) as GameObject;
            if (go == null)
            {
                Release(handle.AssetObject);
                throw new Exception($"[Resource] InstantiateAsync Failed to instantiate asset: {location}");
            }

            GameObjectsMap.Add(go, handle.AssetObject);

            return go;
        }

        public static async UniTask<T> LoadAssetAsync<T>(string location) where T : UnityEngine.Object
        {
            var handle = YooAssets.LoadAssetAsync<T>(location);

            await handle.ToUniTask();

            if (!handle.IsValid)
            {
                throw new Exception($"[Resource] LoadAssetAsync Failed to load asset: {location}");
            }

            ObjectHandlesMap.Add(handle.AssetObject, handle);

            return handle.AssetObject as T;
        }

        public static void ReleaseInstance(GameObject gameObject)
        {
            if (gameObject is null)
            {
                return;
            }

            UnityEngine.Object.Destroy(gameObject);

            if (GameObjectsMap.TryGetValue(gameObject, out UnityEngine.Object obj))
            {
                GameObjectsMap.Remove(gameObject);

                Release(obj);
            }
        }

        public static void Release(UnityEngine.Object unityObject)
        {
            if (unityObject is null)
            {
                return;
            }

            if (ObjectHandlesMap.TryGetValue(unityObject, out OperationHandleBase handle))
            {
                ObjectHandlesMap.Remove(unityObject);

                handle?.ReleaseInternal();
            }
        }
    }
}