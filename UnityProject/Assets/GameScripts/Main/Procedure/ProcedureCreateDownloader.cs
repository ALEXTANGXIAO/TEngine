using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TEngine;
using UnityEngine;
using UnityEngine.Networking;
using YooAsset;
using ProcedureOwner = TEngine.IFsm<TEngine.IProcedureManager>;
using Utility = TEngine.Utility;

namespace GameMain
{
    public class ProcedureCreateDownloader : ProcedureBase
    {
        private int _curTryCount;
        
        private const int MaxTryCount = 3;
        
        public override bool UseNativeDialog { get; }

        private ProcedureOwner _procedureOwner;

        private ResourceDownloaderOperation _downloader;

        private int _totalDownloadCount;

        private string _totalSizeMb;

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            _procedureOwner = procedureOwner;
            
            Log.Info("创建补丁下载器");
            
            UILoadMgr.Show(UIDefine.UILoadUpdate,$"创建补丁下载器...");
            
            CreateDownloader().Forget();
        }

        private async UniTaskVoid CreateDownloader()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(0.5f));

            _downloader = GameModule.Resource.CreateResourceDownloader();

            if (_downloader.TotalDownloadCount == 0)
            {
                Log.Info("Not found any download files !");
                ChangeState<ProcedureDownloadOver>(_procedureOwner);
            }
            else
            {
                //A total of 10 files were found that need to be downloaded
                Log.Info($"Found total {_downloader.TotalDownloadCount} files that need download ！");

                // 发现新更新文件后，挂起流程系统
                // 注意：开发者需要在下载前检测磁盘空间不足
                _totalDownloadCount = _downloader.TotalDownloadCount;
                long totalDownloadBytes = _downloader.TotalDownloadBytes;

                float sizeMb = totalDownloadBytes / 1048576f;
                sizeMb = Mathf.Clamp(sizeMb, 0.1f, float.MaxValue);
                _totalSizeMb = sizeMb.ToString("f1");

                if (!SettingsUtils.EnableUpdateData())
                {
                    UILoadTip.ShowMessageBox($"Found update patch files, Total count {_totalDownloadCount} Total size {_totalSizeMb}MB", MessageShowType.TwoButton,
                        LoadStyle.StyleEnum.Style_StartUpdate_Notice
                        , StartDownFile, Application.Quit);
                }
                else
                {
                    RequestUpdateData().Forget();
                }
            }
        }

        void StartDownFile()
        {
            ChangeState<ProcedureDownloadFile>(_procedureOwner);
        }
        
        
        /// <summary>
        /// 请求更新配置数据。
        /// </summary>
        private async UniTaskVoid RequestUpdateData()
        {
            Log.Warning("On RequestVersion");
            _curTryCount++;

            if (_curTryCount > MaxTryCount)
            {
                UILoadTip.ShowMessageBox(LoadText.Instance.Label_Net_Error, MessageShowType.TwoButton,
                    LoadStyle.StyleEnum.Style_Retry,
                    () =>
                    {
                        _curTryCount = 0;
                        RequestUpdateData().Forget();
                    }, Application.Quit);
                return;
            }
            
            var checkVersionUrl = SettingsUtils.GetUpdateDataUrl();

            UILoadMgr.Show(UIDefine.UILoadUpdate, string.Format(LoadText.Instance.Label_Load_Checking, _curTryCount));
            if (string.IsNullOrEmpty(checkVersionUrl))
            {
                Log.Error("LoadMgr.RequestVersion, remote url is empty or null");
                UILoadTip.ShowMessageBox(LoadText.Instance.Label_RemoteUrlisNull, MessageShowType.OneButton,
                    LoadStyle.StyleEnum.Style_QuitApp,
                    Application.Quit);
                return;
            }
            Log.Info("RequestUpdateData, proxy:" + checkVersionUrl);

            var updateDataStr = await Utility.Http.Get(checkVersionUrl);

            try
            {
                UpdateData updateData = Utility.Json.ToObject<UpdateData>(updateDataStr);
                ShowUpdateType(updateData);
            }
            catch (Exception e)
            {
                Log.Fatal(e);
                throw;
            }
        }

        /// <summary>
        /// 显示更新方式
        /// </summary>
        /// <returns></returns>
        private void ShowUpdateType(UpdateData data)
        {
            UILoadMgr.Show(UIDefine.UILoadUpdate, LoadText.Instance.Label_Load_Checked);
            //底包更新
            if (data.UpdateType == UpdateType.PackageUpdate)
            {
                UILoadTip.ShowMessageBox(LoadText.Instance.Label_Load_Package, MessageShowType.OneButton,
                    LoadStyle.StyleEnum.Style_DownLoadApk,
                    () =>
                    {
                        Log.Info("自定义下载APK");
                        Application.Quit();
                    });
            }
            //资源更新
            else if (data.UpdateType == UpdateType.ResourceUpdate)
            {
                //强制
                if (data.UpdateStyle == UpdateStyle.Force)
                {
                    //提示
                    if (data.UpdateNotice == UpdateNotice.Notice)
                    {
                        NetworkReachability networkReachability = Application.internetReachability;
                        string desc = LoadText.Instance.Label_Load_Force_WIFI;
                        if (networkReachability == NetworkReachability.ReachableViaCarrierDataNetwork)
                        {
                            desc = LoadText.Instance.Label_Load_Force_NO_WIFI;
                        }
                        desc = string.Format(desc, $"{_totalSizeMb}MB");
                        UILoadTip.ShowMessageBox(desc, MessageShowType.TwoButton,
                            LoadStyle.StyleEnum.Style_StartUpdate_Notice,
                            StartDownFile, Application.Quit);
                    }
                    //不提示
                    else if (data.UpdateNotice == UpdateNotice.NoNotice)
                    {
                        StartDownFile();
                    }
                }
                //非强制
                else if (data.UpdateStyle == UpdateStyle.Optional)
                {
                    //提示
                    if (data.UpdateNotice == UpdateNotice.Notice)
                    {
                        UILoadTip.ShowMessageBox(string.Format(LoadText.Instance.Label_Load_Notice,$"{_totalSizeMb}MB"), MessageShowType.TwoButton,
                            LoadStyle.StyleEnum.Style_StartUpdate_Notice,
                            StartDownFile, () =>
                            {
                                ChangeState<ProcedureLoadAssembly>(_procedureOwner);
                            });
                    }
                    //不提示
                    else if (data.UpdateNotice == UpdateNotice.NoNotice)
                    {
                        StartDownFile();
                    }
                }
                else
                {
                    Log.Error("LoadMgr._CheckUpdate, style is error,code:" + data.UpdateStyle);
                }
            }
        }
    }
}