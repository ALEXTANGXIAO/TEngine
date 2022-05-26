using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace TEngine
{
    public static class FileSystem
    {
        public const string ArtResourcePath = "Assets/ArtResources";
        public const string GameResourcePath = AssetConfig.AssetRootPath;
        internal static Dictionary<string, string> _fileFixList = new Dictionary<string, string>();
        private static string _persistentDataPath = null;
        private static string _resRootPath = null;
        private static string _resRootStreamAssetPath = null;
        public const string BuildPath = "Build";
        public const string AssetBundleBuildPath = BuildPath + "/AssetBundles";
        private const string AssetBundleTargetPath = "{0}/AssetBundles";
        public const string Md5List = "Md5List.json";
        /// <summary>
        /// 资源更新读取根目录
        /// </summary>
        /// <returns></returns>
        public static string ResourceRoot
        {
            get
            {
                if (string.IsNullOrEmpty(_resRootPath))
                {
                    _resRootPath = Path.Combine(PersistentDataPath, "TEngine");
                }

                if (!Directory.Exists(_resRootPath))
                {
                    Directory.CreateDirectory(_resRootPath);
                }

                return _resRootPath.FixPath();
            }
        }

        /// <summary>
        /// 持久化数据存储路径
        /// </summary>
        public static string PersistentDataPath
        {
            get
            {
                if (string.IsNullOrEmpty(_persistentDataPath))
                {
#if UNITY_EDITOR_WIN
                    _persistentDataPath = Application.dataPath + "/../TEnginePersistentDataPath";
                    if (!Directory.Exists(_persistentDataPath))
                    {
                        Directory.CreateDirectory(_persistentDataPath);
                    }
#else
                    _persistentDataPath = Application.persistentDataPath;
#endif
                }
                return _persistentDataPath.FixPath();
            }
        }

        /// <summary>
        /// 资源更新读取StreamAsset根目录
        /// </summary>
        /// <returns></returns>
        public static string ResourceRootInStreamAsset
        {
            get
            {
                if (string.IsNullOrEmpty(_resRootStreamAssetPath))
                {
                    _resRootStreamAssetPath = Path.Combine(Application.streamingAssetsPath, "TEngine");
                }
                return _resRootStreamAssetPath.FixPath();
            }
        }
        public static string GetAssetBundlePathInVersion(string bundlename)
        {
            //默认用外部目录
            string path = FilePath($"{ResourceRoot}/AssetBundles/{bundlename}");
            if (!File.Exists(path))
            {
                path = $"{ResourceRootInStreamAsset}/AssetBundles/{bundlename}";
            }

            return path;
        }

        public static string StreamAssetBundlePath
        {
            get { return string.Format(AssetBundleTargetPath, ResourceRootInStreamAsset); }
        }

        internal static string FilePath(string path)
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

        public static string FixPath(this string str)
        {
            str = str.Replace("\\", "/");
            return str;
        }

        public static Stream OpenRead(string filePath)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            byte[] bytes = ReadAllBytesFromOutOrInnerFolder(filePath);
            if (bytes != null)
                return new MemoryStream(bytes);
            else
                return null;
#else
            return File.OpenRead(filePath);
#endif
        }

        /// <summary>
        /// 从指定文件中读取字节串
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns></returns>
        public static byte[] ReadAllBytesFromOutOrInnerFolder(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return null;

#if UNITY_ANDROID && !UNITY_EDITOR
            //外部目录
            if (filePath.StartsWith(Application.persistentDataPath))
            {
                return ReadAllBytesFromOutFolder(filePath);
            }
            else //内部目录
            {
                return ReadAllBytesFromInnerFolder(filePath);
            }
#else
            return ReadAllBytesFromOutFolder(filePath);
#endif
        }

        private static byte[] ReadAllBytesFromOutFolder(string filePath)
        {
            if (!File.Exists(filePath))
            {
                TLogger.LogError("file:[{0}] is not exist!, please check!", filePath);
                return null;
            }
            return File.ReadAllBytes(filePath);
        }

        private static byte[] ReadAllBytesFromInnerFolder(string filePath)
        {
#if !UNITY_ANDROID || UNITY_EDITOR
            filePath = $"file://{filePath}";
#endif
            UnityWebRequest www = UnityWebRequest.Get(filePath);
            UnityWebRequestAsyncOperation request = www.SendWebRequest();
            while (!request.isDone) ;
            byte[] data = www.downloadHandler.data;
            www.downloadHandler.Dispose();
            www.Dispose();
            www = null;
            return data;
        }

        /// <summary>
        /// 读取内部目录
        /// </summary>
        /// <param name="filePath">文件完整路径</param>
        /// <returns></returns>
        private static string ReadTextFromInnerFolder(string filePath)
        {
#if !UNITY_ANDROID || UNITY_EDITOR
            filePath = $"file://{filePath}";
#endif
            UnityWebRequest www = UnityWebRequest.Get(filePath);
            UnityWebRequestAsyncOperation request = www.SendWebRequest();
            while (!request.isDone) ;

            return www.downloadHandler.text;
        }
    }

    [Serializable]
    public struct fileMd5
    {
        public string fileName;
        public string md5;
        public long fileSize;
    }

    [Serializable]
    public class Serialization<T>
    {
        [SerializeField]
        List<T> _target;

        public List<T> ToList()
        {
            return _target;
        }

        public Serialization(List<T> target)
        {
            this._target = target;
        }
    }
}