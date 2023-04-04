using UnityEngine;
using YooAsset;

namespace TEngine
{
    /// <summary>
    /// 资源标识。
    /// </summary>
    [DisallowMultipleComponent, AddComponentMenu("")]
    public sealed class AssetTag: MonoBehaviour
    {
        private AssetOperationHandle _operationHandle;
        
        private int _instanceID = 0;

        private string location;
        
        private void Awake()
        {
            if (location != null && (_instanceID == -1 || _instanceID != gameObject.GetInstanceID()))
            {
                // Bind(GetAsset(operation, assetLocation));
            }
        }
        
        public void Bind(AssetOperationHandle operation,string assetLocation)
        {
            _operationHandle = operation;
            this.location = assetLocation;
            _instanceID = gameObject.GetInstanceID();
        }

        private void OnDestroy()
        {
            if (_operationHandle != null)
            {
                _operationHandle.Release();
                _operationHandle = null;
            }
        }
    }
}