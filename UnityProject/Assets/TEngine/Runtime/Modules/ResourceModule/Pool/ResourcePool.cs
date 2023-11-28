#region Class Documentation
/************************************************************************************************************
Class Name:     ResourcePool对象池。
Type:           Util, Singleton

Example:
                //注册 - 添加对象池路径，满足这个路径开头的GameObject开启对象池：
                ResourceCacheMgr.Instance.RegCacheResPath("Assets/AssetRaw/Effects");
                
                //正常引用资源。
                var obj = await GameModule.Resource.LoadAssetAsync<GameObject>("Sprite",parent:transform);
                
                //回收资源
                GameModule.Resource.FreeGameObject(obj);
                
                //删除资源 放心资源不存在泄露。
                Unity Engine.Object。Destroy(obj);
************************************************************************************************************/
#endregion

using System.Collections.Generic;
using UnityEngine;
using YooAsset;

namespace TEngine
{
    internal class ResourcePool
    {
        private static ResourcePool _instance;

        internal static ResourcePool Instance => _instance ??= new ResourcePool();

        private readonly Dictionary<string, GoPoolNode> _cacheGo = new Dictionary<string, GoPoolNode>();
        private readonly Dictionary<int, GoProperty> _goProperty = new Dictionary<int, GoProperty>();
        private readonly List<DelayDestroyGo> _delayDestroyList = new List<DelayDestroyGo>();
        private readonly List<DelayDestroyGo> _freeDelayNode = new List<DelayDestroyGo>();
        private readonly List<string> _listToDelete = new List<string>();
        private Transform _poolRootTrans;
        private uint _frameID = 1;
        private float _frameTime;
        private float _tickLogTime;
        private bool _pauseGoPool;
        private int _totalPoolObjectCount;
        public bool LogWhenPoolFull = true;

        public bool PoolCacheFreeEnable = true;

        public int TotalPoolObjectCount => _totalPoolObjectCount;

        public int DelayDestroyCount => _delayDestroyList.Count;

        public int FreedDestroyCount => _freeDelayNode.Count;

        public static float PoolWaitReuseTime = 0.2f;

        public bool PauseGoPool
        {
            set => _pauseGoPool = value;
            get => _pauseGoPool;
        }

        public void OnAwake()
        {
            GetRootTrans();
            ResourceCacheMgr.Instance.Init();
        }

        private Transform GetRootTrans()
        {
            if (!Application.isPlaying)
            {
                return null;
            }

            if (_poolRootTrans != null)
            {
                return _poolRootTrans;
            }

            GameObject target = new GameObject("_GO_POOL_ROOT");
            Object.DontDestroyOnLoad(target);
            _poolRootTrans = target.transform;
            return _poolRootTrans;
        }

        public void OnDestroy()
        {
            FreeAllCacheAndGo();
        }

        public void AddCacheGo(string resPath, GameObject go)
        {
            GoProperty property;
            property.ResPath = resPath;
            property.Layer = go.layer;
            property.FrameID = _frameID;
            property.FrameTime = _frameTime;
            property.InitScale = go.transform.localScale;
            AddCacheGo(resPath, go, property);
        }

        private DelayDestroyGo AllocDelayNode()
        {
            if (_freeDelayNode.Count <= 0)
            {
                return new DelayDestroyGo();
            }
            int index = _freeDelayNode.Count - 1;
            DelayDestroyGo delayDestroyGo = _freeDelayNode[index];
            _freeDelayNode.RemoveAt(index);
            return delayDestroyGo;
        }

        private void FreeDelayNode(DelayDestroyGo node) => _freeDelayNode.Add(node);

        public void DelayDestroy(GameObject go, GoProperty property, float delayTime)
        {
            if (delayTime <= 1.0 / 1000.0)
            {
                AddCacheGo(property.ResPath, go, property);
            }
            else
            {
                DelayDestroyGo delayDestroyGo = AllocDelayNode();
                delayDestroyGo.Asset = go;
                delayDestroyGo.HashId = go.GetHashCode();
                delayDestroyGo.Property = property;
                float num = Time.time + delayTime;
                delayDestroyGo.DestroyTime = num;
                int index1 = -1;
                for (int index2 = 0; index2 < _delayDestroyList.Count; ++index2)
                {
                    if (_delayDestroyList[index2].DestroyTime >= num)
                    {
                        index1 = index2;
                        break;
                    }
                }

                if (index1 >= 0)
                {
                    _delayDestroyList.Insert(index1, delayDestroyGo);
                }
                else
                {
                    _delayDestroyList.Add(delayDestroyGo);
                }
            }
        }

