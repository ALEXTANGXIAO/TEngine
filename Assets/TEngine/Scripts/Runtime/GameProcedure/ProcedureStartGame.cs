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
        protected internal override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            GameEvent.Send(TEngineEvent.OnStartGame);
        }
    }
}