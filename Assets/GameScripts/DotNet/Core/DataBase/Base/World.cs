#if TENGINE_NET
namespace TEngine.Core.DataBase;

public sealed class World
{
    public uint Id { get; private init; }
    public IDateBase DateBase { get; private init; }
    public WorldConfigInfo Config => ConfigTableManage.WorldConfigInfo(Id);
    private static readonly Dictionary<uint, World> Worlds = new();

    public World(WorldConfigInfo worldConfigInfo)
    {
        Id = worldConfigInfo.Id;
        var dbType = worldConfigInfo.DbType.ToLower();
        
        switch (dbType)
        {
            case "mongodb":
            {
                DateBase = new MongoDataBase();
                DateBase.Initialize(worldConfigInfo.DbConnection, worldConfigInfo.DbName);
                break;
            }
            default:
                throw new Exception("No supported database");
        }
    }
    
    public static World Create(uint id)
    {
        if (Worlds.TryGetValue(id, out var world))
        {
            return world;
        }

        var worldConfigInfo = ConfigTableManage.WorldConfigInfo(id);
        world = new World(worldConfigInfo);
        Worlds.Add(id, world);
        return world;
    }
}
#endif