namespace TEngine
{
    public interface IUIBehaviour
    {
        void ScriptGenerator();
        void BindMemberProperty();
        void RegisterEvent();
        void OnCreate();
        void OnRefresh();
        void OnUpdate();
        void OnDestroy();
    }
}