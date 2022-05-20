using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.U2D;

namespace TEngine
{
    /// <summary>
    /// 封装的AssetBundle
    /// </summary>
    public class AssetData
    {
        private AssetBundleData _refBundle;
        private Object _assetObject;
        private Dictionary<string, UnityEngine.Object> _subAssetObjects;
        private string _kPath;
        private string _fPath;
        private string _name;
        private int _refCount;
        private bool _useSubAsset;
        private AsyncOperation _asyncLoadRequest;
        private event System.Action<AssetData> _onAsyncLoadComplete;

        public UnityEngine.Object AssetObject => _assetObject;

        public UnityEngine.Object this[string key]
        {
            get
            {
                UnityEngine.Object assetObject = null;

                if (_subAssetObjects != null)
                {
                    if (!_subAssetObjects.TryGetValue(key, out assetObject))
                    {
                        if (_assetObject != null && _assetObject is SpriteAtlas atlas)
                        {
                            assetObject = atlas.GetSprite(key);
                            if (assetObject != null)
                            {
                                _subAssetObjects.Add(key, assetObject);
                            }
                            else
                            {
                                TLogger.LogError($"Can not get sub asset({key}) from {_fPath}");
                            }
                        }
                    }
                }

                return assetObject;
            }
        }

        public string Path => _kPath;

        public string FullPath => _fPath;

        /// <summary>
        /// Asset名
        /// </summary>
        public string Name => _name;

        /// <summary>
        /// 异步操作对象
        /// </summary>
        public AsyncOperation AsyncOp
        {
            get
            {
                return _asyncLoadRequest;
            }
        }

        public event System.Action<AssetData> OnAsyncLoadComplete
        {
            add
            {
                _onAsyncLoadComplete += value;
            }
            remove
            {
                _onAsyncLoadComplete -= value;
            }
        }

        public AssetData(string path = "", AssetBundleData refBundle = null, UnityEngine.Object assetObject = null)
        {
            _kPath = path;
            if (!string.IsNullOrEmpty(_kPath))
            {
                int ei = _kPath.LastIndexOf('/');
                _name = ei >= 0 ? _kPath.Substring(ei + 1) : _kPath;
                _fPath = $"{AssetConfig.AssetRootPath}/{_kPath}";
            }
            else
            {
                _fPath = System.String.Empty;
            }

            _assetObject = assetObject;
            _refBundle = refBundle;
            if (refBundle != null)
            {
                _refBundle.AddRef();
            }
        }

        internal void SetAsPackageAsset(string path)
        {
            _fPath = path;
        }

        /// <summary>
        /// 增引用计数
        /// </summary>
        public void AddRef()
        {
            ++_refCount;
#if UNITY_EDITOR
            //TLogger.LogInfo($"Add AssetData {_fPath} _refCount = {_refCount}");
#endif
        }

        /// <summary>
        /// 减引用计数，当引用计数为0时减其AssetBundle的引用计数
        /// </summary>
        public void DecRef(bool bNoDelay = false)
        {
            --_refCount;
#if UNITY_EDITOR
            //TLogger.LogInfo($"Dec AssetData {_fPath} _refCount = {_refCount}");
#endif
            if (_refCount <= 0)
            {
                if (_refBundle != null)
                {
                    _refBundle.DecRef(bNoDelay);
                    _refBundle.SetAsset(_kPath, null);
                    Unload();
                }
            }
        }

        /// <summary>
        /// 同步加载Asset
        /// </summary>
        /// <param name="withSubAssets">是否加载子Asset，针对Sprite图集</param>
        public void LoadAsset(bool withSubAssets = false)
        {
            _useSubAsset = withSubAssets;

            if (_refBundle.Bundle == null)
                _refBundle.Load();

            if (_refBundle.Bundle != null)
            {
                if (_useSubAsset)
                {
                    UnityEngine.Object[] subAssets = _refBundle.Bundle.LoadAssetWithSubAssets(_fPath);
                    if (subAssets != null && subAssets.Length > 0)
                    {
                        ProcessSubAssets(subAssets);
                    }
                    else
                    {
                        TLogger.LogError($"Can not load the asset '{_fPath}'");
                    }
                }
                else
                {
                    UnityEngine.Object assetObject = _refBundle.Bundle.LoadAsset(_fPath);
                    if (assetObject != null)
                    {
                        _assetObject = assetObject;
                    }
                    else
                    {
                        TLogger.LogError($"Can not load the asset '{_fPath}'");
                    }
                }
            }
        }

        /// <summary>
        /// 异步加载Asset
        /// </summary>
        /// <param name="onComplete">加载回调</param>
        /// <param name="withSubAssets">是否加载子Asset，针对Sprite图集</param>
        public void LoadAsync(System.Action<AssetData> onComplete, bool withSubAssets = false)
        {
            _onAsyncLoadComplete -= onComplete;
            _onAsyncLoadComplete += onComplete;
            _useSubAsset = withSubAssets;

            if (!_refBundle.IsLoadComplete)
            {
                _refBundle.LoadAsync(OnAssetBundleDataLoadComplete);
            }
            else
            {
                OnAssetBundleDataLoadComplete(_refBundle);
            }
        }

