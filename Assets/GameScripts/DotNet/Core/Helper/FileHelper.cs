using System.Collections.Generic;
using System.IO;

namespace TEngine.Core
{
    public static class FileHelper
    {
        /// <summary>
        /// 拷贝文件到目标路径、如果目标目录不存在会自动创建目录
        /// </summary>
        /// <param name="sourceFile"></param>
        /// <param name="destinationFile"></param>
        /// <param name="overwrite"></param>
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
        /// 获取文件路径内的所有文件夹路径
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
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
        /// 把文件夹里所有内容拷贝的目标位置
        /// </summary>
        /// <param name="sourceDirectory"></param>
        /// <param name="destinationDirectory"></param>
        /// <param name="overwrite"></param>
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
        /// 清除文件夹里的所有文件
        /// </summary>
        /// <param name="folderPath"></param>
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