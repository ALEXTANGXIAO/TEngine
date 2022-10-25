
namespace TEngine.Runtime
{
    /// <summary>
    /// 流程加载器 - 资源校验（暂无）
    /// </summary>
    internal class ProcedureResourcesVerify:ProcedureBase
    {
        private bool m_VerifyResourcesComplete = false;

        protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            if (!m_VerifyResourcesComplete)
            {
                return;
            }

            ChangeState<ProcedureCodeInit>(procedureOwner);
        }

        private void OnVerifyResourcesComplete(bool result)
        {
            m_VerifyResourcesComplete = true;
            Log.Info("Verify resources complete, result is '{0}'.", result);
        }
    }
}
