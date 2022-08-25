using UnityEngine;

namespace TEngine.Runtime
{
    [DisallowMultipleComponent, AddComponentMenu("")]
    public sealed class AssetTag : MonoBehaviour
    {
        AssetData _assetData;
        [SerializeField, HideInInspector] private string _path;
        public string Path => _path;

        private int _instanceID = 0;

        private void Awake()
        {
            if (_path != null && (_instanceID == -1 || _instanceID != gameObject.GetInstanceID()))
            {
                Bind(ResMgr.Instance.GetAsset(_path, false));
            }
        }

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