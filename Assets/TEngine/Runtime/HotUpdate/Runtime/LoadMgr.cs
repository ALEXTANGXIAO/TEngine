using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TEngine.UI;
using UnityEngine;

namespace TEngine
{
    public class LoadUpdateLogic
    {

        private static LoadUpdateLogic _instance;

        public Action<int> Download_Complete_Action = null;
        public Action<long> Down_Progress_Action = null;
        public Action<bool, GameStatus> _Unpacked_Complete_Action = null;
        public Action<float, GameStatus> _Unpacked_Progress_Action = null;

        public static LoadUpdateLogic Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new LoadUpdateLogic();
                return _instance;
            }
        }
    }

    public class LoadMgr : TSingleton<LoadMgr>
    {
        /// <summary>
        /// 资源版本号
        /// </summary>
        public string LatestResId { get; set; }

        private bool _loaderUpdateLaunched = false;
        private Action _startGameEvent;
        private Action _firstPackageEvent;
        private int _curTryCount;
        private const int MaxTryCount = 3;
        private bool _connectBack;
        private bool _needUpdate = false;
        private LoadData _currentData;
        private UpdateType _currentType = UpdateType.None;
        private readonly string _downloadFilePath = FileSystem.ResourceRoot + "/";
        private Coroutine _coroutine;
        //加载检测
        private bool _dllLoad;
        private Coroutine _load_data_check;
        public DownloadImpl Downloader { get; set; }

        public LoadMgr()
        {
            _curTryCount = 0;
            _connectBack = false;
            _startGameEvent = null;
        }

        public bool IsLaunched()
        {
            return _instance._loaderUpdateLaunched;
        }

        public void StartLoadInit(Action onUpdateComplete)
        {
            //热更新阶段文本初始化
            LoadText.Instance.InitConfigData(null);
            //热更新UI初始化
            UILoadMgr.Initialize();

            UILoadMgr.Show(UIDefine.UILoadUpdate);

            if (LoadMgr.Instance.IsLaunched())
            {
                StartGame();
            }
            else
            {
#if RELEASE_BUILD || _DEVELOPMENT_BUILD_
            StartLoad(() => { FinishCallBack(onUpdateComplete); });
#else
                onUpdateComplete?.Invoke();
#endif
            }
        }

        /// <summary>
        /// 开启热更新逻辑
        /// </summary>
        /// <param name="action"></param>
        public void StartLoad(Action action)
        {
            _loaderUpdateLaunched = true;
            _startGameEvent = action;
            _connectBack = false;
            _curTryCount = 0;
            RequestVersion();
        }

        private void FinishCallBack(Action callBack)
        {
            GameConfig.Instance.WriteVersion(LatestResId);
            if (_needUpdate)
            {
                callBack();
            }
            else
            {
                callBack();
            }
        }

        /// <summary>
        /// 请求热更数据
        /// </summary>
        private void RequestVersion()
        {
            if (_connectBack)
            {
                return;
            }

            _curTryCount++;

            if (_curTryCount > MaxTryCount)
            {
                LoaderUtilities.ShowMessageBox(LoadText.Instance.Label_Net_Error, MessageShowType.TwoButton,
                    LoadStyle.StyleEnum.Style_Retry,
                    () => {
                        _curTryCount = 0;
                        RequestVersion();
                    }, () =>
                    {
                        Application.Quit();
                    });
                return;
            }

            UILoadMgr.Show(UIDefine.UILoadUpdate, string.Format(LoadText.Instance.Label_Load_Checking, _curTryCount));
            if (string.IsNullOrEmpty(OnlineParamUrl))
            {
                TLogger.LogError("LoadMgr.RequestVersion, remote url is empty or null");
                LoaderUtilities.ShowMessageBox(LoadText.Instance.Label_RemoteUrlisNull, MessageShowType.OneButton,
                    LoadStyle.StyleEnum.Style_QuitApp,
                    Application.Quit);
                return;
            }
            TLogger.LogInfo("LoadMgr.RequestVersion, proxy:" + OnlineParamUrl);


            try
            {
                var onlineParamStr = LoaderUtilities.HttpGet(OnlineParamUrl);
                var onlineParam = (Dictionary<string, object>)MiniJSON.Json.Deserialize(onlineParamStr);
                string onlineResVersion = onlineParam["resVersion"].ToString();
                TLogger.LogInfo($"onlineResVersion:{onlineResVersion}");
                LatestResId = onlineResVersion;
                var resListStr = LoaderUtilities.HttpGet(GetOnlineResListUrl(LatestResId) + "Md5CheckList.json");
                var resDic = (Dictionary<string, object>)MiniJSON.Json.Deserialize(resListStr);
                List<object> resList = (List<object>)resDic["target"];
                _connectBack = true;

                OnHttpResponse(CreateLoadData(onlineParam, resList));
            }
            catch (Exception e)
            {
#if UNITY_EDITOR
                StartGame();
#else
                LoaderUtilities.DelayFun(RequestVersion, new WaitForSeconds(1f));
#endif
                TLogger.LogError(e.Message);
            }
            return;
        }

        internal void OnHttpResponse(LoadData formatData)
        {
            if (formatData == null)
            {
                return;
            }
            //检测一下是不是本地存在该版本，如果存在就本地做回退，不需要走下载流程
            _currentData = formatData;
            var exitLocalVersion = _CheckLocalVersion(_currentData.List);
            if (exitLocalVersion)
            {
                _needUpdate = LatestResId != GameConfig.Instance.ResId;
                StartGame();
                return;
            }
            //检测一下本地是否存在资源，如果存在了就直接解压就行了，如果不存在还需要下载
            _currentData.List = _CheckLocalExitResource(_currentData.List);
            _currentType = _currentData.Type;

            if (_currentData.List.Count <= 0)
            {
                DownLoadCallBack((int)DownLoadResult.AllDownLoaded, GameStatus.AssetLoad, _currentData.All);
                _needUpdate = true;
                return;
            }
            //展示更新类型
            var exitUpdate = ShowUpdateType(_currentData);
            if (exitUpdate)
            {
                _needUpdate = true;
            }
            else
            {
                StartGame();
            }
        }

        /// <summary>
        /// 显示更新方式
        /// </summary>
        /// <returns></returns>
        private bool ShowUpdateType(LoadData data)
        {
            UILoadMgr.Show(UIDefine.UILoadUpdate, LoadText.Instance.Label_Load_Checked);
            //底包更新
            if (data.Type == UpdateType.PackageUpdate)
            {
                if (true)
                {
                    LoaderUtilities.ShowMessageBox(LoadText.Instance.Label_Load_Package, MessageShowType.OneButton,
                        LoadStyle.StyleEnum.Style_DownLoadApk,
                        () => {
                            StartUpdate(data.List);
                        });
                }
                else
                {
                    LoaderUtilities.ShowMessageBox(LoadText.Instance.Label_Load_Plantform, MessageShowType.OneButton,
                        LoadStyle.StyleEnum.Style_DownLoadApk,
                        () =>
                        {
                            //DownLoadPackage();
                        });
                }
                return true;
            }
            //资源更新
            else if (data.Type == UpdateType.ResourceUpdate)
            {
                //强制
                if (data.Style == UpdateStyle.Froce)
                {
                    //提示
                    if (data.Notice == UpdateNotice.Notice)
                    {
                        NetworkReachability n = Application.internetReachability;
                        string desc = LoadText.Instance.Label_Load_Force_WIFI;
                        if (n == NetworkReachability.ReachableViaCarrierDataNetwork)
                        {
                            desc = LoadText.Instance.Label_Load_Force_NO_WIFI;
                        }

                        long totalSize = 0;
                        foreach (var item in data.List)
                        {
                            totalSize += item.Size;
                        }

                        desc = string.Format(desc, LoaderUtilities.FormatData(totalSize));
                        LoaderUtilities.ShowMessageBox(desc, MessageShowType.TwoButton,
                            LoadStyle.StyleEnum.Style_StartUpdate_Notice,
                            () =>
                            {
                                StartUpdate(data.List);
                            }, () =>
                            {
                                StartGame();
                            });
                    }
                    //不提示
                    else if (data.Notice == UpdateNotice.NoNotice)
                    {
                        StartUpdate(data.List);
                    }
                }
                //非强制
                else if (data.Style == UpdateStyle.Optional)
                {
                    //提示
                    if (data.Notice == UpdateNotice.Notice)
                    {
                        LoaderUtilities.ShowMessageBox(LoadText.Instance.Label_Load_Notice, MessageShowType.TwoButton,
                            LoadStyle.StyleEnum.Style_StartUpdate_Notice,
                            () => {
                                StartUpdate(data.List);
                            }, () => {
                                StartGame();
                            });
                    }
                    //不提示
                    else if (data.Notice == UpdateNotice.NoNotice)
                    {
                        StartUpdate(data.List);
                    }
                }
                else
                    TLogger.LogError("LoadMgr._CheckUpdate, style is error,code:" + data.Style);
                return true;
            }
            //没有更新
            return false;
        }

        /// <summary>
        /// 下载结果回调
        /// </summary>
        /// <param name="result"></param>
        /// <param name="status"></param>
        /// <param name="files"></param>
        public void DownLoadCallBack(int result, GameStatus status, List<LoadResource> files = null)
        {
            //下载完成
            if (result == (int)DownLoadResult.AllDownLoaded)
            {
                if (_currentType == UpdateType.PackageUpdate)
                {
                    if (true)
                    {
                        GameConfig.Instance.SignReInstall();
                        if (files != null)
                        {
                            //InstallApk(_downloadFilePath + files[0].Url);
                        }
                        else
                        {
                            TLogger.LogError("LoadMgr.DownLoadCallBack, data exception");
                        }
                    }
                }
                else
                {
                    UnpackFiles(_currentData, FileSystem.ResourceRoot, status, _UnPackComplete, (current, total) =>
                    {
                        _UnPackCallback(current, status, total);
                    });
                }
                _StopLoadingCheck();
            }
            else
            {
                _StopLoadingCheck();
                //网络发生变化，重试继续下载
                if (result == (int)DownLoadResult.NetChanged || result == (int)DownLoadResult.HeadRequestError)
                {
                    _RetryConnect();
                    return;
                }
                if (result == (int)DownLoadResult.DownLoadingError ||
                    result == (int)DownLoadResult.ReceiveNullData ||
                        result == (int)DownLoadResult.DownError ||
                        result == (int)DownLoadResult.ReceiveError ||
                        result == (int)DownLoadResult.Md5Wrong)
                {
                    LoaderUtilities.ShowMessageBox(string.Format(LoadText.Instance.Label_Load_Error, result), MessageShowType.OneButton,
                        LoadStyle.StyleEnum.Style_QuitApp,
                        () => {
                            Application.Quit();
                        });
                }
            }

            ResetRetry();
        }

        /// <summary>
        /// 本地检测,过滤掉本地存在了的资源
        /// </summary>
        /// <param name="res"></param>
        /// <returns></returns>
        private List<LoadResource> _CheckLocalExitResource(List<LoadResource> res)
        {
            var list = new List<LoadResource>();
            foreach (var item in res)
            {
                string resPath = _downloadFilePath + item.Url;
                if (File.Exists(resPath))
                {
                    if (string.CompareOrdinal(LoaderUtilities.GetMd5Hash(resPath), item.Md5) == 0)
                    {
                        continue;
                    }

                    list.Add(item);
                }
                else
                {
                    list.Add(item);
                }
            }
            return list;
        }

        /// <summary>
        /// 检测是否是版本回退了
        /// </summary>
        /// <param name="res"></param>
        /// <returns></returns>
        private bool _CheckLocalVersion(List<LoadResource> res)
        {
            foreach (var item in res)
            {
                string tempVersion = item.Url.Split('_')[1];
                if (!GameConfig.Instance.CheckLocalVersion(tempVersion))
                {
                    return false;
                }
            }

            return true;
        }

        public void StartGame()
        {
            if (_startGameEvent != null)
            {
                _startGameEvent();
            }
        }

        internal LoadData CreateLoadData(Dictionary<string, object> onlineParam, List<object> resList = null)
        {
            if (onlineParam == null)
            {
                return null;
            }
            var data = new LoadData();
            var result = int.Parse("1");
            var versionStr = LoaderUtilities.HttpGet(GetOnlineResListUrl(LatestResId) + "version.json");
            var versionDic = (Dictionary<string, object>)MiniJSON.Json.Deserialize(versionStr);
            string version = versionDic["AppVersion"].ToString();
            switch (result)
            {
                case (int)VersionRequestErrorCode.Ok:
                    {
                        data.Type = UpdateType.ResourceUpdate;
                        data.Style = UpdateStyle.Optional;
                        data.Notice = UpdateNotice.Notice;

                        data.Type = (UpdateType)int.Parse(onlineParam["UpdateType"].ToString());

                        if (GameConfig.Instance.ResId == LatestResId)
                        {
                            TLogger.LogInfo("GameConfig.Instance.ResId 匹配，无需更新");
                            data.Type = UpdateType.None;
                        }
                        if (data.Type != UpdateType.PackageUpdate)
                        {
                            data.Style = (UpdateStyle)int.Parse(onlineParam["UpdateStyle"].ToString());
                            data.Notice = (UpdateNotice)int.Parse(onlineParam["UpdateNotice"].ToString());
                        }
                        data.List = new List<LoadResource>();
                        data.All = new List<LoadResource>();

                        if (resList != null)
                        {
                            for (int i = 0; i < resList.Count; i++)
                            {
                                var item = (Dictionary<string, object>)resList[i];
                                string fileName = item["fileName"].ToString();
                                var itemTemp = new LoadResource
                                {
                                    RemoteUrl = _onlineResListUrl + fileName,
                                    Md5 = item["md5"].ToString(),
                                    Size = (long)item["fileSize"],
                                    Url = fileName + "_" + LatestResId,
                                };
                                data.List.Add(itemTemp);
                                data.All.Add(itemTemp);
                            }
                        }
                        break;
                    }

                case (int)VersionRequestErrorCode.Patch_Not_Exist:
                    LatestResId = string.Empty;
#if UNITY_EDITOR_WIN || _DEVELOPMENT_BUILD_
                    StartGame();
#else
                    LoaderUtilities.ShowMessageBox("response msg error", MessageShowType.OneButton,
                            LoadStyle.StyleEnum.Style_QuitApp,
                            () =>
                            {
                                Application.Quit();
                            });
#endif
                    return null;
                default:
                    LoaderUtilities.ShowMessageBox("response msg error", MessageShowType.OneButton,
                        LoadStyle.StyleEnum.Style_QuitApp, Application.Quit);
                    return null;
            }
            if (GameConfig.Instance.ResId == LatestResId)
            {
                TLogger.LogInfo($"GameConfig.Instance.ResId{GameConfig.Instance.ResId} == onlineResVersion{LatestResId}");
            }
            else
            {
                TLogger.LogInfo($"GameConfig.Instance.ResId{GameConfig.Instance.ResId} != onlineResVersion{LatestResId}");
            }

            return data;
        }

        private string _resListUrl = string.Empty;
        private string _onlineParamUrl = string.Empty;
        internal const string Url = "http://1.12.241.46:8081/TXYXGame/";

        internal string ResListUrl
        {
            get
            {
                if (string.IsNullOrEmpty(_resListUrl))
                {
#if UNITY_EDITOR || UNITY_ANDROID
                    _resListUrl = Url + "AssetBundles/Android/ver_" + GameConfig.Instance.ResId + "/";
#elif UNITY_IOS || UNITY_IPHONE
                    _resListUrl = Url + "AssetBundles/IOS/ver_" + GameConfig.Instance.ResId + "/";
#elif UNITY_STANDALONE_WIN
                    _resListUrl = Url + "AssetBundles/WIN/ver_" + GameConfig.Instance.ResId + "/";
#endif
                }
                return _resListUrl;
            }
        }

        private string _onlineResListUrl = string.Empty;
        internal string GetOnlineResListUrl(string onlineResId)
        {
            if (string.IsNullOrEmpty(_onlineResListUrl))
            {
#if UNITY_EDITOR || UNITY_ANDROID
                _onlineResListUrl = Url + "AssetBundles/Android/ver_" + onlineResId + "/";
#elif UNITY_IOS || UNITY_IPHONE
                _onlineResListUrl = Url + "AssetBundles/IOS/ver_" + onlineResId + "/";
#elif UNITY_STANDALONE_WIN
                _onlineResListUrl = Url + "AssetBundles/WIN/ver_" + onlineResId + "/";
#endif
            }
            TLogger.LogInfo(_onlineResListUrl);
            return _onlineResListUrl;
        }

        internal string OnlineParamUrl
        {
            get
            {
                if (string.IsNullOrEmpty(_onlineParamUrl))
                {
#if UNITY_EDITOR || UNITY_ANDROID
                    _onlineParamUrl = Url + "androidVer.json";
#elif UNITY_IOS || UNITY_IPHONE
                     _onlineParamUrl =  Url + "iosVer.json";
#elif UNITY_STANDALONE_WIN
                    _onlineParamUrl = Url + "winVer.json";
#endif
                }
                return _onlineParamUrl;
            }
        }


        public void StartDownLoad()
        {
            if (Downloader == null)
                return;

            if (Downloader.IsLoading())
            {
                TLogger.LogInfo("Loadermanager.StartUpdate，down loading......");
                return;
            }


            if (_coroutine != null)
            {
                CoroutineUtility.StopCoroutine(_coroutine);
                _coroutine = null;
            }
            //开始下载
            _coroutine = CoroutineUtility.StartCoroutine(Downloader.DownLoad());

            LoaderUtilities.DelayFun(() =>
            {
                if (_load_data_check != null)
                {
                    CoroutineUtility.StopCoroutine(_load_data_check);
                }

                _load_data_check = CoroutineUtility.StartCoroutine(_StartLoadingCheck());
            }, new WaitForSeconds(1));
        }

        public void StopDownLoad()
        {
            if (Downloader == null)
                return;

            Downloader.StopDownLoad();
            if (_coroutine == null) return;
            CoroutineUtility.StopCoroutine(_coroutine);
            _coroutine = null;
        }

        private void ReleaseLoader()
        {
            StopDownLoad();
            Downloader = null;
        }

        #region 数据加载检测
        private long _currentSize;
        private int _currentTryCount;
        private Action _retryAction;
        private IEnumerator _StartLoadingCheck()
        {
            while (Downloader.IsLoading())
            {
                _currentSize = Downloader.CurrentLoadSize();
                yield return new WaitForSeconds(20);
                long newSize = Downloader.CurrentLoadSize();
                if (newSize - _currentSize < 1024)
                {
                    StopDownLoad();
                    LoaderUtilities.ShowMessageBox(LoadText.Instance.Label_DownLoadFailed, MessageShowType.TwoButton,
                        LoadStyle.StyleEnum.Style_DownZip,
                        () =>
                        {
                            StartDownLoad();
                        }, () =>
                        {
                            Application.Quit();
                        });
                }
                else
                {
                    _currentSize = Downloader.CurrentLoadSize();
                }
            }
        }
        private void _StopLoadingCheck()
        {
            if (_load_data_check != null)
            {
                CoroutineUtility.StopCoroutine(_load_data_check);
            }
        }
        //尝试重连
        private void _RetryConnect()
        {
            if (_retryAction == null)
            {
                _retryAction = () => {
                    LoaderUtilities.DelayFun(() =>
                    {
                        _currentTryCount++;
                        UILoadMgr.Show(UIDefine.UILoadUpdate, string.Format(LoadText.Instance.Label_Net_Changed, _currentTryCount));
                        StartDownLoad();
                    }, new WaitForSeconds(3));
                };
            }

            if (_currentTryCount >= 3)
            {
                LoaderUtilities.ShowMessageBox(LoadText.Instance.Label_Net_Error, MessageShowType.TwoButton,
                    LoadStyle.StyleEnum.Style_DownZip,
                    () =>
                    {
                        _currentTryCount = 0;
                        _retryAction.Invoke();
                    }, Quit);
            }
            else
            {
                _retryAction.Invoke();
            }
        }

        private void ResetRetry()
        {
            _currentTryCount = 0;
            _retryAction = null;
        }

        #endregion

        /// <summary>
        /// 开始更新资源
        /// </summary>
        public void StartUpdate(List<LoadResource> list)
        {
            if (list == null || list.Count <= 0)
            {
                TLogger.LogError("Loadermanager.StartUpdate， resource list is empty");
                return;
            }
            //检测内存是否充足
            long totalSize = 0;
            foreach (var item in list)
            {
                totalSize += item.Size;
            }

            if (false)
            {
                LoaderUtilities.ShowMessageBox(LoadText.Instance.Label_Memory_Low_Load, MessageShowType.OneButton,
                    LoadStyle.StyleEnum.Style_QuitApp,
                    () => {
                        Application.Quit();
                    });
                return;
            }

            Downloader = new DownloadImpl(list, _downloadFilePath, (result, files) =>
            {
                DownLoadCallBack(result, GameStatus.AssetLoad, files);
            });
            StartDownLoad();
        }

        /// <summary>
        /// 加压缩进度回调
        /// </summary>
        /// <param name="current"></param>
        /// <param name="status"></param>
        /// <param name="total"></param>
        private void _UnPackCallback(long current, GameStatus status, long total)
        {
            float t;
            if (total == 0)
                t = 0;
            else
            {
                t = (float)current / total;
            }
            LoadUpdateLogic.Instance._Unpacked_Progress_Action?.Invoke(t, status);
        }

        /// <summary>
        /// 解压缩结束回调
        /// </summary>
        /// <param name="result"></param>
        /// <param name="status"></param>
        private void _UnPackComplete(bool result, GameStatus status)
        {
            if (result)
            {
                foreach (var item in _currentData.All)
                {
                    if (item.Url == null)
                    {
                        continue;
                    }

                    LoaderUtilities.DeleteFile(FileSystem.ResourceRoot + "/" + item.Url);
                }

                try
                {
                    if (string.IsNullOrEmpty(LatestResId))
                    {
                        LatestResId = GameConfig.Instance.ResId;
                    }
                    GameConfig.Instance.WriteVersion(LatestResId);
                    FileunzipManager.Instance.CopyMonoAssembly(LatestResId);
                }
                catch (Exception e)
                {
                    TLogger.LogError(e.StackTrace);
                    LoaderUtilities.ShowMessageBox(LoadText.Instance.Label_Load_UnPackError, MessageShowType.OneButton,
                        LoadStyle.StyleEnum.Style_RestartApp,
                        () =>
                        {
                            Application.Quit();
                        });
                    return;
                }

                if (status == GameStatus.AssetLoad)
                {
                    if (_dllLoad == false)
                    {
                        StartGame();
                    }
                    else
                    {
                        LoaderUtilities.ShowMessageBox(LoadText.Instance.Label_RestartApp, MessageShowType.OneButton,
                            LoadStyle.StyleEnum.Style_RestartApp,
                            () =>
                            {
                                Quit();
                            });
                    }
                }
                else
                {
                    if (_firstPackageEvent != null)
                    {
                        _firstPackageEvent.Invoke();
                    }
                    else
                    {
                        StartGame();
                    }
                }
            }
            else
            {
                if (status == GameStatus.First)
                {
                    LoaderUtilities.DeleteFolder(FileSystem.ResourceRoot);
                }
                else
                {
                    foreach (var item in _currentData.List)
                    {
                        if (item.Url == null)
                        {
                            continue;
                        }
                        LoaderUtilities.DeleteFile(FileSystem.ResourceRoot + "/" + item.Url);
                    }
                }

                LoaderUtilities.ShowMessageBox(LoadText.Instance.Label_Load_UnPackError, MessageShowType.OneButton,
                    LoadStyle.StyleEnum.Style_QuitApp,
                    () => {
                        Application.Quit();
                    });
            }
        }

        /// <summary>
        /// 解压包
        /// </summary>
        /// <param name="file">文件地址</param>
        /// <param name="dir">解压目录</param>
        /// <param name="status"></param>
        /// <param name="callback">解压结果回调</param>
        /// <param name="progress">解压进度回调</param>
        public void UnpackFiles(LoadData file, string dir, GameStatus status, Action<bool, GameStatus> callback, Action<long, long> progress)
        {
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            long totalFilesize = 0;
            _dllLoad = false;
            foreach (var item in file.All)
            {
                if (_dllLoad == false)
                {
                    _dllLoad = FastZip.entryExists($"{dir}/{item.Url}", FileSystem.DllScriptPkgName);
                }
                totalFilesize += item.Size;
            }


            FileunzipManager.Instance.StartFastUnZip(file.All, dir, dir);
            CoroutineUtility.StartCoroutine(UpdateProgress(totalFilesize, status, callback, progress));
        }

        public IEnumerator UpdateProgress(long size, GameStatus status, Action<bool, GameStatus> callback, Action<long, long> progress)
        {
            long currentProgress;
            while (FileunzipManager.Instance.IsRunning())
            {
                currentProgress = FileunzipManager.Instance.GetProgress();
                progress.Invoke(currentProgress, size);
                yield return null;
            }
            currentProgress = FileunzipManager.Instance.GetProgress();
            progress.Invoke(currentProgress, size);
            yield return null;
            yield return null;
            yield return null;
            callback?.Invoke(FileunzipManager.Instance.UnPackResult(), status);
        }


        internal void Quit()
        {
            Application.Quit();
        }
    }
}
