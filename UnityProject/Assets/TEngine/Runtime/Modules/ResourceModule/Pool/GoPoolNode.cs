using System.Collections.Generic;
using UnityEngine;

namespace TEngine
{
    internal class GoPoolNode
    {
        public readonly List<GameObject> ListGameObjects = new List<GameObject>();
        public int MaxCacheCnt = 10;
        public int GoRefCnt;
        public int MinCacheCnt;
        public int CacheFreeTime;
        public float PoolGoRefreshTime;

        public bool AddCacheGo(GameObject go)
        {
            if (ListGameObjects.Count >= MaxCacheCnt)
            {
                return false;
            }
            ListGameObjects.Add(go);
            return true;
        }
    }
}