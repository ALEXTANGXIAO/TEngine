namespace TEngine.EntityModule
{
    /// <summary>
    /// Entity构架可以将此组件从Entity上移除这个组件并丢入对象池，给其他此刻需要此组件的Entity使用，
    /// 因此可以节省大量的内存反复创建和释放， 这也是Entity的特性可以大量重复使用组件
    /// </summary>
    public class EntityComponent : EcsObject
    {
        public Entity Entity { get; set; }

        #region Static
        public static T Create<T>() where T : EntityComponent, new()
        {
            var entity = EntitySystem.Instance.CreateComponent<T>();
            return entity;
        }
        #endregion
    }
}