using UnityEngine;

namespace TEngine.Runtime
{
    /// <summary>
    /// 流程加载器 - 资源初始化
    /// </summary>
    public class ProcedureResourcesInit : ProcedureBase
    {
        private bool m_initResourceComplete = false;

        protected internal override void OnInit(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnInit(procedureOwner);
        }

        protected internal override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {

            base.OnEnter(procedureOwner);
            GameEventMgr.Instance.AddEventListener("OnInitResourceComplete", OnInitResourceComplete);
            m_initResourceComplete = false;
            LoaderUtilities.DelayFun((() =>
            {
                GameEventMgr.Instance.Send("OnInitResourceComplete");
            }),new WaitForSeconds(1f));
        }

        protected internal override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            if (m_initResourceComplete)
            {
                ChangeState<ProcedureCodeInit>(procedureOwner);
            }
        }


        protected internal override void OnLeave(IFsm<IProcedureManager> procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);
            GameEventMgr.Instance.RemoveEventListener("OnInitResourceComplete", OnInitResourceComplete);
        }

        protected internal override void OnDestroy(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnDestroy(procedureOwner);
        }

        private void OnInitResourceComplete()
        {
            m_initResourceComplete = true;
            Log.Info("OnInitResourceComplete 初始化资源完成");
        }
    }
}