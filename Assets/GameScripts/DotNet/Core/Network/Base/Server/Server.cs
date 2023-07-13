#if TENGINE_NET
using TEngine.Core;
using TEngine.Core.Network;
#pragma warning disable CS8603
#pragma warning disable CS8625
#pragma warning disable CS8618

namespace TEngine
{
    public sealed class Server
    {
        public uint Id { get; private set; }
        public Scene Scene { get; private set; }
        private readonly Dictionary<uint, ConnectInfo> _sessions = new Dictionary<uint, ConnectInfo>();
        
        private sealed class ConnectInfo : IDisposable
        {
            public Session Session;
            public Entity NetworkComponent;

            public ConnectInfo(Session session, Entity networkComponent)
            {
                Session = session;
                NetworkComponent = networkComponent;
            }

            public void Dispose()
            {
                if (Session != null)
                {
                    Session.Dispose();
                    Session = null;
                }
                
                if (NetworkComponent != null)
                {
                    NetworkComponent.Dispose();
                    NetworkComponent = null;
                }
            }
        }
        
        public Session GetSession(uint targetServerId)
        {
            if (_sessions.TryGetValue(targetServerId, out var connectInfo))
            {
                if (!connectInfo.Session.IsDisposed)
                {
                    return connectInfo.Session;
                }
                
                _sessions.Remove(targetServerId);
            }
            
            // 同一个Server下、只需要内部走下消息派发就可以

            if (Id == targetServerId)
            {
                var serverNetworkComponent = Scene.GetComponent<ServerNetworkComponent>();
                var session = Session.Create(serverNetworkComponent.Network);
                _sessions.Add(targetServerId, new ConnectInfo(session, null));
                return session;
            }
            
            // 不同的Server下需要建立网络连接

            var serverConfigInfo = ConfigTableManage.ServerConfig(targetServerId);

            if (serverConfigInfo == null)
            {
                throw new Exception($"The server with ServerId {targetServerId} was not found in the configuration file");
            }

            var machineConfigInfo = ConfigTableManage.MachineConfig(serverConfigInfo.MachineId);

            if (machineConfigInfo == null)
            {
                throw new Exception($"Server.cs The specified MachineId was not found: {serverConfigInfo.MachineId}");
            }

            var ipEndPoint = NetworkHelper.ToIPEndPoint($"{machineConfigInfo.InnerBindIP}:{serverConfigInfo.InnerPort}");
            var clientNetworkComponent = Entity.Create<ClientNetworkComponent>(Scene);
            clientNetworkComponent.Initialize(NetworkProtocolType.KCP, NetworkTarget.Inner);
            clientNetworkComponent.Connect(ipEndPoint,null, () =>
            {
                Log.Error($"Unable to connect to the target server sourceServerId:{Id} targetServerId:{targetServerId}");
            });
            _sessions.Add(targetServerId, new ConnectInfo(clientNetworkComponent.Session, clientNetworkComponent));
            return clientNetworkComponent.Session;
        }
        
        public void RemoveSession(uint targetServerId)
        {
            if (!_sessions.Remove(targetServerId, out var connectInfo))
            {
                return;
            }
            
            connectInfo.Dispose();
        }

        #region Static

        private static readonly Dictionary<uint, Server> Servers = new Dictionary<uint, Server>();
        
        public static async FTask Create(uint routeId)
        {
            var serverConfigInfo = ConfigTableManage.ServerConfig(routeId);
            
            if (serverConfigInfo == null)
            {
                Log.Error($"not found server by Id:{routeId}");
                return;
            }

            var machineConfigInfo = ConfigTableManage.MachineConfig(serverConfigInfo.MachineId);

            if (machineConfigInfo == null)
            {
                Log.Error($"not found machine by Id:{serverConfigInfo.MachineId}");
                return;
            }
            
            var sceneInfos = Scene.GetSceneInfoByRouteId(routeId);
            await Create(routeId, machineConfigInfo.InnerBindIP, serverConfigInfo.InnerPort, machineConfigInfo.OuterBindIP, sceneInfos);
            // Log.Info($"ServerId:{routeId} is start complete");
        }

        public static async FTask<Server> Create(uint routeId, string innerBindIp, int innerPort, string outerBindIp, List<SceneConfigInfo> sceneInfos)
        {
            if (Servers.TryGetValue(routeId, out var server))
            {
                return server;
            }

            // 创建一个新的Server、创建一个临时Scene给Server当做Scene使用

            server = new Server
            {
                Id = routeId
            };

            server.Scene = await Scene.Create($"ServerScene{routeId}", server, new EntityIdStruct(routeId, 0, 0));

            // 创建网络、Server下的网络只能是内部网络、外部网络是在Scene中定义
            
            if (!string.IsNullOrEmpty(innerBindIp) && innerPort != 0)
            {
                var address = NetworkHelper.ToIPEndPoint(innerBindIp, innerPort);
                var serverNetworkComponent = server.Scene.AddComponent<ServerNetworkComponent>();
                serverNetworkComponent.Initialize(NetworkProtocolType.KCP, NetworkTarget.Inner, address);
            }

            // 创建Server拥有所有的Scene
            
            foreach (var sceneConfig in sceneInfos)
            {
                await Scene.Create(server, outerBindIp, sceneConfig);
            }

            Servers.Add(routeId, server);
            return server;
        }
        
        public static Server Get(uint routeId)
        {
            return Servers.TryGetValue(routeId, out var server) ? server : null;
        }

        #endregion
    }
}
#endif