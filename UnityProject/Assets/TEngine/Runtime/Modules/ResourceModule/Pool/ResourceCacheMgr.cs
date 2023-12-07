using System;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;
using Object = UnityEngine.Object;

namespace TEngine
{
    public class ResourceCacheMgr
    {
        private static ResourceCacheMgr _instance;

        public static ResourceCacheMgr Instance => _instance ??= new ResourceCacheMgr();

        private readonly Dictionary<string, ResCacheData> _cachePool = new Dictionary<string, ResCacheData>();
        private readonly Dictionary<string, ResCacheData> _persistCachePool = new Dictionary<string, ResCacheData>();
        private bool _enableLog = false;
        private readonly List<ResourceCacheConfig> _needCacheResList = new List<ResourceCacheConfig>();
        private readonly List<string> _needPersistResList = new List<string>();
        private GameTimerTick _tickCheckExpire;
        private readonly List<string> _listToDel = new List<string>();
        private bool _pauseCache;

        public int PersistCachePoolCount = 30;

        public bool PauseCache
        {
            set => _pauseCache = value;
            get => _pauseCache;
        }

        internal Dictionary<string, ResCacheData> CachePoolAllData => _cachePool;

        internal Dictionary<string, ResCacheData> PersistPoolAllData => _persistCachePool;

        public int CacheCount => _cachePool.Count;

        public int PersistCount => _persistCachePool.Count;

        public void SetLogEnable(bool enable) => _enableLog = enable;

        /// <summary>
        /// 注册持久化的资源。
        /// </summary>
        /// <param name="resList">持久化的资源起始路径。存在这个路径开始的资源不会释放。</param>
        public void RegPersistResPath(List<string> resList) => _needPersistResList.AddRange((IEnumerable<string>)resList);

        public void InitDefaultCachePool()
        {
            if (!ResourceManager.EnableGoPool)
            {
                return;
            }

            string assetLocation = "need_cache_list";
            
            GameModule.Resource.LoadAssetAsync<TextAsset>(assetLocation, handle =>
            {
                if (handle.AssetObject == null)
                {
                    return;
                }
                TextAsset textAsset = handle.AssetObject as TextAsset;
                if (textAsset == null)
                {
                    return;
                }
                List<ResourceCacheConfig> list = Utility.Json.ToObject<List<ResourceCacheConfig>>(textAsset.text);
                foreach (var config in list)
                {
                    Instance.RegCacheResPath(config.ResPath, config.CacheTime, config.MaxPoolCnt, config.PoolGoFreeTime, config.MinPoolCnt);
                }
            });
        }
        
        /// <summary>
        /// 注册缓存池的资源。
        /// </summary>
        /// <param name="resPath">缓存池的资源起始路径。存在这个路径开始的资源则加入对象池。</param>
        /// <param name="cacheTime">缓存时间。</param>
        /// <param name="maxPoolCnt">缓存池最大容量。</param>
        /// <param name="poolGoFreeTime">缓存池释放时间。</param>
        /// <param name="minPoolCnt">缓存池最小容量。</param>
        /// <remarks> ResourceCacheMgr.Instance.RegCacheResPath("Assets/AssetRaw/Effects"); 所有特效相关资源都加入对象池。 </remarks>
        public void RegCacheResPath(
            string resPath,
            int cacheTime = 0,
            int maxPoolCnt = 30,
            int poolGoFreeTime = 120,
            int minPoolCnt = 0)
        {
            if (_enableLog)
            {
                Log.Warning($"RegCacheResPath: {resPath} cacheTime: {cacheTime} maxPoolCnt: {maxPoolCnt} poolGoFreeTime: {poolGoFreeTime} minPoolCnt: {minPoolCnt}");
            }
            _needCacheResList.Add(new ResourceCacheConfig(resPath, cacheTime, maxPoolCnt, poolGoFreeTime, minPoolCnt));
        }

        public void RefreshCacheTime(string resPath)
        {
            if (!_cachePool.TryGetValue(resPath, out ResCacheData resCacheData))
            {
                return;
            }
            resCacheData.CacheRefreshTime = Time.time;
        }

        public bool IsResourceCached(string resPath)
        {
            string key = resPath;
            return _cachePool.ContainsKey(key) || _persistCachePool.ContainsKey(key);
        }

        public Object GetCacheDataByLocation(string location)
        {
            AssetInfo assetInfo = GameModule.Resource.GetAssetInfo(location);
            return GetCacheData(assetInfo.AssetPath);
        }
        
        public Object GetCacheData(string resPath)
        {
            string key = resPath;
            if (_cachePool.TryGetValue(key, out ResCacheData resCacheData))
            {
                resCacheData.CacheRefreshTime = Time.time;
                return resCacheData.Asset;
            }
            else if (_persistCachePool.TryGetValue(key, out resCacheData))
            {
                resCacheData.CacheRefreshTime = Time.time;
                return resCacheData.Asset;
            }
            return null;
        }

        public int GetMaxGoPoolCnt(string resPath)
        {
            return IsNeedCache(resPath, out int _, out var maxPoolCnt) ? maxPoolCnt : 0;
        }

