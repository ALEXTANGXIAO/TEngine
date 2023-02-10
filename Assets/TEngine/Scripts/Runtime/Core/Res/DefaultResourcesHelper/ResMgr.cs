using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TEngine.Runtime
{
    internal class SceneAssetData
    {
        public AssetData _assetData;
        public LoadSceneMode _loadSceneMode = LoadSceneMode.Single;
    }
    
    internal class ResMgr : TSingleton<ResMgr>
    {
        AssetConfig _assetConfig = new AssetConfig();
        private Dictionary<ScriptableObject, AssetData> _scriptableObjects = new Dictionary<ScriptableObject, AssetData>();
        public ResMgr()
        {
            MonoUtility.AddUpdateListener(Update);
            _assetConfig.Load();
        }

        ~ResMgr()
        {
            _assetConfig.Unload();
        }
        public void AssetUnloadDelay(float value)
        {
            _assetConfig.AssetUnloadDelay = value;
        }

        public void MaxUnloadNumPerFrame(int value)
        {
            _assetConfig.MaxUnloadNumPerFrame = value;
        }

        /// <summary>
        /// 资源更新重载。
        /// </summary>
        public void ReLoad()
        {
            _assetConfig.Unload();
            _assetConfig.Load();
        }

        /// <summary>
        /// 卸载无用资源
        /// </summary>
        public void UnloadUnusedAssetBundle()
        {
            _assetConfig.UnloadUnusedAssetBundle();
        }

        #region 获取资源
        /// <summary>
        /// 从文件获取字符串
        /// </summary>
        /// <param name="path">Asset下路径</param>
        /// <returns></returns>
        public string GetStringFromAsset(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            string result = null;
            AssetData assetData = _assetConfig.GetAssetAtPath(path);

            if (assetData != null)
            {
                assetData.AddRef();
                TextAsset textAsset = assetData.AssetObject as TextAsset;
                result = textAsset.text;
                assetData.DecRef();
            }

            return result;
        }

        public T GetScriptableObject<T>(string path) where T : ScriptableObject
        {
            T result = null;

            if (!string.IsNullOrEmpty(path))
            {
                AssetData assetData = _assetConfig.GetAssetAtPath(path);

                if (assetData != null)
                {
                    assetData.AddRef();
                    result = assetData.AssetObject as T;
                    if (result != null && !_scriptableObjects.ContainsKey(result))
                    {
                        _scriptableObjects.Add(result, assetData);
                    }
                }
            }

            return result;
        }

        public T Load<T>(string path) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            T result = null;

            AssetData assetData;

            bool isSprite = false;

            if (typeof(T) == typeof(Sprite))
            {
                isSprite = true;
                assetData = _assetConfig.GetAssetAtPath(path, true);
            }
            else
            {
                assetData = _assetConfig.GetAssetAtPath(path);
            }


            if (assetData != null)
            {
                if (isSprite)
                {
                    string name = assetData.Name.Split('.')[0];

                    result = assetData[name] as T;
                }
                else
                {
                    result = assetData.AssetObject as T;
                }
                if (result is GameObject)
                {
                    var go = Object.Instantiate(assetData.AssetObject) as GameObject;
                    BindAssetData(go, assetData);
                }
                else
                {
                    assetData.AddRef();
                }
            }

            return result;
        }

        public GameObject Load(string path)
        {
            return GetGameObject(path);
        }

        public GameObject GetGameObject(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            GameObject go = null;
            AssetData assetData = _assetConfig.GetAssetAtPath(path);

            if (assetData != null)
            {
                go = Object.Instantiate(assetData.AssetObject) as GameObject;
                {
                    BindAssetData(go, assetData);
                }
            }

            return go;
        }

        public AssetData GetAsset(string path, bool withSubAssets)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            return _assetConfig.GetAssetAtPath(path, withSubAssets);
        }

        public byte[] GetBytesFromAsset(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            byte[] result = null;
            AssetData assetData = _assetConfig.GetAssetAtPath(path);

            if (assetData != null)
            {
                assetData.AddRef();
                TextAsset textAsset = assetData.AssetObject as TextAsset;
                result = textAsset.bytes;
                assetData.DecRef();
            }

            return result;
        }

        public void DestroyScriptableObject(ScriptableObject scriptableObject)
        {
            if (_scriptableObjects.TryGetValue(scriptableObject, out var assetData))
            {
                assetData.DecRef();
                _scriptableObjects.Remove(scriptableObject);
            }
        }
        #endregion


        #region 异步获取资源
        public void GetGameObjectAsync(string path, System.Action<GameObject> onComplete)
        {
            if (string.IsNullOrEmpty(path))
                onComplete(null);

#if ASSETBUNDLE_ENABLE
            void CallBack(AssetData assetData)
            {
                GameObject go = null;
                if (assetData != null)
                {
                    go = Object.Instantiate(assetData.AssetObject) as GameObject;
                    BindAssetData(go, assetData);
                    assetData.OnAsyncLoadComplete -= CallBack;
                }

                onComplete(go);
            }

            _assetConfig.GetAssetAtPathAsync(path, false, CallBack);
#else
            onComplete(GetGameObject(path));
#endif
        }
        
        public void GetAssetAtPathAsync<T>(string path, bool withSubAssets, System.Action<T> onComplete) where T : class
        {
            if (string.IsNullOrEmpty(path))
            {
                onComplete(null);
            }
#if ASSETBUNDLE_ENABLE
            void CallBack(AssetData assetData)
            {
                if (assetData != null)
                {
                    if (assetData.AssetObject is GameObject)
                    {
                        GameObject go = Object.Instantiate(assetData.AssetObject) as GameObject;
                        BindAssetData(go, assetData);
                    }
                    else
                    {
                        assetData.AddRef();
                    }
                    assetData.OnAsyncLoadComplete -= CallBack;

                    onComplete(assetData.AssetObject as T);
                }
                _assetConfig.GetAssetAtPathAsync(path, withSubAssets, CallBack);
            }
#else
            onComplete(GetAsset(path,withSubAssets)?.AssetObject as T);
#endif
        }

        public void GetAssetAtPathAsync(string path, bool withSubAssets, System.Action<AssetData> onComplete)
        {
            if (string.IsNullOrEmpty(path))
            {
                onComplete(null);
            }
#if ASSETBUNDLE_ENABLE
            void CallBack(AssetData assetData)
            {
                if (assetData != null)
                {
                    if (assetData.AssetObject is GameObject)
                    {
                        GameObject go = Object.Instantiate(assetData.AssetObject) as GameObject;
                        BindAssetData(go, assetData);
                    }
                    else
                    {
                        assetData.AddRef();
                    }
                    assetData.OnAsyncLoadComplete -= CallBack;

                    onComplete(assetData);
                }
                _assetConfig.GetAssetAtPathAsync(path, withSubAssets, CallBack);
            }
#else
            onComplete(GetAsset(path,withSubAssets));
#endif
        }

        public void GetAssetAsync(string path, bool withSubAssets, System.Action<AssetData> onComplete)
        {
            if (string.IsNullOrEmpty(path)) onComplete(null);

#if ASSETBUNDLE_ENABLE
            _assetConfig.GetAssetAtPathAsync(path, withSubAssets, onComplete);
#else
            onComplete(GetAsset(path, withSubAssets));
#endif
        }

        #endregion

        public bool Exists(string path)
        {
            return _assetConfig.Exists(path);
        }

        public void Release(GameObject go)
        {
            if (go == null) return;

            Object.Destroy(go);
        }

        private void BindAssetData(GameObject go, AssetData assetData)
        {
            bool isActive = go.activeSelf;
            GameObject prefab = (GameObject)assetData.AssetObject;
            if (!go.activeSelf && prefab.activeSelf)
            {
                go.SetActive(true);
            }
            else if (!prefab.activeSelf)
            {
                TLogger.LogWarning($"Try to get gameObject by an inactive Prefab ({assetData.Path})!");
            }
            go.AddComponent<AssetTag>().Bind(assetData);
            if (isActive != go.activeSelf)
            {
                go.SetActive(isActive);
            }
        }

        /// <summary>
        /// 获取RawBytes下资源完整路径
        /// </summary>
        /// <param name="rawPath">路径会包含RawBytes，适应获取资源的其他接口使用习惯</param>
        /// <returns></returns>
        public static string GetRawBytesFullPath(string rawPath)
        {
            if (string.IsNullOrEmpty(rawPath))
                return rawPath;

#if ASSETBUNDLE_ENABLE
            //string target = GameConfig.Instance.FilePath($"{FileSystem.ResourceRoot}/{rawPath}");
            //if (!File.Exists(target))
            //{
            //    target = $"{FileSystem.ResourceRootInStreamAsset}/{rawPath}";
            //}   
            
            //return target;
            return string.Empty;
#else
            return $"{Application.dataPath}/TResources/{rawPath}";
#endif
        }

        #region Scene
        //已经加载好的场景
        private List<SceneAssetData> _loadedScenes = new List<SceneAssetData>();
        
        //加载中的场景
        private List<SceneAssetData> _loadingScenes = new List<SceneAssetData>();

        private List<SceneAssetData> _cachedDeleteFromLoadingScenes = new List<SceneAssetData>();
        /// <summary>
        /// 场景加载
        /// </summary>
        /// <param name="sceneName">场景名</param>
        /// /// <param name="mode">加载模式</param>
        /// <returns>异步操作对象</returns>
        /// <seealso>SceneLoader</seealso>提供了加载进度展示，也可以自定义包装来加载场景
        public AsyncOperation LoadScene(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
        {
            AssetData assetData = _assetConfig.GetSceneAsset(sceneName, mode);
            SceneAssetData sData = new SceneAssetData();
            sData._assetData = assetData;
            sData._loadSceneMode = mode;
            if(assetData != null)
            {
                _loadingScenes.Add(sData);
                assetData.AddRef();

                return assetData.AsyncOp;
            }
            else
                return null;
        }

        /// <summary>
        /// 场景是否已经加载过了
        /// </summary>
        /// <param name="sceneName"></param>
        /// <returns></returns>
        public bool IsSceneLoaded(string sceneName)
        {
            foreach (SceneAssetData loadedScene in _loadedScenes)
            {
                if (loadedScene != null 
                    && loadedScene._assetData != null 
                    && !string.IsNullOrEmpty(loadedScene._assetData.Name)
                    && loadedScene._assetData.Name.Equals(sceneName))
                {
                    return true;
                }
            }
            return false;
        }
        
        /// <summary>
        /// 场景是否在加载过程中
        /// </summary>
        /// <param name="sceneName"></param>
        /// <returns></returns>
        public bool IsSceneLoading(string sceneName)
        {
            foreach (SceneAssetData loadingScene in _loadingScenes)
            {
                if (loadingScene._assetData != null 
                    && !string.IsNullOrEmpty(loadingScene._assetData.Name)
                    && loadingScene._assetData.Name.Equals(sceneName))
                {
                    return true;
                }
            }
            return false;
        }
        
        /// <summary>
        /// 清理掉已经加载的场景
        /// </summary>
        void UnloadLoadedScenes()
        {
            foreach (SceneAssetData loadedScene in _loadedScenes)
            {
                if (loadedScene != null && loadedScene._assetData != null)
                {
                    loadedScene._assetData.DecRef(true);
                }
            }
            _loadedScenes.Clear();
            Resources.UnloadUnusedAssets();
        }

        /// <summary>
        /// 轮询资源。
        /// </summary>
        private void Update()
        { 
            _assetConfig.Update(Time.deltaTime);

            foreach (SceneAssetData loadingScene in _loadingScenes)
            {
                if (loadingScene._assetData != null &&loadingScene._assetData.AsyncOp != null)
                {
                    if (!loadingScene._assetData.AsyncOp.isDone)
                    {
                        if (loadingScene._assetData.AsyncOp.progress >= 0.9f)
                        {
                            if (loadingScene._loadSceneMode == LoadSceneMode.Single)
                            {
                                UnloadLoadedScenes();
                            }
                            // 激活新场景
                            loadingScene._assetData.AsyncOp.allowSceneActivation = true;
                            _cachedDeleteFromLoadingScenes.Add(loadingScene);
                            _loadedScenes.Add(loadingScene);
                        }
                    }
                }
            }

            //从Loading队列中删除掉加载完成的场景
            foreach (SceneAssetData deleteScene in _cachedDeleteFromLoadingScenes)
            {
                _loadingScenes.Remove(deleteScene);
            }
            _cachedDeleteFromLoadingScenes.Clear();
            
        }
        
        #endregion
    }
}