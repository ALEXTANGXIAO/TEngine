namespace TEngine
{
    /// <summary>
    /// 模块化基础组件。
    /// </summary>
    /// <typeparam name="T">游戏框架模块抽象类。</typeparam>
    public abstract class Module<T> where T : GameFrameworkModuleBase
    {
        private static T _instance;

        public static T Instance => _instance ??= GameModule.Get<T>();
    }
}