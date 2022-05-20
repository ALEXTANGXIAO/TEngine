namespace TEngine
{
    /// <summary>
    /// 热更层由此组件进行更新（ILRuntime不支持多继承, 接口继承）
    /// </summary>
    public class UpdateComponent :ECSComponent, IUpdate
    {
        public virtual void Update()
        {

        }
    }
}