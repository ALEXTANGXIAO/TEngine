namespace TEngine.Runtime.UIModule
{
    public partial class UISys : BehaviourSingleton<UISys>
    {
        public static int DesignWidth => 750;

        public static int DesignHeight => 1624;

        public static UIManager Mgr => UIManager.Instance;

        public override void Active()
        {
            base.Active();

            RegisterAllController();
        }

        public override void Update()
        {
            UIManager.Instance.OnUpdate();
        }
    }
}