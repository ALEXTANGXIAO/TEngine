using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

namespace TEngine
{
    public class FileunzipManager
    {
        private static FileunzipManager _instance;
        #region FastUnZip
        //快速解压的方法
        private Thread _unpackThread;
        private List<LoadResource> _zipPath;
        private string _unzipPath;
        private readonly ulong[] _progress = { 0 };
        private int _fileIndex;
        private string _fileDir = "";
        private long _currentBuffSize;
        private int Max = 0;
        private bool _bussiness;
        private bool _unpackresult;

        public static FileunzipManager Instance => _instance ?? (_instance = new FileunzipManager());
        /// <summary>
        /// 设置版本回退
        /// </summary>
        /// <param name="count"></param>
        public void ResetVersionCount(int count)
        {
            Max = count;
        }

        /// <summary>
        /// 解压缩
        /// </summary>
        /// <param name="zippath">压缩包的地址</param>
        /// <param name="unzippath">解压的路径</param>
        public void StartFastUnZip(List<LoadResource> zippath, string dir, string unzippath)
        {
            _zipPath = zippath;
            _unzipPath = unzippath;
            _fileDir = dir;

            if (_zipPath == null || zippath.Count <= 0)
                return;

            if (string.IsNullOrEmpty(_unzipPath))
                return;

            if (_unpackThread != null)
            {
                Finish();
            }

            _bussiness = true;
            _progress[0] = 0;
            _fileIndex = 0;
            _currentBuffSize = 0;
            _unpackThread = new Thread(FastUnPack);
            _unpackThread.Start();
        }

        private void FastUnPack()
        {
            _unpackresult = true;
            var oldersVersion = GetOldestVersion(_fileDir);
            for (var i = 0; i < _zipPath.Count; i++)
            {
                if (_zipPath[i].Url == null)
                {
                    _fileIndex++;
                    continue;
                }

                var targetPath = VersionCheck(_zipPath[_fileIndex].Url, _fileDir, oldersVersion);
                int result = FastZip.decompress_File($"{_fileDir}/{_zipPath[_fileIndex].Url}", targetPath, null, null, _progress);
                if (result != 1)
                {
                    break;
                }

                if (!VerifyMd5(targetPath))
                {
                    break;
                }

                if (!BackUpCurrentVersionDll(targetPath))
                {
                    break;
                }

                _currentBuffSize += (long)_progress[0];
                _fileIndex++;
            }

            _unpackresult = _fileIndex >= _zipPath.Count;
            Finish();
        }

        /// <summary>
        /// 备份当前版本需要的dll
        /// </summary>
        private bool BackUpCurrentVersionDll(string path)
        {
#if UNITY_EDITOR
            return true;
#endif
#if ENABLE_MONO
            var sourcePath = $"{FileSystem.ResourceRoot}/{FileSystem.DllScriptPkgName}";
            if (!File.Exists(sourcePath))
            {
                TLogger.LogError($"不存在dll，path:{sourcePath}");
                return true;
            }

            var targetDll = $"{path}/{FileSystem.DllScriptPkgName}";
            if (sourcePath == targetDll)
            {
                //将母包的dll备份
                File.Copy(sourcePath, $"{FileSystem.ResourceRoot}/{FileSystem.DllScriptPkgBackUpName}", true);
                return true;
            }

            if (!File.Exists(targetDll))
            {
                if (File.Exists(sourcePath))
                {
                    File.Copy(sourcePath, targetDll, true);
                }
                else
                {
                    TLogger.LogError("FileunzipManager.BackUpCurrentVersionDll, File not exit,dllPath:" + sourcePath);
                    return false;
                }
            }
            else
            {
                File.Copy(targetDll, sourcePath, true);
            }

            return true;
#else
            TLogger.LogInfo("非mono版本不需要备份");
            return true;
#endif
        }

