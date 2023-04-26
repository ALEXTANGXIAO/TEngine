using ICSharpCode.SharpZipLib.Zip;
using System.IO;
using ICSharpCode.SharpZipLib.Checksums;
using UnityEngine;

public class ZipWrapper : MonoBehaviour
{
    #region ZipCallback

    public abstract class ZipCallback
    {
        /// <summary>
        /// 压缩单个文件或文件夹前执行的回调。
        /// </summary>
        /// <param name="entry">压缩实例。</param>
        /// <returns>如果返回true，则压缩文件或文件夹，反之则不压缩文件或文件夹。</returns>
        public virtual bool OnPreZip(ZipEntry entry)
        {
            return true;
        }

        /// <summary>
        /// 压缩单个文件或文件夹后执行的回调。
        /// </summary>
        /// <param name="entry">压缩实例。</param>
        public virtual void OnPostZip(ZipEntry entry)
        {
        }

        /// <summary>
        /// 压缩执行完毕后的回调。
        /// </summary>
        /// <param name="result">true表示压缩成功，false表示压缩失败。</param>
        public virtual void OnFinished(bool result)
        {
        }
    }

    #endregion

    #region UnzipCallback

    public abstract class UnzipCallback
    {
        /// <summary>
        /// 解压单个文件或文件夹前执行的回调。
        /// </summary>
        /// <param name="entry">压缩实例。</param>
        /// <returns>如果返回true，则压缩文件或文件夹，反之则不压缩文件或文件夹。</returns>
        public virtual bool OnPreUnzip(ZipEntry entry)
        {
            return true;
        }

        /// <summary>
        /// 解压单个文件或文件夹后执行的回调。
        /// </summary>
        /// <param name="entry">压缩实例。</param>
        public virtual void OnPostUnzip(ZipEntry entry)
        {
        }

        /// <summary>
        /// 解压执行完毕后的回调。
        /// </summary>
        /// <param name="result">true表示解压成功，false表示解压失败。</param>
        public virtual void OnFinished(bool result)
        {
        }
    }

    #endregion

    /// <summary>
    /// 压缩文件和文件夹。
    /// </summary>
    /// <param name="fileOrDirectoryArray">文件夹路径和文件名。</param>
    /// <param name="outputPathName">压缩后的输出路径文件名。</param>
    /// <param name="password">压缩密码。</param>
    /// <param name="zipCallback">ZipCallback对象，负责回调。</param>
    /// <returns></returns>
    public static bool Zip(string[] fileOrDirectoryArray, string outputPathName, string password = null, ZipCallback zipCallback = null)
    {
        if ((null == fileOrDirectoryArray) || string.IsNullOrEmpty(outputPathName))
        {
            if (null != zipCallback)
                zipCallback.OnFinished(false);

            return false;
        }

        ZipOutputStream zipOutputStream = new ZipOutputStream(File.Create(outputPathName));
        // 压缩质量和压缩速度的平衡点
        zipOutputStream.SetLevel(6);
        if (!string.IsNullOrEmpty(password))
            zipOutputStream.Password = password;

        for (int index = 0; index < fileOrDirectoryArray.Length; ++index)
        {
            bool result = false;
            string fileOrDirectory = fileOrDirectoryArray[index];
            if (Directory.Exists(fileOrDirectory))
                result = ZipDirectory(fileOrDirectory, string.Empty, zipOutputStream, zipCallback);
            else if (File.Exists(fileOrDirectory))
                result = ZipFile(fileOrDirectory, string.Empty, zipOutputStream, zipCallback);

            if (!result)
            {
                if (null != zipCallback)
                    zipCallback.OnFinished(false);

                return false;
            }
        }

        zipOutputStream.Finish();
        zipOutputStream.Close();

        if (null != zipCallback)
            zipCallback.OnFinished(true);

        return true;
    }

