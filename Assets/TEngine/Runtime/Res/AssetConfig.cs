using System.IO;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace TEngine
{
    /// <summary>
    /// 资源配置
    /// </summary>
    public class AssetConfig
    {
        public const string AssetRootPath = "Assets/TResources";
        public const string AssetBundleMeta = "AssetBundleMeta.bin";

        private float _assetUnloadDelay = 10f;
        private int _maxUnloadNumPerFrame = 5;

        public float AssetUnloadDelay
        {
            set => _assetUnloadDelay = value;
            get { return _assetUnloadDelay; }
        }
        public int MaxUnloadNumPerFrame
        {
            set => _maxUnloadNumPerFrame = value;
            get { return _maxUnloadNumPerFrame; }
        }

        readonly Dictionary<string, AssetBundleData> _bundleDatas = new Dictionary<string, AssetBundleData>();
        readonly Dictionary<string, string> _assetPath2BundleDatas = new Dictionary<string, string>();

        /// <summary>
        /// 加载资源依赖
        /// </summary>
        public void Load()
        {
#if ASSETBUNDLE_ENABLE
            Stream stream = FileSystem.OpenRead(FileSystem.GetAssetBundlePathInVersion(AssetBundleMeta));
            BinaryReader reader = new BinaryReader(stream);
            uint resVersion = reader.ReadUInt32();
            int count = reader.ReadInt32();
            ulong offset = reader.ReadUInt32();
            AssetBundleData.Offset = offset;
            string bundleName;
            AssetBundleData assetBundleData;
            AssetBundleData depAssetBundleData;
            int assetCount;
            int depCount;
            string[] assetPaths;
            for (int i = 0; i < count; ++i)
            {
                bundleName = reader.ReadString();
                assetCount = reader.ReadInt32();
                assetPaths = new string[assetCount];
                for (int j = 0; j < assetCount; ++j)
                {
                    assetPaths[j] = reader.ReadString();
                    _assetPath2BundleDatas.Add(assetPaths[j], bundleName);
                }
                depCount = reader.ReadInt32();
                if (!_bundleDatas.TryGetValue(bundleName, out assetBundleData))
                {
                    assetBundleData = new AssetBundleData(bundleName);
                    _bundleDatas.Add(assetBundleData.Name, assetBundleData);
                    assetBundleData.Dependencies = new AssetBundleData[depCount];
                }
                else
                {
                    if (assetBundleData.Dependencies == null)
                        assetBundleData.Dependencies = new AssetBundleData[depCount];
                }

                assetBundleData.InitAssets(assetPaths);

                for (int j = 0; j < depCount; ++j)
                {
                    bundleName = reader.ReadString();
                    if (!_bundleDatas.TryGetValue(bundleName, out depAssetBundleData))
                    {
                        depAssetBundleData = new AssetBundleData(bundleName);
                        _bundleDatas.Add(bundleName, depAssetBundleData);
                    }
                    assetBundleData.Dependencies[j] = depAssetBundleData;
                }
            }
            stream.Close();
#endif
        }

        public void Unload()
        {
#if ASSETBUNDLE_ENABLE
            foreach(var bundleData in _bundleDatas)
            {
                bundleData.Value.Unload(true);
            }
            _bundleDatas.Clear();
            _assetPath2BundleDatas.Clear();
#endif
        }

        public void UnloadFalse()
        {
#if ASSETBUNDLE_ENABLE
            foreach(var bundleData in _bundleDatas)
            {
                bundleData.Value.UnloadBundleFalse();
            }
            _bundleDatas.Clear();
            _assetPath2BundleDatas.Clear();
#endif
        }

        public void TryReload()
        {
            if (_bundleDatas.Count > 0)
            {
                UnloadFalse();
                Load();
            }
        }

        /// <summary>
        /// 延迟卸载更新
        /// </summary>
        /// <param name="delta">游戏帧间隔</param>
        public void Update(float delta)
        {
#if ASSETBUNDLE_ENABLE
            var iter = _bundleDatas.GetEnumerator();
            AssetBundleData bundleData;
            int count = 0;
            while (iter.MoveNext())
            {
                bundleData = iter.Current.Value;
                if (bundleData.Unloadable)
                {
                    if (bundleData.Update(delta) >= AssetUnloadDelay && count < MaxUnloadNumPerFrame)
                    {
                        bundleData.Unload();
                        ++count;
                    }
                }
            }
            iter.Dispose();
#endif
        }

        /// <summary>
        /// 立即卸载没有引用的AssetBundle
        /// </summary>
        public void UnloadUnusedAssetBundle()
        {
            var iter = _bundleDatas.GetEnumerator();
            AssetBundleData bundleData;
            while (iter.MoveNext())
            {
                bundleData = iter.Current.Value;
                if (bundleData.Unloadable)
                    bundleData.Unload();
            }
            iter.Dispose();
        }

        /// <summary>
        /// 同步获取Asset
        /// </summary>
        /// <param name="path">是否加载子Asset，针对Sprite图集</param>
        /// <param name="withSubAssets">是否加载子Asset，针对Sprite图集</param>
        /// <returns>Asset数据</returns>
        public AssetData GetAssetAtPath(string path, bool withSubAssets = false)
        {
            AssetData assetData = null;
#if ASSETBUNDLE_ENABLE
            AssetBundleData bundleData = FindAssetBundle(path);
            if (bundleData != null)
            {
                assetData = bundleData.GetAsset(path);
                if(assetData == null)
                {
                    assetData = new AssetData(path, bundleData);
                    assetData.LoadAsset(withSubAssets);
                    bundleData.SetAsset(path, assetData);
                }
            }
#elif UNITY_EDITOR
            if (withSubAssets)
            {
                UnityEngine.Object[] subAssets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath($"{AssetRootPath}/{path}");
                if (subAssets != null && subAssets.Length > 0)
                {
                    assetData = new AssetData(path);
                    assetData.ProcessSubAssets(subAssets);
                }
                else
                {
                    TLogger.LogError($"Can not load the asset '{path}'");
                }
            }
            else
            {
                UnityEngine.Object obj = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>($"{AssetRootPath}/{path}");
                if (obj != null)
                {
                    assetData = new AssetData(path, null, obj);
                }
                else
                {
                    TLogger.LogError($"Can not load the asset '{path}'");
                }
            }
#endif

            return assetData;
        }

        public AssetData GetPackageAssetAtPath(string path, bool withSubAssets = false)
        {
            AssetData assetData = null;
#if ASSETBUNDLE_ENABLE
            AssetBundleData bundleData = FindAssetBundle(path);
            if (bundleData != null)
            {
                assetData = bundleData.GetAsset(path);
                if(assetData == null)
                {
                    assetData = new AssetData(path, bundleData);
                    assetData.SetAsPackageAsset(path);
                    assetData.LoadAsset(withSubAssets);
                    bundleData.SetAsset(path, assetData);
                }
            }
#elif UNITY_EDITOR
            if (withSubAssets)
            {
                UnityEngine.Object[] subAssets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(path);
                if (subAssets != null && subAssets.Length > 0)
                {
                    assetData = new AssetData(path);
                    assetData.SetAsPackageAsset(path);
                    assetData.ProcessSubAssets(subAssets);
                }
                else
                {
                    TLogger.LogError($"Can not load the asset '{path}'");
                }
            }
            else
            {
                UnityEngine.Object obj = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                if (obj != null)
                {
                    assetData = new AssetData(path, null, obj);
                    assetData.SetAsPackageAsset(path);
                }
                else
                {
                    TLogger.LogError($"Can not load the asset '{path}'");
                }
            }
#endif

            return assetData;
        }

        /// <summary>
        /// 资源是否存在
        /// </summary>
        /// <param name="path">通过右键菜单Get Asset Path获取的路径</param>
        /// <returns>true：存在，false：不存在</returns>
        public bool Exists(string path)
        {
#if ASSETBUNDLE_ENABLE
            return _assetPath2BundleDatas.ContainsKey(path);
#elif UNITY_EDITOR
            return File.Exists($"{AssetRootPath}/{path}");
#else
            return false;
#endif
        }

        /// <summary>
        /// 异步获取Asset
        /// </summary>
        /// <param name="path">通过右键菜单Get Asset Path获取的路径</param>
        /// <param name="withSubAssets">是否加载子Asset，针对Sprite图集</param>
        /// <param name="onComplete">加载回调</param>
        /// <returns>Asset数据</returns>
        public void GetAssetAtPathAsync(string path, bool withSubAssets, System.Action<AssetData> onComplete)
        {
#if ASSETBUNDLE_ENABLE
            AssetData assetData = null;
            AssetBundleData bundleData = FindAssetBundle(path);
            if (bundleData != null)
            {
                assetData = bundleData.GetAsset(path);
                if (assetData != null)
                {
                    assetData.LoadAsync(onComplete, withSubAssets);
                }
                else
                {
                    assetData = new AssetData(path, bundleData);
                    bundleData.SetAsset(path, assetData);
                    assetData.LoadAsync(onComplete, withSubAssets);
                }
            }
            else
                onComplete(null);
#endif
        }

        /// <summary>
        /// 获取场景Asset
        /// </summary>
        /// <param name="sceneName">场景名</param>
        /// <returns>Asset数据</returns>
        public AssetData GetSceneAsset(string sceneName, LoadSceneMode mode)
        {
            AssetData assetData = null;

#if ASSETBUNDLE_ENABLE
            AssetBundleData bundleData = FindAssetBundle(sceneName);
            if (bundleData != null)
            {
                assetData = new AssetData(sceneName, bundleData);
                assetData.LoadScene(mode);
            }
            else
            {
                TLogger.LogError($"Can not load scene asset '{sceneName}'");
            }
#else
            assetData = new AssetData(sceneName);
            assetData.LoadScene(mode);
#endif

            return assetData;
        }

        AssetBundleData FindAssetBundle(string path)
        {
            string bundleName;
            AssetBundleData assetBundleData = null;
            if (_assetPath2BundleDatas.TryGetValue(path, out bundleName))
            {
                if (!_bundleDatas.TryGetValue(bundleName, out assetBundleData))
                {
                    TLogger.LogError($"Can not get AssetBundleData with AssetBundle '{bundleName}'!");
                }
            }
            else
            {
                TLogger.LogError($"Can not find '{path}' in any AssetBundle!");
            }
            return assetBundleData;
        }
    }
}
