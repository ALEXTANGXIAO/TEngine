using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using TEngine.Editor;
using TEngine.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEditor.Build.Reporting;
using UnityEditor.Callbacks;
using UnityEditor.Compilation;
using Assembly = System.Reflection.Assembly;
using Debug = UnityEngine.Debug;

namespace TEngineCore.Editor
{
	public class Builder : IBuilder
	{
		internal static readonly Builder Instance = new Builder();

		/// <summary>
		/// 数据部分
		/// </summary>
		internal BuilderEditor CurBuilderData;

		/// <summary>
		/// AB构建器
		/// </summary>
		internal AssetbundleBuilder AssetbundleBuilder = new AssetbundleBuilder();

		/// <summary>
		/// 设置当前的BuidlerConfigData
		/// </summary>
		/// <param name="platform"></param>
		/// <param name="configName"></param>
		public void SetBuilderConfig(BuilderUtility.PlatformType platform, string configName, string configPath = "")
		{
			CurBuilderData = BuilderUtility.LoadConfig(platform, configName, configPath);
			if (CurBuilderData == null)
			{
				TLogger.LogError("未找到配置，config:" + configName);
				Process.GetCurrentProcess().Kill();
			}

			CurBuilderData.platform = platform;
		}

		/// <summary>
		/// 设置当前的BuidlerConfigData
		/// </summary>
		/// <param name="platform"></param>
		/// <param name="configName"></param>
		public void SetBuilderConfig(BuilderEditor tmpBuilder)
		{
			CurBuilderData = tmpBuilder;
		}

