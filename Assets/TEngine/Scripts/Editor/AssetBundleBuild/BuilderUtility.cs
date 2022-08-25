using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;
using TEngine.Runtime;

namespace TEngineCore.Editor
{
    public class BuilderUtility
    {
        public enum PlatformType
        {
            Windows,
            OSX,
            Android,
            iOS
        }

        public enum BuildType
        {
            Editor,
            Release,
            Development
        }

        public enum AssetSourceType
        {
            GameSource,
            ArtSource
        }

        [Serializable]
        public enum EnableLogLevel
        {
            Info,
            Assert,
            Warning,
            Error,
            Exception
        }


        [Serializable]
        public enum ScriptBackend
        {
            Mono,
            IL2CPP,
        }


        //Bundle配置策略
        [Serializable]
        public enum BundlePolicy
        {
            [Tooltip("目录下所有文件都会都会分配一个ab名")]
            SingleFile,
            [Tooltip("目录下所有文件都以当前目录名为ab名")]
            Directory,
            [Tooltip("（只检索第一层子目录）\n当前目录下的第一层子目录文件，都会以该目录＋其子目录命名。\n若第一层有单文件则该目录+single命名。")]
            ChildDirectory,
            [Tooltip("当前目录下的文件会采取其已定义好的ab名作为AB名，否则会采用单文件策略命名")]
            CustomAbName,
        }
        //打包Builder策略
        [Serializable]
        public enum BuilderBundlePolicy
        {
            SingleFile,
            Directory,
            Configuration,
        }

        //配置位置
        private const string CONFIG_FOLDER_PATH = FileSystem.BuildPath;

        [MenuItem("Assets/Create/[TEngine] 新建打包配置", priority = -10)]
        private static void CreateBuilderConfig()
        {

            if (Selection.activeObject == null)
            {
                Debug.LogError("请选择目标文件夹");
                return;
            }

#if UNITY_EDITOR_OSX
            string filter = $"{CONFIG_FOLDER_PATH}/IOS";
#elif UNITY_EDITOR_WIN
            string filter = $"{CONFIG_FOLDER_PATH}/Win";
#else
            string filter = $"{CONFIG_FOLDER_PATH}/Android";
#endif

            string targetDirPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (!targetDirPath.StartsWith(filter))
            {
                Debug.LogError($"请在{filter}下新建打包配置\n------------------也可使用菜单栏“TEngine/创建AB配置”");
                return;
            }

            if (!Directory.Exists(targetDirPath))
                Directory.CreateDirectory(targetDirPath);

            string defaultName = "Default";
            string suffix = ".asset";
            var path = $"{targetDirPath}/{defaultName}{suffix}";
            int index = 0;

            int safeCount = 50;
            while (File.Exists(path) && safeCount-- > 0)
            {
                path = $"{targetDirPath}/{defaultName}_{++index}{suffix}";
            }

            if (File.Exists(path))
            {
                Debug.LogError($"已存在超过50的配置，请整理或选择另外文件夹下创建");
                return;
            }

            path = AssetDatabase.GenerateUniqueAssetPath(path);
            var builder = ScriptableObject.CreateInstance<BuilderEditor>();
            AssetDatabase.CreateAsset(builder, path);
            EditorGUIUtility.PingObject(builder);
            AssetDatabase.OpenAsset(builder);
        }

        internal static void SetMacroDefines(ref string macroDefines, BuildType buildType, bool updateMacros = true)
        {
            //逻辑上需最后一个调用
            SetBuildTypeMacroDefines(ref macroDefines, buildType, false);

            if (updateMacros)
                UpdateMacros(macroDefines);
        }

