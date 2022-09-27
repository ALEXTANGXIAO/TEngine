using System;
using System.Collections.Generic;
using UnityEngine;

namespace TEngine.Runtime
{
    public class GameObjectHelper : UnitySingleton<GameObjectHelper>
    {
        private readonly Dictionary<string, PoolData> _poolDictionary = new Dictionary<string, PoolData>();

        public void RegisterPool(string objectName, GameObject ret, int capacity = 100)
        {
            if (_poolDictionary.ContainsKey(objectName))
            {
                Log.Fatal($"Repeated RegisterPool: {objectName}");
            }
            else
            {
                _poolDictionary.Add(objectName, new PoolData(ret, this.gameObject, capacity));
            }
        }

        public void RegisterPool(GameObject ret, int capacity = 100)
        {
            if (_poolDictionary.ContainsKey(ret.name))
            {
                Log.Fatal($"Repeated RegisterPool: {ret.name}");
            }
            else
            {
                _poolDictionary.Add(ret.name, new PoolData(ret, this.gameObject, capacity));
            }
        }

        public GameObject Get(string objectName, Action<GameObject> callback = null)
        {
            GameObject ret = null;

            if (_poolDictionary.ContainsKey(objectName) && _poolDictionary[objectName].Count > 0)
            {
                ret = _poolDictionary[objectName].Get();
            }
            else
            {
                ret = UnityEngine.Object.Instantiate(Resources.Load<GameObject>(objectName));
                ret.name = objectName;
            }

            callback?.Invoke(ret);

            return ret;
        }

        public void GetAsync(string objectName, Action<GameObject> callback)
        {
            if (_poolDictionary.ContainsKey(objectName) && _poolDictionary[objectName].Count > 0)
            {
                callback(_poolDictionary[objectName].Get());
            }
            else
            {
                TResources.LoadAsync<GameObject>(objectName, ret =>
                {
                    ret.name = objectName;

                    callback(ret);
                });
            }
        }

        public void Push(string objectName, GameObject ret)
        {
            if (_poolDictionary.ContainsKey(objectName))
            {
                _poolDictionary[objectName].Push(ret);
            }
            else
            {
                _poolDictionary.Add(objectName, new PoolData(ret, this.gameObject));
            }
        }

        public void Push(GameObject ret)
        {
            if (_poolDictionary.ContainsKey(ret.name))
            {
                _poolDictionary[ret.name].Push(ret);
            }
            else
            {
                _poolDictionary.Add(ret.name, new PoolData(ret, this.gameObject));
            }
        }

        public void Release(string objectName)
        {
            if (_poolDictionary.ContainsKey(objectName))
            {
                var poolData = _poolDictionary[objectName];

                poolData.Release();

                _poolDictionary.Remove(objectName);
            }
        }

        public void Clear()
        {
            _poolDictionary.Clear();
        }
    }
}