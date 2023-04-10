namespace TEngine
{
    /// <summary>
    /// UI行为接口。
    /// </summary>
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