        internal static void SetBuildTypeMacroDefines(ref string macroDefines, BuildType buildType, bool updateMacros = true)
        {
            EditorUserBuildSettings.development = buildType == BuildType.Development;
            ModifyMacroDefine(ref macroDefines, "HOT_FIX", true);
            switch (buildType)
            {
                case BuildType.Editor:
                    ClearMacros(ref macroDefines, false);
                    break;
                case BuildType.Release:
                    ModifyMacroDefine(ref macroDefines, "RELEASE_BUILD", true);
                    ModifyMacroDefine(ref macroDefines, "_DEVELOPMENT_BUILD_", false);
                    ModifyMacroDefine(ref macroDefines, "ASSETBUNDLE_ENABLE", true);
                    break;
                case BuildType.Development:
                    ModifyMacroDefine(ref macroDefines, "_DEVELOPMENT_BUILD_", true);
                    ModifyMacroDefine(ref macroDefines, "RELEASE_BUILD", false);
                    ModifyMacroDefine(ref macroDefines, "ASSETBUNDLE_ENABLE", true);
                    break;
            }

            if (updateMacros)
            {
                UpdateMacros(macroDefines);
            }
        }


        /// <summary>
        /// 设置bugly宏定义
        /// </summary>
        /// <param name="enable"></param>
        internal static void EnableGMSymbols(ref string macro, bool enable, bool immediateUpdate = true)
        {
            ModifyMacroDefine(ref macro, "ENABLE_GM", enable);

            if (immediateUpdate)
                UpdateMacros(macro);
        }


        /// <summary>
        /// 设置是否忽略首包解压
        /// </summary>
        /// <param name="enable"></param>
        internal static void IgnorFirstZip(ref string macro, bool enable, bool immediateUpdate = true)
        {
            ModifyMacroDefine(ref macro, "IGNOR_FIRST_ZIP", enable);

            if (immediateUpdate)
                UpdateMacros(macro);
        }

        /// <summary>
        /// 设置应用商店地址
        /// <param name = "param"></param>
        /// </summary>
        internal static void SetAppURL(string param)
        {
            try
            {
                var stream = new FileStream($"{Application.dataPath}/Resources/{FileSystem.ArtResourcePath}.txt", FileMode.OpenOrCreate);
                var writer = new StreamWriter(stream);
                writer.Write(param);
                writer.Flush();
                writer.Dispose();
                writer.Close();
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
                throw;
            }
        }


        /// <summary>
        /// 清除宏
        /// </summary>
        /// <param name="macroDefines"></param>
        internal static void ClearMacros(ref string macroDefines, bool immediateUpdate = true)
        {
            ModifyMacroDefine(ref macroDefines, "ENABLE_MONO", false);
            ModifyMacroDefine(ref macroDefines, "RELEASE_BUILD", false);
            ModifyMacroDefine(ref macroDefines, "_DEVELOPMENT_BUILD_", false);
            ModifyMacroDefine(ref macroDefines, "IGNOR_FIRST_ZIP", false);
            ModifyMacroDefine(ref macroDefines, "HOT_FIX", false);
            ModifyMacroDefine(ref macroDefines, "USE_LUA_PAK_FILE", false);
            ModifyMacroDefine(ref macroDefines, "USE_LUA_DISCRETE_FILE", false);
            ModifyMacroDefine(ref macroDefines, "ASSETBUNDLE_ENABLE", false);
            ModifyMacroDefine(ref macroDefines, "MONO_ENCRYPT", false);
            ModifyMacroDefine(ref macroDefines, "ENABLE_GM", false);
            ModifyMacroDefine(ref macroDefines, "IGNOR_FIRST_ZIP", false);

            if (immediateUpdate)
                UpdateMacros(macroDefines);
        }

        public static void RefreshBackendMacro(ref string macro, ScriptBackend backend, bool immediateUpdate = true)
        {
            switch (backend)
            {
                case ScriptBackend.Mono:
                    ModifyMacroDefine(ref macro, "ENABLE_MONO", true);
                    break;
                case ScriptBackend.IL2CPP:
                    ModifyMacroDefine(ref macro, "ENABLE_MONO", false);
                    break;
            }
            if (immediateUpdate)
                UpdateMacros(macro);
        }