    /// <summary>
    /// 解压Zip包。
    /// </summary>
    /// <param name="filePathName">Zip包的文件路径名。</param>
    /// <param name="outputPath">解压输出路径。</param>
    /// <param name="password">解压密码。</param>
    /// <param name="unzipCallback">UnzipCallback对象，负责回调。</param>
    /// <returns></returns>
    public static bool UnzipFile(string filePathName, string outputPath, string password = null, UnzipCallback unzipCallback = null)
    {
        if (string.IsNullOrEmpty(filePathName) || string.IsNullOrEmpty(outputPath))
        {
            if (null != unzipCallback)
                unzipCallback.OnFinished(false);

            return false;
        }

        try
        {
            return UnzipFile(File.OpenRead(filePathName), outputPath, password, unzipCallback);
        }
        catch (System.Exception exception)
        {
            Debug.LogError("[ZipUtility.UnzipFile]: " + exception);

            if (null != unzipCallback)
                unzipCallback.OnFinished(false);

            return false;
        }
    }

    /// <summary>
    /// 解压Zip包。
    /// </summary>
    /// <param name="fileBytes">Zip包字节数组。</param>
    /// <param name="outputPath">解压输出路径。</param>
    /// <param name="password">解压密码。</param>
    /// <param name="unzipCallback">UnzipCallback对象，负责回调。</param>
    /// <returns></returns>
    public static bool UnzipFile(byte[] fileBytes, string outputPath, string password = null, UnzipCallback unzipCallback = null)
    {
        if ((null == fileBytes) || string.IsNullOrEmpty(outputPath))
        {
            if (null != unzipCallback)
                unzipCallback.OnFinished(false);

            return false;
        }

        bool result = UnzipFile(new MemoryStream(fileBytes), outputPath, password, unzipCallback);
        if (!result)
        {
            if (null != unzipCallback)
                unzipCallback.OnFinished(false);
        }

        return result;
    }

    /// <summary>
    /// 解压Zip包
    /// </summary>
    /// <param name="inputStream">Zip包输入流。</param>
    /// <param name="outputPath">解压输出路径。</param>
    /// <param name="password">解压密码。</param>
    /// <param name="unzipCallback">UnzipCallback对象，负责回调。</param>
    /// <returns></returns>
    public static bool UnzipFile(Stream inputStream, string outputPath, string password = null, UnzipCallback unzipCallback = null)
    {
        if ((null == inputStream) || string.IsNullOrEmpty(outputPath))
        {
            if (null != unzipCallback)
                unzipCallback.OnFinished(false);

            return false;
        }

        // 创建文件目录
        if (!Directory.Exists(outputPath))
            Directory.CreateDirectory(outputPath);

        // 解压Zip包
        ZipEntry zipEntry = null;
        using (ZipInputStream zipInputStream = new ZipInputStream(inputStream))
        {
            if (!string.IsNullOrEmpty(password))
                zipInputStream.Password = password;

            while (null != (zipEntry = zipInputStream.GetNextEntry()))
            {
                if (string.IsNullOrEmpty(zipEntry.Name))
                    continue;

                if ((null != unzipCallback) && !unzipCallback.OnPreUnzip(zipEntry))
                    continue; // 过滤

                string filePathName = Path.Combine(outputPath, zipEntry.Name);

                // 创建文件目录
                if (zipEntry.IsDirectory)
                {
                    Directory.CreateDirectory(filePathName);
                    continue;
                }

                // 写入文件
                try
                {
                    using (FileStream fileStream = File.Create(filePathName))
                    {
                        byte[] bytes = new byte[1024];
                        while (true)
                        {
                            int count = zipInputStream.Read(bytes, 0, bytes.Length);
                            if (count > 0)
                                fileStream.Write(bytes, 0, count);
                            else
                            {
                                if (null != unzipCallback)
                                    unzipCallback.OnPostUnzip(zipEntry);

                                break;
                            }
                        }
                    }
                }
                catch (System.Exception exception)
                {
                    Debug.LogError($"[ZipUtility.UnzipFile]: {exception}");

                    if (null != unzipCallback)
                        unzipCallback.OnFinished(false);

                    return false;
                }
            }
        }

        if (null != unzipCallback)
        {
            unzipCallback.OnFinished(true);
        }

        return true;
    }

