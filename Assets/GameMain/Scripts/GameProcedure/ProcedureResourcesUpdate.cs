using System;
using TEngine.Runtime.HotUpdate;
using UnityEngine;

namespace TEngine.Runtime
{
    /// <summary>
    /// 流程加载器 - 资源下载/资源热更新/资源校验
    /// </summary>
    internal class ProcedureResourcesUpdate : ProcedureBase
    {
        private LoadData _currentData;
        private OnlineVersionInfo _onlineVersionInfo;
        private IFsm<IProcedureManager> _procedureOwner;
        private bool _dllLoad = false;

        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);

            GameEvent.AddEventListener<bool>(LoadMgr.DownLoadFinish,AllDownLoaded);

            _procedureOwner = procedureOwner;

            _currentData = procedureOwner.GetData<LoadData>("LoadData");
            _onlineVersionInfo = procedureOwner.GetData<OnlineVersionInfo>("OnlineVersionInfo");

            _currentData.List = LoadMgr.Instance.CheckLocalExitResource(_currentData.List);

            if (_currentData.List.Count <= 0)
            {
                LoadMgr.Instance.DownLoadCallBack((int)DownLoadResult.AllDownLoaded, GameStatus.AssetLoad, _currentData.All);
            }
            else
            {
                ShowUpdateType(_currentData);
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
                LoaderUtilities.ShowMessageBox(LoadText.Instance.Label_Load_Package, MessageShowType.OneButton,
                    LoadStyle.StyleEnum.Style_DownLoadApk,
                    () =>
                    {
                        LoadMgr.Instance.StartUpdate(data.List);
                    });
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
                                LoadMgr.Instance.StartUpdate(data.List);
                            }, () =>
                            {
                                StartGame();
                            });
                    }
                    //不提示
                    else if (data.Notice == UpdateNotice.NoNotice)
                    {
                        LoadMgr.Instance.StartUpdate(data.List);
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
                            () =>
                            {
                                LoadMgr.Instance.StartUpdate(data.List);
                            }, () =>
                            {
                                StartGame();
                            });
                    }
                    //不提示
                    else if (data.Notice == UpdateNotice.NoNotice)
                    {
                        LoadMgr.Instance.StartUpdate(data.List);
                    }
                }
                else
                    TLogger.LogError("LoadMgr._CheckUpdate, style is error,code:" + data.Style);
                return true;
            }
            //没有更新
            return false;
        }

        private void StartGame()
        {
            ChangeState<ProcedureStartGame>(_procedureOwner);
        }

        /// <summary>
        /// 全部下载完成回调
        /// </summary>
        /// <param name="result"></param>
        private void AllDownLoaded(bool result)
        {
            if (result)
            {
                try
                {
                    if (string.IsNullOrEmpty(_onlineVersionInfo.ResourceVersion.ToString()))
                    {
                        _onlineVersionInfo.ResourceVersion = int.Parse(GameConfig.Instance.ResId);
                    }
                    GameConfig.Instance.WriteResVersion(_onlineVersionInfo.ResourceVersion.ToString());
                    UILoadMgr.Show(UIDefine.UILoadUpdate, "所有资源下载完成，稍后进入游戏！");
                    LoaderUtilities.DelayFun((() =>
                    {
                        UILoadMgr.HideAll();
                    }), new WaitForSeconds(2));

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
                            Application.Quit();
                        });
                }
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

                LoaderUtilities.ShowMessageBox(LoadText.Instance.Label_Load_UnPackError, MessageShowType.OneButton,
                    LoadStyle.StyleEnum.Style_QuitApp,
                    () => {
                        Application.Quit();
                    });
            }
        }
    }
}
