using System;
using System.Collections.Generic;
using System.IO;

namespace TEngine
{
    public static partial class Utility
    {
        /// <summary>
        /// Folder 相关的实用函数。
        /// </summary>
        public static partial class Folder
        {
            /// <summary>
            /// 清理文件夹。
            /// </summary>
            /// <param name="path">文件夹路径。</param>
            /// <returns>操作成功。</returns>
            public static bool ClearFolder(string path)
            {
                try
                {
                    var di = new DirectoryInfo(path);
                    if (!di.Exists)
                    {
                        return false;
                    }

                    foreach (var file in di.GetFiles())
                    {
                        file.Delete();
                    }

                    foreach (var dir in di.GetDirectories())
                    {
                        dir.Delete(true);
                    }

                    return true;
                }
                catch (Exception e)
                {
                    throw new GameFrameworkException($"ClearFolder invalid:{e.Message}");
                }
            }

            /// <summary>
            /// 拷贝文件到根目录。
            /// </summary>
            /// <param name="sourceRootPath">源文件目录。</param>
            /// <param name="destRootPath">目标文件目录。</param>
            /// <param name="searchOption">查找选项。</param>
            /// <returns>操作成功。</returns>
            public static bool CopyFilesToRootPath(string sourceRootPath, string destRootPath, SearchOption searchOption = SearchOption.AllDirectories)
            {
                string[] fileNames = Directory.GetFiles(sourceRootPath, "*", searchOption);
                foreach (string fileName in fileNames)
                {
                    string destFileName = System.IO.Path.Combine(destRootPath, fileName.Substring(sourceRootPath.Length));
                    FileInfo destFileInfo = new FileInfo(destFileName);
                    if (destFileInfo.Directory != null && !destFileInfo.Directory.Exists)
                    {
                        destFileInfo.Directory.Create();
                    }

                    System.IO.File.Copy(fileName, destFileName, true);
                }

                return true;
            }

            /// <summary>
            /// 拷贝文件夹。
            /// </summary>
            /// <param name="srcPath">需要被拷贝的文件夹路径。</param>
            /// <param name="tarPath">拷贝目标路径。</param>
            public static bool CopyFolder(string srcPath, string tarPath)
            {
                if (!Directory.Exists(srcPath))
                {
                    return false;
                }

                if (!Directory.Exists(tarPath))
                {
                    Directory.CreateDirectory(tarPath);
                }

                //获得源文件下所有文件
                List<string> files = new List<string>(Directory.GetFiles(srcPath));
                files.ForEach(f =>
                {
                    string destFile = System.IO.Path.Combine(tarPath, System.IO.Path.GetFileName(f));
                    System.IO.File.Copy(f, destFile, true); //覆盖模式
                });

                //获得源文件下所有目录文件
                List<string> folders = new List<string>(Directory.GetDirectories(srcPath));
                folders.ForEach(f =>
                {
                    string destDir = System.IO.Path.Combine(tarPath, System.IO.Path.GetFileName(f));
                    CopyFolder(f, destDir); //递归实现子文件夹拷贝
                });
                return true;
            }
            
            /// <summary>
            /// 拷贝文件。
            /// </summary>
            /// <param name="sourceRootPath">源文件目录。</param>
            /// <param name="destRootPath">目标文件目录。</param>
            /// <param name="searchOption">搜索选项。</param>
            /// <returns>操作成功。</returns>
            public static bool CopyFiles(string sourceRootPath, string destRootPath, SearchOption searchOption = SearchOption.AllDirectories)
            {
                string[] fileNames = Directory.GetFiles(sourceRootPath, "*", searchOption);
                foreach (string fileName in fileNames)
                {
                    FileInfo sourceFileInfo = new FileInfo(fileName);
                    string destFileName = System.IO.Path.Combine(destRootPath, sourceFileInfo.Name);
                    FileInfo destFileInfo = new FileInfo(destFileName);
                    if (destFileInfo.Directory != null && !destFileInfo.Directory.Exists)
                    {
                        destFileInfo.Directory.Create();
                    }

                    System.IO.File.Copy(fileName, destFileName, true);
                }

                return true;
            }
        }
    }
}