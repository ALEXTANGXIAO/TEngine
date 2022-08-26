namespace TEngine.Runtime.UIModule
{
    public partial class UISys:BehaviourSingleton<UISys>
    {
        public static int DesginWidth => 750;

        public static int DesginHeight => 1624;
      
        public static UIManager Mgr
        {
            get { return UIManager.Instance; }
        }
        
        public override void Active()
        {
            base.Active();
            
            RegisterAllController();
        }

        public override void Update()
        {
            base.Update();
            UIManager.Instance.OnUpdate();
        }
    }
}