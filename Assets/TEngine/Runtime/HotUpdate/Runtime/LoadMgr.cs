using System;
using System.Collections.Generic;
using System.IO;
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

        private Action _startGameEvent;
        private int _curTryCount;
        private const int MaxTryCount = 3;
        private bool _connectBack;
        private bool _needUpdate = false;
        private LoadData _currentData;
        private UpdateType _currentType = UpdateType.None;
        private readonly string _downloadFilePath = FileSystem.ResourceRoot + "/";

        public LoadMgr()
        {
            _curTryCount = 0;
            _connectBack = false;
            _startGameEvent = null;
        }

        public void StartLoadInit(Action onUpdateComplete)
        {
#if RELEASE_BUILD || _DEVELOPMENT_BUILD_
            StartLoad(() => { FinishCallBack(onUpdateComplete); });
#else
            onUpdateComplete();
#endif
        }

        /// <summary>
        /// 开启热更新逻辑
        /// </summary>
        /// <param name="action"></param>
        public void StartLoad(Action action)
        {
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

            }

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
           
            //TODO
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

        void StartGame()
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
                        //TODO
#endif
                    return null;
                default:
                    
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
    }
}
