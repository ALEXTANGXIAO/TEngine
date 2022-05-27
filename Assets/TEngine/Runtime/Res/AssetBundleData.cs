using UnityEngine;
using System.Collections.Generic;

namespace TEngine
{
    /// <summary>
    /// 封装的AssetBundle Plus
    /// </summary>
    public class AssetBundleData
    {
        public AssetBundle Bundle;
        public AssetBundleData[] Dependencies;

        private AssetData[] _assets = null;
        private Dictionary<string, int> _path2Index = null;

        internal static ulong Offset;

        string _name;
        float _unloadTimer;
        int _refCount;
        int _depCount;
        bool _isInvokeLoad;
        bool _bLoaded;

        public int RefCount => _refCount;
        public int DepCount => _depCount;

        class AsyncLoadRequest
        {
            public AssetBundleCreateRequest Request;
            public int DependedRefCount;
        }
        AsyncLoadRequest _asyncLoadRequest;
        private event System.Action<AssetBundleData> _onAsyncLoadComplete;

        public string Name
        {
            get
            {
                return _name;
            }
        }

        /// <summary>
        /// 是否能被卸载
        /// </summary>
        public bool Unloadable
        {
            get
            {
                return Bundle != null && _refCount <= 0 && _depCount <= 0;
            }
        }

        /// <summary>
        /// 是否正在加载
        /// </summary>
        public bool IsAsyncLoading => _asyncLoadRequest != null;

        public bool IsLoadComplete => _bLoaded;

        public event System.Action<AssetBundleData> OnAsyncLoadComplete
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

        public AssetBundleData(string name)
        {
            Bundle = null;
            _name = name;
            _unloadTimer = 0f;
            _refCount = 0;
            _depCount = 0;
            _asyncLoadRequest = null;
            _isInvokeLoad = false;
            _bLoaded = false;
        }

        public void InitAssets(string[] assets)
        {
            if (assets.Length > 0)
            {
                _assets = new AssetData[assets.Length];
                _path2Index = new Dictionary<string, int>();

                for (int i = 0; i < assets.Length; ++i)
                {
                    _path2Index.Add(assets[i], i);
                    _assets[i] = null;
                }
            }
        }

        public AssetData GetAsset(string assetPath)
        {
            if (_assets != null)
            {
                if (_path2Index.ContainsKey(assetPath))
                    return _assets[_path2Index[assetPath]];
                else
                {
                    TLogger.LogError($"Try to get asset data '{assetPath}' which not in the bundle '{_name}'");
                }
            }
            else
            {
                TLogger.LogError($"Try to get asset data in the bundle '{_name}' which not include any asset");
            }

            return null;
        }

        public void SetAsset(string assetPath, AssetData assetData)
        {
            if (_assets != null)
            {
                if (_path2Index.ContainsKey(assetPath))
                {
                    _assets[_path2Index[assetPath]] = assetData;
                }
                else
                {
                    TLogger.LogError($"Try to set asset data '{assetPath}' which not in the bundle '{_name}'");
                }
            }
            else
            {
                TLogger.LogError($"Try to set asset data in the bundle '{_name}' which not include any asset");
            }
        }

        /// <summary>
        /// 同步加载AssetBundle接口
        /// </summary>
        /// <param name="bDepended">该AssetBundle是否被其他AssetBundle依赖</param>
        public void Load(bool bDepended = false)
        {
            if (Bundle == null)
            {
                // 置标志，用于循环依赖检测
                _isInvokeLoad = true;

                for (int i = 0; i < Dependencies.Length; ++i)
                {
                    if (!Dependencies[i]._isInvokeLoad)
                    {
                        Dependencies[i].Load(true);
                    }
                    else
                    {
                        TLogger.LogWarning($"Dependency cycle detected for '{Dependencies[i]._name}'");
                    }
                }
                Bundle = AssetBundle.LoadFromFile(FileSystem.GetAssetBundlePathInVersion(_name), 0, Offset);

                _isInvokeLoad = false;
            }

            if (Bundle != null)
            {
                if (bDepended)
                {
                    AddDepRef();
                }
            }
            else
            {
                TLogger.LogError($"Can not load AssetBundle '{_name}'!");
            }
        }

        /// <summary>
        /// 异步加载AssetBundle接口
        /// </summary>
        /// <param name="onComplete">加载回调</param>
        /// <param name="bDepended">该AssetBundle是否被其他AssetBundle依赖</param>
        public void LoadAsync(System.Action<AssetBundleData> onComplete, bool bDepended = false)
        {
            _onAsyncLoadComplete -= onComplete;
            _onAsyncLoadComplete += onComplete;

            if (Bundle == null)
            {
                if (_asyncLoadRequest == null)
                {
                    _isInvokeLoad = true;

                    for (int i = 0; i < Dependencies.Length; ++i)
                    {
                        if (!Dependencies[i]._isInvokeLoad)
                        {
                            Dependencies[i].LoadAsync(OnDependenciesAsyncLoadComplete, true);
                        }
                        else
                        {
                            TLogger.LogWarning($"Dependency cycle detected for '{Dependencies[i]._name}'");
                        }
                    }
                    _asyncLoadRequest = new AsyncLoadRequest
                    {
                        Request = AssetBundle.LoadFromFileAsync(FileSystem.GetAssetBundlePathInVersion(_name), 0, Offset),
                        DependedRefCount = bDepended ? 1 : 0
                    };
                    _asyncLoadRequest.Request.completed += OnAssetBundleLoadComplete;

                    _isInvokeLoad = false;
                }
                else
                {
                    if (bDepended)
                    {
                        ++_asyncLoadRequest.DependedRefCount;
                    }
                }
            }
            else
            {
                if (bDepended)
                {
                    AddDepRef();
                }

                OnDependenciesAsyncLoadComplete(this);
            }
        }

