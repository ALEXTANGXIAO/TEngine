using System.Collections.Generic;

namespace HybridCLR.Editor
{
    public static partial class BuildConfig
    {

        /// <summary>
        /// 所有热更新dll列表。放到此列表中的dll在打包时OnFilterAssemblies回调中被过滤。
        /// </summary>
        public static List<string> HotUpdateAssemblies { get; } = new List<string>
        {
            "TEngine.Runtime.dll",
            "HotFix.dll",
        };

        public static List<string> AOTMetaAssemblies { get; } = new List<string>()
        {
            "mscorlib.dll",
            "System.dll",
            "System.Core.dll", // 如果使用了Linq，需要这个

            //
            // 注意！修改这个列表请同步修改HotFix2模块中App.cs文件中的 LoadMetadataForAOTAssembly函数中aotDllList列表。
            // 两者需要完全一致
            //
        };
    }
}
