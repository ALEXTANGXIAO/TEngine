namespace TEngineCore
{
    /// <summary>
    /// ECS组件更新接口（减少组件for循环开销）
    /// </summary>
    public interface IFixedUpdate
    {
        void FixedUpdate();
    }
}