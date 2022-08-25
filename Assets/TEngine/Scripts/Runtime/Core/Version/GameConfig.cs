using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace TEngine.Runtime
{
    [Serializable]
    public struct VersionConfig
    {
        /// <summary>
        /// APP版本号
        /// </summary>
        public string AppVersion;
        /// <summary>
        /// APP对应的资源版本号
        /// </summary>
        public string BaseResVersion;
        /// <summary>
        /// 资源版本号
        /// </summary>
        public string ResVersion;
    }

    public class GameConfig
    {
        public const string CONFIG = "version.json";
        public const string SUFFIX = "_Version";
        private VersionConfig _versionConfig;
        private readonly Dictionary<string, string> _fileFixList = new Dictionary<string, string>();

        private static GameConfig _instance;

        public static GameConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GameConfig();
                    _instance.InitAppVersionInfo();
                }

                return _instance;
            }
        }
        /// <summary>
        /// 初始化版本信息
        /// </summary>
        private bool InitAppVersionInfo()
        {
            string configContent = string.Empty;
            var externalPath = Path.Combine(FileSystem.ResourceRoot, CONFIG);
            if (File.Exists(externalPath))
            {
                configContent = File.ReadAllText(externalPath);
                TLogger.LogInfo($"GameConfig,{externalPath} exit,info:{configContent}");
            }
            else
            {
                configContent = GetInnerVersion();
                //内部目录的版本配置如果没读到，返回初始化配置失败
                if (configContent == string.Empty)
                {
#if !UNITY_EDITOR
#if RELEASE_BUILD || _DEVELOPMENT_BUILD_
                    TLogger.LogError($"version config not find in InnerPath,please check it");
#endif
                    return false;
#endif
                }
            }

            if (!string.IsNullOrEmpty(configContent))
            {
                try
                {
                    _instance._versionConfig = JsonUtility.FromJson<VersionConfig>(configContent);
                }
                catch
                {
                    TLogger.LogError($"VersionConfig Json failed,content{configContent}");
                    return false;
                }
                //设置一下版本
                SetLoadFilePath(FileSystem.ResourceRoot, ResId);
                return true;
            }
            else
            {
#if UNITY_EDITOR
                _instance._versionConfig = new VersionConfig
                {
                    AppVersion = Application.version,
                    BaseResVersion = "0",
                    ResVersion = "0",
                };
                UpdateConfig();
                SetLoadFilePath(FileSystem.ResourceRoot, ResId);
                return true;
#else
                TLogger.LogError("version vonfig miss,please check it");
                return false;
#endif
            }
        }

        /// <summary>
        /// 获取内部version.json配置
        /// </summary>
        /// <returns></returns>
        internal string GetInnerVersion()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            var innerPath = Path.Combine(FileSystem.ResourceRootInStreamAsset, CONFIG);
#else
            var innerPath = $"file://{Path.Combine(FileSystem.ResourceRootInStreamAsset, CONFIG)}";