        public bool AddCacheGo(string resPath, GameObject go, GoProperty property)
        {
            if (_poolRootTrans == null)
            {
                DoDestroy(go);
                return false;
            }

            go.SetActive(false);
            GoPoolNode orNewResourceNode = GetOrNewResourceNode(resPath);
            if (!orNewResourceNode.AddCacheGo(go))
            {
                if (LogWhenPoolFull)
                {
                    Log.Info("cache is full, ResPath[{0}] Max cache count:{1}", resPath, orNewResourceNode.MaxCacheCnt);
                }

                DoDestroy(go);
                return false;
            }

            go.transform.SetParent(GetRootTrans(), false);
            int hashCode = go.GetHashCode();
            property.FrameID = _frameID;
            property.FrameTime = _frameTime;
            AddGoProperty(hashCode, property);
            ++_totalPoolObjectCount;
            if (orNewResourceNode.CacheFreeTime != 0)
            {
                orNewResourceNode.PoolGoRefreshTime = _frameTime;
            }

            return true;
        }

        public void AddNewRecycleProperty(GameObject go, string resPath, Vector3 initScale)
        {
            if (PauseGoPool)
            {
                return;
            }

            GoProperty property;
            property.ResPath = resPath;
            property.Layer = go.layer;
            property.FrameID = _frameID;
            property.FrameTime = _frameTime;
            property.InitScale = initScale;
            AddGoProperty(go.GetHashCode(), property);
        }

        public GameObject AllocCacheGoByLocation(
            string location,
            Transform parentTrans = null,
            bool haveLocalPos = false,
            Vector3 localPos = default,
            Quaternion localRot = default,
            bool initEnable = true)
        {
            AssetInfo assetInfo = GameModule.Resource.GetAssetInfo(location);
            return AllocCacheGo(assetInfo.AssetPath, parentTrans, haveLocalPos, localPos, localRot, initEnable);
        }

        public GameObject AllocCacheGo(
            string resPath,
            Transform parentTrans = null,
            bool haveLocalPos = false,
            Vector3 localPos = default,
            Quaternion localRot = default,
            bool initEnable = true)
        {
            if (_cacheGo.TryGetValue(resPath, out GoPoolNode goPoolNode))
            {
                List<GameObject> listGo = goPoolNode.ListGameObjects;
                ResourceCacheMgr.Instance.GetCacheData(resPath);
                for (int index = 0; index < listGo.Count; ++index)
                {
                    GameObject gameObject = listGo[index];
                    if (gameObject == null)
                    {
                        --_totalPoolObjectCount;
                        listGo[index] = listGo[^1];
                        listGo.RemoveAt(listGo.Count - 1);
                        --index;
                    }
                    else
                    {
                        int hashCode = gameObject.GetHashCode();
                        if (!_goProperty.TryGetValue(hashCode, out GoProperty goProperty))
                        {
                            --_totalPoolObjectCount;
                            Log.Warning("AllocCacheGo Find property failed, bug [{0}]", gameObject.name);
                            listGo[index] = listGo[^1];
                            listGo.RemoveAt(listGo.Count - 1);
                            --index;
                        }
                        else if (goProperty.FrameTime > _frameTime || goProperty.FrameTime + PoolWaitReuseTime < _frameTime)
                        {
                            gameObject.transform.SetParent(null);
                            gameObject.transform.localScale = goProperty.InitScale;
                            if (PauseGoPool)
                            {
                                RemoveGoProperty(hashCode);
                            }

                            Transform transform = gameObject.transform;
                            transform.SetParent(parentTrans);
                            gameObject.layer = goProperty.Layer;
                            if (haveLocalPos)
                            {
                                transform.localPosition = localPos;
                                transform.localRotation = localRot;
                            }

                            gameObject.SetActive(initEnable);
                            listGo[index] = listGo[^1];
                            listGo.RemoveAt(listGo.Count - 1);
                            --_totalPoolObjectCount;
                            return gameObject;
                        }
                    }
                }
            }

            return null;
        }

        public static void FreeAllCacheAndGo()
        {
            ResourcePool.Instance.FreeAllCacheGo();
            ResourceCacheMgr.Instance.RemoveAllCache();
        }

        public static void FreeAllPoolGo()
        {
            Instance.FreeAllCacheGo();
        }

        public void FreeAllCacheGo()
        {
            using Dictionary<string, GoPoolNode>.Enumerator enumerator = _cacheGo.GetEnumerator();
            while (enumerator.MoveNext())
            {
                List<GameObject> listGo = enumerator.Current.Value.ListGameObjects;
                for (int index = 0; index < listGo.Count; ++index)
                {
                    GameObject go = listGo[index];
                    if (go != null)
                    {
                        go.transform.SetParent(null, false);
                        DoDestroy(go);
                    }
                }

                listGo.Clear();
            }

            _cacheGo.Clear();
            _goProperty.Clear();
            _totalPoolObjectCount = 0;
        }

        private GoPoolNode GetOrNewResourceNode(string resPath)
        {
            if (!_cacheGo.TryGetValue(resPath, out GoPoolNode orNewResourceNode))
            {
                orNewResourceNode = new GoPoolNode();
                ResourceCacheMgr.Instance.GetCacheCfg(resPath, out orNewResourceNode.MaxCacheCnt, out orNewResourceNode.CacheFreeTime,
                    out orNewResourceNode.MinCacheCnt);
                _cacheGo.Add(resPath, orNewResourceNode);
            }

            return orNewResourceNode;
        }

