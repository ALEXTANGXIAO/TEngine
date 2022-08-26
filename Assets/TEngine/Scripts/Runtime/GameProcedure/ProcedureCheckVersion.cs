using System;
using System.Collections.Generic;
using TEngine.Runtime.HotUpdate;
using UnityEngine;
using UnityEngine.Networking;

namespace TEngine.Runtime
{
    /// <summary>
    /// 流程加载器 - 检查版本更新
    /// </summary>
    public class ProcedureCheckVersion:ProcedureBase
    {
        private bool m_CheckVersionComplete = false;
        private bool m_NeedUpdateVersion = false;
        private OnlineVersionInfo m_VersionInfo = null;
        private UnityWebRequest m_UnityWebRequest;
        private UnityWebRequestAsyncOperation m_result;
        private int _curTryCount;
        private const int MaxTryCount = 3;

        private void PreLoadHotUpdate()
        {
            //热更新阶段文本初始化
            LoadText.Instance.InitConfigData(null);
            //热更新UI初始化
            UILoadMgr.Initialize();

            UILoadMgr.Show(UIDefine.UILoadUpdate);
        }

        protected internal override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            m_CheckVersionComplete = false;
            m_NeedUpdateVersion = false;
            m_VersionInfo = null;

            PreLoadHotUpdate();

            RequestVersion();
        }

        /// <summary>
        /// 请求热更数据
        /// </summary>
        private void RequestVersion()
        {
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

            var checkVersionUrl = Utility.Text.Format(HotUpdateMgr.Instance.BuildInfo.CheckVersionUrl,
                GetPlatformPath());

            UILoadMgr.Show(UIDefine.UILoadUpdate, string.Format(LoadText.Instance.Label_Load_Checking, _curTryCount));
            if (string.IsNullOrEmpty(checkVersionUrl))
            {
                TLogger.LogError("LoadMgr.RequestVersion, remote url is empty or null");
                LoaderUtilities.ShowMessageBox(LoadText.Instance.Label_RemoteUrlisNull, MessageShowType.OneButton,
                    LoadStyle.StyleEnum.Style_QuitApp,
                    Application.Quit);
                return;
            }
            TLogger.LogInfo("LoadMgr.RequestVersion, proxy:" + checkVersionUrl);

            Log.Info($"Check Version Url=>{checkVersionUrl}");

            m_UnityWebRequest = UnityWebRequest.Get(checkVersionUrl);

            m_result = m_UnityWebRequest.SendWebRequest();
        }

