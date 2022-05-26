using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace TEngine.Editor
{
    public class TEngineEditorUtil
    {
        [MenuItem("TEngine/GenMd5List", priority = 1500)]
        public static void GenMd5List()
        {
            try
            {
                //string targetPath = Path.GetDirectoryName(target);
                //if (!Directory.Exists(targetPath))
                //{
                //    Directory.CreateDirectory(targetPath);
                //}
                string source = FileSystem.ResourceRootInStreamAsset;
                var files = Directory.GetFiles(source, "*", SearchOption.AllDirectories);
                var fileNames = new List<string>();
                var fileInfos = new Dictionary<string,long>();
                foreach (var file in files)
                {
                    if (file.EndsWith(".meta"))
                    {
                        continue;
                    }
                    fileNames.Add(file.Substring(source.Length + 1));
                    fileInfos.Add(file, GetFileSize(file));
                }

                GeneralMd5CheckList(source, files, fileInfos, fileNames);
                //FastZip.compress_File_List(9, target, fileList.ToArray(), null, false, fileNames.ToArray());
            }
            catch (Exception e)
            {
                TLogger.LogError(e.ToString());
                throw;
            }
        }

        /// <summary>
        /// 获取文件大小
        /// </summary>
        /// <param name="sFullName"></param>
        /// <returns></returns>
        public static long GetFileSize(string sFullName)
        {
            long lSize = 0;
            if (File.Exists(sFullName))
            {
                lSize = new FileInfo(sFullName).Length;
            }
            return lSize;
        }

        /// <summary>
        /// 生成md5文件列表
        /// </summary>
        /// <param name="source">目录</param>
        /// <param name="files">文件列表</param>
        /// <param name="fileList">压缩的文件列表</param>
        /// <param name="fileNames">文件名字列表</param>
        private static void GeneralMd5CheckList(string source, string[] files, Dictionary<string, long> fileInfos, List<string> fileNames)
        {
            try
            {
                var md5List = new List<fileMd5>();
                foreach (var fileInfo in fileInfos)
                {
                    var file = fileInfo.Key;

                    if (file.EndsWith(".meta") || file.EndsWith(".DS_Store"))
                    {
                        continue;
                    }
                    var md5 = GetMd5Hash(file);
                    var fd5 = new fileMd5
                    {
                        fileName = file.Substring(source.Length + 1).Replace('\\', '/'),
                        md5 = md5,
                        fileSize = fileInfo.Value,
                        
                    };
                    md5List.Add(fd5);
                }

                var configPath = $"{source}/{FileSystem.Md5List}";
                var stream = new FileStream(configPath, FileMode.OpenOrCreate);

                var writer = new StreamWriter(stream);
                writer.Write(JsonUtility.ToJson(new Serialization<fileMd5>(md5List)));
                writer.Flush();
                writer.Dispose();
                writer.Close();

                //fileList.Add(configPath);
                fileNames.Add(FileSystem.Md5List);
            }
            catch (Exception e)
            {
                TLogger.LogError(e.ToString());
                throw;
            }
            TLogger.LogInfoSuccessd("Gen Md5 List Success");
        }

        /// <summary>
        /// 获取文件的md5码
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetMd5Hash(string fileName)
        {
            if (!File.Exists(fileName))
            {
                TLogger.LogWarning($"not exit file,path:{fileName}");
                return string.Empty;
            }
            try
            {
                using (FileStream file = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    MD5 md5 = new MD5CryptoServiceProvider();
                    byte[] retVal = md5.ComputeHash(file);
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < retVal.Length; i++)
                    {
                        sb.Append(retVal[i].ToString("x2"));
                    }
                    return sb.ToString();
                }

            }
            catch (Exception ex)
            {
                TLogger.LogError("GetMD5Hash() fail,error:" + ex.Message);
                return string.Empty;
            }
        }
    }
}
