using System;
using System.Collections.Generic;
using TEngine.Core.Network;
using TEngine.Core;
#if TENGINE_NET
using TEngine.Core.DataBase;
#endif
#pragma warning disable CS8625
#pragma warning disable CS8618
namespace TEngine
{
    public sealed class Scene : Entity, INotSupportedPool
    {
        public string Name { get; private set; }
        public uint LocationId { get; private set; }
#if TENGINE_UNITY
        public Session Session { get; private set; }
        public SceneConfigInfo SceneInfo { get; private set; }
#endif
#if TENGINE_NET
        public string SceneType { get; private set; }
        public World World { get; private set; }
        public Server Server { get; private set; }
#endif
        public static readonly List<Scene> Scenes = new List<Scene>();
        
        public override void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }

            Name = null;
            this.LocationId = 0;
#if TENGINE_NET
            World = null;
            Server = null;
            SceneType = null;
#endif
#if TENGINE_UNITY
            SceneInfo = null;
            if (Session is { IsDisposed: false })
            {
                Session.Dispose();
                Session = null;
            }
#endif
            Scenes.Remove(this);
            base.Dispose();
        }

        public static void DisposeAllScene()
        {
            foreach (var scene in Scenes.ToArray())
            {
                scene.Dispose();
            }
        }
#if TENGINE_UNITY
        public static Scene Create(string name)
        {
            var sceneId = IdFactory.NextRunTimeId();
            var scene = Create<Scene>(sceneId, sceneId);
            scene.Scene = scene;
            scene.Parent = scene;
            scene.Name = name;
            Scenes.Add(scene);
            return scene;
        }
        public void CreateSession(string remoteAddress, NetworkProtocolType networkProtocolType, Action onConnectComplete, Action onConnectFail,Action onConnectDisconnect, int connectTimeout = 5000)
        {
            var address = NetworkHelper.ToIPEndPoint(remoteAddress);
            var clientNetworkComponent = GetComponent<ClientNetworkComponent>() ?? AddComponent<ClientNetworkComponent>();
            clientNetworkComponent.Initialize(networkProtocolType, NetworkTarget.Outer);
            clientNetworkComponent.Connect(address, onConnectComplete, onConnectFail,onConnectDisconnect, connectTimeout);
            Session = clientNetworkComponent.Session;
        }
#else
        /// <summary>
        /// 创建一个Scene、但这个Scene是在某个Scene下面的Scene
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="sceneName"></param>
        /// <param name="sceneType"></param>
        /// <returns></returns>
        public static async FTask<Scene> Create(Scene scene, string sceneType, string sceneName)
        {
            var newScene = Create<Scene>(scene);
            newScene.Scene = scene;
            newScene.Parent = scene;
            newScene.Name = sceneName;
            newScene.SceneType = sceneType;
            newScene.Server = scene.Server;
            newScene.LocationId = scene.Server.Id;
            
            if (scene.World !=null)
            {
                newScene.World = scene.World;
            }
            
            if (!string.IsNullOrEmpty(sceneType))
            {
                await EventSystem.Instance.PublishAsync(new OnCreateScene(scene));
            }
            
            Scenes.Add(scene);
            return scene;
        }
        
        /// <summary>
        /// 创建一个Scene。
        /// </summary>
        /// <param name="server"></param>
        /// <param name="sceneType"></param>
        /// <param name="sceneName"></param>
        /// <param name="sceneId"></param>
        /// <param name="worldId"></param>
        /// <param name="networkProtocol"></param>
        /// <param name="outerBindIp"></param>
        /// <param name="outerPort"></param>
        /// <returns></returns>
        public static async FTask<Scene> Create(Server server, string sceneType, string sceneName, long sceneId =0, uint worldId =0, string networkProtocol = null, string outerBindIp = null, int outerPort = 0)
        {
            if (sceneId == 0)
            {
                sceneId = new EntityIdStruct(server.Id, 0, 0);
            }
            
            var scene = Create<Scene>(sceneId, sceneId);
            scene.Scene = scene;
            scene.Parent = scene;
            scene.Name = sceneName;
            scene.SceneType = sceneType;
            scene.Server = server;
            scene.LocationId = server.Id;

            if (worldId != 0)
            {
                // 有可能不需要数据库、所以这里默认0的情况下就不创建数据库了
                scene.World = World.Create(worldId);
            }

            if (!string.IsNullOrEmpty(networkProtocol) && !string.IsNullOrEmpty(outerBindIp) && outerPort != 0)
            {
                // 设置Scene的网络、目前只支持KCP和TCP
                var networkProtocolType = Enum.Parse<NetworkProtocolType>(networkProtocol);
                var serverNetworkComponent = scene.AddComponent<ServerNetworkComponent>();
                var address = NetworkHelper.ToIPEndPoint($"{outerBindIp}:{outerPort}");
                serverNetworkComponent.Initialize(networkProtocolType, NetworkTarget.Outer, address);
            }

            if (!string.IsNullOrEmpty(sceneType))
            {
                switch (sceneType)
                {
                    case "Addressable":
                    {
                        scene.AddComponent<AddressableManageComponent>();
                        break;
                    }
                    default:
                    {
                        // 没有SceneType目前只有代码创建的Scene才会这样、目前只有Server的Scene是这样
                        await EventSystem.Instance.PublishAsync(new OnCreateScene(scene));
                        break;
                    }
                }
            }

            Scenes.Add(scene);
            return scene;
        }

        public static List<SceneConfigInfo> GetSceneInfoByServerConfigId(uint serverConfigId)
        {
            var list = new List<SceneConfigInfo>();
            var allSceneConfig = ConfigTableManage.AllSceneConfig();

            foreach (var sceneConfigInfo in allSceneConfig)
            {
                if (sceneConfigInfo.ServerConfigId != serverConfigId)
                {
                    continue;
                }
                
                list.Add(sceneConfigInfo);
            }

            return list;
        }
#endif
    }
}
