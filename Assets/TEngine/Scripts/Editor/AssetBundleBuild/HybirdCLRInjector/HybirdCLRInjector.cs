using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using HybridCLR.Editor;
using TEngine.Runtime;
using UnityEditor;
using UnityEngine;

namespace TEngine.Editor
{
    /// <summary>
    /// HybirdCLR 热更新 打包插入管线
    /// </summary>
    public static class HybirdCLRInjectorEditor
    {
        [TEngineBuilderInjector(BuilderInjectorMoment.BeforeCollect_AssetBundle)]
        public static void BeforeCollectAssetBundle()
        {
            CompileDllHelper.CompileDllActiveBuildTarget();
            Log.Warning("CompileDllHelper.CompileDllActiveBuildTarget()");
            
            var target = EditorUserBuildSettings.activeBuildTarget;
            var dllDir = Constant.Setting.AssetRootPath+"/Dll";

            string hotfixDllSrcDir = BuildConfig.GetHotFixDllsOutputDirByTarget(target);
            foreach (var dll in BuildConfig.HotUpdateAssemblies)
            {
                string dllPath = $"{hotfixDllSrcDir}/{dll}";
                string dllBytesPath = $"{dllDir}/{dll}.bytes";
                try
                {
                    File.Copy(dllPath, dllBytesPath, true);
                }
                catch (Exception e)
                {
                    Log.Fatal(e.Message);
                }
            }
        }
    }
}