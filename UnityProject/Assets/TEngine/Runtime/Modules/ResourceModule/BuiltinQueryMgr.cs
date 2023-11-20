using System.Collections.Generic;
using System.IO;
using UnityEngine;
using YooAsset;

namespace TEngine
{
    public class StreamingAssetsDefine
    {
        public const string RootFolderName = "yoo";
    }

#if UNITY_EDITOR
    /// <summary>
    /// 内置资源资源查询帮助类。
    /// </summary>
    public sealed class BuiltinQueryMgr
    {
        public static void Init()
        {
        }

        /// <summary>
        /// 内置游戏版本。
        /// </summary>
        public static string InternalGameVersion = string.Empty;

        public static bool FileExists(string packageName, string fileName)
        {
            string filePath = Path.Combine(Application.streamingAssetsPath, StreamingAssetsDefine.RootFolderName, packageName, fileName);
            return File.Exists(filePath);
        }
    }
#else
/// <summary>
/// 内置资源资源查询帮助类。
/// </summary>
public sealed class BuiltinQueryMgr
{
	private static bool _isInit = false;
	private static readonly HashSet<string> _cacheData = new HashSet<string>();
    private static string _internalGameVersion = string.Empty;
    /// <summary>
    /// 内置游戏版本。
    /// </summary>
    public static string InternalGameVersion
    {
        get
        {
            if (_isInit == false)
            {
                Init();
            }
            return _internalGameVersion;
        }
    }
    
	/// <summary>
	/// 初始化。
	/// </summary>
    private static void Init()
	{
		if (_isInit == false)
		{
			_isInit = true;
			var manifest = Resources.Load<BuiltinFileManifest>("BuiltinFileManifest");
			if (manifest != null)
			{
				foreach (string fileName in manifest.builtinFiles)
				{
					_cacheData.Add(fileName);
				}
                _internalGameVersion = manifest.internalGameVersion;
            }
		}
	}

	/// <summary>
	/// 内置文件查询方法。
	/// </summary>
	public static bool FileExists(string packageName, string fileName)
	{
		if (_isInit == false)
        {
            Init();
        }
		return _cacheData.Contains(fileName);
	}
}
#endif


#if UNITY_EDITOR
    internal class PreprocessBuild : UnityEditor.Build.IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        private const string BuiltinFilePath = "Assets/TEngine/AssetSetting/Resources/";

        private const string BuiltinFileName = "BuiltinFileManifest.asset";

        /// <summary>
        /// 在构建应用程序前处理
        /// </summary>
        public void OnPreprocessBuild(UnityEditor.Build.Reporting.BuildReport report)
        {
            GenBuiltinFileManifest();
        }

        [UnityEditor.MenuItem("YooAsset/GenBuiltinFileManifest", priority = 0)]
        public static void GenBuiltinFileManifest()
        {
            string saveFilePath = $"{BuiltinFilePath}{BuiltinFileName}";
            if (File.Exists(saveFilePath))
            {
                File.Delete(saveFilePath);
            }

            string folderPath = $"{Application.dataPath}/StreamingAssets/{StreamingAssetsDefine.RootFolderName}";
            DirectoryInfo root = new DirectoryInfo(folderPath);
            if (root.Exists == false)
            {
                Debug.Log($"没有发现YooAsset内置目录 : {folderPath}");
                return;
            }

            var manifest = ScriptableObject.CreateInstance<BuiltinFileManifest>();
            FileInfo[] files = root.GetFiles("*", SearchOption.AllDirectories);
            foreach (var fileInfo in files)
            {
                if (fileInfo.Extension == ".meta")
                {
                    continue;
                }

                if (fileInfo.Name.Equals("PackageManifest_DefaultPackage.version"))
                {
                    string internalGameVersion = File.ReadAllText($"{fileInfo.DirectoryName}/{fileInfo.Name}");
                    manifest.internalGameVersion = internalGameVersion;
                    Debug.Log($"内置版本号未: {manifest.internalGameVersion}");
                    continue;
                }

                if (fileInfo.Name.StartsWith("PackageManifest_"))
                {
                    continue;
                }

                manifest.builtinFiles.Add(fileInfo.Name);
            }

            if (Directory.Exists(BuiltinFilePath) == false)
            {
                Directory.CreateDirectory(BuiltinFilePath);
            }

            UnityEditor.AssetDatabase.CreateAsset(manifest, saveFilePath);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
            Debug.Log($"一共{manifest.builtinFiles.Count}个内置文件，内置资源清单保存成功 : {saveFilePath}");
        }
    }
#endif
}