    /// <summary>
    /// 压缩文件。
    /// </summary>
    /// <param name="filePathName">文件路径名。</param>
    /// <param name="parentRelPath">要压缩的文件的父相对文件夹。</param>
    /// <param name="zipOutputStream">压缩输出流。</param>
    /// <param name="zipCallback">ZipCallback对象，负责回调。</param>
    /// <param name="useCrc">是否使用Crc。</param>
    /// <returns></returns>
    private static bool ZipFile(string filePathName, string parentRelPath, ZipOutputStream zipOutputStream, ZipCallback zipCallback = null, bool useCrc = false)
    {
        ZipEntry entry = null;
        FileStream fileStream = null;
        try
        {
            string entryName = $"{parentRelPath}/{Path.GetFileName(filePathName)}";
            entry = new ZipEntry(entryName);
            entry.DateTime = System.DateTime.Now;

            if ((null != zipCallback) && !zipCallback.OnPreZip(entry))
                return true; // 过滤

            fileStream = File.OpenRead(filePathName);
            byte[] buffer = new byte[fileStream.Length];
            var read = fileStream.Read(buffer, 0, buffer.Length);
            fileStream.Close();

            entry.Size = buffer.Length;

            if (useCrc)
            {
                Crc32 crc32 = new Crc32();
                crc32.Reset();
                crc32.Update(buffer);
                entry.Crc = crc32.Value;
            }

            zipOutputStream.PutNextEntry(entry);
            zipOutputStream.Write(buffer, 0, buffer.Length);
        }
        catch (System.Exception exception)
        {
            Debug.LogError($"[ZipUtility.ZipFile]: {exception}");
            return false;
        }
        finally
        {
            if (null != fileStream)
            {
                fileStream.Close();
                fileStream.Dispose();
            }
        }

        if (null != zipCallback)
            zipCallback.OnPostZip(entry);

        return true;
    }

    /// <summary>
    /// 压缩文件夹。
    /// </summary>
    /// <param name="path">要压缩的文件夹。</param>
    /// <param name="parentRelPath">要压缩的文件夹的父相对文件夹。</param>
    /// <param name="zipOutputStream">压缩输出流。</param>
    /// <param name="zipCallback">ZipCallback对象，负责回调。</param>
    /// <returns></returns>
    private static bool ZipDirectory(string path, string parentRelPath, ZipOutputStream zipOutputStream, ZipCallback zipCallback = null)
    {
        ZipEntry entry = null;
        try
        {
            string entryName = Path.Combine(parentRelPath, Path.GetFileName(path) + '/');
            entry = new ZipEntry(entryName)
            {
                DateTime = System.DateTime.Now,
                Size = 0
            };

            if ((null != zipCallback) && !zipCallback.OnPreZip(entry))
            {
                return true; // 过滤
            }

            zipOutputStream.PutNextEntry(entry);
            zipOutputStream.Flush();

            string[] files = Directory.GetFiles(path);
            foreach (var file in files)
            {
                // 排除Unity中可能的 .meta 文件
                if (file.EndsWith(".meta") == true)
                {
                    Debug.LogWarning(file + " not to zip");
                    continue;
                }

                ZipFile(file, Path.Combine(parentRelPath, Path.GetFileName(path)), zipOutputStream, zipCallback);
            }
        }
        catch (System.Exception exception)
        {
            Debug.LogError($"[ZipUtility.ZipDirectory]: {exception}");
            return false;
        }

        string[] directories = Directory.GetDirectories(path);
        foreach (var directory in directories)
        {
            if (!ZipDirectory(directory, Path.Combine(parentRelPath, Path.GetFileName(path)), zipOutputStream, zipCallback))
            {
                return false;
            }
        }

        if (null != zipCallback)
        {
            zipCallback.OnPostZip(entry);
        }

        return true;
    }
}