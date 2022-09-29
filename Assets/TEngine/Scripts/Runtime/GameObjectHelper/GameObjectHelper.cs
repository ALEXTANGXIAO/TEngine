using System;
using System.Collections.Generic;
using UnityEngine;

namespace TEngine.Runtime
{
    public class GameObjectHelper : UnitySingleton<GameObjectHelper>
    {
        private readonly Dictionary<string, PoolData> _poolDictionary = new Dictionary<string, PoolData>();

        /// <summary>
        /// 注册对象池
        /// </summary>
        /// <param name="objectName">对象池名称</param>
        /// <param name="ret">注册的GameObject</param>
        /// <param name="capacity">容量</param>
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

        /// <summary>
        /// 注册对象池
        /// </summary>
        /// <param name="ret">注册的GameObject</param>
        /// <param name="capacity">容量</param>
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

        /// <summary>
        /// 从对象池获取物体，没有对象池则直接实例化
        /// </summary>
        /// <param name="objectName"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public GameObject Get(string objectName, Action<GameObject> callback = null)
        {
            GameObject ret = null;

            if (_poolDictionary.ContainsKey(objectName) && _poolDictionary[objectName].Count > 0)
            {
                ret = _poolDictionary[objectName].Get();
            }
            else
            {
                ret = UnityEngine.Object.Instantiate(TResources.Load<GameObject>(objectName));
                ret.name = objectName;
            }

            callback?.Invoke(ret);

            return ret;
        }

        /// <summary>
        /// 从对象池异步获取，没有对象池则直接实例化
        /// </summary>
        /// <param name="objectName"></param>
        /// <param name="callback"></param>
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

        /// <summary>
        /// 压入对象池，没有对象池则创建
        /// </summary>
        /// <param name="objectName"></param>
        /// <param name="ret"></param>
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

        /// <summary>
        /// 压入对象池，没有对象池则创建
        /// </summary>
        /// <param name="ret"></param>
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
        
        /// <summary>
        /// 释放对象池
        /// </summary>
        /// <param name="objectName"></param>
        public void Release(string objectName)
        {
            if (_poolDictionary.ContainsKey(objectName))
            {
                var poolData = _poolDictionary[objectName];

                poolData.Release();

                _poolDictionary.Remove(objectName);
            }
        }

        /// <summary>
        /// 清理所有池子（建议切换场景使用）
        /// </summary>
        public void Clear()
        {
            _poolDictionary.Clear();
        }
    }
}