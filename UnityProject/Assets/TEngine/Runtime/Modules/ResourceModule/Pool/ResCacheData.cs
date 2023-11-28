using UnityEngine;

namespace TEngine
{
    /// <summary>
    /// 资源缓存数据。
    /// </summary>
    internal class ResCacheData : IMemory
    {
        /// <summary>
        /// 资源实例。
        /// </summary>
        public Object Asset;
        
        /// <summary>
        /// 缓存刷新时间。
        /// </summary>
        public float CacheRefreshTime;
        
        /// <summary>
        /// 是否自动过期。
        /// </summary>
        public bool AutoExpire;
        
        /// <summary>
        /// 缓存过期时间。
        /// </summary>
        public int CacheExpireTime;

        public void Clear()
        {
            Asset = null;
            CacheRefreshTime = 0f;
            AutoExpire = false;
            CacheExpireTime = 0;
        }
    }
}