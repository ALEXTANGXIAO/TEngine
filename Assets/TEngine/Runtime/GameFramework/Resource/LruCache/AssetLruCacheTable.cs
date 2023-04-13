using YooAsset;

namespace TEngine
{
    /// <summary>
    /// 资源Lru缓存表。
    /// </summary>
    public sealed class AssetLruCacheTable:LruCacheTable<string,OperationHandleBase>
    {
        public AssetLruCacheTable(int capacity) : base(capacity)
        {
            OnRemoveCallback += OnRelease;
        }

        private void OnRelease(OperationHandleBase handleBase)
        {
            handleBase.ReleaseInternal();
        }
    }
}