        /// <summary>
        /// 卸载AssetBundle及其中包含的Asset
        /// </summary>
        /// <param name="bForce">强制卸载</param>
        public void Unload(bool bForce = false)
        {
            // 当仅作为依赖时_assets为空
            if (_assets != null)
            {
                for (int i = 0; i < _assets.Length; ++i)
                {
                    if (_assets[i] != null)
                    {
                        _assets[i].Unload();
                        _assets[i] = null;
                    }
                }
            }
            if (Bundle != null)
            {
                Bundle.Unload(true);
                Bundle = null;
                TLogger.LogInfo($"Unload Bundle {_name}");
            }
            _onAsyncLoadComplete = null;
            _asyncLoadRequest = null;
            _isInvokeLoad = false;
            _bLoaded = false;

            if (!bForce)
            {
                for (int i = 0; i < Dependencies.Length; ++i)
                {
                    Dependencies[i].DecDepRef();
                }
            }
        }

        //不卸载bundle相关的资源，防止出错
        public void UnloadBundleFalse()
        {
            if (_assets != null)
            {
                for (int i = 0; i < _assets.Length; ++i)
                {
                    if (_assets[i] != null)
                    {
                        _assets[i].Unload();
                        _assets[i] = null;
                    }
                }
            }
            if (Bundle != null)
            {
                Bundle.Unload(false);
                Bundle = null;
                TLogger.LogInfo($"Unload Bundle {_name}");
            }
            _onAsyncLoadComplete = null;
            _asyncLoadRequest = null;
            _isInvokeLoad = false;
            _bLoaded = false;
        }

        /// <summary>
        /// 增加依赖计数
        /// </summary>
        /// <param name="count">引用计数增量</param>
        /// <remark>异步加载未完成时，存在另外的加载请求，此时加载请求里记录依赖次数</remark>
        public void AddDepRef(int count = 1)
        {
            _depCount += count;
        }

        /// <summary>
        /// 减依赖计数
        /// </summary>
        public void DecDepRef()
        {
            --_depCount;
            if (_depCount == 0)
            {
                _unloadTimer = 0f;
            }
            else if (_depCount < 0)
            {
                _depCount = 0;
                TLogger.LogWarning($"{_name} _depCount < 0");
            }
        }

        /// <summary>
        /// 增加引用计数
        /// </summary>
        public void AddRef()
        {
            ++_refCount;
            //TLogger.LogInfo($"Add AssetBundle {_name} refCount = {_refCount}");
        }

        /// <summary>
        /// 减引用计数
        /// </summary>
        public void DecRef(bool bNoDelay)
        {
            --_refCount;
            //TLogger.LogInfo($"Dec AssetBundle {_name} refCount = {_refCount}");
            if (_refCount == 0)
            {
                _unloadTimer = bNoDelay ? System.Single.MaxValue : 0f;
            }
            else if (_refCount < 0)
            {
                _refCount = 0;
                TLogger.LogWarning($"{_name} _refCount < 0");
            }
        }

        /// <summary>
        /// 卸载时间更新
        /// </summary>
        /// <param name="delta">游戏时间帧间隔</param>
        /// <returns>从引用计数为0开始到现在的时长</returns>
        public float Update(float delta)
        {
            if (Unloadable)
            {
                _unloadTimer += delta;
            }

            return _unloadTimer;
        }

        void OnAssetBundleLoadComplete(AsyncOperation asyncOperation)
        {
            if (asyncOperation.isDone)
            {
                if (_asyncLoadRequest != null && _asyncLoadRequest.Request == asyncOperation)
                {
                    AssetBundleCreateRequest assetBundleCreateRequest = asyncOperation as AssetBundleCreateRequest;
                    if (assetBundleCreateRequest != null)
                    {
                        Bundle = assetBundleCreateRequest.assetBundle;
                        if (Bundle != null)
                        {
                            AddDepRef(_asyncLoadRequest.DependedRefCount);
                            OnDependenciesAsyncLoadComplete(this);
                        }
                        else
                        {
                            TLogger.LogError($"Can not load AssetBundle '{_name}' asynchronously!");
                        }
                    }

                    _asyncLoadRequest = null;
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
        void OnDependenciesAsyncLoadComplete(AssetBundleData depBundle)
        {
            bool bDepAllLoaded = true;
            for (int i = 0; i < Dependencies.Length; ++i)
            {
                if (Dependencies[i].Bundle == null)
                {
                    bDepAllLoaded = false;
                    break;
                }
            }

            if (Bundle != null && bDepAllLoaded && _onAsyncLoadComplete != null)
            {
                _onAsyncLoadComplete(this);
                _bLoaded = true;
            }
        }
    }
}