        void OnAssetBundleDataLoadComplete(AssetBundleData bundleData)
        {
            if (_refBundle == bundleData)
            {
                _refBundle.OnAsyncLoadComplete -= OnAssetBundleDataLoadComplete;

                if (_useSubAsset)
                {
                    if (_subAssetObjects == null)
                    {
                        if (_asyncLoadRequest == null)
                        {
                            _asyncLoadRequest = bundleData.Bundle.LoadAssetWithSubAssetsAsync(_fPath);
                            _asyncLoadRequest.completed += OnAssetLoadComplete;
                        }
                    }
                    else
                    {
                        _onAsyncLoadComplete(this);
                    }
                }
                else
                {
                    if (_assetObject == null)
                    {
                        if (_asyncLoadRequest == null)
                        {
                            _asyncLoadRequest = bundleData.Bundle.LoadAssetAsync(_fPath);
                            _asyncLoadRequest.completed += OnAssetLoadComplete;
                        }
                    }
                    else
                    {
                        _onAsyncLoadComplete(this);
                    }
                }
            }
            else
                TLogger.LogError("Return a mismatch AssetBundleData for OnAssetBundleDataLoadComplete");
        }

        void OnAssetLoadComplete(AsyncOperation asyncOperation)
        {
            if (asyncOperation.isDone)
            {
                if (_asyncLoadRequest != null && _asyncLoadRequest == asyncOperation)
                {
                    AssetBundleRequest assetBundleRequest = asyncOperation as AssetBundleRequest;
                    if (assetBundleRequest != null)
                    {
                        if (_useSubAsset)
                        {
                            if (assetBundleRequest.allAssets != null && assetBundleRequest.allAssets.Length > 0)
                            {
                                ProcessSubAssets(assetBundleRequest.allAssets);
                            }
                            else
                            {
                                TLogger.LogError($"Can not load the asset '{_fPath}'");
                            }
                        }
                        else
                        {
                            _assetObject = assetBundleRequest.asset;
                            if (_assetObject == null)
                            {
                                TLogger.LogError($"Can not load the asset '{_fPath}'");
                            }
                        }
                    }

                    _asyncLoadRequest = null;
                    _onAsyncLoadComplete(this);
                }
                else
                {
                    TLogger.LogError("Return a mismatch asyncOperation for AssetBundleCreateRequest");
                }
            }
            else
            {
                TLogger.LogError("Return a not done AsyncOperation for AssetBundleCreateRequest");
            }
        }


        internal void ProcessSubAssets(UnityEngine.Object[] subAssets)
        {
            _subAssetObjects = new Dictionary<string, UnityEngine.Object>();

            if (subAssets[0] is SpriteAtlas) // SpriteAtlas
            {
                _assetObject = subAssets[0];
                // load all 懒加载//TODO
                SpriteAtlas spriteAtlas = subAssets[0] as SpriteAtlas;
                Sprite[] sprites = new Sprite[spriteAtlas.spriteCount];
                int count = spriteAtlas.GetSprites(sprites);
                for (int i = 0; i < count; ++i)
                {
                    string name = sprites[i].name;
                    int ci = name.LastIndexOf("(Clone)", System.StringComparison.Ordinal);
                    if (ci >= 0)
                        name = name.Substring(0, ci);
                    _subAssetObjects.Add(name, sprites[i]);
                }
            }
            else 
            {
                for (int i = 0; i < subAssets.Length; ++i)
                {
                    if (subAssets[i].GetType() != typeof(UnityEngine.Texture2D))
                    {
                        _subAssetObjects.Add(subAssets[i].name, subAssets[i]);
                    }
                }
            }
        }

        /// <summary>
        /// 加载场景
        /// </summary>
        /// <remarks>先加载场景AssetBundle，才能使用SceneManager中的加载接口</remarks>
        public void LoadScene(LoadSceneMode mode)
        {
            if (_refBundle != null && _refBundle.Bundle == null)
                _refBundle.Load();

            _asyncLoadRequest = SceneManager.LoadSceneAsync(_name, mode);
            if (_asyncLoadRequest != null)
                _asyncLoadRequest.allowSceneActivation = false;
        }

        /// <summary>
        /// 卸载Asset
        /// </summary>
        /// <param name="bForce">是否强制卸载</param>
        public void Unload(bool bForce = false)
        {
            if (bForce || _refCount <= 0)
            {
                _assetObject = null;
                _subAssetObjects?.Clear();
                _refBundle = null;
                _onAsyncLoadComplete = null;
                _asyncLoadRequest = null;
            }
            else
            {
                TLogger.LogWarning($"Try to unload refcount > 0 asset({_fPath})!");
            }
        }
    }
}