		/// <summary>
		/// 切换平台
		/// </summary>
		/// <param name="platform"></param>
		public void SwitchPlatform(BuilderUtility.PlatformType platform)
		{
			if (CurBuilderData == null)
			{
				TLogger.LogError("未设置BuilderData，请先调用接口：SetBuilderConfig");
				return;
			}

			CurBuilderData.platform = platform;
			switch (CurBuilderData.platform)
			{
				case BuilderUtility.PlatformType.Windows:
				{
					EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone,
						BuildTarget.StandaloneWindows64);

					PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.Mono2x);
					PlayerSettings.stripEngineCode = false;
					PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.Standalone, ManagedStrippingLevel.Disabled);
					PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Standalone, CurBuilderData.bundleIdentifier);
				}
					break;
				case BuilderUtility.PlatformType.Android:
				{
					EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);

					EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
					PlayerSettings.Android.bundleVersionCode = 1;
					PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android,
						CurBuilderData.scriptingBackend == BuilderUtility.ScriptBackend.Mono ? ScriptingImplementation.Mono2x : ScriptingImplementation.IL2CPP);
					Debug.Log("=============CurBuilderData.scriptingBackend：" + CurBuilderData.scriptingBackend);
					PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, CurBuilderData.bundleIdentifier);
					PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.Android, ApiCompatibilityLevel.NET_4_6);
				}
					break;
				case BuilderUtility.PlatformType.iOS:
				{
					EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, BuildTarget.iOS);

					PlayerSettings.iOS.appleEnableAutomaticSigning = false;
					PlayerSettings.iOS.scriptCallOptimization = ScriptCallOptimizationLevel.SlowAndSafe;
					PlayerSettings.iOS.targetOSVersionString = "8.0";
					PlayerSettings.iOS.buildNumber = "1";
					PlayerSettings.SetScriptingBackend(BuildTargetGroup.iOS, ScriptingImplementation.IL2CPP);
					PlayerSettings.stripEngineCode = false;
					PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.iOS, ManagedStrippingLevel.Disabled);
					PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, CurBuilderData.bundleIdentifier);
					PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.iOS, ApiCompatibilityLevel.NET_4_6);
				}
					break;
			}
		}


		/// <summary>
		/// 构建包体
		/// </summary>
		/// <param name="isDirect">是否直接出包</param>
		internal void Build(bool isDirect = false, BuildOptions option = BuildOptions.None)
		{
			var platform = CurBuilderData.platform;

			SwitchPlatform(platform);

			DoBuild(isDirect, option);
		}


		/// <summary>
		/// 执行完整构建流程
		/// </summary>
		/// <param name="isDirectBuild"></param>
		/// <param name="option"></param>
		void DoBuild(bool isDirectBuild, BuildOptions option)
		{
			var buildStart = DateTime.Now;

			if (!isDirectBuild)
			{
				if (!BuildAssetBundle())
					return;
			}
			else
			{
				Instance.AssetbundleBuilder.CopyAssetBundles();
			}

			BuildPackage(option);
			Debug.Log("Apk打包总耗时：" + (DateTime.Now - buildStart).TotalMinutes.ToString("F2") + " 分钟");
		}

		/// <summary>
		/// 构建AB包部分
		/// </summary>
		/// <returns></returns>
		public bool BuildAssetBundle()
		{
			if (CurBuilderData == null)
			{
				TLogger.LogError("未设置BuilderData，请先调用接口：SetBuilderConfig");
				return false;
			}

			var start = DateTime.Now;

			BuilderInjectorHandler(BuilderInjectorMoment.BeforeCollect_AssetBundle);
			if (!Instance.AssetbundleBuilder.CollectAssetBundles(CurBuilderData.builderBundlePolicy, CurBuilderData.bundleConfig))
				return false;

			if (CurBuilderData.bCollectShaderVariant)
			{
				Debug.Log("CollectShaderVariant");
				var list = ShaderVariantCollector.ClollectSharderAndVariant();
				string abName = ShaderVariantCollector.GetShaderVariantAbName();
				Dictionary<string, string> additionalRes = new Dictionary<string, string>();
				foreach (var sv in list)
				{
					if (!additionalRes.ContainsKey(sv))
						additionalRes.Add(sv, abName);
					else
						Debug.LogError("重复的变体路径：" + sv);
				}

				list.Clear();
				Instance.AssetbundleBuilder.InsertAdditionalTopRes(additionalRes, false);
			}

			Instance.AssetbundleBuilder.SetAndCheckAssetDependencies();

			BuilderInjectorHandler(BuilderInjectorMoment.BeforeBuild_AssetBundle);

			Instance.AssetbundleBuilder.BuildAssetBundlesAfterCollect(CurBuilderData.buildType == BuilderUtility.BuildType.Development, CurBuilderData.bIncrementBuildAB);
			BuilderInjectorHandler(BuilderInjectorMoment.AfterBuild_AssetBundle);
			Debug.Log("AB打包总耗时：" + (DateTime.Now - start).TotalMinutes.ToString("F2") + " 分钟");
			return true;
		}

		public void BuildActive(bool isDirectBuild,BuildOptions options = BuildOptions.None)
		{
			var platform = CurBuilderData.platform;

			var buildStart = DateTime.Now;

			if (!isDirectBuild)
			{
				BuildAssetsCommand.BuildAndCopyABAOTHotUpdateDlls();

				if (!BuildAssetBundle())
					return;
			}
			else
			{
				Instance.AssetbundleBuilder.CopyAssetBundles();
			}

			if (CurBuilderData == null)
			{
				TLogger.LogError("未设置BuilderData，请先调用接口：SetBuilderConfig");
				return;
			}

			CurBuilderData.ApplyArgs("");

			PlayerSettings.productName = CurBuilderData.productName;
			PlayerSettings.bundleVersion = CurBuilderData.bundleVersion + "." + CurBuilderData._bBaseVersion;

			if (EditorUserBuildSettings.development)
				options |= BuildOptions.Development;
			if (EditorUserBuildSettings.connectProfiler)
				options |= BuildOptions.ConnectWithProfiler;
			if (EditorUserBuildSettings.buildWithDeepProfilingSupport)
				options |= BuildOptions.EnableDeepProfilingSupport;

			string applicationName = string.Empty;
			BuildTarget target = BuildTarget.NoTarget;
			switch (CurBuilderData.platform)
			{
				case BuilderUtility.PlatformType.Android:
					applicationName = CurBuilderData.bExportAndroidProject ? $"{CurBuilderData.productName}" : $"{CurBuilderData.productName}.apk";
					target = BuildTarget.Android;
					break;
				case BuilderUtility.PlatformType.iOS:
					applicationName = $"{CurBuilderData.productName}.ipa";
					target = BuildTarget.iOS;
					break;
				case BuilderUtility.PlatformType.Windows:
					applicationName = $"{CurBuilderData.productName}.exe";
					target = BuildTarget.StandaloneWindows64;
					break;
				default:
					UnityEngine.Debug.LogError("Not supported target platform");
					return;
			}

			SetAppStoreUrl();

			LoaderUtilities.DeleteFolder(FileSystem.ResourceRoot);

			BuilderInjectorHandler(BuilderInjectorMoment.BeforeBuild_Apk);

			BuilderInjectorHandler(BuilderInjectorMoment.BeforeBuild_FirstZip);

			string export = CurBuilderData.ProjectExportPath;
			if (string.IsNullOrEmpty(export))
				export = $"{FileSystem.BuildPath}/{(CurBuilderData.platform).ToString()}/{applicationName}";

			BuildReport result = BuildPipeline.BuildPlayer(new string[] { EditorBuildSettings.scenes[0].path },
				export, target, options);

			switch (result.summary.result)
			{
				case BuildResult.Unknown:
					TLogger.LogInfo("Build Package FAIL!  未知错误！");
					break;
				case BuildResult.Succeeded:
					TLogger.LogInfo("Build Package OK!");
					break;
				case BuildResult.Failed:
					TLogger.LogInfo("Build Package FAIL!");
					break;
				case BuildResult.Cancelled:
					TLogger.LogInfo("Build Package Cancelled!");
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			if (Application.isBatchMode && result.summary.result != BuildResult.Succeeded)
			{
				Process.GetCurrentProcess().Kill();
			}

			BuilderInjectorHandler(BuilderInjectorMoment.AfterBuild_Apk);

			Debug.Log("一键打包总耗时：" + (DateTime.Now - buildStart).TotalMinutes.ToString("F2") + " 分钟");
		}

		/// <summary>
		/// 输出apk或安卓工程
		/// </summary>
		/// <param name="option"></param>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public void BuildPackage(BuildOptions option = BuildOptions.None)
		{
			if (CurBuilderData == null)
			{
				TLogger.LogError("未设置BuilderData，请先调用接口：SetBuilderConfig");
				return;
			}

			CurBuilderData.ApplyArgs("");

			PlayerSettings.productName = CurBuilderData.productName;
			PlayerSettings.bundleVersion = CurBuilderData.bundleVersion + "." + CurBuilderData._bBaseVersion;

			if (EditorUserBuildSettings.development)
				option |= BuildOptions.Development;
			if (EditorUserBuildSettings.connectProfiler)
				option |= BuildOptions.ConnectWithProfiler;
			if (EditorUserBuildSettings.buildWithDeepProfilingSupport)
				option |= BuildOptions.EnableDeepProfilingSupport;

			string applicationName = string.Empty;
			BuildTarget target = BuildTarget.NoTarget;
			switch (CurBuilderData.platform)
			{
				case BuilderUtility.PlatformType.Android:
					applicationName = CurBuilderData.bExportAndroidProject ? $"{CurBuilderData.productName}" : $"{CurBuilderData.productName}.apk";
					target = BuildTarget.Android;
					break;
				case BuilderUtility.PlatformType.iOS:
					applicationName = $"{CurBuilderData.productName}.ipa";
					target = BuildTarget.iOS;
					break;
				case BuilderUtility.PlatformType.Windows:
					applicationName = $"{CurBuilderData.productName}.exe";
					target = BuildTarget.StandaloneWindows64;
					break;
				default:
					UnityEngine.Debug.LogError("Not supported target platform");
					return;
			}

			SetAppStoreUrl();

			LoaderUtilities.DeleteFolder(FileSystem.ResourceRoot);

			BuilderInjectorHandler(BuilderInjectorMoment.BeforeBuild_Apk);

			BuilderInjectorHandler(BuilderInjectorMoment.BeforeBuild_FirstZip);

			MakeFirstZip();

			BuilderInjectorHandler(BuilderInjectorMoment.AfterBuild_FirstZip);

			CopyRawBytes();

			string export = CurBuilderData.ProjectExportPath;
			if (string.IsNullOrEmpty(export))
				export = $"{FileSystem.BuildPath}/{(CurBuilderData.platform).ToString()}/{applicationName}";

			BuildReport result = BuildPipeline.BuildPlayer(new string[] { EditorBuildSettings.scenes[0].path },
				export, target, option);

			switch (result.summary.result)
			{
				case BuildResult.Unknown:
					TLogger.LogInfo("Build Package FAIL!  未知错误！");
					break;
				case BuildResult.Succeeded:
					TLogger.LogInfo("Build Package OK!");
					break;
				case BuildResult.Failed:
					TLogger.LogInfo("Build Package FAIL!");
					break;
				case BuildResult.Cancelled:
					TLogger.LogInfo("Build Package Cancelled!");
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			if (Application.isBatchMode && result.summary.result != BuildResult.Succeeded)
			{
				Process.GetCurrentProcess().Kill();
			}

			BuilderInjectorHandler(BuilderInjectorMoment.AfterBuild_Apk);
		}


		/// <summary>
		/// Builder流程插入处理器
		/// </summary>
		/// <param name="moment"></param>
		internal void BuilderInjectorHandler(BuilderInjectorMoment moment)
		{
			try
			{
				var compilationAssemblies = CompilationPipeline.GetAssemblies();
				List<Assembly> assemblies = new List<Assembly>();
				foreach (var assembly in compilationAssemblies)
				{
					assemblies.Add(Assembly.LoadFrom(assembly.outputPath));
				}

				foreach (var assembly in assemblies)
				{
					var types = assembly.GetTypes();
					foreach (var type in types)
					{
						MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Static |
						                                       BindingFlags.Public | BindingFlags.IgnoreCase);
						foreach (var method in methods)
						{
							foreach (var att in method.GetCustomAttributes(false))
							{
								if (att is TEngineBuilderInjectorAttribute a)
								{
									if (!assembly.FullName.Contains("Editor"))
									{
										Debug.LogError($"{type.Name}.cs不在Editor中，已跳过");
										continue;
									}

									if (type.IsSubclassOf(typeof(UnityEngine.Object)))
									{
										Debug.LogError(
											$"{type.Name}.cs中函数：{method.Name}，标记了BuilderInjectorHandler，请勿在mono脚本上调用，已跳过");
										continue;
									}

									if (!a.IsInMoment(moment))
										continue;

									if (method.IsStatic)
									{
										method.Invoke(null, null);
									}
									else
									{
										var obj = Activator.CreateInstance(type);
										method.Invoke(obj, null);
									}

									Debug.Log($"已完成执行插入方法：{method.Name}");
								}
							}
						}
					}
				}
			}
			catch (Exception e)
			{
				Debug.LogError($"BuilderInjectorHandler_{moment.ToString()}:" + e);
			}
		}

		/// <summary>
		/// 拷贝RawBytes文件部分
		/// </summary>
		private static void CopyRawBytes()
		{
			//复制到StreamingAsset下
			string target = $"{string.Format("{0}/RawBytes", FileSystem.ResourceRootInStreamAsset)}";
			if (Directory.Exists(target))
				Directory.Delete(target, true);
			Directory.CreateDirectory(target);

			LoaderUtilities.CopyDirectory(AssetConfig.AssetRootPath + "/" + "RawBytes", target);

			LoaderUtilities.ClearMeta(target);

			AssetDatabase.Refresh();
		}


		[PostProcessBuild(1)]
		static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
		{
			Debug.Log($"target: {target.ToString()}");
			Debug.Log($"pathToBuiltProject: {pathToBuiltProject}");
			Debug.Log($"productName: {PlayerSettings.productName}");
			Debug.Log($"version:{GameConfig.Instance.GameBundleVersion}");
			string versionStr = GameConfig.Instance.GameBundleVersion.Replace(".", "");
			long versionLong = long.Parse(versionStr);
			Debug.Log($"versionStr:{versionStr}");

			GameConfig.Instance.WriteBaseResVersion(GameConfig.Instance.ResId);

			if (target == BuildTarget.Android)
			{
				string explore = BuilderUtility.GetArgumentValue("ExploreAndroidProject");
				if (Instance.CurBuilderData.scriptingBackend == BuilderUtility.ScriptBackend.Mono && !string.IsNullOrEmpty(explore) && explore == "true")
				{
					byte[] versionByte = new byte[8];
					for (int i = 0; i < versionByte.Length; ++i)
						versionByte[i] = (byte)((versionLong >> (i * 8)) & 0xff);
#if UNITY_2019_4_OR_NEWER
					string dllPath =
						$"{pathToBuiltProject}/unityLibrary/src/main/assets/bin/Data/Managed/Assembly-CSharp.dll";
#else
                    string dllPath =
                        $"{pathToBuiltProject}/{PlayerSettings.productName}/src/main/assets/bin/Data/Managed/Assembly-CSharp.dll";
#endif

					if (File.Exists(dllPath))
					{
						Debug.Log("Encrypt Assembly-CSharp.dll Start");

						byte[] bytes = File.ReadAllBytes(dllPath);
						for (int i = 0; i < 901; ++i)
							bytes[i] ^= 0x31;
						for (int i = 0; i < bytes.Length; i += 2)
							bytes[i] ^= bytes[bytes.Length - i - 1];
						using (FileStream dllStream = File.OpenWrite(dllPath))
						{
							dllStream.WriteByte((byte)'L');
							dllStream.WriteByte((byte)'U');
							dllStream.WriteByte((byte)'A');
							dllStream.WriteByte((byte)'C');
							foreach (var t in versionByte)
								dllStream.WriteByte(t);

							dllStream.Write(bytes, 0, bytes.Length);
						}

						Debug.Log("Encrypt Assembly-CSharp.dll Success");

						Debug.Log("Encrypt libmonobdwgc-2.0.so Start !!");

						Debug.Log($"Current is : {(EditorUserBuildSettings.development ? "development" : "release")}");

#if UNITY_2019_4_OR_NEWER
						string armv7ASoPath =
							$"{pathToBuiltProject}/unityLibrary/src/main/jniLibs/armeabi-v7a/libmonobdwgc-2.0.so";
						string x86SoPath = $"{pathToBuiltProject}/unityLibrary/src/main/jniLibs/x86/libmonobdwgc-2.0.so";
#else
                        string armv7ASoPath =
                            $"{pathToBuiltProject}/{PlayerSettings.productName}/src/main/jniLibs/armeabi-v7a/libmonobdwgc-2.0.so";
                        string x86SoPath = $"{pathToBuiltProject}/{PlayerSettings.productName}/src/main/jniLibs/x86/libmonobdwgc-2.0.so";
#endif
#if UNITY_2019_4_OR_NEWER
						AppendDll(dllPath, $"{pathToBuiltProject}/unityLibrary/src/main/assets/");
#else
                        AppendDll(dllPath, $"{pathToBuiltProject}/{PlayerSettings.productName}/src/main/assets/");
#endif
					}
					else
					{
						Debug.LogError(dllPath + "  Not Found!!");
					}
				}
			}

			string[] dirList =
			{
				$"{Environment.CurrentDirectory}/Library/il2cpp_android_armeabi-v7a/il2cpp_cache", $"{Environment.CurrentDirectory}/Library/il2cpp_android_arm64-v8a/il2cpp_cache"
			};
			foreach (var dir in dirList)
			{
				if (Directory.Exists(dir))
				{
					DirectoryInfo root = new DirectoryInfo(dir);
					DirectoryInfo[] dics = root.GetDirectories();
					foreach (var childDir in dics)
					{
						if (childDir.Name.Contains("linkresult_"))
						{
							DirectoryInfo directory = new DirectoryInfo(childDir.FullName);
							FileInfo[] files = directory.GetFiles();
							for (int i = 0; i < files.Length; i++)
							{
								if (files[i].Name == "libil2cpp.sym.so")
								{
									string newPath = null;
									if (files[i].FullName.Contains("armeabi-v7a"))
										newPath = $"{Environment.CurrentDirectory}/SymbolsTemp/armeabi-v7a";
									else if (files[i].FullName.Contains("arm64-v8a"))
										newPath = $"{Environment.CurrentDirectory}/SymbolsTemp/arm64-v8a";
									if (!Directory.Exists(newPath))
									{
										Directory.CreateDirectory(newPath);
									}

									File.Copy(files[i].FullName, $"{newPath}/libil2cpp.sym.so", true);
								}
							}
						}
					}
				}
				else
				{
					//TLogger.LogWarning($"文件夹不存在：{dir}");
				}
			}
		}


		/// <summary>
		/// 加塞一下dll
		/// </summary>
		/// <param name="dll_path"></param>
		/// <param name="des_path"></param>
		static void AppendDll(string dll_path, string des_path)
		{
			//备份到本地目录
			File.Copy(dll_path, $"{FileSystem.BuildPath}/{"Script.bin"}", true);
			//备份到streamAsset目录
			File.Copy(dll_path, $"{des_path}/TEngineResRoot/{"Script.bin"}", true);
		}

		/// <summary>
		/// 设置跳转地址
		/// </summary>
		internal void SetAppStoreUrl()
		{
#if UNITY_IOS
            BuilderUtility.SetAppURL(CurBuilderData.appUrl);
#endif
		}

		/// <summary>
		/// 是否首包压缩
		/// </summary>
		internal void MakeFirstZip()
		{
			string outputPath = $"{Application.streamingAssetsPath}/{"First.zip"}";
			if (File.Exists(outputPath))
			{
				File.Delete(outputPath);
			}
		}
	}
}