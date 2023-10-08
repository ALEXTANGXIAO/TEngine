namespace TEngine
{
    /// <summary>
    /// UI行为接口。
    /// </summary>
    public interface IUIBehaviour
    {
        /// <summary>
        /// 自动生成代码绑定。
        /// </summary>
        void ScriptGenerator();
        
        /// <summary>
        /// 绑定自定义UI元素。
        /// </summary>
        void BindMemberProperty();
        
        /// <summary>
        /// 注册UI事件。
        /// <remarks>方法=>AddUIEvent 在面板Destroy的时候会自动移除监听。</remarks>
        /// </summary>
        void RegisterEvent();
        
        /// <summary>
        /// 面板创建成功。
        /// </summary>
        void OnCreate();
        
        /// <summary>
        /// 面板刷新。
        /// </summary>
        void OnRefresh();
        
        /// <summary>
        /// 面板轮询。
        /// </summary>
        void OnUpdate();
        
        /// <summary>
        /// 面板销毁。
        /// </summary>
        void OnDestroy();
    }
}