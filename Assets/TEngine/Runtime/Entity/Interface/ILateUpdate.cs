namespace TEngine.EntityModule
{
    /// <summary>
    /// Entity组件物理更新接口（减少组件for循环开销）
    /// </summary>
    public interface ILateUpdate
    {
        void LateUpdate();
    }
}