#endif
            var www = UnityWebRequest.Get(innerPath);
            var request = www.SendWebRequest();
            while (!request.isDone)
            {
            }
            var configContent = www.downloadHandler.text;

            TLogger.LogInfo($"GameConfig,{innerPath} exit,info:{configContent}");
            return configContent;
        }

        /// <summary>
        /// 底包版本号
        /// </summary>
        public string AppId
        {
            get
            {
                return _versionConfig.AppVersion;
            }
        }

        /// <summary>
        /// 资源版本号
        /// </summary>
        public string ResId
        {
            get
            {
                return _versionConfig.ResVersion;
            }
            set
            {
                _versionConfig.ResVersion = value;
            }
        }

        /// <summary>
        /// 基础母包资源版本号
        /// </summary>
        /// <returns></returns>
        public string BaseResId()
        {
            return _versionConfig.BaseResVersion;
        }

        /// <summary>
        /// 游戏版本号，提供给外面展示使用
        /// </summary>
        public string GameBundleVersion
        {
            get
            {
                return $"{AppId}.{ResId}";
            }
        }

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="filePath">文件路径</param>
        public static void DeleteFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch (Exception e)
            {
                TLogger.LogError(e.ToString());
            }
        }

        /// <summary>
        /// 是否是首次解压
        /// </summary>
        /// <returns></returns>
        internal bool IsFirst()
        {
            TLogger.LogInfo("ConfigInfo:" + _versionConfig.AppVersion + "|" + _versionConfig.ResVersion);
            var innerVersion = GetInnerVersion();
            VersionConfig innerVersionConfig;

            if (innerVersion == string.Empty)
            {
                //配置都找不到，就直接返回解压流程了
                return true;
            }

            try
            {
                innerVersionConfig = JsonUtility.FromJson<VersionConfig>(innerVersion);
            }
            catch
            {
                //如果反序列化失败了，就默认返回给业务逻辑需要解压了
                TLogger.LogError($"GameConfig,JsonUtility.FromJson failed,{innerVersion}");
                return true;
            }
            //版本号如果不一致，就表示要解压了
            if (innerVersionConfig.AppVersion != _versionConfig.AppVersion)
            {
                return true;
            }
            //如果版本号一致，就看是否解压过
            var extrnalPath = Path.Combine(FileSystem.ResourceRoot, CONFIG);
            return !File.Exists(extrnalPath);
        }

        internal bool ResetVersionConfig()
        {
            return InitAppVersionInfo();
        }

        /// <summary>
        /// 写入App内部资源版本号
        /// </summary>
        /// <param name="resId"></param>
        public void WriteBaseResVersion(string resId)
        {
            if (string.IsNullOrEmpty(resId))
            {
                TLogger.LogWarning("ResVersion is null or empty,please check!");
                return;
            }

            _versionConfig.BaseResVersion = resId;
            UpdateConfig();
            SetLoadFilePath(FileSystem.ResourceRoot, ResId);
            TLogger.LogInfo("GameConfig,WriteVersion to sdk:" + GameConfig.Instance.GameBundleVersion);
        }


        /// <summary>
        /// 写入资源版本号
        /// </summary>
        /// <param name="resId"></param>
        public void WriteResVersion(string resId)
        {
            if (string.IsNullOrEmpty(resId))
            {
                TLogger.LogWarning("ResVersion is null or empty,please check!");
                return;
            }

            _versionConfig.ResVersion = resId;
            UpdateConfig();
            SetLoadFilePath(FileSystem.ResourceRoot, ResId);
            TLogger.LogInfo("GameConfig,WriteVersion to sdk:" + GameConfig.Instance.GameBundleVersion);
        }

        private void UpdateConfig()
        {
            try
            {
                string path = Path.Combine(FileSystem.ResourceRoot, CONFIG);
                if (!File.Exists(path))
                {
                    MakeAllDirectory(path);
                    FileStream file = File.Create(path);
                    file.Close();
                }
                File.WriteAllText(path, JsonUtility.ToJson(_versionConfig));
            }
            catch (Exception e)
            {
                TLogger.LogError(e.StackTrace);
            }
        }

        private static readonly char[] DirectorySeperators =
        {
            Path.DirectorySeparatorChar,
            Path.AltDirectorySeparatorChar,
        };

        /// <summary>
        /// 创建文件夹
        /// </summary>
        /// <param name="path">目标目录地址</param>
        /// <param name="is_last_file"></param>
        public static void MakeAllDirectory(string path, bool is_last_file = true)
        {
            try
            {
                if (is_last_file)
                {
                    path = Path.GetDirectoryName(path);
                }
                if (path == null)
                    return;

                var pathFragments = path.Split(DirectorySeperators);
                if (pathFragments.Length <= 0)
                    return;

                path = pathFragments[0];
                if (path != string.Empty && !Directory.Exists(path))
                    Directory.CreateDirectory(path);

                for (var i = 1; i < pathFragments.Length; i++)
                {
                    path += Path.DirectorySeparatorChar;

                    string pathFragment = pathFragments[i];
                    if (string.IsNullOrEmpty(pathFragment))
                        continue;

                    path += pathFragment;
                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);
                }
            }
            catch (Exception e)
            {
                TLogger.LogError(e.ToString());
                throw;
            }
        }


        /// <summary>
        /// 获取后缀为SUFFIX的目录
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        internal List<string> GetExitVersions(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            var dir = new DirectoryInfo(path);
            var fileinfo = dir.GetFileSystemInfos();
            var tmp = new List<string>();
            foreach (var file in fileinfo)
            {
                var subName = file.Name;
                if (!(file is DirectoryInfo)) continue;

                if (subName.IndexOf(SUFFIX) >= 0)
                {
                    tmp.Add(subName.Split('_')[0]);
                }
            }
            return tmp;
        }
        /// <summary>
        /// 检测本地是否存在了该版本
        /// </summary>
        /// <param name="resVersion"></param>
        /// <returns></returns>
        internal bool CheckLocalVersion(string resVersion)
        {
            var versionLocal = GetExitVersions(FileSystem.ResourceRoot);
            foreach (var item in versionLocal)
            {
                TLogger.LogInfo(item + "|" + resVersion);
                if (item == resVersion)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 设置最新的文件路径
        /// </summary>
        public void SetLoadFilePath(string path, string version, bool userDlc = false)
        {
            var dirs = GetExitVersions(path);

            dirs.Sort((a, b) => VersionToLong(b) < VersionToLong(a) ? 1 : -1);

            _fileFixList.Clear();
            foreach (var item in dirs)
            {
                if (VersionToLong(item) > VersionToLong(version))
                {
                    continue;
                }
                var subFolder = $"{item}{SUFFIX}/";
                var dirPath = $"{path}/{subFolder}";
                var dir = new DirectoryInfo(dirPath);
                var tempFile = dir.GetFileSystemInfos("*", SearchOption.AllDirectories);
                foreach (var file in tempFile)
                {
                    var subFilename = file.FullName.FixPath().Replace(subFolder, "");
                    subFilename = subFilename.Replace("Assets/../", "");
                    if (_fileFixList.ContainsKey(subFilename))
                    {
                        _fileFixList[subFilename] = file.FullName.FixPath();
                    }
                    else
                    {
                        _fileFixList.Add(subFilename, file.FullName.FixPath());
                    }
                }
            }
        }
        /// <summary>
        /// 版本号转换成long
        /// </summary>
        /// <param name="str">版本号</param>
        /// <returns></returns>
        private long VersionToLong(string str)
        {
            return long.Parse(str.Replace(".", ""));
        }
        /// <summary>
        /// 返回资源所在的最新目录
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        internal string FilePath(string path)
        {
            path = path.FixPath();
#if UNITY_EDITOR_WIN
            path = path.Replace("Assets/../", "");
#endif
            if (_fileFixList.ContainsKey(path))
            {
                return _fileFixList[path];
            }
            else
            {
                return path;
            }
        }
    }
}
