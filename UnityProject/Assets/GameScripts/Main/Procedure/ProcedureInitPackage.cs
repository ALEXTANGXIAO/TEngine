using System;
using Cysharp.Threading.Tasks;
using TEngine;
using UnityEngine;
using YooAsset;
using ProcedureOwner = TEngine.IFsm<TEngine.IProcedureManager>;
using Utility = TEngine.Utility;

namespace GameMain
{
    /// <summary>
    /// 流程 => 初始化Package。
    /// </summary>
    public class ProcedureInitPackage : ProcedureBase
    {
        public override bool UseNativeDialog { get; }

        private ProcedureOwner _procedureOwner;

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);

            _procedureOwner = procedureOwner;

            //Fire Forget立刻触发UniTask初始化Package
            InitPackage(procedureOwner).Forget();
        }

        private async UniTaskVoid InitPackage(ProcedureOwner procedureOwner)
        {
            try
            {
                if (GameModule.Resource.PlayMode == EPlayMode.HostPlayMode ||
                    GameModule.Resource.PlayMode == EPlayMode.WebPlayMode)
                {
                    if (SettingsUtils.EnableUpdateData())
                    {
                        UpdateData updateData = await RequestUpdateData();

                        if (updateData != null)
                        {
                            if (!string.IsNullOrEmpty(updateData.HostServerURL))
                            {
                                SettingsUtils.FrameworkGlobalSettings.HostServerURL = updateData.HostServerURL;
                            }

                            if (!string.IsNullOrEmpty(updateData.FallbackHostServerURL))
                            {
                                SettingsUtils.FrameworkGlobalSettings.FallbackHostServerURL =
                                    updateData.FallbackHostServerURL;
                            }
                        }
                    }
                }

                var initializationOperation = await GameModule.Resource.InitPackage();

                if (initializationOperation.Status == EOperationStatus.Succeed)
                {
                    //热更新阶段文本初始化
                    LoadText.Instance.InitConfigData(null);

                    GameEvent.Send(RuntimeId.ToRuntimeId("RefreshVersion"));

                    EPlayMode playMode = GameModule.Resource.PlayMode;

                    // 编辑器模式。
                    if (playMode == EPlayMode.EditorSimulateMode)
                    {
                        Log.Info("Editor resource mode detected.");
                        ChangeState<ProcedurePreload>(procedureOwner);
                    }
                    // 单机模式。
                    else if (playMode == EPlayMode.OfflinePlayMode)
                    {
                        Log.Info("Package resource mode detected.");
                        ChangeState<ProcedureInitResources>(procedureOwner);
                    }
                    // 可更新模式。
                    else if (playMode == EPlayMode.HostPlayMode ||
                             playMode == EPlayMode.WebPlayMode)
                    {
                        // 打开启动UI。
                        UILoadMgr.Show(UIDefine.UILoadUpdate);

                        Log.Info("Updatable resource mode detected.");
                        ChangeState<ProcedureUpdateVersion>(procedureOwner);
                    }
                    else
                    {
                        Log.Error("UnKnow resource mode detected Please check???");
                    }
                }
                else
                {
                    // 打开启动UI。
                    UILoadMgr.Show(UIDefine.UILoadUpdate);

                    Log.Error($"{initializationOperation.Error}");

                    // 打开启动UI。
                    UILoadMgr.Show(UIDefine.UILoadUpdate, $"资源初始化失败！");

                    UILoadTip.ShowMessageBox(
                        $"资源初始化失败！点击确认重试 \n \n <color=#FF0000>原因{initializationOperation.Error}</color>",
                        MessageShowType.TwoButton,
                        LoadStyle.StyleEnum.Style_Retry
                        , () => { Retry(procedureOwner); }, UnityEngine.Application.Quit);
                }
            }
            catch (Exception e)
            {
                OnInitPackageFailed(procedureOwner, e.Message);
            }
        }

        private void OnInitPackageFailed(ProcedureOwner procedureOwner, string message)
        {
            // 打开启动UI。
            UILoadMgr.Show(UIDefine.UILoadUpdate);

            Log.Error($"{message}");

            // 打开启动UI。
            UILoadMgr.Show(UIDefine.UILoadUpdate, $"资源初始化失败！");

            UILoadTip.ShowMessageBox($"资源初始化失败！点击确认重试 \n \n <color=#FF0000>原因{message}</color>", MessageShowType.TwoButton,
                LoadStyle.StyleEnum.Style_Retry
                , () => { Retry(procedureOwner); },
                Application.Quit);
        }

        private void Retry(ProcedureOwner procedureOwner)
        {
            // 打开启动UI。
            UILoadMgr.Show(UIDefine.UILoadUpdate, $"重新初始化资源中...");

            InitPackage(procedureOwner).Forget();
        }

        /// <summary>
        /// 请求更新配置数据。
        /// </summary>
        private async UniTask<UpdateData> RequestUpdateData()
        {
            // 打开启动UI。
            UILoadMgr.Show(UIDefine.UILoadUpdate);

            var checkVersionUrl = SettingsUtils.GetUpdateDataUrl();

            if (string.IsNullOrEmpty(checkVersionUrl))
            {
                Log.Error("LoadMgr.RequestVersion, remote url is empty or null");
                return null;
            }

            Log.Info("RequestUpdateData, proxy:" + checkVersionUrl);
            try
            {
                var updateDataStr = await Utility.Http.Get(checkVersionUrl);
                UpdateData updateData = Utility.Json.ToObject<UpdateData>(updateDataStr);
                return updateData;
            }
            catch (Exception e)
            {
                // 打开启动UI。
                UILoadTip.ShowMessageBox("请求配置数据失败！点击确认重试", MessageShowType.TwoButton,
                    LoadStyle.StyleEnum.Style_Retry
                    , () => { InitPackage(_procedureOwner).Forget(); }, Application.Quit);
                Log.Warning(e);
                return null;
            }
        }
    }
}