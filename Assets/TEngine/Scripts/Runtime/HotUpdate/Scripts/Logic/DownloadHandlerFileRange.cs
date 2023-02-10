using System.IO;
using UnityEngine.Networking;

namespace TEngine.Runtime.HotUpdate
{
    public class DownloadHandlerFileRange : DownloadHandlerScript, IDownload
    {
        private readonly string _url;
        private string _path;
        private string _md5;
        private FileStream _fileStream;
        private UnityWebRequest _unityWebRequest;

        private long _totalFileSize = 0;
        private long _curFileSize = 0;
        public bool HasError { get; private set; }
        protected BackgroundDownloadStatus _status = BackgroundDownloadStatus.NotBegin;

        private DownloadImpl _imp;

        public void SetImp(DownloadImpl imp)
        {
            _imp = imp;
        }

        public DownloadHandlerFileRange(string url, string path, long totalLength, string md5) : base(new byte[1024 * 1024])
        {
            TLogger.LogInfo($"DownloadHandlerFileRange url:{url},path:{path},totalLength:{totalLength}");

            _url = url;
            _path = path;
            _md5 = md5;

            _path = path + "_md5_" + md5;

            var dirPath = Path.GetDirectoryName(_path);
            if (dirPath != null)
            {
                TLogger.LogInfo($"DownloadHandlerFileRange dirPath：{dirPath}");
                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }
            }

            _totalFileSize = totalLength;
            _status = BackgroundDownloadStatus.NotBegin;
        }

        /// <summary>
        /// 兼容断点续传
        /// </summary>
        public void StartDownload()
        {
            if (_status != BackgroundDownloadStatus.NotBegin)
            {
                return;
            }

            var index = _path.IndexOf("_md5_");
            var fileMd5 = string.Empty;
            if (File.Exists(_path))
            {
                fileMd5 = LoaderUtilities.GetMd5Hash(_path);
            }
            else
            {
                if (index >= 0)
                {
                    fileMd5 = LoaderUtilities.GetMd5Hash(_path.Substring(0, index));
                }
            }

            if (fileMd5 == _md5)
            {
                _status = BackgroundDownloadStatus.Done;
                return;
            }

            try
            {
                _fileStream = new FileStream(_path, FileMode.OpenOrCreate, FileAccess.Write);
                var localFileSize = _fileStream.Length;
                _fileStream.Seek(localFileSize, SeekOrigin.Begin);
                _curFileSize = localFileSize;
                _unityWebRequest = UnityWebRequest.Get(_url);
                _unityWebRequest.SetRequestHeader("Range", "bytes=" + localFileSize + "-" + _totalFileSize);
                _unityWebRequest.downloadHandler = this;
                _unityWebRequest.SendWebRequest();

                _status = BackgroundDownloadStatus.Downloading;
            }
            catch (System.Exception e)
            {
                TLogger.LogError($"DownloadHandlerFileRange.StartDownload,Exception,{e.StackTrace}");
                _status = BackgroundDownloadStatus.Failed;
                throw;
            }
        }

        public new void Dispose()
        {
            if (_status == BackgroundDownloadStatus.Downloading)
            {
                _status = BackgroundDownloadStatus.Failed;
            }

            base.Dispose();

            if (_fileStream != null)
            {
                _fileStream.Close();
                _fileStream.Dispose();
                _fileStream = null;
            }

            if (_unityWebRequest != null)
            {
                _unityWebRequest.Abort();
                _unityWebRequest.Dispose();
                _unityWebRequest = null;
            }
        }
        #region
        public float Progress => _totalFileSize == 0 ? 0 : ((float)_curFileSize) / _totalFileSize;

        public long TotalSize => _totalFileSize;

        public long CurrentSize => _curFileSize;

        protected override bool ReceiveData(byte[] data, int dataLength)
        {
            if (data == null || dataLength <= 0)
            {
                return false;
            }
            _fileStream.Write(data, 0, dataLength);
            _fileStream.Flush();
            _curFileSize += dataLength;
            LoadUpdateLogic.Instance.DownProgressAction?.Invoke(_curFileSize);

            return true;
        }
        #endregion
        #region IEnumerator
        public object Current
        {
            get
            {
                return null;
            }
        }

        public BackgroundDownloadStatus Status => _status;

        public bool MoveNext()
        {
            if (_status == BackgroundDownloadStatus.Done ||
                _status == BackgroundDownloadStatus.Failed ||
                _status == BackgroundDownloadStatus.NetworkError)
            {
                return false;
            }

            if (_unityWebRequest.isNetworkError || _unityWebRequest.isHttpError)
            {
                _status = BackgroundDownloadStatus.NetworkError;
            }
            else if (_unityWebRequest.isDone)
            {
                _status = BackgroundDownloadStatus.Done;
            }

            return _status == BackgroundDownloadStatus.Downloading;
        }

        public void Reset()
        { }
        #endregion
    }
}
