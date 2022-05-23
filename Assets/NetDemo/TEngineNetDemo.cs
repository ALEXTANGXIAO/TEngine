using TEngineCore.Net;

namespace TEngineCore
{
    public class TEngineNetDemo : TEngineCore.TEngine
    {
        protected override void RegisterAllSystem()
        {
            base.RegisterAllSystem();
            AddLogicSys(UISys.Instance);
            AddLogicSys(DataCenterSys.Instance);
        }

        protected override void StartGame()
        {
            UISys.Mgr.ShowWindow<TEngineLoginUI>();

            GameClient.Instance.Connect("127.0.0.1", 54809,true);
        }
    }
}
