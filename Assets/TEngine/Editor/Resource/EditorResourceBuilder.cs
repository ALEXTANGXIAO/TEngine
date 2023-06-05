using UnityEditor;
using UnityEngine;
using YooAsset.Editor;

public static class EditorResourceBuilder
{
    
    public static void Build()
    {
        BuildInternal(BuildTarget.Android,buildPipeline:EBuildPipeline.BuiltinBuildPipeline);
    }
    
    public static void BuildInternal(
        BuildTarget buildTarget,
        string packageVersion = "1.0",
        EBuildPipeline buildPipeline = EBuildPipeline.ScriptableBuildPipeline,
        EBuildMode buildMode = EBuildMode.IncrementalBuild)
    {
        Debug.Log($"开始构建 : {buildTarget}");

        // 构建参数
        string defaultOutputRoot = AssetBundleBuilderHelper.GetDefaultOutputRoot();
        BuildParameters buildParameters = new BuildParameters();
        buildParameters.OutputRoot = defaultOutputRoot;
        buildParameters.BuildTarget = buildTarget;
        buildParameters.BuildPipeline = buildPipeline;
        buildParameters.BuildMode = buildMode;
        buildParameters.PackageName = "DefaultPackage";
        buildParameters.PackageVersion = packageVersion;
        buildParameters.VerifyBuildingResult = true;
        buildParameters.CompressOption = ECompressOption.LZ4;
        buildParameters.OutputNameStyle = EOutputNameStyle.HashName;
        buildParameters.CopyBuildinFileOption = ECopyBuildinFileOption.None;

        // 执行构建
        AssetBundleBuilder builder = new AssetBundleBuilder();
        var buildResult = builder.Run(buildParameters);
        if (buildResult.Success)
        {
            Debug.Log($"构建成功 : {buildResult.OutputPackageDirectory}");
        }
        else
        {
            Debug.LogError($"构建失败 : {buildResult.ErrorInfo}");
        }
    }

    // 从构建命令里获取参数示例
    private static string GetBuildPackageName()
    {
        foreach (string arg in System.Environment.GetCommandLineArgs())
        {
            if (arg.StartsWith("buildPackage"))
                return arg.Split("="[0])[1];
        }

        return string.Empty;
    }
}