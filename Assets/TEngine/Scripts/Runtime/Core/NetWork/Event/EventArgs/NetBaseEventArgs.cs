using System;

namespace TEngine.Runtime
{
    public abstract class NetBaseEventArgs : EventArgs, IMemory
    {
        /// <summary>
        /// 初始化构造函数
        /// </summary>
        public NetBaseEventArgs()
        {
        }

        /// <summary>
        /// 清理引用。
        /// </summary>
        public abstract void Clear();
    }
}