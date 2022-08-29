namespace TEngine.Runtime
{
    public static partial class TEngineEvent
    {
        public static readonly int OnStartGame = StringId.StringToHash("TEngineEvent.OnStartGame");
    }
    
    /// <summary>
    /// 流程加载器 - 终点StartGame
    /// </summary>
    public class ProcedureStartGame : ProcedureBase
    {
        protected internal override void OnInit(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnInit(procedureOwner);
        }

        protected internal override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            GameEventMgr.Instance.Send(TEngineEvent.OnStartGame);
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