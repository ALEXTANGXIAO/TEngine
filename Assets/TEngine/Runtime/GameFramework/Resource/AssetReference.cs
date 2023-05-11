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
    public sealed class AssetReference : MonoBehaviour
    {
        private AssetOperationHandle _operationHandle;

        private int _instanceID = 0;
        private string _assetLocation;
        private AssetGroup _assetGroup;

        /// <summary>
        /// 资源组。
        /// </summary>
        public AssetGroup AssetGroup => _assetGroup;

#pragma warning disable CS0628
        public AssetReference Parent { protected set; get; }
#pragma warning restore CS0628
        
        // ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
        // ReSharper disable once InconsistentNaming
        public AssetOperationHandle assetOperationHandle => _operationHandle;
        
        public string AssetLocation => _assetLocation;

        private void Awake()
        {
            if (_assetGroup == null)
            {
                _assetGroup = AssetGroup.Alloc();
            }
        }

        public void Bind(AssetOperationHandle operation, string assetLocation, AssetReference parent = null)
        {
            _operationHandle = operation;
            this._assetLocation = assetLocation;
            _instanceID = gameObject.GetInstanceID();
            if (parent != null)
            {
                Parent = parent;
            }
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
        /// 引用资源数据到资源组内。
        /// </summary>
        /// <param name="handle">资源操作句柄。</param>
        /// <param name="assetTag">资源标识。</param>
        /// <returns>是否注册成功。</returns>
        public bool Reference(AssetOperationHandle handle, string assetTag = "")
        {
            return _assetGroup.Reference(handle, assetTag);
        }

        /// <summary>
        /// 从资源组内释放资源数据。
        /// </summary>
        /// <param name="assetTag"></param>
        /// <returns></returns>
        public bool Release(string assetTag)
        {
            return _assetGroup.Release(assetTag);
        }

        /// <summary>
        /// 从资源组内释放资源数据。
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        public bool Release(AssetOperationHandle handle)
        {
            return _assetGroup.Release(handle);
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
            return _assetGroup.LoadAsset<T>(assetName, parent);
        }

        /// <summary>
        /// 同步加载资源。
        /// </summary>
        /// <param name="assetName">要加载资源的名称。</param>
        /// <typeparam name="T">要加载资源的类型。</typeparam>
        /// <param name="assetOperationHandle">资源操作句柄。</param>
        /// <returns>资源实例。</returns>
        // ReSharper disable once ParameterHidesMember
        public T LoadAsset<T>(string assetName, out AssetOperationHandle assetOperationHandle) where T : Object
        {
            return _assetGroup.LoadAsset<T>(assetName,out assetOperationHandle);
        }
        
        /// <summary>
        /// 同步加载资源。
        /// </summary>
        /// <param name="assetName">要加载资源的名称。</param>
        /// <param name="parent">父节点位置。</param>
        /// <typeparam name="T">要加载资源的类型。</typeparam>
        /// <param name="assetOperationHandle">资源操作句柄。</param>
        /// <returns>资源实例。</returns>
        // ReSharper disable once ParameterHidesMember
        public T LoadAsset<T>(string assetName, Transform parent, out AssetOperationHandle assetOperationHandle) where T : Object
        {
            return _assetGroup.LoadAsset<T>(assetName, parent,out assetOperationHandle);
        }

        /// <summary>
        /// 异步加载资源实例。
        /// </summary>
        /// <param name="assetName">要加载的实例名称。</param>
        /// <param name="cancellationToken">取消操作Token。</param>
        /// <returns>资源实实例。</returns>
        public async UniTask<T> LoadAssetAsync<T>(string assetName, CancellationToken cancellationToken)
            where T : Object
        {
            return await _assetGroup.LoadAssetAsync<T>(assetName, cancellationToken);
        }

        /// <summary>
        /// 异步加载游戏物体。
        /// </summary>
        /// <param name="assetName">要加载的游戏物体名称。</param>
        /// <param name="cancellationToken">取消操作Token。</param>
        /// <returns>异步游戏物体实例。</returns>
        public async UniTask<GameObject> LoadGameObjectAsync(string assetName, CancellationToken cancellationToken)
        {
            return await _assetGroup.LoadGameObjectAsync(assetName, cancellationToken);
        }

        /// <summary>
        /// 绑定资源引用。
        /// </summary>
        /// <param name="go">游戏物体实例。</param>
        /// <param name="handle">资源句柄。</param>
        /// <param name="location">资源定位地址。</param>
        /// <param name="parent">父级引用。</param>
        /// <returns>资源引用组件。</returns>
        /// <exception cref="GameFrameworkException">捕获异常。</exception>
        public static AssetReference BindAssetReference(GameObject go, AssetOperationHandle handle, string location = "", AssetReference parent = null)
        {
            if (go == null)
            {
                throw new GameFrameworkException($"BindAssetReference Failed => GameObject is null!");
            }

            if (handle == null)
            {
                throw new GameFrameworkException($"BindAssetReference Failed => AssetOperationHandle is null!");
            }

            var ret = go.AddComponent<AssetReference>();

            ret.Bind(operation: handle, assetLocation: location, parent: parent);

            return ret;
        }
        
        /// <summary>
        /// 绑定资源引用。
        /// </summary>
        /// <param name="go">游戏物体实例。</param>
        /// <param name="location">资源定位地址。</param>
        /// <param name="parent">父级引用。</param>
        /// <returns>资源引用组件。</returns>
        /// <exception cref="GameFrameworkException">捕获异常。</exception>
        public static AssetReference BindAssetReference(GameObject go,string location = "", AssetReference parent = null)
        {
            if (go == null)
            {
                throw new GameFrameworkException($"BindAssetReference Failed => GameObject is null!");
            }

            var ret = go.AddComponent<AssetReference>();

            ret.Bind(operation: null, assetLocation: location, parent: parent);

            return ret;
        }

        #endregion
    }
}