        public static void RefreshLogLevelMacro(ref string macro, EnableLogLevel logLevel, bool immediateUpdate = true)
        {
            switch (logLevel)
            {
                case EnableLogLevel.Info:
                    ModifyMacroDefine(ref macro, "ENABLE_LOG_INFO", true);
                    ModifyMacroDefine(ref macro, "ENABLE_LOG_ASSERT", true);
                    ModifyMacroDefine(ref macro, "ENABLE_LOG_WARNING", true);
                    ModifyMacroDefine(ref macro, "ENABLE_LOG_ERROR", true);
                    ModifyMacroDefine(ref macro, "ENABLE_LOG_EXCEPTION", true);
                    break;
                case EnableLogLevel.Assert:
                    ModifyMacroDefine(ref macro, "ENABLE_LOG_INFO", false);
                    ModifyMacroDefine(ref macro, "ENABLE_LOG_ASSERT", true);
                    ModifyMacroDefine(ref macro, "ENABLE_LOG_WARNING", true);
                    ModifyMacroDefine(ref macro, "ENABLE_LOG_ERROR", true);
                    ModifyMacroDefine(ref macro, "ENABLE_LOG_EXCEPTION", true);
                    break;
                case EnableLogLevel.Warning:
                    ModifyMacroDefine(ref macro, "ENABLE_LOG_INFO", false);
                    ModifyMacroDefine(ref macro, "ENABLE_LOG_ASSERT", false);
                    ModifyMacroDefine(ref macro, "ENABLE_LOG_WARNING", true);
                    ModifyMacroDefine(ref macro, "ENABLE_LOG_ERROR", true);
                    ModifyMacroDefine(ref macro, "ENABLE_LOG_EXCEPTION", true);
                    break;
                case EnableLogLevel.Error:
                    ModifyMacroDefine(ref macro, "ENABLE_LOG_INFO", false);
                    ModifyMacroDefine(ref macro, "ENABLE_LOG_ASSERT", false);
                    ModifyMacroDefine(ref macro, "ENABLE_LOG_WARNING", false);
                    ModifyMacroDefine(ref macro, "ENABLE_LOG_ERROR", true);
                    ModifyMacroDefine(ref macro, "ENABLE_LOG_EXCEPTION", true);
                    break;
                case EnableLogLevel.Exception:
                    ModifyMacroDefine(ref macro, "ENABLE_LOG_INFO", false);
                    ModifyMacroDefine(ref macro, "ENABLE_LOG_ASSERT", false);
                    ModifyMacroDefine(ref macro, "ENABLE_LOG_WARNING", false);
                    ModifyMacroDefine(ref macro, "ENABLE_LOG_ERROR", false);
                    ModifyMacroDefine(ref macro, "ENABLE_LOG_EXCEPTION", true);
                    break;
            }

            if (immediateUpdate)
                UpdateMacros(macro);
        }

