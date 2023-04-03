namespace TEngine
{
    public interface IUIBehaviour
    {
        void ScriptGenerator();
        void RegisterEvent();
        void OnCreate();
        void OnUpdate(float elapseSeconds, float realElapseSeconds);
        void OnClose(bool isShutdown, object userData);
        void OnPause();
        void OnResume();
    }
}