using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using YooAsset;

namespace TEngine
{
    /// <summary>
    /// 资源引用标识。
    /// </summary>
    [DisallowMultipleComponent, AddComponentMenu("")]
    public sealed class AssetReference: MonoBehaviour
    {
        private AssetOperationHandle _operationHandle;
        private int _instanceID = 0;
        private string _assetLocation;
        private AssetGroup _assetGroup;

        /// <summary>
        /// 资源组。
        /// </summary>
        public AssetGroup AssetGroup => _assetGroup;
        
        private void Awake()
        {
            if (_assetGroup == null)
            {
                _assetGroup = AssetGroup.Alloc();
            }
        }
        
        public void Bind(AssetOperationHandle operation,string assetLocation)
        {
            _operationHandle = operation;
            this._assetLocation = assetLocation;
            _instanceID = gameObject.GetInstanceID();
        }

        private void OnDestroy()
        {
            if (_operationHandle != null)
            {
                _operationHandle.Release();
                _operationHandle = null;
            }
            if (_assetGroup == null)
            {
                 AssetGroup.Release(_assetGroup);
                 _assetGroup = null;
            }
        }

        #region Public Methods
        /// <summary>
        /// 注册资源数据到资源组内。
        /// </summary>
        /// <param name="handle">资源操作句柄。</param>
        /// <param name="assetTag">资源标识。</param>
        /// <returns>是否注册成功。</returns>
        public bool Register(AssetOperationHandle handle,string assetTag = "")
        {
            return _assetGroup.Register(handle,assetTag);
        }

        /// <summary>
        /// 从资源组内反注册资源数据。
        /// </summary>
        /// <param name="assetTag"></param>
        /// <returns></returns>
        public bool UnRegister(string assetTag)
        {
            return _assetGroup.UnRegister(assetTag);
        }
        
        /// <summary>
        /// 从资源组内反注册资源数据。
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        public bool UnRegister(AssetOperationHandle handle)
        {
            return _assetGroup.UnRegister(handle);
        }

     
        /// <summary>
        /// 同步加载资源。
        /// </summary>
        /// <param name="assetName">要加载资源的名称。</param>
        /// <typeparam name="T">要加载资源的类型。</typeparam>
        /// <returns>资源实例。</returns>
        public T LoadAsset<T>(string assetName) where T : Object
        {
            return _assetGroup.LoadAsset<T>(assetName);
        }

        /// <summary>
        /// 同步加载资源。
        /// </summary>
        /// <param name="assetName">要加载资源的名称。</param>
        /// <param name="parent">父节点位置。</param>
        /// <typeparam name="T">要加载资源的类型。</typeparam>
        /// <returns>资源实例。</returns>
        public T LoadAsset<T>(string assetName, Transform parent) where T : Object
        {
            return _assetGroup.LoadAsset<T>(assetName,parent);
        }

        /// <summary>
        /// 异步加载资源实例。
        /// </summary>
        /// <param name="assetName">要加载的实例名称。</param>
        /// <param name="cancellationToken">取消操作Token。</param>
        /// <returns>资源实实例。</returns>
        public async UniTask<T> LoadAssetAsync<T>(string assetName, CancellationToken cancellationToken) where T : Object
        {
            return await _assetGroup.LoadAssetAsync<T>(assetName,cancellationToken);
        }

        /// <summary>
        /// 异步加载游戏物体。
        /// </summary>
        /// <param name="assetName">要加载的游戏物体名称。</param>
        /// <param name="cancellationToken">取消操作Token。</param>
        /// <returns>异步游戏物体实例。</returns>
        public async UniTask<GameObject> LoadGameObjectAsync(string assetName, CancellationToken cancellationToken)
        {
            return await _assetGroup.LoadGameObjectAsync(assetName,cancellationToken);
        }
        
                
        public static bool BindAssetReference(GameObject go,AssetOperationHandle handle,string location)
        {
            if (go == null)
            {
                throw new GameFrameworkException($"BindAssetReference Failed => GameObject is null!");
            }

            if (handle == null)
            {
                throw new GameFrameworkException($"BindAssetReference Failed => AssetOperationHandle is null!");
            }

            go.AddComponent<AssetReference>().Bind(handle,location);

            return true;
        }
        #endregion
    }
}