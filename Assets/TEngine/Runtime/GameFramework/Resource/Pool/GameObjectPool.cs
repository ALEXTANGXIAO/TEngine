using System.Collections.Generic;
using UnityEngine;
using YooAsset;

namespace TEngine
{
	/// <summary>
	/// 游戏物体对象池。
	/// </summary>
	internal class GameObjectPool
	{
		private readonly GameObject _root;
		private readonly Queue<InstantiateOperation> _cacheOperations;
		private readonly bool _dontDestroy;
		private readonly int _initCapacity;
		private readonly int _maxCapacity;
		private readonly float _destroyTime;
		private float _lastRestoreRealTime = -1f;

		/// <summary>
		/// 资源句柄。
		/// </summary>
		public AssetOperationHandle AssetHandle { private set; get; }

		/// <summary>
		/// 资源定位地址。
		/// </summary>
		public string Location { private set; get; }

		/// <summary>
		/// 内部缓存总数。
		/// </summary>
		public int CacheCount => _cacheOperations.Count;

		/// <summary>
		/// 外部使用总数。
		/// </summary>
		public int SpawnCount { private set; get; } = 0;

		/// <summary>
		/// 是否常驻不销毁。
		/// </summary>
		public bool DontDestroy => _dontDestroy;

		/// <summary>
		/// 游戏物体对象池。
		/// </summary>
		/// <param name="poolingRoot">对象池根节点。</param>
		/// <param name="location">资源定位地址。</param>
		/// <param name="dontDestroy">是否常驻不销毁。</param>
		/// <param name="initCapacity">初始容量。</param>
		/// <param name="maxCapacity">最大容量。</param>
		/// <param name="destroyTime">对象池销毁时间。</param>
		public GameObjectPool(GameObject poolingRoot, string location, bool dontDestroy, int initCapacity, int maxCapacity, float destroyTime)
		{
			_root = new GameObject(location);
			_root.transform.parent = poolingRoot.transform;
			Location = location;

			_dontDestroy = dontDestroy;
			_initCapacity = initCapacity;
			_maxCapacity = maxCapacity;
			_destroyTime = destroyTime;

			// 创建缓存池
			_cacheOperations = new Queue<InstantiateOperation>(initCapacity);
		}

		/// <summary>
		/// 创建对象池。
		/// </summary>
		/// <param name="package">资源包。</param>
		public void CreatePool(ResourcePackage package)
		{
			// 加载游戏对象
			AssetHandle = package.LoadAssetAsync<GameObject>(Location);

			// 创建初始对象
			for (int i = 0; i < _initCapacity; i++)
			{
				var operation = AssetHandle.InstantiateAsync(_root.transform);
				operation.Completed += Operation_Completed;
				_cacheOperations.Enqueue(operation);
			}
		}
		private void Operation_Completed(AsyncOperationBase obj)
		{
			if (obj.Status == EOperationStatus.Succeed)
			{
				var op = obj as InstantiateOperation;
				if (op.Result != null)
					op.Result.SetActive(false);
			}
		}

		/// <summary>
		/// 销毁游戏对象池。
		/// </summary>
		public void DestroyPool()
		{
			// 卸载资源对象
			AssetHandle.Release();
			AssetHandle = null;

			// 销毁游戏对象
			Object.Destroy(_root);
			_cacheOperations.Clear();

			SpawnCount = 0;
		}

		/// <summary>
		/// 查询静默时间内是否可以销毁。
		/// </summary>
		public bool CanAutoDestroy()
		{
			if (_dontDestroy)
				return false;
			if (_destroyTime < 0)
				return false;

			if (_lastRestoreRealTime > 0 && SpawnCount <= 0)
				return (Time.realtimeSinceStartup - _lastRestoreRealTime) > _destroyTime;
			else
				return false;
		}

		/// <summary>
		/// 游戏对象池是否已经销毁。
		/// </summary>
		public bool IsDestroyed()
		{
			return AssetHandle == null;
		}

		/// <summary>
		/// 回收操作。
		/// </summary>
		/// <param name="operation">资源实例化操作句柄。</param>
		public void Restore(InstantiateOperation operation)
		{
			if (IsDestroyed())
			{
				DestroyInstantiateOperation(operation);
				return;
			}

			SpawnCount--;
			if (SpawnCount <= 0)
				_lastRestoreRealTime = Time.realtimeSinceStartup;

			// 如果外部逻辑销毁了游戏对象
			if (operation.Status == EOperationStatus.Succeed)
			{
				if (operation.Result == null)
					return;
			}

			// 如果缓存池还未满员
			if (_cacheOperations.Count < _maxCapacity)
			{
				SetRestoreGameObject(operation.Result);
				_cacheOperations.Enqueue(operation);
			}
			else
			{
				DestroyInstantiateOperation(operation);
			}
		}

		/// <summary>
		/// 丢弃操作。
		/// </summary>
		/// <param name="operation">资源实例化操作句柄。</param>
		public void Discard(InstantiateOperation operation)
		{
			if (IsDestroyed())
			{
				DestroyInstantiateOperation(operation);
				return;
			}

			SpawnCount--;
			if (SpawnCount <= 0)
				_lastRestoreRealTime = Time.realtimeSinceStartup;

			DestroyInstantiateOperation(operation);
		}

		/// <summary>
		/// 获取一个游戏对象。
		/// </summary>
		/// <param name="parent">父节点位置。</param>
		/// <param name="position">位置。</param>
		/// <param name="rotation">旋转。</param>
		/// <param name="forceClone">是否强制克隆。</param>
		/// <param name="userDatas">用户自定义数据。</param>
		/// <returns>Spawn操作句柄。</returns>
		public SpawnHandle Spawn(Transform parent, Vector3 position, Quaternion rotation, bool forceClone, params System.Object[] userDatas)
		{
			InstantiateOperation operation;
			if (forceClone == false && _cacheOperations.Count > 0)
				operation = _cacheOperations.Dequeue();
			else
				operation = AssetHandle.InstantiateAsync();

			SpawnCount++;
			SpawnHandle handle = new SpawnHandle(this, operation, parent, position, rotation, userDatas);
			YooAssets.StartOperation(handle);
			return handle;
		}

		private void DestroyInstantiateOperation(InstantiateOperation operation)
		{
			// 取消异步操作
			operation.Cancel();

			// 销毁游戏对象
			if (operation.Result != null)
			{
				Object.Destroy(operation.Result);
			}
		}
		private void SetRestoreGameObject(GameObject gameObj)
		{
			if (gameObj != null)
			{
				gameObj.SetActive(false);
				gameObj.transform.SetParent(_root.transform);
				gameObj.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
			}
		}
	}
}