        /// <summary>
        /// 更新projectsetting宏定义
        /// </summary>
        /// <param name="bMacroChanged"></param>
        internal static void UpdateMacros(string macroDefines)
        {
            string _macroDefines = String.Empty;
            switch (EditorUserBuildSettings.activeBuildTarget)
            {
                case BuildTarget.StandaloneWindows64:
                case BuildTarget.StandaloneWindows:
                    _macroDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone);
                    EditorUserBuildSettings.selectedBuildTargetGroup = BuildTargetGroup.Standalone;
                    break;
                case BuildTarget.Android:
                    _macroDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android);
                    EditorUserBuildSettings.selectedBuildTargetGroup = BuildTargetGroup.Android;
                    break;
                case BuildTarget.iOS:
                    _macroDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS);
                    EditorUserBuildSettings.selectedBuildTargetGroup = BuildTargetGroup.iOS;
                    break;
            }

            if (!_macroDefines.Equals(macroDefines))
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, macroDefines);
        }

        internal static void ModifyMacroDefine(ref string macroDefines, string macro, bool flag)
        {
            if (flag)
            {
                if (macroDefines.IndexOf(macro, StringComparison.Ordinal) < 0)
                    macroDefines = macroDefines.Length > 0 ? $"{macroDefines};{macro}" : macro;
            }
            else
            {
                int index = macroDefines.IndexOf(macro, StringComparison.Ordinal);
                if (index >= 0)
                {
                    macroDefines = macroDefines.Remove(index, macro.Length);

                    index = Mathf.Max(index - 1, 0);
                    if (macroDefines.Length > 0 && macroDefines[index] == ';')
                        macroDefines = macroDefines.Remove(index, 1);
                }
            }
        }

        internal static BuilderEditor LoadConfig(PlatformType target, string config, string configPath)
        {
            if (string.IsNullOrEmpty(configPath))
            {
                configPath = CONFIG_FOLDER_PATH;
            }

            string path;
            switch (target)
            {
                case PlatformType.Android:
                    path = $"{configPath}/Android/{config}.asset";
                    break;
                case PlatformType.iOS:
                    path = $"{configPath}/iOS/{config}.asset";
                    break;
                case PlatformType.Windows:
                    path = $"{configPath}/Win/{config}.asset";
                    break;
                default:
                    path = $"{configPath}/New Builder.asset";
                    break;
            }

            string defualtPath;
            switch (target)
            {
                case PlatformType.Android:
                    defualtPath = $"{configPath}/Android_Release.asset";
                    break;
                case PlatformType.Windows:
                    defualtPath = $"{configPath}/Win_Release.asset";
                    break;
                default:
                    defualtPath = $"{configPath}/iOS_Release.asset";
                    break;
            }

            if (!File.Exists(path))
            {
                path = defualtPath;
            }

            Debug.Log("当前使用配置:" + path);
            return AssetDatabase.LoadAssetAtPath<BuilderEditor>(path);
        }

        /// <summary>
        /// 提取控制台参数
        /// </summary>
        /// <param name="keyString"></param>
        /// <returns></returns>
        public static string GetArgumentValue(string keyString)
        {
            var args = Environment.GetCommandLineArgs();

            keyString = $"-{keyString}";
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].Equals(keyString))
                {
                    i++;
                    return i < args.Length ? args[i] : null;
                }
            }
            return null;
        }


        /// <summary>
        /// 从序列化对象中获取数据
        /// </summary>
        /// <param name="serializedObject"></param>
        /// <param name="argv"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetArgvsFromSerializedObject<T>(SerializedObject serializedObject, string argv)
        {
            object result = null;
            if (typeof(T) == typeof(Boolean))
            {
                if (serializedObject.FindProperty(argv) != null)
                {
                    result = (object)serializedObject.FindProperty(argv).boolValue;
                }
            }
            else if (typeof(T) == typeof(int))
            {
                if (serializedObject.FindProperty(argv) != null)
                {
                    result = (object)serializedObject.FindProperty(argv).intValue;
                }
            }
            else if (typeof(T) == typeof(string))
            {
                if (serializedObject.FindProperty(argv) != null)
                {
                    result = (object)serializedObject.FindProperty(argv).stringValue;
                }
            }
            else if (typeof(T) == typeof(BuilderBundlePolicy))
            {
                if (serializedObject.FindProperty(argv) != null)
                {
                    result = (object)serializedObject.FindProperty(argv).enumValueIndex;
                }
            }
            else if (typeof(T) == typeof(BundlePolicyConfig))
            {
                if (serializedObject.FindProperty(argv) != null)
                {
                    result = (object)serializedObject.FindProperty(argv).objectReferenceValue;
                }
            }
            else if (typeof(T) == typeof(EnableLogLevel))
            {
                if (serializedObject.FindProperty(argv) != null)
                {
                    result = (object)serializedObject.FindProperty(argv).enumValueIndex;
                }
            }

            return (T)result;
        }

        /// <summary>
        /// 策略快查
        /// </summary>
        /// <param name="bundleBundlePolicy"></param>
        /// <param name="assetBundleConfig"></param>
        /// <returns></returns>
        internal static bool PolicyEasyCheck(BuilderBundlePolicy bundleBundlePolicy, List<BundleConfigItem> assetBundleConfig)
        {
            if (bundleBundlePolicy != BuilderBundlePolicy.Configuration)
            {
                Debug.Log("非策略模式，资源已全部覆盖");
                return true;
            }

            if (assetBundleConfig == null || assetBundleConfig.Count == 0)
            {
                Debug.LogError("不正常的策略：策略为空");
                return false;
            }

            EditorUtility.DisplayProgressBar("正在进行资源检查", "快速检查中", 0.7f);

            bool result = true;

            HashSet<string> filter = new HashSet<string>();
            HashSet<string> customDirs = new HashSet<string>();
            string suffix = Application.dataPath.Substring(0, Application.dataPath.Length - 6);

            foreach (var dir in assetBundleConfig)
            {
                string path = AssetDatabase.GetAssetPath(dir.mObject);
                var policy = (BundlePolicy)(dir.buildType);

                if (policy == BundlePolicy.CustomAbName)
                    customDirs.Add(suffix + path);
                filter.Add(suffix + path);
            }

            Check(new DirectoryInfo(Application.dataPath + "/Game/AssetSource/GameResources/"));

            //针对自定义AB名
            foreach (string dir in customDirs)
            {
                using (FileTree fileTree =
                    FileTree.CreateWithExcludeFilter(dir, new[] { ".meta", ".unity", ".DS_Store" }))
                {
                    List<FileInfo> files = fileTree.GetAllFiles();
                    for (int i = 0; i < files.Count; ++i)
                    {
                        if (string.IsNullOrEmpty(AssetImporter.GetAtPath(files[i].GetAssetPath()).assetBundleName))
                        {
                            result = false;
                            Debug.LogError("（自定义AB名策略）发现未命名的资源：" + files[i].GetAssetPath());
                        }
                    }
                }
            }

            if (result)
                Debug.Log("策略已完全覆盖");


            EditorUtility.ClearProgressBar();
            return result;

            void Check(FileSystemInfo info)
            {
                if (filter.Contains(info.FullName.Replace("\\", "/")))
                    return;
                if (!info.Exists) return;
                DirectoryInfo dir = info as DirectoryInfo;
                //不是目录
                if (dir == null) return;
                if (dir.Name.Equals("RawBytes")) return;

                FileSystemInfo[] files = dir.GetFileSystemInfos();
                for (int i = 0; i < files.Length; i++)
                {
                    FileInfo file = files[i] as FileInfo;
                    //是文件
                    if (file != null && !file.FullName.EndsWith(".meta"))
                    {
                        Debug.LogError("发现无任何策略的单文件！：" + file.FullName);
                        result = false;
                    }
                    else
                        Check(files[i]);
                }
            }
        }

        /// <summary>
        /// 策略快查
        /// </summary>
        /// <returns></returns>
        internal static bool PolicyEasyCheck(BuilderBundlePolicy bundleBundlePolicy, BundlePolicyConfig assetBundleConfig = null)
        {
            if (assetBundleConfig == null || assetBundleConfig.directoryBuild == null || assetBundleConfig.directoryBuild.Count == 0)
            {
                if (bundleBundlePolicy == BuilderBundlePolicy.Configuration)
                {
                    Debug.LogError("不正常的策略：策略为空");
                    return false;
                }
                else
                {
                    return true;
                }
            }

            var targets = assetBundleConfig.directoryBuild;

            return PolicyEasyCheck(bundleBundlePolicy, targets);
        }


        /// <summary>
        /// 将数据导出成CSV
        /// </summary>
        /// <param name="name"></param>
        /// <param name="content"></param>
        public static void ExportCsv(string name, StringBuilder content)
        {
            string suffix = Application.dataPath.Substring(0, Application.dataPath.Length - 6);
            string logFilePath = suffix + "/BuildBundleInfo/" + name;
            if (File.Exists(logFilePath))
                File.Delete(logFilePath);

            if (!Directory.Exists(suffix + "/BuildBundleInfo"))
                Directory.CreateDirectory(suffix + "/BuildBundleInfo");

            using (var writer = System.IO.File.CreateText(logFilePath))
            {
                writer.Write(Encoding.UTF8.GetString(Encoding.Default.GetBytes(content.ToString())));
                writer.Close();
            }

            string str = string.Empty;
            using (StreamReader sr = new StreamReader(logFilePath, Encoding.UTF8))
            {
                str = sr.ReadToEnd();
                sr.Close();
            }

            //以UTF-8带BOM格式重新写入文件
            Encoding newEncoding = new UTF8Encoding(true);
            using (StreamWriter sw = new StreamWriter(logFilePath, false, newEncoding))
            {
                sw.Write(str);
                sw.Close();
            }


            EditorUtility.ClearProgressBar();
            UnityEngine.Debug.Log(logFilePath + $" 导出完成.");
        }

        /// <summary>
        /// 清理路径下数据
        /// </summary>
        internal static void ClearAllByPath(string path = null, bool deleteZip = false)
        {
            if (string.IsNullOrEmpty(path))
            {
                path = FileSystem.ResourceRootInStreamAsset;
            }
            if (Directory.Exists(path))
                Directory.Delete(path, true);

            if (File.Exists($"{path}.meta"))
                File.Delete($"{path}.meta");

            AssetDatabase.Refresh();

            //if (deleteZip)
            //{
            //    LoaderUtilities.DeleteFile(FileSystem.FirstZipSteamAssetsPath);
            //}
        }



        public static void RunExternalCommand(string cmd, string args, string workDir = ".")
        {
            Process p = new Process();
            p.StartInfo.FileName = cmd;
            p.StartInfo.Arguments = args;
            //p.StartInfo.UseShellExecute = false;
            //p.StartInfo.RedirectStandardInput = false;
            //p.StartInfo.RedirectStandardOutput = true;
            //p.StartInfo.RedirectStandardError = true;
            //p.StartInfo.CreateNoWindow = false;
            p.StartInfo.WorkingDirectory = workDir;
            p.Start();
            //Debug.Log(p.StandardOutput.ReadToEnd());
            p.WaitForExit();
            p.Close();
        }
        public static void RunPythonExternalCommand(string cmd, string args, string workDir = ".")
        {
            Process p = new Process();
            p.StartInfo.FileName = "python";
            p.StartInfo.Arguments = cmd + " " + args;

            //p.StartInfo.UseShellExecute = false;
            //p.StartInfo.RedirectStandardInput = false;
            //p.StartInfo.RedirectStandardOutput = true;
            //p.StartInfo.RedirectStandardError = true;
            //p.StartInfo.CreateNoWindow = false;
            p.StartInfo.WorkingDirectory = workDir;
            p.Start();
            //Debug.Log(p.StandardOutput.ReadToEnd());
            p.WaitForExit();
            p.Close();
        }

        internal static uint GetIntHash(string str)
        {
            int h = 0;
            int len = str.Length;
            if (len > 0)
            {
                int off = 0;

                for (int i = 0; i < len; i++)
                {
                    char c = str[off++];
                    h = 31 * h + c;
                }
            }
            return (uint)h;
        }

        internal static string GetStringHash(string str)
        {
            return GetIntHash(str).ToString();
        }
    }
    public static class FileInfoExtension
    {
        public static string GetAssetPath(this FileInfo fileInfo)
        {
            string uniPath = fileInfo.FullName.Replace('\\', '/');
            return uniPath.Substring(uniPath.IndexOf("Assets/"));
        }

        public static string NameNoExtension(this FileInfo fileInfo)
        {
            return fileInfo.Name.Substring(0, fileInfo.Name.LastIndexOf('.'));
        }
    }
}