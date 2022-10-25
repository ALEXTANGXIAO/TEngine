namespace TEngine.Runtime
{
    /// <summary>
    /// 流程加载器 - 开始起点
    /// </summary>
    public class ProcedureLaunch : ProcedureBase
    {

        protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);
            ChangeState<ProcedureSplash>(procedureOwner);
        }
    }
}