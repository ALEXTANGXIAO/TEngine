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
        SettingsUtils.HybridCLRCustomGlobalSettings.Enable = HybridCLR.Editor.SettingsUtil.Enable;
        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
        Debug.Log("同步AOT和HotUpdate程序集 HybridCLR到TEngineSettings成功。");
    }
}