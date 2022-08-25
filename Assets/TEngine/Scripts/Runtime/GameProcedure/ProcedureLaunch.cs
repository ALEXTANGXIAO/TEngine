namespace TEngine.Runtime
{
    /// <summary>
    /// 流程加载器 - 开始起点
    /// </summary>
    public class ProcedureLaunch : ProcedureBase
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
            ChangeState<ProcedureSplash>(procedureOwner);
        }
    }
}