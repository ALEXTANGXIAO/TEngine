namespace TEngine
{
    internal struct ResourceCacheConfig
    {
        public readonly string ResPath;
        public readonly int CacheTime;
        public readonly int MaxPoolCnt;
        public readonly int PoolGoFreeTime;
        public readonly int MinPoolCnt;

        public ResourceCacheConfig(
            string resPath,
            int cacheTime,
            int maxPoolCnt,
            int poolGoFreeTime,
            int minPoolCnt)
        {
            ResPath = resPath;
            CacheTime = cacheTime;
            MaxPoolCnt = maxPoolCnt;
            PoolGoFreeTime = poolGoFreeTime;
            MinPoolCnt = minPoolCnt;
        }
    }
}