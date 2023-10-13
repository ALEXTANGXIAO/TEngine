using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using YooAsset;

namespace TEngine
{
    /// <summary>
    /// 资源引用。
    /// </summary>
    /// <remarks>用于绑定资源引用关系。</remarks>
    [DisallowMultipleComponent, AddComponentMenu("")]
    public sealed class AssetReference : MonoBehaviour
    {
        private int _instanceID = 0;
        private string _assetLocation;
        private AssetGroup _assetGroup;
        private AssetOperationHandle _operationHandle;

        /// <summary>
        /// 父级资源引用。
        /// </summary>
        public AssetReference Parent { private set; get; }

        /// <summary>
        /// 资源操作句柄。
        /// </summary>
        public AssetOperationHandle assetOperationHandle => _operationHandle;

        /// <summary>
        /// 资源定位地址。
        /// </summary>
        public string AssetLocation => _assetLocation;

        /// <summary>
        /// 脏初始化资源分组。
        /// </summary>
        public void DirtyInitAssetGroup()
        {
            if (_assetGroup == null)
            {
                _assetGroup = AssetGroup.Alloc();
            }
        }

        /// <summary>
        /// 建立资源引用绑定。
        /// </summary>
        /// <param name="operation">资源操作句柄。</param>
        /// <param name="assetLocation">资源定位地址。</param>
        /// <param name="parent">父级资源引用。(NullAble)</param>
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

            if (_assetGroup != null)
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
            DirtyInitAssetGroup();
            return _assetGroup.Reference(handle, assetTag);
        }

        /// <summary>
        /// 引用资源数据到资源组内。
        /// </summary>
        /// <param name="address">资源定位地址。</param>
        /// <param name="handle">资源操作句柄。</param>
        /// <param name="assetTag">资源标识。</param>
        /// <returns>是否注册成功。</returns>
        public bool Reference(string address, AssetOperationHandle handle, string assetTag = "")
        {
            DirtyInitAssetGroup();
            return _assetGroup.Reference(address, handle, assetTag);
        }

        /// <summary>
        /// 从资源组内释放资源数据。
        /// </summary>
        /// <param name="assetTag">资源标识。</param>
        /// <returns>是否释放成功。</returns>
        public bool ReleaseByTag(string assetTag)
        {
            DirtyInitAssetGroup();
            return _assetGroup.ReleaseByTag(assetTag);
        }

        /// <summary>
        /// 从资源组内释放资源数据。
        /// </summary>
        /// <param name="handle">资源操作句柄。</param>
        /// <returns>是否释放成功。</returns>
        public bool Release(AssetOperationHandle handle)
        {
            DirtyInitAssetGroup();
            return _assetGroup.Release(handle);
        }

        /// <summary>
        /// 从资源组内释放资源数据。
        /// </summary>
        /// <param name="address">资源定位地址。</param>
        /// <returns>是否释放成功。</returns>
        public bool Release(string address)
        {
            DirtyInitAssetGroup();
            return _assetGroup.Release(address);
        }

        /// <summary>
        /// 同步加载资源。
        /// </summary>
        /// <param name="assetName">要加载资源的名称。</param>
        /// <typeparam name="T">要加载资源的类型。</typeparam>
        /// <returns>资源实例。</returns>
        public T LoadAsset<T>(string assetName) where T : Object
        {
            DirtyInitAssetGroup();
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
            DirtyInitAssetGroup();
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
            DirtyInitAssetGroup();
            return _assetGroup.LoadAsset<T>(assetName, out assetOperationHandle);
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
            DirtyInitAssetGroup();
            return _assetGroup.LoadAsset<T>(assetName, parent, out assetOperationHandle);
        }

        /// <summary>
        /// 异步加载资源实例。
        /// </summary>
        /// <param name="assetName">要加载的实例名称。</param>
        /// <param name="cancellationToken">取消操作Token。</param>
        /// <returns>资源实实例。</returns>
        public async UniTask<T> LoadAssetAsync<T>(string assetName, CancellationToken cancellationToken = default) where T : Object
        {
            DirtyInitAssetGroup();
            return await _assetGroup.LoadAssetAsync<T>(assetName, cancellationToken);
        }

        /// <summary>
        /// 异步加载游戏物体。
        /// </summary>
        /// <param name="assetName">要加载的游戏物体名称。</param>
        /// <param name="cancellationToken">取消操作Token。</param>
        /// <returns>异步游戏物体实例。</returns>
        public async UniTask<GameObject> LoadGameObjectAsync(string assetName, CancellationToken cancellationToken = default)
        {
            DirtyInitAssetGroup();
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

            var ret = go.GetOrAddComponent<AssetReference>();

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
        public static AssetReference BindAssetReference(GameObject go, string location = "", AssetReference parent = null)
        {
            if (go == null)
            {
                throw new GameFrameworkException($"BindAssetReference Failed => GameObject is null!");
            }

            var ret = go.GetOrAddComponent<AssetReference>();

            ret.Bind(operation: null, assetLocation: location, parent: parent);

            return ret;
        }

        #endregion
    }
}