        public bool GetCacheCfg(
            string resPath,
            out int maxPoolCnt,
            out int cacheFreeTime,
            out int minPoolCnt)
        {
            return IsNeedCache(resPath, out int _, out maxPoolCnt, out cacheFreeTime, out minPoolCnt);
        }

        public bool IsNeedCache(string resPath, out int cacheTime)
        {
            return IsNeedCache(resPath, out cacheTime, out _);
        }

        public bool IsNeedCache(string resPath, out int cacheTime, out int maxPoolCnt)
        {
            return IsNeedCache(resPath, out cacheTime, out maxPoolCnt, out _, out _);
        }

        public bool IsNeedCache(
            string resPath,
            out int cacheTime,
            out int maxPoolCnt,
            out int poolGoFreeTime,
            out int minPoolCnt)
        {
            cacheTime = 0;
            maxPoolCnt = 0;
            poolGoFreeTime = 0;
            minPoolCnt = 0;
            foreach (var needCacheRes in _needCacheResList)
            {
                if (resPath.StartsWith(needCacheRes.ResPath))
                {
                    cacheTime = needCacheRes.CacheTime;
                    maxPoolCnt = needCacheRes.MaxPoolCnt;
                    poolGoFreeTime = needCacheRes.PoolGoFreeTime;
                    minPoolCnt = needCacheRes.MinPoolCnt;
                    return true;
                }
            }

            if (!IsNeedPersist(resPath))
            {
                return false;
            }
            cacheTime = 0;
            maxPoolCnt = PersistCachePoolCount;
            poolGoFreeTime = 0;
            minPoolCnt = 0;
            return true;
        }

        public bool IsNeedPersist(string resPath)
        {
            foreach (var persistKey in _needPersistResList)
            {
                if (resPath.IndexOf(persistKey, StringComparison.Ordinal) >= 0)
                {
                    return true;
                }
            }
            return false;
        }

        public List<string> GetAllPersistCache()
        {
            List<string> allPersistCache = new List<string>();
            using Dictionary<string, ResCacheData>.Enumerator enumerator = _persistCachePool.GetEnumerator();
            while (enumerator.MoveNext())
            {
                allPersistCache.Add(enumerator.Current.Key);
            }
            return allPersistCache;
        }

        public bool AddCache(
            string resPath,
            Object obj,
            int cacheTime,
            bool forcePersist = false)
        {
            if (null == obj)
            {
                Log.Info("add cache failed, resPath: {0}", resPath);
                return false;
            }

            bool flag = IsNeedPersist(resPath) || forcePersist;
            string str = resPath;
            if (_persistCachePool.ContainsKey(str))
            {
                return true;
            }
            if (_cachePool.TryGetValue(str, out ResCacheData resCacheData1))
            {
                if (flag)
                {
                    _cachePool.Remove(str);
                    _persistCachePool.Add(str, resCacheData1);
                }

                return true;
            }

            if (PauseCache)
            {
                return true;
            }
            ResCacheData resCacheData2 = MemoryPool.Acquire<ResCacheData>();
            resCacheData2.Asset = obj;
            resCacheData2.AutoExpire = false;
            resCacheData2.CacheRefreshTime = Time.time;
            resCacheData2.CacheExpireTime = cacheTime;
            if (cacheTime > 0)
            {
                resCacheData2.AutoExpire = true;
            }
            if (flag)
            {
                _persistCachePool.Add(str, resCacheData2);
            }
            else
            {
                _cachePool.Add(str, resCacheData2);
            }

            return true;
        }

        public void RemoveAllCache() => _cachePool.Clear();

        public void RemoveCache(string resPath)
        {
            if (!_cachePool.TryGetValue(resPath,out ResCacheData resCacheData))
            {
                return;
            }
            MemoryPool.Release(resCacheData);
            _cachePool.Remove(resPath);
        }

        public int GetCacheCount() => _cachePool.Count + _persistCachePool.Count;

        internal void Init() => _tickCheckExpire = new GameTimerTick(1f,CheckExpireCache);

        internal void OnUpdate() => _tickCheckExpire.OnUpdate();

        private void CheckExpireCache()
        {
            float time = Time.time;
            ResourcePool instance = ResourcePool.Instance;
            using Dictionary<string, ResCacheData>.Enumerator enumerator = _cachePool.GetEnumerator();
            while (enumerator.MoveNext())
            {
                KeyValuePair<string, ResCacheData> current = enumerator.Current;
                string key1 = current.Key;
                current = enumerator.Current;
                ResCacheData resCacheData = current.Value;
                if (resCacheData.AutoExpire && resCacheData.CacheRefreshTime + resCacheData.CacheExpireTime < time)
                {
                    if (resCacheData.Asset is GameObject && !instance.IsNeedAutoFree(key1))
                    {
                        resCacheData.CacheRefreshTime = Time.time;
                    }
                    else
                    {
                        List<string> listToDel = _listToDel;
                        current = enumerator.Current;
                        string key2 = current.Key;
                        listToDel.Add(key2);
                    }
                }
            }

            foreach (var resPath in _listToDel)
            {
                MemoryPool.Release(_cachePool[resPath]);
                _cachePool.Remove(resPath);
                ResourcePool.Instance.FreeGoByResPath(resPath);
            }

            _listToDel.Clear();
        }
    }
}