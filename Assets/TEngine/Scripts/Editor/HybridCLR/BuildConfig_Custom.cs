using System.Collections.Generic;

namespace HybridCLR.Editor
{
    public static partial class BuildConfig
    {
        /// <summary>
        /// 所有热更新dll列表。放到此列表中的dll在构建AB的插入管线执行。
        /// </summary>
        public static List<string> HotUpdateAssemblies { get; } = new List<string>
        {
            "HotFix.dll",
        };

        public static List<string> AOTMetaAssemblies { get; } = new List<string>()
        {
            "mscorlib.dll",
            "System.dll",
            "System.Core.dll",
        };
    }
}
