using System;
using System.Collections.Generic;
using System.IO;
using TEngineCore.Editor;
using UnityEditor;
using UnityEngine;

namespace TEngine.Editor
{
    public class TEngineEditorUtil
    {
        [MenuItem("TEngine/GenMd5List", priority = 1500)]
        public static void GenMd5List(string source, string target = "")
        {
            try
            {
                //string targetPath = Path.GetDirectoryName(target);
                //if (!Directory.Exists(targetPath))
                //{
                //    Directory.CreateDirectory(targetPath);
                //}
                var files = Directory.GetFiles(source, "*", SearchOption.AllDirectories);
                var fileList = new List<string>();
                var fileNames = new List<string>();
                var fileSizes = new List<long>();
                foreach (var file in files)
                {
                    if (file.EndsWith(".meta"))
                    {
                        continue;
                    }
                    fileNames.Add(file.Substring(source.Length + 1));
                    fileList.Add(file);
                    fileSizes.Add(file.Length);
                }

                GeneralMd5CheckList(source, files, fileList, fileNames,fileSizes);
                //FastZip.compress_File_List(9, target, fileList.ToArray(), null, false, fileNames.ToArray());
            }
            catch (Exception e)
            {
                TLogger.LogError(e.ToString());
                throw;
            }
        }


        /// <summary>
        /// 生成md5文件列表
        /// </summary>
        /// <param name="source">目录</param>
        /// <param name="files">文件列表</param>
        /// <param name="fileList">压缩的文件列表</param>
        /// <param name="fileNames">文件名字列表</param>
        /// /// <param name="fileSizes">文件大小列表</param>
        private static void GeneralMd5CheckList(string source, string[] files, List<string> fileList, List<string> fileNames, List<long> fileSizes)
        {
            try
            {
                var md5List = new List<fileMd5>();
                foreach (var file in files)
                {
                    if (file.EndsWith(".meta") || file.EndsWith(".DS_Store"))
                    {
                        continue;
                    }
                    var md5 = LoaderUtilities.GetMd5Hash(file);
                    var fd5 = new fileMd5
                    {
                        fileName = file.Substring(source.Length + 1).Replace('\\', '/'),
                        md5 = md5,
                        //fileSize = //todo
                        
                    };
                    md5List.Add(fd5);
                }

                var configPath = $"{source}/{FileSystem.Md5CheckList}";
                var stream = new FileStream(configPath, FileMode.OpenOrCreate);

                var writer = new StreamWriter(stream);
                writer.Write(JsonUtility.ToJson(new Serialization<fileMd5>(md5List)));
                writer.Flush();
                writer.Dispose();
                writer.Close();

                fileList.Add(configPath);
                fileNames.Add(FileSystem.Md5CheckList);
            }
            catch (Exception e)
            {
                TLogger.LogError(e.ToString());
                throw;
            }
        }
    }
}