        /// <summary>
        /// 拷贝dll到内部目录
        /// 临时方法，后面转il2cpp编译就去掉了
        /// </summary>
        /// <param name="exit_dll"></param>
        public void CopyMonoAssembly(string version)
        {
#if UNITY_EDITOR
            return;
#endif
#if ENABLE_MONO
            var source_path = GameConfig.Instance.FilePath($"{FileSystem.ResourceRoot}/{FileSystem.DllScriptPkgName}");
            var des_path = $"{FileSystem.OldPersistentDataPath}/{FileSystem.DllScriptPkgName}";
            TLogger.LogInfo($"FileUnZip.CopyMonoAssembly,source_path:{source_path}");
            TLogger.LogInfo($"FileUnZip.CopyMonoAssembly,des_path:{des_path}");
            if (File.Exists(des_path))
            {
                File.SetAttributes(des_path, FileAttributes.Normal);
            }

            if (File.Exists(source_path))
            {
                File.SetAttributes(source_path, FileAttributes.Normal);
                File.Copy(source_path, des_path, true);
            }
#endif
        }

        public void BackDllInStream()
        {
#if UNITY_EDITOR
            return;
#endif
#if ENABLE_MONO
            //打dll备份出来,为了兼容dll更新，以防止以后又需要dll更新了
            var sourcePath = $"{FileSystem.ResourceRoot}/{FileSystem.DllScriptPkgName}";
            string dllInStream = $"{FileSystem.ResourceRootInStreamAsset}/{FileSystem.DllScriptPkgName}";
            TLogger.LogInfo($"Copy dll from {dllInStream} to {sourcePath}");
            var www = UnityWebRequest.Get(dllInStream);
            var dh = new DownloadHandlerFile(sourcePath) { removeFileOnAbort = true };
            www.downloadHandler = dh;
            www.SendWebRequest();
            while (!www.isDone)
            {
                if (www.isHttpError || www.isNetworkError)
                {
                    TLogger.LogInfo("FileUnzipManager.BackDllInStream,not found dll");
                    return;
                }
            }
            File.Copy(sourcePath, $"{FileSystem.ResourceRoot}/{FileSystem.DllScriptPkgBackUpName}", true);
            File.Copy(sourcePath, $"{FileSystem.OldPersistentDataPath}/{FileSystem.DllScriptPkgName}", true);
#endif
        }

        /// <summary>
        /// 版本检测，低版本的资源退回到根目录下
        /// </summary>
        /// <param name="version">版本号</param>
        /// <param name="path">解压缩目录</param>
        /// <param name="oldestVersion">最低的版本号</param>
        /// <returns></returns>
        internal string VersionCheck(string version, string path, string oldestVersion)
        {
            var folder = version.Split('_');
            if (folder.Length < 2) return path;

            var versionLongCurrent = long.Parse(folder[1].Replace(".", ""));
            var versionLongOld = long.Parse(oldestVersion.Replace(".", ""));

            if (versionLongOld >= versionLongCurrent)
            {
                DeleteUnUsedVersion(folder[1]);
                return path;
            }

            var newPath = $"{path}/{folder[1]}{GameConfig.SUFFIX}";
            LoaderUtilities.MakeAllDirectory(newPath);

            return newPath;
        }

        /// <summary>
        /// 获取最低的需要保留的版本号
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        internal string GetOldestVersion(string path)
        {
            if (path.IndexOf(FileSystem.FirstPackageName) >= 0)
                return "0";

            var dirs = GameConfig.Instance.GetExitVersions(path);
            foreach (var item in _zipPath)
            {
                if (string.IsNullOrEmpty(item.Url))
                {
                    continue;

                }

                var folder = item.Url.Split('_');
                if (folder.Length < 2)
                    continue;

                if (!dirs.Contains(folder[1]))
                {
                    dirs.Add(folder[1]);
                }
            }

            Sort(dirs);
            var version = "0";
            if (dirs.Count > Max)
            {
                int versionIndex = dirs.Count - Max - 1;
                version = dirs[versionIndex];
                DeleteUnUsedVersion(version);
            }

            return version;
        }

