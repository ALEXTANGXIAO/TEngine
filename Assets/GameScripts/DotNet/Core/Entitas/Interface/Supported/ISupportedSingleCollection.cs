#if TENGINE_NET
namespace TEngine;

/// <summary>
/// Entity是单一集合、保存到数据库的时候不会跟随父组件保存在一个集合里、会单独保存在一个集合里
/// 需要配合SingleCollectionAttribute一起使用、如在Entity类头部定义SingleCollectionAttribute(typeOf(Unit))
/// SingleCollectionAttribute用来定义这个Entity是属于哪个Entity的子集
/// </summary>
public interface ISupportedSingleCollection { }

[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public class SingleCollectionAttribute : Attribute
{
    public readonly Type RootType;
    public readonly string CollectionName;

    public SingleCollectionAttribute(Type rootType, string collectionName)
    {
        RootType = rootType;
        CollectionName = collectionName;
    }
}
#endif