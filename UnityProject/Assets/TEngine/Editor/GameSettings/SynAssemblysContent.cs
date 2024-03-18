using UnityEditor;
using UnityEngine;

/// <summary>
/// 同步程序集上下文。
/// </summary>
public static class SyncAssemblyContent
{
    public static void RefreshAssembly()
    {
        SettingsUtils.SetHybridCLRHotUpdateAssemblies(HybridCLR.Editor.SettingsUtil.HotUpdateAssemblyFilesIncludePreserved);
        SettingsUtils.SetHybridCLRAOTMetaAssemblies(HybridCLR.Editor.SettingsUtil.AOTAssemblyNames);
        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
    }
}