        private void Sort(List<string> dirs)
        {
            dirs.Sort((a, b) =>
            {
                var versionLongA = long.Parse(a.Replace(".", ""));
                var versionLongB = long.Parse(b.Replace(".", ""));
                return versionLongB < versionLongA ? 1 : -1;
            });
        }

        //删除不用了的版本
        private void DeleteUnUsedVersion(string path)
        {
            string folder = $"{_fileDir}/{path}{GameConfig.SUFFIX}";
            if (Directory.Exists(folder))
            {
                LoaderUtilities.CopyDirectory(folder, _fileDir);
                Directory.Delete(folder, true);
            }
        }

        /// <summary>
        /// 返回压缩包里面的文件的总长度，字节为单位
        /// </summary>
        /// <param name="path">压缩包的路径</param>
        /// <returns></returns>
        public UInt64 GetZipLength(string path)
        {
            return FastZip.getFileInfo(path);
        }

        /// <summary>
        /// 返回进度
        /// </summary>
        /// <returns></returns>
        public long GetProgress()
        {
            return _currentBuffSize + (long)_progress[0];
        }

        /// <summary>
        /// 线程是否在执行中
        /// </summary>
        /// <returns></returns>
        public bool IsRunning()
        {
            return _unpackThread != null && _bussiness;
        }
        /// <summary>
        /// 解压缩的结果
        /// </summary>
        /// <returns></returns>
        public bool UnPackResult()
        {
            return _unpackresult;
        }
        /// <summary>
        /// 解压缩状态
        /// </summary>
        /// <returns></returns>
        public bool IsBusiness()
        {
            return _bussiness;
        }

        /// <summary>
        /// 结束线程
        /// </summary>
        public void Finish()
        {
            _bussiness = false;
            if (_unpackThread == null) return;
            if (!_unpackThread.IsAlive)
            {
                _unpackThread = null;
            }
            else
            {
                try
                {
                    _unpackThread.Join(2000);
                }
                catch (Exception e)
                {
                    TLogger.LogError($"Fileunzipmanager.Finish,{e.StackTrace}");
                }
                _unpackThread = null;
            }
        }
        #endregion

        #region 文件Md5校验

        private static bool VerifyMd5(string path)
        {
            bool result = true;
            try
            {
                string configPath = path + "/" + FileSystem.Md5List;
                if (!File.Exists(configPath))
                {
                    TLogger.LogError("could not find config,path:" + configPath);
                    result = false;
                }
                else
                {
                    var fileList = File.ReadAllText(configPath);
                    var list = JsonUtility.FromJson<Serialization<fileMd5>>(fileList).ToList();

                    var logFilePath = $"{path}/{FileSystem.Md5VerifyLog}";
                    var fileStream = new FileStream(logFilePath, FileMode.OpenOrCreate);
                    var writer = new StreamWriter(fileStream);
                    foreach (var fileMd5 in list)
                    {
                        string log;
                        var tmpFile = Path.GetFullPath($"{path}/{fileMd5.fileName}");
                        if (!File.Exists(tmpFile))
                        {
                            log = $"file: {fileMd5.fileName} not exit";
                            writer.Write(log);
                            result = false;
                            continue;
                        }

                        string md5Raw = LoaderUtilities.GetMd5Hash(tmpFile);
                        if (!md5Raw.Equals(fileMd5.md5, StringComparison.Ordinal))
                        {
                            log = $"file: {fileMd5.fileName} \r\n Normal Md5: {md5Raw} \r\n Config Md5: {fileMd5.md5};Unpack Failed \r\n";
                            writer.Write(log);
                            result = false;
                        }
                        else
                        {
                            log = $"file: {fileMd5.fileName} \r\n Normal Md5: {md5Raw} \r\n Config Md5: {fileMd5.md5};Unpack ok \r\n";
                            writer.Write(log);
                        }
                    }

                    writer.Flush();
                    writer.Dispose();
                    writer.Close();
                }
            }
            catch (Exception e)
            {
                result = false;
                TLogger.LogError(e.ToString());
            }

            return result;
        }
        #endregion
    }
}
