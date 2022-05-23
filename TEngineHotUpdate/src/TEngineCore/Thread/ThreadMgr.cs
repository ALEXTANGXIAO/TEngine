namespace TEngineCore
{
    public class ThreadMgr : UnitySingleton<ThreadMgr>
    {
        protected override void OnLoad()
        {
            base.OnLoad();
            StartThread();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            ShutDownThread();
        }

        private void StartThread()
        {
            
        }

        private void ShutDownThread()
        {
            
        }
    }
}