using System.Collections.Generic;
using System.IO;

namespace TEngine
{
    /// <summary>
    /// 文件操作助手类，提供了各种文件操作方法。
    /// </summary>
    public static class FileHelper
    {
        /// <summary>
        /// 获取相对路径的完整路径。
        /// </summary>
        /// <param name="relativePath">相对路径。</param>
        /// <returns>完整路径。</returns>
        public static string GetFullPath(string relativePath)
        {
            return Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), relativePath));
        }

        /// <summary>
        /// 将文件复制到目标路径，如果目标目录不存在会自动创建目录。
        /// </summary>
        /// <param name="sourceFile">源文件路径。</param>
        /// <param name="destinationFile">目标文件路径。</param>
        /// <param name="overwrite">是否覆盖已存在的目标文件。</param>
        public static void Copy(string sourceFile, string destinationFile, bool overwrite)
        {
            var directoriesByFilePath = GetDirectoriesByFilePath(destinationFile);

            foreach (var dir in directoriesByFilePath)
            {
                if (Directory.Exists(dir))
                {
                    continue;
                }

                Directory.CreateDirectory(dir);
            }

            File.Copy(sourceFile, destinationFile, overwrite);
        }

        /// <summary>
        /// 获取文件路径内的所有文件夹路径。
        /// </summary>
        /// <param name="filePath">文件路径。</param>
        /// <returns>文件夹路径列表。</returns>
        public static List<string> GetDirectoriesByFilePath(string filePath)
        {
            var dir = "";
            var directories = new List<string>();
            var fileDirectories = filePath.Split('/');

            for (var i = 0; i < fileDirectories.Length - 1; i++)
            {
                dir = $"{dir}{fileDirectories[i]}/";
                directories.Add(dir);
            }

            return directories;
        }

        /// <summary>
        /// 将文件夹内的所有内容复制到目标位置。
        /// </summary>
        /// <param name="sourceDirectory">源文件夹路径。</param>
        /// <param name="destinationDirectory">目标文件夹路径。</param>
        /// <param name="overwrite">是否覆盖已存在的文件。</param>
        public static void CopyDirectory(string sourceDirectory, string destinationDirectory, bool overwrite)
        {
            // 创建目标文件夹

            if (!Directory.Exists(destinationDirectory))
            {
                Directory.CreateDirectory(destinationDirectory);
            }

            // 获取当前文件夹中的所有文件

            var files = Directory.GetFiles(sourceDirectory);

            // 拷贝文件到目标文件夹

            foreach (var file in files)
            {
                var fileName = Path.GetFileName(file);
                var destinationPath = Path.Combine(destinationDirectory, fileName);
                File.Copy(file, destinationPath, overwrite);
            }

            // 获取源文件夹中的所有子文件夹

            var directories = Directory.GetDirectories(sourceDirectory);

            // 递归方式拷贝文件夹

            foreach (var directory in directories)
            {
                var directoryName = Path.GetFileName(directory);
                var destinationPath = Path.Combine(destinationDirectory, directoryName);
                CopyDirectory(directory, destinationPath, overwrite);
            }
        }

        /// <summary>
        /// 获取目录下的所有文件
        /// </summary>
        /// <param name="folderPath">文件夹路径。</param>
        /// <param name="searchPattern">需要查找的文件通配符</param>
        /// <param name="searchOption">查找的类型</param>
        /// <returns></returns>
        public static string[] GetDirectoryFile(string folderPath, string searchPattern, SearchOption searchOption)
        {
            return Directory.GetFiles(folderPath, searchPattern, searchOption);
        }

        /// <summary>
        /// 清空文件夹内的所有文件。
        /// </summary>
        /// <param name="folderPath">文件夹路径。</param>
        public static void ClearDirectoryFile(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                return;
            }
        
            var files = Directory.GetFiles(folderPath);
        
            foreach (var file in files)
            {
                File.Delete(file);
            }
        }
    }
}