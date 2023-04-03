using UnityEngine;

namespace TEngine
{
    /// <summary>
    /// 界面辅助器基类。
    /// </summary>
    public abstract class UIWindowHelperBase : MonoBehaviour
    {
        /// <summary>
        /// 实例化界面。
        /// </summary>
        /// <param name="uiWindowAsset">要实例化的界面资源。</param>
        /// <returns>实例化后的界面。</returns>
        public abstract object InstantiateUIWindow(object uiWindowAsset);

        /// <summary>
        /// 创建界面。
        /// </summary>
        /// <param name="uiWindowInstance">界面实例。</param>
        /// <param name="uiGroup">界面所属的界面组。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>界面。</returns>
        public abstract UIWindow CreateUIWindow(object uiWindowInstance, IUIGroup uiGroup, object userData);

        /// <summary>
        /// 释放界面。
        /// </summary>
        /// <param name="uiWindowAsset">要释放的界面资源。</param>
        /// <param name="uiWindowInstance">要释放的界面实例。</param>
        public abstract void ReleaseUIWindow(object uiWindowAsset, object uiWindowInstance);
    }
}
