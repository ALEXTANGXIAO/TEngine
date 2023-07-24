#if TENGINE_NET
namespace TEngine;

/// <summary>
/// Entity保存到数据库的时候会根据子组件设置分离存储特性分表存储在不同的集合表中
/// </summary>
public interface ISingleCollectionRoot { }
#endif