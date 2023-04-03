using UnityEngine;

namespace TEngine
{
    /// <summary>
    /// 界面组辅助器基类。
    /// </summary>
    public abstract class UIGroupHelperBase : MonoBehaviour
    {
        /// <summary>
        /// 设置界面组深度。
        /// </summary>
        /// <param name="depth">界面组深度。</param>
        public abstract void SetDepth(int depth);
    }
}