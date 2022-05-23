using UnityEngine;

namespace TEngineCore
{
    /// <summary>
    /// GameObject与AssetData的绑定对象，用于管理对AssetData的引用计数
    /// </summary>
    [DisallowMultipleComponent, AddComponentMenu("")]
    public sealed class AssetTag : MonoBehaviour
    {
        AssetData _assetData;
        [SerializeField, HideInInspector] private string _path;
        /// <summary>
        /// 对应的资源路径
        /// </summary>
        public string Path => _path;

        /// <summary>
        /// 缓存池中的归还时间戳
        /// </summary>
        public float PoolReturnTimestamp { get; set; }

        private int _instanceID = 0;
        private void Awake()
        {
            // 处理Cloned GameObject上引用计数不正确的问题
            if (_path != null && (_instanceID == -1 || _instanceID != gameObject.GetInstanceID()))
                Bind(ResMgr.Instance.GetAsset(_path, false));
        }

        /// <summary>
        /// GameObject绑定AssetData
        /// </summary>
        /// <param name="assetData">Asset数据</param>
        public void Bind(AssetData assetData)
        {
            _assetData = assetData;
            _assetData.AddRef();
            _path = _assetData.Path;
            _instanceID = gameObject.GetInstanceID();
        }

        private void OnDestroy()
        {
            _assetData.DecRef();
        }
    }

}