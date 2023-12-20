using System;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using YooAsset.Editor;
using BuildResult = UnityEditor.Build.Reporting.BuildResult;

namespace TEngine
{
    /// <summary>
    /// 打包工具类。
    /// <remarks>通过CommandLineReader可以不前台开启Unity实现静默打包以及CLI工作流，详见CommandLineReader.cs example1</remarks>
    /// </summary>
    public static class ReleaseTools
    {
        public static void BuildDll()
        {
            string platform = CommandLineReader.GetCustomArgument("platform");
            if (string.IsNullOrEmpty(platform))
            {
                Debug.LogError($"Build Asset Bundle Error！platform is null");
                return;
            }

            BuildTarget target = GetBuildTarget(platform);
            BuildDLLCommand.BuildAndCopyDlls(target);
        }

        public static void BuildAssetBundle()
        {
            string outputRoot = CommandLineReader.GetCustomArgument("outputRoot");
            if (string.IsNullOrEmpty(outputRoot))
            {
                Debug.LogError($"Build Asset Bundle Error！outputRoot is null");
                return;
            }

            string packageVersion = CommandLineReader.GetCustomArgument("packageVersion");
            if (string.IsNullOrEmpty(packageVersion))
            {
                Debug.LogError($"Build Asset Bundle Error！packageVersion is null");
                return;
            }

            string platform = CommandLineReader.GetCustomArgument("platform");
            if (string.IsNullOrEmpty(platform))
            {
                Debug.LogError($"Build Asset Bundle Error！platform is null");
                return;
            }

            BuildTarget target = GetBuildTarget(platform);
            BuildInternal(target, outputRoot);
            Debug.LogWarning($"Start BuildPackage BuildTarget:{target} outputPath:{outputRoot}");
        }

        private static BuildTarget GetBuildTarget(string platform)
        {
            BuildTarget target = BuildTarget.NoTarget;
            switch (platform)
            {
                case "Android":
                    target = BuildTarget.Android;
                    break;
                case "IOS":
                    target = BuildTarget.iOS;
                    break;
                case "Windows":
                    target = BuildTarget.StandaloneWindows64;
                    break;
                case "MacOS":
                    target = BuildTarget.StandaloneOSX;
                    break;
                case "Linux":
                    target = BuildTarget.StandaloneLinux64;
                    break;
                case "WebGL":
                    target = BuildTarget.WebGL;
                    break;
                case "Switch":
                    target = BuildTarget.Switch;
                    break;
                case "PS4":
                    target = BuildTarget.PS4;
                    break;
                case "PS5":
                    target = BuildTarget.PS5;
                    break;
            }

            return target;
        }

        private static void BuildInternal(BuildTarget buildTarget, string outputRoot, string packageVersion = "1.0")
        {
            Debug.Log($"开始构建 : {buildTarget}");

            BuildParameters.SBPBuildParameters sbpBuildParameters = new BuildParameters.SBPBuildParameters();
            sbpBuildParameters.WriteLinkXML = true;
            
            // 构建参数
            BuildParameters buildParameters = new BuildParameters();
            buildParameters.StreamingAssetsRoot = AssetBundleBuilderHelper.GetDefaultStreamingAssetsRoot();
            buildParameters.BuildOutputRoot = outputRoot; //AssetBundleBuilderHelper.GetDefaultBuildOutputRoot();
            buildParameters.BuildTarget = buildTarget;
            buildParameters.BuildPipeline = EBuildPipeline.ScriptableBuildPipeline;
            buildParameters.BuildMode = EBuildMode.IncrementalBuild;
            buildParameters.PackageName = "DefaultPackage";
            buildParameters.PackageVersion = packageVersion;
            buildParameters.VerifyBuildingResult = true;
            buildParameters.SharedPackRule = new ZeroRedundancySharedPackRule();
            buildParameters.CompressOption = ECompressOption.LZMA;
            buildParameters.OutputNameStyle = EOutputNameStyle.HashName;
            buildParameters.CopyBuildinFileOption = ECopyBuildinFileOption.ClearAndCopyAll;
            buildParameters.SBPParameters = sbpBuildParameters;

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

        [MenuItem("TEngine/Build/一键打包Windows", false, 30)]
        public static void AutomationBuild()
        {
            BuildDLLCommand.BuildAndCopyDlls(BuildTarget.StandaloneWindows64);
            AssetDatabase.Refresh();
            BuildInternal(BuildTarget.StandaloneWindows64, Application.dataPath + "/../Builds/Windows", "1.0");
            AssetDatabase.Refresh();
            BuildImp(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64, $"{Application.dataPath}/../Builds/Windows/{GetBuildPackageVersion()}Windows.exe");
        }
        
        // 构建版本相关
        private static string GetBuildPackageVersion()
        {
            int totalMinutes = DateTime.Now.Hour * 60 + DateTime.Now.Minute;
            return DateTime.Now.ToString("yyyy-MM-dd") + "-" + totalMinutes;
        }
        
        [MenuItem("TEngine/Build/一键打包Android", false, 30)]
        public static void AutomationBuildAndroid()
        {
            BuildDLLCommand.BuildAndCopyDlls(BuildTarget.Android);
            AssetDatabase.Refresh();
            BuildInternal(BuildTarget.Android, outputRoot:Application.dataPath + "/../Bundles",packageVersion: GetBuildPackageVersion());
            AssetDatabase.Refresh();
            BuildImp(BuildTargetGroup.Android, BuildTarget.Android, $"{Application.dataPath}/../Build/Android/{GetBuildPackageVersion()}Android.apk");
        }
        
        [MenuItem("TEngine/Build/一键打包IOS", false, 30)]
        public static void AutomationBuildIOS()
        {
            BuildDLLCommand.BuildAndCopyDlls(BuildTarget.iOS);
            AssetDatabase.Refresh();
            BuildInternal(BuildTarget.iOS, outputRoot:Application.dataPath + "/../Bundles",packageVersion: GetBuildPackageVersion());
            AssetDatabase.Refresh();
            BuildImp(BuildTargetGroup.iOS, BuildTarget.iOS, $"{Application.dataPath}/../Build/IOS/XCode_Project");
        }

        public static void BuildImp(BuildTargetGroup buildTargetGroup, BuildTarget buildTarget, string locationPathName)
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(buildTargetGroup, BuildTarget.StandaloneWindows64);
            AssetDatabase.Refresh();

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = new[] { "Assets/Scenes/main.unity" },
                locationPathName = locationPathName,
                targetGroup = buildTargetGroup,
                target = buildTarget,
                options = BuildOptions.None
            };
            var report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;
            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"Build success: {summary.totalSize / 1024 / 1024} MB");
            }
            else
            {
                Debug.Log($"Build Failed" + summary.result);
            }
        }
    }
}