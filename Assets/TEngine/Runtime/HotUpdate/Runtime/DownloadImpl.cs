using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TEngine
{
    /// <summary>
    /// 下载状态
    /// </summary>
    public enum BackgroundDownloadStatus
    {
        NotBegin = 0,       //未开始
        Downloading = 1,    //下载中
        NetworkError = 2,   //网络变化
        Done = 3,           //下载完成
        Failed = 4,         //下载失败
    }

    /// <summary>
    /// 资源加载过程中的状态
    /// </summary>
    public enum DownLoadResult
    {
        StartDownLoad = 0,      //开始下载
        HeadRequestFail = 1,    //请求头失败
        DownLoadRequestFail = 2,    //现在请求失败            
        AreadyDownLoaded = 3,       //已经下载过而且下载好了
        DownLoading = 4,        //下载中            
        NetChanged = 5,
        DownLoaded = 6,         //下载完成            
        DownLoadingError = 7,//接收数据的那个过程中出错            
        HeadRequestError = 8,//获取下载包大小报错
        ReceiveNullData = 9,//接受到空数据            
        DownError = 10,//数据没有接受完但是isDone为true            
        ReceiveError = 11,//接收数据失败
        Md5Wrong = 12,//md5错误
        AllDownLoaded = 13//全部下载完成
    }

    public class DownloadImpl
    {
        private List<LoadResource> _files;
        private string _path;
        private Action<int, List<LoadResource>> _callback = null;
        private long _totalFileSize = 0;
        private long _downLoadedSize = 0;
        private long _currentLoadSize = 0;

        private float _last_record_time = 0f;
        private float _last_record_process = 0f;
        private long _speed = 0;

        IDownload _downloader = null;

        public DownloadImpl(List<LoadResource> files, string path, Action<int, List<LoadResource>> callback)
        {
            _files = files;
            _path = path;
            _callback = callback;
            _downLoadedSize = 0;
            _totalFileSize = 0;
            foreach (var item in files)
            {
                _totalFileSize += item.Size;
            }

            var dirPath = Path.GetDirectoryName(_path);
            if (dirPath != null)
            {
                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }
            }
        }

        public IEnumerator DownLoad()
        {
            _downLoadedSize = 0;
            bool result = false;
            foreach (var item in _files)
            {
                string remoteUrl = item.RemoteUrl + "?" + GameConfig.Instance.GameBundleVersion;
                Callback(DownLoadResult.StartDownLoad);
                _downloader = GetDownloader(remoteUrl, _path + item.Url, item.Size, item.Md5, this);
                _downloader.StartDownload();
                yield return _downloader;

                if (_downloader != null)
                {
                    _downloader.Dispose();
                    result = _DealWithDownLoadOk(DownLoadResult.DownLoaded, _downloader.Status, item);
                    if (result == false)
                    {
                        yield break;
                    }

                    if (_downloader.Status == BackgroundDownloadStatus.Done)
                    {
                        _downLoadedSize += item.Size;
                    }
                }
                yield return null;
            }

            if (result)
            {
                _DownLoaded(_files);
            }
        }

        void _DownLoaded(List<LoadResource> list)
        {
            Callback(DownLoadResult.AllDownLoaded, list);
        }

        bool _DealWithDownLoadOk(DownLoadResult downloadType, BackgroundDownloadStatus status, LoadResource data)
        {
            string fileLocalPath = _path + data.Url;

            if (status == BackgroundDownloadStatus.NetworkError)
            {
                Callback(DownLoadResult.NetChanged);
                return false;
            }

            if (status == BackgroundDownloadStatus.Failed)
            {
                LoaderUtilities.DeleteFile(fileLocalPath);
                TLogger.LogError("DownloaderImpl._DownLoaded, Load failed");
                Callback(DownLoadResult.ReceiveError);
                return false;
            }

            int index = fileLocalPath.IndexOf("_md5_");
            var tempMd5 = LoaderUtilities.GetMd5Hash(fileLocalPath);
            if (index >= 0)
            {
                var fileInfo = new FileInfo(fileLocalPath);
                string newFilename = fileLocalPath.Substring(0, index);
                if (tempMd5 == data.Md5)
                {
                    if (File.Exists(newFilename))
                    {
                        File.Delete(newFilename);
                    }

                    fileInfo.MoveTo(newFilename);
                    Callback(downloadType);
                }
                else
                {
                    if (File.Exists(newFilename))
                    {
                        return true;
                    }
                    else
                    {
                        TLogger.LogError($"DownloaderImpl._DownLoaded, Current md5：{tempMd5},Target md5：{data.Md5} not match,path:{data.Url}");
                        LoaderUtilities.DeleteFile(fileLocalPath);
                        Callback(DownLoadResult.Md5Wrong);
                        return false;
                    }
                }
            }
            else
            {
                if (tempMd5 == data.Md5)
                {
                    Callback(downloadType);
                }
                else
                {
                    TLogger.LogError($"DownloaderImpl._DownLoaded, Current md5：{tempMd5},Target md5：{data.Md5} not match,path:{data.Url}");
                    LoaderUtilities.DeleteFile(fileLocalPath);
                    Callback(DownLoadResult.Md5Wrong);
                    return false;
                }
            }
            return true;
        }

        void Callback(DownLoadResult result, List<LoadResource> files = null)
        {
            _callback?.Invoke((int)result, files);
        }

        /// <summary>
        /// 文件总大小
        /// </summary>
        public long FileSize
        {
            get
            {
                return _totalFileSize;
            }
        }

        public long DownLoadSize
        {
            get => _downLoadedSize;
            set => _downLoadedSize = value;
        }

        public long CurrentLoadSize()
        {
            return _downLoadedSize + _downloader.CurrentSize;
        }

        /// <summary>
        /// 返回下载速度
        /// </summary>
        public long Speed
        {
            get
            {
                if (_downloader == null)
                    return 0;

                if (Time.time - _last_record_time < 0.5)
                {
                    return _speed;
                }

                float progress = _downloader.Progress;
                if (progress == _last_record_process)
                {
                    return _speed;
                }

                _speed = (long)((progress - _last_record_process) * _downloader.TotalSize / (Time.time - _last_record_time));
                _last_record_process = progress;
                _last_record_time = Time.time;
                return _speed;
            }
        }

        public static IDownload GetDownloader(string url, string path, long totalLength, string md5, DownloadImpl imp)
        {
            DownloadHandlerFileRange loader = new DownloadHandlerFileRange(url, path, totalLength, md5);
            loader.SetImp(imp);
            return loader;
        }

        public bool IsLoading()
        {
            if (_downloader == null)
                return false;
            return _downloader.Status == BackgroundDownloadStatus.Downloading;
        }

        public BackgroundDownloadStatus Statue()
        {
            if (_downloader == null)
                return BackgroundDownloadStatus.NotBegin;
            return _downloader.Status;
        }

        public bool IsNetWorkChanged()
        {
            if (_downloader == null)
                return false;
            return _downloader.Status == BackgroundDownloadStatus.NetworkError;
        }

        public void StopDownLoad()
        {
            if (_downloader != null)
            {
                _downloader.Dispose();
            }
        }

        public void Release()
        {
            _files = null;
            _path = "";
            _callback = null;
            _totalFileSize = 0;
            StopDownLoad();
        }
    }

    public interface IDownload : IEnumerator
    {
        float Progress { get; }
        long TotalSize { get; }
        long CurrentSize { get; }
        BackgroundDownloadStatus Status { get; }

        void Dispose();
        void StartDownload();
    }
}