        private LoadData GenLoadData()
        {
            var onlineResListUrl = m_VersionInfo.UpdatePrefixUri + "/" + m_VersionInfo.ResourceVersion +"/";
            Log.Warning(onlineResListUrl);
            string resListStr = string.Empty;
            try
            {
                resListStr = LoaderUtilities.HttpGet(onlineResListUrl + "Md5List.json");

            }
            catch (Exception e)
            {
                LoaderUtilities.ShowMessageBox(LoadText.Instance.Label_Net_Error+e.Message, MessageShowType.TwoButton,
                    LoadStyle.StyleEnum.Style_Retry,
                    () => {
                        RequestVersion();
                    }, () =>
                    {
                        Application.Quit();
                    });
                return null;
            }
     
            var resDic = (Dictionary<string, object>)MiniJSON.Json.Deserialize(resListStr);
            List<object> resList = (List<object>)resDic["_target"];
            var data = new LoadData();

            data.Type = UpdateType.ResourceUpdate;
            data.Style = UpdateStyle.Optional;
            data.Notice = UpdateNotice.Notice;
            data.Type = m_VersionInfo.UpdateType;

            if (GameConfig.Instance.ResId == m_VersionInfo.ResourceVersion.ToString())
            {
                TLogger.LogInfo("GameConfig.Instance.ResId 匹配，无需更新");

                UILoadMgr.Show(UIDefine.UILoadUpdate, "校验资源版中...");

                LoaderUtilities.DelayFun((() =>
                {
                    UILoadMgr.Show(UIDefine.UILoadUpdate, "资源版校验完成!!!");
                }), new WaitForSeconds(1f));

                LoaderUtilities.DelayFun((() =>
                {
                    UILoadMgr.HideAll();
                }),new WaitForSeconds(2f));
                
                data.Type = UpdateType.None;
            }
            if (data.Type != UpdateType.PackageUpdate)
            {
                data.Style = m_VersionInfo.UpdateStyle;
                data.Notice = m_VersionInfo.UpdateNotice;
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
                        RemoteUrl = onlineResListUrl + fileName,
                        Md5 = item["md5"].ToString(),
                        Size = (long)item["fileSize"],
                        Url = fileName,
                    };
                    data.List.Add(itemTemp);
                    data.All.Add(itemTemp);
                }
            }

            return data;
        }

        protected internal override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);
            if (m_result == null || !m_result.isDone)
            {
                return;
            }
            else
            {
                CheckWebRequest();
            }

            if (!m_CheckVersionComplete)
            {
                return;
            }

            if (m_NeedUpdateVersion)
            {
                var data = GenLoadData();
                if (data == null)
                {
                    m_UnityWebRequest.Dispose();
                    m_result = null;
                    m_UnityWebRequest = null;
                    return;
                }
                procedureOwner.SetData<LoadData>("LoadData", data);
                procedureOwner.SetData<OnlineVersionInfo>("OnlineVersionInfo", m_VersionInfo);
                ChangeState<ProcedureResourcesUpdate>(procedureOwner);
            }
            else
            {
                var data = GenLoadData();
                if (data == null)
                {
                    m_UnityWebRequest.Dispose();
                    m_result = null;
                    m_UnityWebRequest = null;
                    return;
                }
                procedureOwner.SetData<LoadData>("LoadData", data);
                procedureOwner.SetData<OnlineVersionInfo>("OnlineVersionInfo", m_VersionInfo);
                ChangeState<ProcedureCodeInit>(procedureOwner);
            }
        }

        /// <summary>
        /// 检测版本检查网络请求
        /// </summary>
        private void CheckWebRequest()
        {
            if (m_result == null || !m_result.isDone)
            {
                return;
            }

            if (m_UnityWebRequest != null && m_UnityWebRequest.isDone)
            {
                if (m_UnityWebRequest.error == null)
                {
                    byte[] versionInfoBytes = m_UnityWebRequest.downloadHandler.data;
                    string versionInfoString = Utility.Converter.GetString(versionInfoBytes);
                    m_VersionInfo = Utility.Json.ToObject<OnlineVersionInfo>(versionInfoString);
                    if (m_VersionInfo == null)
                    {
                        Log.Fatal("Parse VersionInfo failure.");
                        return;
                    }

                    Log.Info("Latest game version is '{0} ({1})', local game version is '{2} ({3})'.",
                        m_VersionInfo.GameVersion,
                        m_VersionInfo.InternalResourceVersion.ToString(), 
                        Version.GameVersion, 
                        Version.InternalGameVersion.ToString());

                    Log.Warning("GameConfig.Instance.ResId " + GameConfig.Instance.ResId);
                    Log.Warning("Version.ResourceVersion " + m_VersionInfo.ResourceVersion);

                    if (string.Equals(GameConfig.Instance.ResId, m_VersionInfo.ResourceVersion.ToString()))
                    {
                        m_NeedUpdateVersion = false;
                    }
                    else
                    {
                        m_NeedUpdateVersion = true;
                    }
                    m_CheckVersionComplete = true;
                }
                else
                {
                    Log.Fatal($"Check version failure :{m_UnityWebRequest.error}");
                    m_UnityWebRequest.Dispose();
                    m_result = null;
                    m_UnityWebRequest = null;

                    LoaderUtilities.DelayFun(RequestVersion, new WaitForSeconds(1f));
                }
            }
        }

        /// <summary>
        /// 跳转到APP下载路径
        /// </summary>
        /// <param name="userData"></param>
        private void GotoUpdateApp(object userData)
        {
            string url = null;
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            url = HotUpdateMgr.Instance.BuildInfo.WindowsAppUrl;
#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            url = HotUpdateMgr.Instance.BuildInfo.MacOSAppUrl;
#elif UNITY_IOS
            url = HotUpdateMgr.Instance.BuildInfo.IOSAppUrl;
#elif UNITY_ANDROID
            url = HotUpdateMgr.Instance.BuildInfo.AndroidAppUrl;
#endif
            if (!string.IsNullOrEmpty(url))
            {
                Application.OpenURL(url);
            }
        }

        /// <summary>
        /// 获取不同平台的资源检测路径
        /// </summary>
        /// <returns></returns>
        private string GetPlatformPath()
        {
#if UNITY_EDITOR
#if UNITY_STANDALONE_WIN
            return "Windows";
#elif UNITY_EDITOR_OSX
            return "IOS";
#elif UNITY_IOS
            return "IOS";
#elif UNITY_ANDROID
            return "Android";
#else
            return "Unknow"
#endif
#else
            // 这里和 PlatformUtility.GetPlatformPath() 对应。由 Unity.RuntimePlatform 得到 平台标识符
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.WindowsPlayer:
                    return "Windows";

                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.OSXPlayer:
                    return "MacOS";

                case RuntimePlatform.IPhonePlayer:
                    return "IOS";

                case RuntimePlatform.Android:
                    return "Android";

                case RuntimePlatform.WSAPlayerX64:
                case RuntimePlatform.WSAPlayerX86:
                case RuntimePlatform.WSAPlayerARM:
                    return "WSA";

                case RuntimePlatform.WebGLPlayer:
                    return "WebGL";

                case RuntimePlatform.LinuxEditor:
                case RuntimePlatform.LinuxPlayer:
                    return "Linux";

                default:
                    Log.Fatal(Application.platform.ToString());
                    throw new System.NotSupportedException(Utility.Text.Format("Platform '{0}' is not supported.", Application.platform));
            }
#endif

        }
    }
}
