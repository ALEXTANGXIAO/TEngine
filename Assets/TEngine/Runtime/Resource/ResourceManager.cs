using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using YooAsset;

namespace TEngine
{
    internal partial class ResourceManager:IResourceManager
    {
        public string ApplicableGameVersion { get; }
        public int InternalResourceVersion { get; }
        public int DownloadingMaxNum { get; set; }
        public int FailedTryAgain { get; set; }
        public string ReadOnlyPath { get; }
        public string ReadWritePath { get; }
        public string PackageName { get; set; }
        public EPlayMode PlayMode { get; set; }
        public EVerifyLevel VerifyLevel { get; set; }
        public long Milliseconds { get; set; }
        
        public void SetReadOnlyPath(string readOnlyPath)
        {
            throw new System.NotImplementedException();
        }

        public void SetReadWritePath(string readWritePath)
        {
            throw new System.NotImplementedException();
        }

        public void Initialize()
        {
            throw new System.NotImplementedException();
        }

        public InitializationOperation InitPackage()
        {
            throw new System.NotImplementedException();
        }

        public void UnloadAsset(object asset)
        {
            throw new System.NotImplementedException();
        }

        public void UnloadUnusedAssets()
        {
            throw new System.NotImplementedException();
        }

        public void ForceUnloadAllAssets()
        {
            throw new System.NotImplementedException();
        }

        public HasAssetResult HasAsset(string assetName)
        {
            throw new System.NotImplementedException();
        }

        public T LoadAsset<T>(string assetName) where T : Object
        {
            throw new System.NotImplementedException();
        }

        public T LoadAsset<T>(string assetName, Transform parent) where T : Object
        {
            throw new System.NotImplementedException();
        }

        public UniTask<T> LoadAssetAsync<T>(string assetName, CancellationToken cancellationToken) where T : Object
        {
            throw new System.NotImplementedException();
        }

        public UniTask<GameObject> LoadGameObjectAsync(string assetName, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public AssetOperationHandle LoadAssetGetOperation<T>(string assetName) where T : Object
        {
            throw new System.NotImplementedException();
        }

        public AssetOperationHandle LoadAssetAsyncHandle<T>(string assetName) where T : Object
        {
            throw new System.NotImplementedException();
        }
    }
}