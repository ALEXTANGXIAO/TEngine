using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEngine.Runtime
{
    /// <summary>
    /// 流程加载器 - 闪屏
    /// </summary>
    public class ProcedureSplash : ProcedureBase
    {
        protected internal override void OnInit(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnInit(procedureOwner);
        }

        protected internal override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
        }

        protected internal override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            // TODO: 这里可以播放一个 Splash 动画
            // ...
            if (ResourceComponent.Instance.EditorResourceMode == true)
            {
                Log.Info("编辑器模式 ChangeState<ProcedureStartGame>");
                ChangeState<ProcedureStartGame>(procedureOwner);
            }
            else if (ResourceComponent.Instance.ResourceMode == ResourceMode.Package)
            {
                Log.Info("单机模式 ChangeState<ProcedureInitResources>");
                ChangeState<ProcedureResourcesInit>(procedureOwner);
            }
            else
            {
                Log.Info("可更新模式 ChangeState<ProcedureCheckVersion>");
                ChangeState<ProcedureCheckVersion>(procedureOwner);
            }
        }

        protected internal override void OnLeave(IFsm<IProcedureManager> procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);
        }

        protected internal override void OnDestroy(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnDestroy(procedureOwner);
        }
    }
}
