using UnityEditor;
using UnityEngine;
using YooAsset.Editor;

namespace TEngine
{
    /// <summary>
    /// 打包工具类。
    /// <remarks>通过CommandLineReader可以不前台开启Unity实现静默打包，详见CommandLineReader.cs example1</remarks>
    /// </summary>
    public static class ReleaseTools
    {
        public static void BuildPackage()
        {
            string outputRoot = CommandLineReader.GetCustomArgument("outputRoot");
            BuildTarget target = BuildTarget.StandaloneWindows64;
            BuildInternal(target,outputRoot);
            Debug.LogWarning($"Start BuildPackage BuildTarget:{target} outputPath:{outputRoot}");
        }
        
        private static void BuildInternal(BuildTarget buildTarget,string outputRoot)
        {
            Debug.Log($"开始构建 : {buildTarget}");

            // 构建参数
            BuildParameters buildParameters = new BuildParameters();
            buildParameters.StreamingAssetsRoot = AssetBundleBuilderHelper.GetDefaultStreamingAssetsRoot();
            buildParameters.BuildOutputRoot = outputRoot;//AssetBundleBuilderHelper.GetDefaultBuildOutputRoot();
            buildParameters.BuildTarget = buildTarget;
            buildParameters.BuildPipeline = EBuildPipeline.BuiltinBuildPipeline;
            buildParameters.BuildMode = EBuildMode.ForceRebuild;
            buildParameters.PackageName = "DefaultPackage";
            buildParameters.PackageVersion = "1.0";
            buildParameters.VerifyBuildingResult = true;
            buildParameters.SharedPackRule = new ZeroRedundancySharedPackRule();
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
}