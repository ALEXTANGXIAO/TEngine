using GameFramework;

namespace TEngine
{
    public interface ISetAssetObject : IMemory
    {
        /// <summary>
        /// 资源定位地址。
        /// </summary>
        string Location { get; }

        /// <summary>
        /// 设置资源。
        /// </summary>
        void SetAsset(UnityEngine.Object asset);

        /// <summary>
        /// 是否可以回收。
        /// </summary>
        bool IsCanRelease();
    }
}