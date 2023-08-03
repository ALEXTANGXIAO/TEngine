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
    public class Scene : Entity, INotSupportedPool
    {
        public string Name { get; private set; }
        public uint LocationId { get; private set; }
#if TENGINE_UNITY
        public Session Session { get; private set; }
        public SceneConfigInfo SceneInfo { get; private set; }
#endif
#if TENGINE_NET
        public int SceneType { get; private set; }
        public int SceneSubType { get; private set; }
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
            SceneType = 0;
            SceneSubType = 0;
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
        public static Scene Create()
        {
            var sceneId = IdFactory.NextRunTimeId();
            var scene = Create<Scene>(sceneId, sceneId);
            scene.Scene = scene;
            scene.Parent = scene;
            Scenes.Add(scene);
            return scene;
        }
        
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
        /// 创建一个Scene。
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="sceneType"></param>
        /// <param name="sceneSubType"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static async FTask<T> Create<T>(Scene scene, int sceneType, int sceneSubType) where T : Scene, new()
        {
            var newScene = Create<T>(scene);
            newScene.Scene = newScene;
            newScene.Parent = scene;
            newScene.SceneType = sceneType;
            newScene.SceneSubType = sceneSubType;
            newScene.Server = scene.Server;
            newScene.LocationId = scene.Server.Id;
            
            if (scene.World != null)
            {
                newScene.World = scene.World;
            }
            
            if (sceneType > 0)
            {
                await EventSystem.Instance.PublishAsync(new OnCreateScene(newScene));
            }
            
            Scenes.Add(newScene);
            return newScene;
        }
        
        /// <summary>
        /// 创建一个Scene。
        /// </summary>
        /// <param name="server"></param>
        /// <param name="sceneType"></param>
        /// <param name="sceneSubType"></param>
        /// <param name="sceneId"></param>
        /// <param name="worldId"></param>
        /// <param name="networkProtocol"></param>
        /// <param name="outerBindIp"></param>
        /// <param name="outerPort"></param>
        /// <returns></returns>
        public static async FTask<Scene> Create(Server server, int sceneType = 0, int sceneSubType = 0, long sceneId = 0, uint worldId = 0, string networkProtocol = null, string outerBindIp = null, int outerPort = 0)
        {
            if (sceneId == 0)
            {
                sceneId = new EntityIdStruct(server.Id, 0, 0);
            }
            
            var scene = Create<Scene>(sceneId, sceneId);
            scene.Scene = scene;
            scene.Parent = scene;
            scene.SceneType = sceneType;
            scene.SceneSubType = sceneSubType;
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

            if (sceneType > 0)
            {
                await EventSystem.Instance.PublishAsync(new OnCreateScene(scene));
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
