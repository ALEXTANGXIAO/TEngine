#if TENGINE_NET
using TEngine.Core;
using TEngine.Core.DataBase;
#pragma warning disable CS8603

namespace TEngine.Logic;

public static class ConfigTableSystem
{
    public static void Bind()
    {
        LoadConfigAsync();
        
        // 框架需要一些的配置文件来启动服务器和创建网络服务所以需要ServerConfig.xlsx和MachineConfig.xlsx的配置
        // 由于配置表的代码是生成在框架外面的、框架没办法直接获取到配置文件
        // 考虑到这两个配置文件开发者可能会修改结构、所以提供了一个委托来让开发者开自己定义如何获取框架需要的东西
        // 本来想提供一个接口让玩家把ServerConfig和MachineConfig添加到框架中、但这样就不支持热更
        // 提供委托方式就可以支持配置表热更。因为配置表读取本就是支持热更的
        // 虽然这块稍微麻烦点、但好在配置一次以后基本不会改动、后面有更好的办法我会把这个给去掉
        ConfigTableManage.ServerConfig = serverId =>
        {
            if (!ServerConfigData.Instance.TryGet(serverId, out var serverConfig))
            {
                return null;
            }
        
            return new ServerConfigInfo()
            {
                Id = serverConfig.Id,
                InnerPort = serverConfig.InnerPort,
                MachineId = serverConfig.MachineId
            };
        };
        ConfigTableManage.MachineConfig = machineId =>
        {
            if (!MachineConfigData.Instance.TryGet(machineId, out var machineConfig))
            {
                return null;
            }
        
            return new MachineConfigInfo()
            {
                Id = machineConfig.Id,
                OuterIP = machineConfig.OuterIP,
                OuterBindIP = machineConfig.OuterBindIP,
                InnerBindIP = machineConfig.InnerBindIP,
                ManagementPort = machineConfig.ManagementPort
            };
        };
        ConfigTableManage.WorldConfigInfo = worldId =>
        {
            if (!WorldConfigData.Instance.TryGet(worldId, out var worldConfig))
            {
                return null;
            }
        
            return new WorldConfigInfo()
            {
                Id = worldConfig.Id,
                WorldName = worldConfig.WorldName,
                DbConnection = worldConfig.DbConnection,
                DbName = worldConfig.DbName,
                DbType = worldConfig.DbType
            };
        };
        ConfigTableManage.SceneConfig = sceneId =>
        {
            if (!SceneConfigData.Instance.TryGet(sceneId, out var sceneConfig))
            {
                return null;
            }
        
            return new SceneConfigInfo()
            {
                Id = sceneConfig.Id,
                SceneType = SceneType.SceneTypeDic[sceneConfig.SceneType],
                SceneSubType = SceneSubType.SceneSubTypeDic[sceneConfig.SceneSubType],
                SceneTypeStr = sceneConfig.SceneType,
                SceneSubTypeStr = sceneConfig.SceneSubType,
                NetworkProtocol = sceneConfig.NetworkProtocol,
                ServerConfigId = sceneConfig.ServerConfigId,
                WorldId = sceneConfig.WorldId,
                OuterPort = sceneConfig.OuterPort
            };
        };
        ConfigTableManage.AllServerConfig = () =>
        {
            var list = new List<ServerConfigInfo>();
        
            foreach (var serverConfig in ServerConfigData.Instance.List)
            {
                list.Add(new ServerConfigInfo()
                {
                    Id = serverConfig.Id,
                    InnerPort = serverConfig.InnerPort,
                    MachineId = serverConfig.MachineId
                });
            }
        
            return list;
        };
        ConfigTableManage.AllMachineConfig = () =>
        {
            var list = new List<MachineConfigInfo>();
        
            foreach (var machineConfig in MachineConfigData.Instance.List)
            {
                list.Add(new MachineConfigInfo()
                {
                    Id = machineConfig.Id,
                    OuterIP = machineConfig.OuterIP,
                    OuterBindIP = machineConfig.OuterBindIP,
                    InnerBindIP = machineConfig.InnerBindIP,
                    ManagementPort = machineConfig.ManagementPort
                });
            }
        
            return list;
        };
        ConfigTableManage.AllSceneConfig = () =>
        {
            var list = new List<SceneConfigInfo>();
        
            foreach (var sceneConfig in SceneConfigData.Instance.List)
            {
                list.Add(new SceneConfigInfo()
                {
                    Id = sceneConfig.Id,
                    EntityId = sceneConfig.EntityId,
                    SceneType = SceneType.SceneTypeDic[sceneConfig.SceneType],
                    SceneSubType = SceneSubType.SceneSubTypeDic[sceneConfig.SceneSubType],
                    SceneTypeStr = sceneConfig.SceneType,
                    SceneSubTypeStr = sceneConfig.SceneSubType,
                    NetworkProtocol = sceneConfig.NetworkProtocol,
                    ServerConfigId = sceneConfig.ServerConfigId,
                    WorldId = sceneConfig.WorldId,
                    OuterPort = sceneConfig.OuterPort
                });
            }
        
            return list;
        };
    }

    public static async Task LoadConfigAsync()
    {
        await ConfigLoader.Instance.LoadAsync();
    }
}
#endif