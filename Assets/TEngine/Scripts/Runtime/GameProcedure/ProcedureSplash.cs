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
        protected internal override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds,
            float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            // TODO: 这里可以播放一个 Splash 动画
            // ...
            if (ResourceComponent.Instance.ResourceMode == ResourceMode.Package)
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
    }
}