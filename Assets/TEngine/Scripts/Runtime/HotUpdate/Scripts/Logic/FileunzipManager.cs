using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

namespace TEngine.Runtime.HotUpdate
{
#pragma warning disable CS0162
#pragma warning disable CS0649
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
        private long _currentBuffSize;
        private bool _bussiness;
        private bool _unpackresult;

        public static FileunzipManager Instance => _instance ?? (_instance = new FileunzipManager());
        /// <summary>
        /// 设置版本回退
        /// </summary>
        /// <param name="count"></param>
        public void ResetVersionCount(int count)
        {
            
        }

        /// <summary>
        /// 拷贝dll到内部目录
        /// </summary>
        /// <param name="exit_dll"></param>
        public void CopyMonoAssembly(string version)
        {
            return;
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