        private void AddGoProperty(int hashCode, GoProperty property)
        {
            if (!_goProperty.ContainsKey(hashCode))
                ++GetOrNewResourceNode(property.ResPath).GoRefCnt;
            _goProperty[hashCode] = property;
        }

        private void RemoveGoProperty(int hashCode)
        {
            if (!_goProperty.TryGetValue(hashCode, out GoProperty goProperty))
            {
                return;
            }

            --GetOrNewResourceNode(goProperty.ResPath).GoRefCnt;
            _goProperty.Remove(hashCode);
        }

        private void DoDestroy(GameObject go)
        {
            RemoveGoProperty(go.GetHashCode());
            Object.Destroy(go);
        }

        public bool IsNeedAutoFree(string resPath)
        {
            return !_cacheGo.TryGetValue(resPath, out GoPoolNode goPoolNode) || goPoolNode.GoRefCnt <= goPoolNode.ListGameObjects.Count;
        }

        public void FreeGoByResPath(string resPath)
        {
            if (!_cacheGo.TryGetValue(resPath, out GoPoolNode goPoolNode))
            {
                return;
            }

            List<GameObject> listGo = goPoolNode.ListGameObjects;
            Log.Assert(goPoolNode.GoRefCnt <= listGo.Count);
            foreach (var go in listGo)
            {
                if (!(go == null))
                {
                    DoDestroy(go);
                }
            }

            listGo.Clear();
            _cacheGo.Remove(resPath);
        }

        public bool IsExistInCache(string resPath)
        {
            return _cacheGo.TryGetValue(resPath, out GoPoolNode goPoolNode) && goPoolNode.GoRefCnt > 0;
        }

        public bool IsNeedRecycle(GameObject go, out GoProperty property, bool forceNoPool)
        {
            int hashCode = go.GetHashCode();
            if (!_goProperty.TryGetValue(hashCode, out property))
                return false;
            if (PauseGoPool)
            {
                RemoveGoProperty(hashCode);
                return false;
            }

            if (!ResourceCacheMgr.Instance.IsResourceCached(property.ResPath))
            {
                RemoveGoProperty(hashCode);
                return false;
            }

            if (!forceNoPool)
            {
                return true;
            }

            RemoveGoProperty(hashCode);
            return false;
        }

        public void ClearAllDelayDestroy()
        {
            List<DelayDestroyGo> delayDestroyList = _delayDestroyList;
            for (int index = 0; index < delayDestroyList.Count; ++index)
            {
                DelayDestroyGo node = delayDestroyList[index];
                AddCacheGo(node.Property.ResPath, node.Asset, node.Property);
                FreeDelayNode(node);
            }

            delayDestroyList.Clear();
            _freeDelayNode.Clear();
        }

        public void CheckPoolCacheFree()
        {
            if (!PoolCacheFreeEnable)
            {
                return;
            }

            using Dictionary<string, GoPoolNode>.Enumerator enumerator = _cacheGo.GetEnumerator();
            while (enumerator.MoveNext())
            {
                GoPoolNode goPoolNode = enumerator.Current.Value;
                string key = enumerator.Current.Key;
                if (goPoolNode.CacheFreeTime != 0 && goPoolNode.CacheFreeTime + goPoolNode.PoolGoRefreshTime < _frameTime)
                {
                    List<GameObject> listGo = goPoolNode.ListGameObjects;
                    for (int index = 0; index < listGo.Count; ++index)
                    {
                        GameObject go = listGo[index];
                        if (go != null)
                        {
                            go.transform.SetParent(null, false);
                            DoDestroy(go);
                        }
                    }

                    listGo.Clear();
                    _listToDelete.Add(key);
                }
            }

            foreach (var location in _listToDelete)
            {
                _cacheGo.Remove(location);
            }

            if (_listToDelete.Count <= 0)
            {
                return;
            }

            _listToDelete.Clear();
        }

        public void OnUpdate()
        {
            float time = Time.time;
            int num = -1;
            for (int index = 0; index < _delayDestroyList.Count; ++index)
            {
                DelayDestroyGo delayDestroy = _delayDestroyList[index];
                if (delayDestroy.DestroyTime <= time)
                {
                    num = index;
                    if (delayDestroy.Asset == null)
                    {
                        Log.Warning("delay destroy gameobject is freed: {0}", delayDestroy.Property.ResPath);
                        RemoveGoProperty(delayDestroy.HashId);
                    }
                    else
                    {
                        AddCacheGo(delayDestroy.Property.ResPath, delayDestroy.Asset, delayDestroy.Property);
                    }

                    {
                        FreeDelayNode(delayDestroy);
                    }
                }
                else
                {
                    break;
                }
            }

            if (num >= 0)
            {
                _delayDestroyList.RemoveRange(0, num + 1);
            }

            CheckPoolCacheFree();

            LateUpdate();
            
            ResourceCacheMgr.Instance.OnUpdate();
        }

        private void LateUpdate()
        {
            ++_frameID;
            _frameTime = Time.time;
        }
    }
}