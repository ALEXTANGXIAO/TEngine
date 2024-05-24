using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameBase
{
    public interface ISingleton
    {
        /// <summary>
        /// 激活接口，通常用于在某个时机手动实例化
        /// </summary>
        void Active();

        /// <summary>
        /// 释放接口
        /// </summary>
        void Release();
    }

    /// <summary>
    /// 框架中的全局对象与Unity场景依赖相关的DontDestroyOnLoad需要统一管理，方便重启游戏时清除工作
    /// </summary>
    public static class SingletonSystem
    {
        private static List<ISingleton> _singletons;
        private static Dictionary<string, GameObject> _gameObjects;

        public static void Retain(ISingleton go)
        {
            if (_singletons == null)
            {
                _singletons = new List<ISingleton>();
            }

            _singletons.Add(go);
        }

        public static void Retain(GameObject go)
        {
            if (_gameObjects == null)
            {
                _gameObjects = new Dictionary<string, GameObject>();
            }

            if (_gameObjects.TryAdd(go.name, go))
            {
                if (Application.isPlaying)
                {
                    Object.DontDestroyOnLoad(go);
                }
            }
        }

        public static void Release(GameObject go)
        {
            if (_gameObjects != null && _gameObjects.ContainsKey(go.name))
            {
                _gameObjects.Remove(go.name);
                Object.Destroy(go);
            }
        }

        public static void Release(ISingleton go)
        {
            if (_singletons != null && _singletons.Contains(go))
            {
                _singletons.Remove(go);
            }
        }

        public static void Release()
        {
            if (_gameObjects != null)
            {
                foreach (var item in _gameObjects)
                {
                    Object.Destroy(item.Value);
                }

                _gameObjects.Clear();
            }

            if (_singletons != null)
            {
                for (int i = _singletons.Count -1; i >= 0; i--)
                {
                    _singletons[i].Release();
                }

                _singletons.Clear();
            }

            Resources.UnloadUnusedAssets();
        }

        public static GameObject GetGameObject(string name)
        {
            GameObject go = null;
            if (_gameObjects != null)
            {
                _gameObjects.TryGetValue(name, out go);
            }
            
            return go;
        }

        internal static bool ContainsKey(string name)
        {
            if (_gameObjects != null)
            {
                return _gameObjects.ContainsKey(name);
            }

            return false;
        }

        public static void Restart()
        {
            if (Camera.main != null)
            {
                Camera.main.gameObject.SetActive(false);
            }

            Release();
            SceneManager.LoadScene(0);
        }

        internal static ISingleton GetSingleton(string name)
        {
            for (int i = 0; i < _singletons.Count; ++i)
            {
                if (_singletons[i].ToString() == name)
                {
                    return _singletons[i];
                }
            }

            return null;
        }
    }
}