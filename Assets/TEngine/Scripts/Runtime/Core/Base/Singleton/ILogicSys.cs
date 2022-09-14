namespace TEngine.Runtime
{
    public interface ILogicSys
    {
        bool OnInit();

        void OnDestroy();

        void OnStart();

        void OnUpdate();

        void OnLateUpdate();

        void OnPause();

        void OnResume();

        void OnDrawGizmos();
    }

}