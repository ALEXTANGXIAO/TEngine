using UnityEngine;

namespace TEngine.Runtime
{
    /// <summary>
    /// 流程加载器 - 资源初始化
    /// </summary>
    public class ProcedureResourcesInit : ProcedureBase
    {
        public static int OnInitResourceCompleteEvent = StringId.StringToHash("OnInitResourceComplete");
        
        private bool m_initResourceComplete = false;

        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {

            base.OnEnter(procedureOwner);
            GameEvent.AddEventListener(OnInitResourceCompleteEvent, OnInitResourceComplete);
            m_initResourceComplete = false;
            LoaderUtilities.DelayFun((() =>
            {
                GameEvent.Send(OnInitResourceCompleteEvent);
            }),new WaitForSeconds(1f));
        }

        protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            if (m_initResourceComplete)
            {
                ChangeState<ProcedureCodeInit>(procedureOwner);
            }
        }


        protected override void OnLeave(IFsm<IProcedureManager> procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);
            GameEvent.RemoveEventListener(OnInitResourceCompleteEvent, OnInitResourceComplete);
        }

        private void OnInitResourceComplete()
        {
            m_initResourceComplete = true;
            Log.Info("OnInitResourceComplete 初始化资源完成");
        }
    }
}