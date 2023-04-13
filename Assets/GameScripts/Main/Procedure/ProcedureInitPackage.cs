using System;
using Cysharp.Threading.Tasks;
using TEngine;
using YooAsset;
using ProcedureOwner = TEngine.IFsm<TEngine.IProcedureManager>;

namespace GameMain
{
    /// <summary>
    /// 流程 => 初始化Package。
    /// </summary>
    public class ProcedureInitPackage : ProcedureBase
    {
        public override bool UseNativeDialog { get; }

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);
            
            //Fire Forget立刻触发UniTask初始化Package
            InitPackage(procedureOwner).Forget();
        }

        private async UniTaskVoid InitPackage(ProcedureOwner procedureOwner)
        {
            var initializationOperation = GameModule.Resource.InitPackage();

            await UniTask.Delay(TimeSpan.FromSeconds(1f));

            await initializationOperation.ToUniTask();

            if (initializationOperation.Status == EOperationStatus.Succeed)
            {
                // 编辑器模式。
                if (GameModule.Resource.playMode == EPlayMode.EditorSimulateMode)
                {
                    Log.Info("Editor resource mode detected.");
                    ChangeState<ProcedurePreload>(procedureOwner);
                }
                // 单机模式。
                else if (GameModule.Resource.playMode == EPlayMode.OfflinePlayMode)
                {
                    Log.Info("Package resource mode detected.");
                    ChangeState<ProcedureInitResources>(procedureOwner);
                }
                // 可更新模式。
                else if (GameModule.Resource.playMode == EPlayMode.HostPlayMode)
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

                UILoadTip.ShowMessageBox($"资源初始化失败！点击确认重试 \n \n <color=#FF0000>原因{initializationOperation.Error}</color>", MessageShowType.TwoButton,
                    LoadStyle.StyleEnum.Style_Retry
                    , () => { Retry(procedureOwner); }, UnityEngine.Application.Quit);
            }
        }

        private void Retry(ProcedureOwner procedureOwner)
        {
            // 打开启动UI。
            UILoadMgr.Show(UIDefine.UILoadUpdate, $"重新初始化资源中...");

            InitPackage(procedureOwner).Forget();
        }
    }
}