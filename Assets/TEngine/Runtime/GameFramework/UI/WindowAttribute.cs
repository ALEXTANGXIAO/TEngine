using System;

namespace TEngine
{
    [AttributeUsage(AttributeTargets.Class)]
    public class WindowAttribute : Attribute
    {
        /// <summary>
        /// 资源实体可寻址路径/路径。
        /// </summary>
        public string AssetName;

        /// <summary>
        /// 全屏窗口标记。
        /// </summary>
        public bool FullScreen;

        public WindowAttribute(string assetName, bool fullScreen = false)
        {
            AssetName = assetName;
            FullScreen = fullScreen;
        }
    }
}