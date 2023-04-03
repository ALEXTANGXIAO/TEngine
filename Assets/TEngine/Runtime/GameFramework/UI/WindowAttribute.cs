using System;

namespace TEngine
{
    [AttributeUsage(AttributeTargets.Class)]
    public class WindowAttribute : Attribute
    {
        /// <summary>
        /// 窗口层级
        /// </summary>
        public int WindowLayer;
        
        /// <summary>
        /// 资源定位地址。
        /// </summary>
        public string AssetName;

        /// <summary>
        /// 全屏窗口标记。
        /// </summary>
        public bool FullScreen;

        public WindowAttribute(int windowLayer,string assetName = "", bool fullScreen = false)
        {
            WindowLayer = windowLayer;
            AssetName = assetName;
            FullScreen = fullScreen;
        }
    }
}