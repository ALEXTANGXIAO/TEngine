
namespace TEngine.Runtime
{
    /// <summary>
    /// 流程加载器 - 资源校验（暂无）
    /// </summary>
    internal class ProcedureResourcesVerify:ProcedureBase
    {
        private bool m_VerifyResourcesComplete = false;

        protected internal override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
        }

        protected internal override void OnLeave(IFsm<IProcedureManager> procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);
        }

        protected internal override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds, float realElapseSeconds)
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
