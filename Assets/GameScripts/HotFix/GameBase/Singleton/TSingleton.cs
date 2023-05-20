using System.Collections.Generic;
using TEngine;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameBase
{
    /// <summary>
    /// 单例接口。
    /// </summary>
    public interface ISingleton
    {
        void Active();

        void Release();
    }

    /// <summary>
    /// 单例管理器(统一化持久和释放)。
    /// </summary>
    public static class SingletonMgr
    {
        private static List<ISingleton> _singletonList;
        private static Dictionary<string, GameObject> _gameObjects;
        private static GameObject _root;

        public static GameObject Root
        {
            get
            {
                if (_root == null)
                {
                    _root = GameObject.Find("[GameModule]");

                    if (_root == null)
                    {
                        _root = new GameObject("[GameModule]")
                        {
                            transform =
                            {
                                position = Vector3.zero
                            }
                        };
                    }
                    UnityEngine.Object.DontDestroyOnLoad(_root);
                }
                return _root;
            }
        }

        public static void Retain(ISingleton go)
        {
            if (_singletonList == null)
            {
                _singletonList = new List<ISingleton>();
            }

            _singletonList.Add(go);
        }

        public static void Retain(GameObject go)
        {
            if (_gameObjects == null)
            {
                _gameObjects = new Dictionary<string, GameObject>();
            }

            if (!_gameObjects.ContainsKey(go.name))
            {
                _gameObjects.Add(go.name, go);
                if (Application.isPlaying)
                {
                    UnityEngine.Object.DontDestroyOnLoad(go);
                }
            }
        }

        public static void Release(GameObject go)
        {
            if (_gameObjects != null && _gameObjects.ContainsKey(go.name))
            {
                _gameObjects.Remove(go.name);
                UnityEngine.Object.Destroy(go);
            }
        }

        public static void Release(ISingleton go)
        {
            if (_singletonList != null && _singletonList.Contains(go))
            {
                _singletonList.Remove(go);
            }
        }

        public static void Release()
        {
            if (_gameObjects != null)
            {
                foreach (var item in _gameObjects)
                {
                    UnityEngine.Object.Destroy(item.Value);
                }

                _gameObjects.Clear();
            }

            if (_singletonList != null)
            {
                for (int i = 0; i < _singletonList.Count; ++i)
                {
                    _singletonList[i].Release();
                }

                _singletonList.Clear();
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

        internal static ISingleton GetSingleton(string name)
        {
            for (int i = 0; i < _singletonList.Count; ++i)
            {
                if (_singletonList[i].ToString() == name)
                {
                    return _singletonList[i];
                }
            }

            return null;
        }

        /// <summary>
        /// 释放所有单例。
        /// </summary>
        public static void ReStart()
        {
            Release();

            SceneManager.LoadScene(0);
        }
    }

    /// <summary>
    /// 全局单例对象（非线程安全）。
    /// </summary>
    /// <typeparam name="T">泛型T。</typeparam>
    public abstract class TSingleton<T> : ISingleton where T : TSingleton<T>, new()
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (null == _instance)
                {
                    _instance = new T();
                    _instance.Init();
#if UNITY_EDITOR
                    Log.Info($"TSingleton Instance:{typeof(T).Name}");
#endif
                    SingletonMgr.Retain(_instance);
                }

                return _instance;
            }
        }

        public static bool IsValid => _instance != null;

        protected TSingleton()
        {
        }

        protected virtual void Init()
        {
        }

        public virtual void Active()
        {
        }

        public virtual void Release()
        {
            if (_instance != null)
            {
                SingletonMgr.Release(_instance);
                _instance = null;
            }
        }
    }
}