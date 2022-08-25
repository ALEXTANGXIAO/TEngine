using System;
using System.IO;
using TEngine;
using TEngine.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace TEngineCore.Editor
{
    public class BuilderEditor : SoEditorBase
    {
        #region 顶端

        [BuilderEditor("全局宏展示：", ContentType.Label, EditorContent.HorizontalStart + ",FlowA:disPlayType:1")]
        internal string symbolLaebl;
        private ContentLayout _symbolLaeblLayout = new ContentLayout(GUILayout.Height(100), GUILayout.MaxWidth(70));


        [SerializeField]
        [BuilderEditor("", ContentType.ScrollLabel, "FlowA:disPlayType:1")]
        private string symbolWin;
        private ContentLayout _symbolWinLayout = new ContentLayout(GUILayout.MaxHeight(100), GUILayout.ExpandWidth(true));

        [SerializeField]
        [BuilderEditor("↓↑", ContentType.Button, EditorContent.HorizontalEnd + ",FlowA:disPlayType:1,CB:SwitchSymbolWinLayout")]
        private int _switchSymbolWinL;
        private ContentLayout _switchSymbolWinLLayout = new ContentLayout(GUILayout.Height(100), GUILayout.MaxWidth(25));


        #endregion


        #region 说明部分

        //[BuilderEditor("使用说明", ContentType.Button, ",CB:DoHelper")]
        //private int helper;
        //private ContentLayout _helperLayout = new ContentLayout(GUILayout.Width(100));


        [SerializeField]
        [BuilderEditor("打包模式切换", ContentType.Enum)]
        internal BuilderDisplayType disPlayType = BuilderDisplayType.普通模式;
        private ContentLayout _disPlayTypeLayout = new ContentLayout();



        #endregion

        #region 标题

        [BuilderEditor("普通模式", ContentType.Label, "FlowA:disPlayType:0")]
        internal string titleEasy;
        private ContentLayout _titleEasyLayout = new ContentLayout(null);


        [BuilderEditor("专业模式", ContentType.Label, "FlowA:disPlayType:1")]
        internal string titleDev;
        private ContentLayout _titleDevLayout = new ContentLayout(null);


        [BuilderEditor("30", ContentType.Space)]
        internal char space1;

        [BuilderEditor("参数", ContentType.Label, "")]
        internal string titleArgs;
        private ContentLayout _titleArgsLayout = new ContentLayout(null);

        [SerializeField]
        [BuilderEditor("检测到未应用的操作！！请应用参数", ContentType.Label, "FlowA:hasSave:0&autoUdate:0")]
        internal bool hasSave = false;
        private ContentLayout _hasSaveLayout = new ContentLayout(null);


        [BuilderEditor("0", ContentType.Space, EditorContent.HorizontalStart)]
        internal char spaceautoUdate1;

        [SerializeField]
        [BuilderEditor("参数自动应用", ContentType.Toggle, "CB:SwitchApplyArgs")]
        internal bool autoUdate = false;

        [BuilderEditor("应用参数", ContentType.Button, "FlowA:autoUdate:0,CB:ApplyArgs")]
        internal string autoUdateBtn;

        [BuilderEditor("0", ContentType.Space, EditorContent.HorizontalEnd)]
        internal char spaceautoUdate2;

        #endregion

        #region 打包选项

        [SerializeField]
        [BuilderEditor("目标平台", ContentType.Enum, "CB:SwitchPlatform")]
        internal BuilderUtility.PlatformType platform = BuilderUtility.PlatformType.Android;
        private ContentLayout _platformLayout = new ContentLayout(null);

        [SerializeField]
        [BuilderEditor("打包模式", ContentType.Enum, "CB:SwitchBuildType")]
        internal BuilderUtility.BuildType buildType = BuilderUtility.BuildType.Release;

        [FormerlySerializedAs("_enableLogLevel")]
        [SerializeField]
        [BuilderEditor("Log输出等级（仅针对Release版本）", ContentType.Enum, "FlowA:disPlayType:1,CB:SwitchLogLevel")]
        internal BuilderUtility.EnableLogLevel enableLogLevel = BuilderUtility.EnableLogLevel.Warning;

        [FormerlySerializedAs("_scriptingBackend")]
        [SerializeField]
        [BuilderEditor("编译类型", ContentType.Enum, "FlowA:disPlayType:1&platform:0r1r2,CB:SwitchScriptingBackend")]
        internal BuilderUtility.ScriptBackend scriptingBackend = BuilderUtility.ScriptBackend.Mono;


        #endregion

        #region 额外开关


        [FormerlySerializedAs("_bCleanAssetBundleCache")]
        [SerializeField]
        [BuilderEditor("增量构建AssetBundle", ContentType.Toggle, "FlowA:buildType:0r2&disPlayType:1")]
        internal bool bIncrementBuildAB = false;

        [FormerlySerializedAs("_bEnableProfiler")]
        [SerializeField]
        [BuilderEditor("Enable Profiler", ContentType.Toggle, "FlowA:buildType:2&disPlayType:1,CB:EnableProfiler")]
        internal bool bEnableProfiler = false;

#if UNITY_2019_1_OR_NEWER
        [FormerlySerializedAs("_bEnableDeepProfiler")]
        [SerializeField] [BuilderEditor("Enable DeepProfiling Support", ContentType.Toggle, "FlowA:buildType:2&disPlayType:1,CB:EnableDeepProfiler")]
        internal bool bEnableDeepProfiler = false;
#endif

        [FormerlySerializedAs("_bExportAndroidProject")]
        [SerializeField]
        [BuilderEditor("导出Android工程", ContentType.Toggle, "FlowA:buildType:1r2&disPlayType:1,CB:EnableExportAndroidProject")]
        internal bool bExportAndroidProject = false;

        [FormerlySerializedAs("_bCollectShaderVariant")]
        [SerializeField]
        [BuilderEditor("单独打包Shader", ContentType.Toggle, "FlowA:disPlayType:1")]
        internal bool bCollectShaderVariant = false;

        [SerializeField]
        [BuilderEditor("启动GM", ContentType.Toggle, "FlowA:disPlayType:1,CB:EnableGM")]
        private bool bEnableGM = false;

        //[SerializeField]
        //[BuilderEditor("剔除首包解压", ContentType.Toggle, "FlowA:disPlayType:1,CB:EnableIgnoreFirstZip")]
        //internal bool bIgnoreFirstZip = false;

        //[FormerlySerializedAs("_bIgnorHotFix")]
        //[SerializeField]
        //[BuilderEditor("忽略热更", ContentType.Toggle, "FlowA:disPlayType:1,CB:IgnoreHotFix")]
        //private bool bIgnoreHotFix = false;
        //private ContentLayout _bIgnoreHotFixLayout = new ContentLayout();

        //[FormerlySerializedAs("_bUseDlc")]
        //[SerializeField]
        //[BuilderEditor("启用大小包模式", ContentType.Toggle, "FlowA:disPlayType:1,CB:UseDlc")]
        //internal bool bUseDlc = false;
        //private ContentLayout _bUseDlcLayout = new ContentLayout();

        #endregion



        #region 加密部分
        [SerializeField]
        [BuilderEditor("开启加密", ContentType.Toggle, "FlowA:disPlayType:1")]
        internal bool bEnableEncrypt = true;

        //[SerializeField]
        //[BuilderEditor("   \u2022  Lua加密", ContentType.Toggle, "FlowA:bEnableEncrypt:1,FlowS:bEnableEncrypt:1")]
        //internal bool bEnableLuaEncrypt = true;


        [SerializeField]
        [BuilderEditor("   \u2022AB加密偏移(0:不加密):", ContentType.TextField, "FlowA:bEnableEncrypt:1,FlowS:bEnableEncrypt:1")]
        internal string abEncryptOffset = "0";
        private ContentLayout _abEncryptOffsetLayout = new ContentLayout(GUILayout.ExpandWidth(true));


        #endregion


        #region 打包参数
        [FormerlySerializedAs("_builderPolicy")]
        [SerializeField]
        [BuilderEditor("AssetBundle策略", ContentType.Enum, "")]
        internal BuilderUtility.BuilderBundlePolicy builderBundlePolicy = BuilderUtility.BuilderBundlePolicy.Directory;

        [FormerlySerializedAs("_assetBundleConfig")]
        [SerializeField]
        [BuilderEditor("AssetBundle配置", ContentType.Obj, ",FlowA:builderBundlePolicy:2")]
        internal BundlePolicyConfig bundleConfig;

        [FormerlySerializedAs("_bundleIdentifier")]
        [SerializeField]
        [BuilderEditor("包名", ContentType.TextField, "FlowA:disPlayType:1,CB:ChangeBundleIdentifier")]
        internal string bundleIdentifier = "com.TEngine.TEngineDemo";

        [FormerlySerializedAs("_productName")]
        [SerializeField]
        [BuilderEditor("产品名", ContentType.TextField, "FlowA:disPlayType:1,CB:ChangeProductName")]
        internal string productName = "TEngine";

        [FormerlySerializedAs("_bundleVersion")]
        [SerializeField]
        [BuilderEditor("APP版本号", ContentType.TextField, "FlowA:disPlayType:1,CB:ChangeBundleVersion")]
        internal string bundleVersion = "1.0";

        [FormerlySerializedAs("_ABVersion")]
        [SerializeField]
        [BuilderEditor("资源版本号", ContentType.TextField, "FlowA:disPlayType:1,CB:ChangeABVersion")]
        internal string ABVersion = "0";

        [BuilderEditor("15", ContentType.Space)]
        private char space12;

        #endregion

        #region 打包按钮
        [BuilderEditor("Copy And Encrpt DLL", ContentType.Button, ",CB:CopyDLL,FlowA:disPlayType:1")]
        private int copydll;

        [BuilderEditor("Build AssetBundle", ContentType.Button, ",CB:BuildAssetBundle,FlowA:disPlayType:1")]
        private int buildAssetBundle;

        [BuilderEditor("Gen Md5(生成MD5)", ContentType.Button, ",CB:GenMd5,FlowA:disPlayType:1")]
        private int genMd5;

        [BuilderEditor("Build", ContentType.Button, "CB:BuildApk")]
        private int build;

        [BuilderEditor("直接出包（跳过ab环节）", ContentType.Button, "CB:DirectBuildApk,FlowA:disPlayType:1")]
        private int directBuild;

        #endregion

        /// <summary>
        /// 工程导出目录
        /// </summary>
        [SerializeField] internal string ProjectExportPath;
        [SerializeField] internal string _bBaseVersion = "0";
        [SerializeField] internal string _bResVersion = "0";


        /// <summary>
        /// 用于额外补充风格\必要的初始化
        /// </summary>
        public override void AdditionalInit()
        {
            SwitchSymbolWinLayout("Const");


            _symbolWinLayout.SetStyles(EditorStyles.helpBox);
            _symbolWinLayout.EditorStyles.fontSize = 12;

            _titleEasyLayout.EditorStyles = new GUIStyle() { fontSize = 50 };
            _titleEasyLayout.EditorStyles.normal.textColor = Color.white;
            _titleDevLayout.EditorStyles = new GUIStyle() { fontSize = 50 };
            _titleDevLayout.EditorStyles.normal.textColor = Color.white;

            //参数Label
            _titleArgsLayout.EditorStyles = new GUIStyle("boldLabel");
            _titleArgsLayout.EditorStyles.fontSize = 12;
            //未保存的警告
            _hasSaveLayout.EditorStyles = new GUIStyle("boldLabel");
            _hasSaveLayout.EditorStyles.normal.textColor = Color.red;
            _hasSaveLayout.EditorStyles.fontSize = 12;

            //目标平台
            _platformLayout.EditorStyles = new GUIStyle("MiniPopup");
            _platformLayout.EditorStyles.normal.textColor = new Color(30 / 255f, 144 / 255f, 255 / 255f);

            //_bIgnoreHotFixLayout.EditorStyles = EditorStyles.radioButton;
            //_bUseDlcLayout.EditorStyles = EditorStyles.radioButton;

        }

        public override void OnAnyThingChange(string args)
        {
            hasSave = false;
        }

        /// <summary>
        /// 使用说明
        /// </summary>
        /// <param name="args"></param>
        private void DoHelper(string args)
        {
            //打开本地MD
            var scriptObj = MonoScript.FromScriptableObject(this);
            var path = AssetDatabase.GetAssetPath(scriptObj).Replace("\\", "/");
            path = path.Substring(0, path.LastIndexOf("/", StringComparison.Ordinal)) + "/BuilderUserHelper.md";

            EditorUtility.OpenWithDefaultApp(path);
        }

        /// <summary>
        /// 切换平台
        /// </summary>
        /// <param name="args"></param>
        internal void SwitchPlatform(string args)
        {
            if (BuilderUtility.PlatformType.TryParse<BuilderUtility.PlatformType>(args, out var plat))
            {
                switch (plat)
                {
                    case BuilderUtility.PlatformType.Windows:
                        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone,
                            BuildTarget.StandaloneWindows);
                        break;
                    case BuilderUtility.PlatformType.OSX:
                        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone,
                            BuildTarget.StandaloneOSX);
                        break;
                    case BuilderUtility.PlatformType.Android:
                        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
                        break;
                    case BuilderUtility.PlatformType.iOS:
                        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, BuildTarget.iOS);
                        break;
                }
            }

            LoadMacroConfigures();
        }


        /// <summary>
        /// 切换日志等级
        /// </summary>
        /// <param name="args"></param>
        private void SwitchLogLevel(string args)
        {
            if (!autoUdate)
                return;
            LoadMacroConfigures();
            if (BuilderUtility.EnableLogLevel.TryParse<BuilderUtility.EnableLogLevel>(args, out var plat))
            {
                BuilderUtility.RefreshLogLevelMacro(ref _macroDefines, plat);
            }
        }


        private void SwitchScriptingBackend(string args)
        {
            if (!autoUdate)
                return;
            LoadMacroConfigures();
            if (BuilderUtility.ScriptBackend.TryParse<BuilderUtility.ScriptBackend>(args, out var scriptBackend))
            {
                BuilderUtility.RefreshBackendMacro(ref _macroDefines, scriptBackend);
            }
        }

        /// <summary>
        /// 更改打包方式：Editor、Android、IOS
        /// </summary>
        /// <param name="args"></param>
        internal void SwitchBuildType(string args)
        {
            if (!autoUdate)
                return;
            LoadMacroConfigures();
            if (BuilderUtility.BuildType.TryParse<BuilderUtility.BuildType>(args, out var plat))
            {
                BuilderUtility.SetBuildTypeMacroDefines(ref _macroDefines, plat);
            }
        }

        private void EnableProfiler(string args)
        {
            if (!autoUdate)
                return;
            bool enable = args.Equals("1");
            EditorUserBuildSettings.connectProfiler = enable;
        }
        private void EnableDeepProfiler(string args)
        {
            if (!autoUdate)
                return;
            bool enable = args.Equals("1");

#if UNITY_2019_1_OR_NEWER
            EditorUserBuildSettings.buildWithDeepProfilingSupport = enable;
#endif
        }

        private void EnableExportAndroidProject(string args)
        {
            if (!autoUdate)
                return;
            bool enable = args.Equals("1");
            EditorUserBuildSettings.exportAsGoogleAndroidProject = enable;
        }

        private void EnableGM(string args)
        {
            if (!autoUdate)
                return;
            LoadMacroConfigures();
            BuilderUtility.EnableGMSymbols(ref _macroDefines, args.Equals("1"));
        }

        /// <summary>
        /// 忽略首包解压
        /// </summary>
        /// <param name="args"></param>
        private void EnableIgnoreFirstZip(string args)
        {
            if (!autoUdate)
                return;
            LoadMacroConfigures();
            BuilderUtility.IgnorFirstZip(ref _macroDefines, args.Equals("1"));
        }


        /// <summary>
        /// 策略快查
        /// </summary>
        /// <param name="args"></param>
        private void DoCheckPolicy(string args)
        {
            BuilderUtility.PolicyEasyCheck(builderBundlePolicy, bundleConfig);
        }


        /// <summary>
        /// 更改包名
        /// </summary>
        /// <param name="args"></param>
        private void ChangeBundleIdentifier(string args)
        {
            if (!autoUdate)
                return;
            PlayerSettings.SetApplicationIdentifier(EditorUserBuildSettings.selectedBuildTargetGroup, args);
        }

        /// <summary>
        /// 更改产品名
        /// </summary>
        /// <param name="args"></param>
        private void ChangeProductName(string args)
        {
            if (!autoUdate)
                return;
            PlayerSettings.productName = args;
        }

        /// <summary>
        /// 更改版本号
        /// </summary>
        /// <param name="args"></param>
        private void ChangeBundleVersion(string args)
        {
            if (!autoUdate)
                return;
            PlayerSettings.bundleVersion = args;
        }

        /// <summary>
        /// 更改AB版本号
        /// </summary>
        /// <param name="args"></param>
        private void ChangeABVersion(string args)
        {
            if (!autoUdate)
                return;
            GameConfig.Instance.WriteResVersion(args);
        }

        /// <summary>
        /// 应用商店地址
        /// </summary>
        /// <param name="args"></param>
        private void ChangeAppUrl(string args)
        {
            BuilderUtility.SetAppURL(args);
        }

        private void SwitchApplyArgs(string args)
        {
            if (args.Equals("1"))
                ApplyArgs("");
        }

        /// <summary>
        /// 统一应用参数
        /// </summary>
        internal void ApplyArgs(string args)
        {
            LoadMacroConfigures();
            BuilderUtility.ClearMacros(ref _macroDefines, false);

            //分平台关闭开关
            switch (buildType)
            {
                case BuilderUtility.BuildType.Editor:
                    break;
                case BuilderUtility.BuildType.Release:
                    {
                        //关闭Profiler
                        bEnableProfiler = false;
#if UNITY_2019_1_OR_NEWER
                    bEnableDeepProfiler = false;
#endif
                        //禁止增量打包
                        bIncrementBuildAB = false;
                    }
                    break;
                case BuilderUtility.BuildType.Development:
                    break;
            }
            if (!bExportAndroidProject)
            {
                EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
            }

            //加密部分
            if (!bEnableEncrypt)
            {
                //bEnableLuaEncrypt = false;
            }

            //宏部分
            BuilderUtility.RefreshLogLevelMacro(ref _macroDefines, enableLogLevel, false);
            BuilderUtility.RefreshBackendMacro(ref _macroDefines, scriptingBackend, false);
            BuilderUtility.EnableGMSymbols(ref _macroDefines, bEnableGM, false);
            //必须最后一个调用
            BuilderUtility.SetMacroDefines(ref _macroDefines, buildType, false);

            //Setting部分
            EditorUserBuildSettings.connectProfiler = bEnableProfiler;
            EditorUserBuildSettings.exportAsGoogleAndroidProject = bExportAndroidProject;
#if UNITY_2019_1_OR_NEWER
            EditorUserBuildSettings.buildWithDeepProfilingSupport = bEnableDeepProfiler;
#endif
            //参数部分
            PlayerSettings.SetApplicationIdentifier(EditorUserBuildSettings.selectedBuildTargetGroup, bundleIdentifier);
            PlayerSettings.productName = productName;
            PlayerSettings.bundleVersion = bundleVersion;

            BuilderUtility.UpdateMacros(_macroDefines);

            hasSave = true;

            Save();
            AssetDatabase.Refresh();
        }

        private void GenMd5(string args)
        {
            if (EditorApplication.isCompiling)
            {
                EditorUtility.DisplayDialog("Build AssetBundle", "请等待编译完成", "ok");
                return;
            }
            TEngineEditorUtil.GenMd5List();
            GUIUtility.ExitGUI();
            AssetDatabase.Refresh();
        }

        private void BuildAssetBundle(string args)
        {

            if (EditorApplication.isCompiling)
            {
                EditorUtility.DisplayDialog("Build AssetBundle", "请等待编译完成", "ok");
                return;
            }

            ApplyArgs("");

            if (!BuilderUtility.PolicyEasyCheck(builderBundlePolicy, bundleConfig))
            {
                if (!EditorUtility.DisplayDialog("资源检查警告", "发现策略未覆盖到的资源，是否继续", "继续", "退出打包"))
                {
                    return;
                }
            }

            Builder.Instance.SetBuilderConfig(this);

            Builder.Instance.BuildAssetBundle();
            GUIUtility.ExitGUI();

        }

        public static void CopyFilefolder(string sourceFilePath, string targetFilePath)
        {
            string[] files = Directory.GetFiles(sourceFilePath);
            string fileName;
            string destFile;
            if (!Directory.Exists(targetFilePath))
            {
                Directory.CreateDirectory(targetFilePath);
            }
            foreach (string s in files)
            {
                fileName = Path.GetFileName(s);
                destFile = Path.Combine(targetFilePath, fileName);
                File.Copy(s, destFile, true);
            }
   
            string[] filefolders = Directory.GetFiles(sourceFilePath);

            DirectoryInfo dirinfo = new DirectoryInfo(sourceFilePath);

            DirectoryInfo[] subFileFolder = dirinfo.GetDirectories();
            for (int j = 0; j < subFileFolder.Length; j++)
            {
                string subSourcePath = sourceFilePath + "\\" + subFileFolder[j].ToString();
                string subTargetPath = targetFilePath + "\\" + subFileFolder[j].ToString();
                CopyFilefolder(subSourcePath, subTargetPath);
            }
        }

        public bool FileRename(string sourceFile, string destinationPath, string destinationFileName)
        {
            FileInfo tempFileInfo;
            FileInfo tempBakFileInfo;
            DirectoryInfo tempDirectoryInfo;

            tempFileInfo = new FileInfo(sourceFile);
            tempDirectoryInfo = new DirectoryInfo(destinationPath);
            tempBakFileInfo = new FileInfo(destinationPath + "\\" + destinationFileName);
            try
            {
                if (!tempDirectoryInfo.Exists)
                {
                    tempDirectoryInfo.Create();
                }
                if (tempBakFileInfo.Exists)
                {
                    tempBakFileInfo.Delete();
                }
                tempFileInfo.MoveTo(destinationPath + "\\" + destinationFileName);

                return true;
            }
            catch (Exception ex)
            {
                TLogger.LogError(ex.Message);
                return false;
            }
        }


        private void CopyDLL(string args)
        {
            if (EditorApplication.isCompiling)
            {
                EditorUtility.DisplayDialog("Build AssetBundle", "请等待编译完成", "ok");
                return;
            }

            ApplyArgs("");

            if (File.Exists(Application.dataPath + $"\\TResources\\DLL\\{Constant.Setting.HotFixedDllName}.dll.bytes"))
            {
                TLogger.LogWarning("存在DLL的bytes 执行删除操作");
                FileInfo del = new FileInfo(Application.dataPath + $"\\TResources\\DLL\\{Constant.Setting.HotFixedDllName}.dll.bytes");
                del.Delete();
            }
            else
            {
                TLogger.LogInfo("不存在DLL的bytes 直接拷贝加密");
            }

            FileInfo fi = new FileInfo(Application.dataPath + $"/DLL/{Constant.Setting.HotFixedDllName}.dll");

            fi.CopyTo(Path.Combine(Path.GetDirectoryName(Application.dataPath + "\\TResources\\DLL\\"), $"{Constant.Setting.HotFixedDllName}.dll.bytes"));

            TLogger.LogInfoSuccessd("拷贝加密DLL的bytes成功");

            AssetDatabase.Refresh();

            GUIUtility.ExitGUI();
        }

        private void BuildApk(string args)
        {
            if (EditorApplication.isCompiling)
            {
                EditorUtility.DisplayDialog("Build Apk", "请等待编译完成", "ok");
                return;
            }
            Save();

            if (bundleVersion.Split('.').Length != 2)
            {
                Debug.LogError("版本号需要两位（*.*）");
                return;
            }

            if (buildType == (BuilderUtility.BuildType)BuilderUtility.BuildType.Editor)
            {
                Debug.LogError("编辑器模式不支持打包，请看描述");
            }
            else
            {

                if (!BuilderUtility.PolicyEasyCheck(builderBundlePolicy, bundleConfig))
                {
                    if (!EditorUtility.DisplayDialog("资源检查警告", "发现策略未覆盖到的资源，是否继续", "继续", "退出打包"))
                    {
                        return;
                    }
                }

                ApplyArgs("");
                Builder.Instance.SetBuilderConfig(this);
                Builder.Instance.Build(false);

            }

            GUIUtility.ExitGUI();
        }

        /// <summary>
        /// 直接打APK包
        /// </summary>
        /// <param name="args"></param>
        private void DirectBuildApk(string args)
        {
            if (EditorApplication.isCompiling)
            {
                EditorUtility.DisplayDialog("Direct Build Apk", "请等待编译完成", "ok");
                return;
            }

            Save();

            if (bundleVersion.Split('.').Length != 2)
            {
                Debug.LogError("版本号需要两位（*.*）");
                return;
            }

            if (buildType == (BuilderUtility.BuildType)BuilderUtility.BuildType.Editor)
            {
                Debug.LogError("编辑器模式不支持打包，请看描述");
            }
            else
            {
                if (!Directory.Exists(FileSystem.AssetBundleBuildPath) ||
                    Directory.GetFileSystemEntries(FileSystem.AssetBundleBuildPath).Length <= 0)
                {
                    Debug.LogWarning("未打包assetbundle资源");
                }

                ApplyArgs("");
                Builder.Instance.SetBuilderConfig(this);
                Builder.Instance.Build(true);
            }

            GUIUtility.ExitGUI();
        }


        internal void SwitchSymbolWinLayout(string args)
        {
            if (!args.Equals("Const"))
            {
                if (_switchSymbolWinL == 0)
                {
                    _switchSymbolWinL = 1;
                }
                else
                {
                    _switchSymbolWinL = 0;
                }
            }

            if (_switchSymbolWinL == 1)
                symbolWin = _macroDefines.Replace(";", "------");
            else
                symbolWin = _macroDefines.Replace(";", "\n");
        }

        /// <summary>
        /// 保存数据
        /// </summary>
        internal void Save()
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }

        [MenuItem("TEngine/A B 配置|Create Asset Bundle Config", priority = 1500)]
        internal static void OpenBuilder()
        {
            bool isExists = false;
            string[] assetGuids = AssetDatabase.FindAssets("t:BuilderEditor");
            for (int i = 0, len = assetGuids.Length; i < len; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(assetGuids[i]);
                if (assetPath.EndsWith(".asset"))
                {
                    Type type = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
                    if (type == typeof(BuilderEditor) && !assetPath.StartsWith("Packages/"))
                    {
                        isExists = true;
                        string directory = assetPath.Substring(0, assetPath.LastIndexOf('/'));
                        EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(directory));
                        break;
                    }
                }
            }

            if (!isExists)
            {
                if (!EditorUtility.DisplayDialog("创建打包配置定位失败", "是否自动创建，默认为Release", "继续", "退出"))
                    return;

                string androidDir = "Assets/BuildConfig/Editor" + "/Android";
                if (!Directory.Exists(androidDir))
                    Directory.CreateDirectory(androidDir);

                string iosDir = "Assets/BuildConfig/Editor" + "/iOS";
                if (!Directory.Exists(iosDir))
                    Directory.CreateDirectory(iosDir);

                AssetDatabase.Refresh();

                string path = androidDir + "/Android_Release.asset";
                int index = 1;
                while (File.Exists(path))
                {
                    path = androidDir + $"/Android_Release_{index++}.asset";
                }

                BuilderEditor builder = CreateInstance<BuilderEditor>();
                AssetDatabase.CreateAsset(builder, path);
                EditorGUIUtility.PingObject(builder);
                AssetDatabase.OpenAsset(builder);
            }
        }
        internal void OnInit()
        {
            LoadMacroConfigures();
            if (autoUdate)
                MacroInit();
        }

        /// <summary>
        /// 宏初始化
        /// </summary>
        internal void MacroInit()
        {
            ApplyArgs("");
        }

        #region 宏同步

        private static string _macroDefines = "";

        internal static void LoadMacroConfigures()
        {
            switch (EditorUserBuildSettings.activeBuildTarget)
            {
                case BuildTarget.StandaloneWindows64:
                case BuildTarget.StandaloneWindows:
                    _macroDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone);
                    EditorUserBuildSettings.selectedBuildTargetGroup = BuildTargetGroup.Standalone;
                    break;
                case BuildTarget.StandaloneOSX:
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
        }

        #endregion

        internal enum BuilderDisplayType
        {
            普通模式,
            专业模式
        }
    }
}