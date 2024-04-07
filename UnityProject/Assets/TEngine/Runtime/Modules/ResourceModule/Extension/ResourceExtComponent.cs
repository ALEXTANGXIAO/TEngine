using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace TEngine
{
    /// <summary>
    /// 资源组件拓展。
    /// </summary>
    [DisallowMultipleComponent]
    public partial class ResourceExtComponent : Module
    {
        private readonly TimeoutController _timeoutController = new TimeoutController();
        
        /// <summary>
        /// 正在加载的资源列表。
        /// </summary>
        private readonly HashSet<string> _assetLoadingList = new HashSet<string>();
        
        /// <summary>
        /// 检查是否可以释放间隔
        /// </summary>
        [SerializeField] private float m_CheckCanReleaseInterval = 30f;

        private float m_CheckCanReleaseTime = 0.0f;

        /// <summary>
        /// 对象池自动释放时间间隔
        /// </summary>
        [SerializeField] private float m_AutoReleaseInterval = 60f;

        /// <summary>
        /// 保存加载的图片对象
        /// </summary>
#if ODIN_INSPECTOR
        [ShowInInspector]
#endif
        private LinkedList<LoadAssetObject> m_LoadAssetObjectsLinkedList;

        /// <summary>
        /// 散图集合对象池
        /// </summary>
        private IObjectPool<AssetItemObject> m_AssetItemPool;


#if UNITY_EDITOR
        public LinkedList<LoadAssetObject> LoadAssetObjectsLinkedList
        {
            get => m_LoadAssetObjectsLinkedList;
            set => m_LoadAssetObjectsLinkedList = value;
        }
#endif
        private IEnumerator Start()
        {
            yield return new WaitForEndOfFrame();
            ObjectPoolModule objectPoolComponent = GameModule.Get<ObjectPoolModule>();
            m_AssetItemPool = objectPoolComponent.CreateMultiSpawnObjectPool<AssetItemObject>(
                "SetAssetPool",
                m_AutoReleaseInterval, 16, 60, 0);
            m_LoadAssetObjectsLinkedList = new LinkedList<LoadAssetObject>();
            
            InitializedResources();
        }

        private void Update()
        {
            m_CheckCanReleaseTime += Time.unscaledDeltaTime;
            if (m_CheckCanReleaseTime < (double)m_CheckCanReleaseInterval)
            {
                return;
            }

            ReleaseUnused();
        }

        /// <summary>
        /// 回收无引用的缓存资产。
        /// </summary>
#if ODIN_INSPECTOR
        [Button("Release Unused")]
#endif
        public void ReleaseUnused()
        {
            if (m_LoadAssetObjectsLinkedList == null)
            {
                return;
            }

            LinkedListNode<LoadAssetObject> current = m_LoadAssetObjectsLinkedList.First;
            while (current != null)
            {
                var next = current.Next;
                if (current.Value.AssetObject.IsCanRelease())
                {
                    m_AssetItemPool.Unspawn(current.Value.AssetTarget);
                    MemoryPool.Release(current.Value.AssetObject);
                    m_LoadAssetObjectsLinkedList.Remove(current);
                }

                current = next;
            }

            m_CheckCanReleaseTime = 0f;
        }

        private void SetAsset(ISetAssetObject setAssetObject, Object assetObject)
        {
            m_LoadAssetObjectsLinkedList.AddLast(new LoadAssetObject(setAssetObject, assetObject));
            setAssetObject.SetAsset(assetObject);
        }
        
        private async UniTask TryWaitingLoading(string assetObjectKey)
        {
            if (_assetLoadingList.Contains(assetObjectKey))
            {
                try
                {
                    await UniTask.WaitUntil(
                            () => !_assetLoadingList.Contains(assetObjectKey))
#if UNITY_EDITOR
                        .AttachExternalCancellation(_timeoutController.Timeout(TimeSpan.FromSeconds(60)));
                    _timeoutController.Reset();
#else
                    ;
#endif
                
                }
                catch (OperationCanceledException ex)
                {
                    if (_timeoutController.IsTimeout())
                    {
                        Log.Error($"LoadAssetAsync Waiting {assetObjectKey} timeout. reason:{ex.Message}");
                    }
                }
            }
        }
    }
}