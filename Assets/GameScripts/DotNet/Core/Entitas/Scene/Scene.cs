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
        public uint RouteId { get; private set; }
#if TENGINE_UNITY
        public Session Session { get; private set; }
        public SceneConfigInfo SceneInfo { get; private set; }
#endif
#if TENGINE_NET
        public World World { get; private set; }
        public Server Server { get; private set; }
        public uint SceneConfigId { get; private set; }
        public SceneConfigInfo SceneInfo => ConfigTableManage.SceneConfig(SceneConfigId);
#endif
        public static readonly List<Scene> Scenes = new List<Scene>();
        
        public override void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }

            Name = null;
            RouteId = 0;
#if TENGINE_NET
            World = null;
            Server = null;
            SceneConfigId = 0;
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
            var runTimeId = IdFactory.NextRunTimeId();
            var scene = CreateScene(runTimeId);
            scene.Name = name;
            Scenes.Add(scene);
            return scene;
        }
        public void CreateSession(string remoteAddress, NetworkProtocolType networkProtocolType, Action onConnectComplete, Action onConnectFail, int connectTimeout = 5000)
        {
            var address = NetworkHelper.ToIPEndPoint(remoteAddress);
            var clientNetworkComponent = AddComponent<ClientNetworkComponent>();
            clientNetworkComponent.Initialize(networkProtocolType, NetworkTarget.Outer);
            clientNetworkComponent.Connect(address, onConnectComplete, onConnectFail, connectTimeout);
            Session = clientNetworkComponent.Session;
        }
#else
        /// <summary>
        /// 创建一个Scene
        /// </summary>
        /// <param name="server"></param>
        /// <param name="outerBindIp"></param>
        /// <param name="sceneInfo"></param>
        /// <param name="runEvent"></param>
        /// <param name="onSetNetworkComplete"></param>
        /// <returns></returns>
        public static async FTask<Scene> Create(Server server, string outerBindIp, SceneConfigInfo sceneInfo, Action<Session> onSetNetworkComplete = null, bool runEvent = true)
        {
            var scene = CreateScene(sceneInfo.EntityId);
            sceneInfo.Scene = scene;
            scene.Name = sceneInfo.Name;
            scene.RouteId = sceneInfo.RouteId;
            scene.Server = server;
            scene.SceneConfigId = sceneInfo.Id;

            if (sceneInfo.WorldId != 0)
            {
                // 有可能不需要数据库、所以这里默认0的情况下就不创建数据库了
                scene.World = World.Create(sceneInfo.WorldId);
            }

            if (!string.IsNullOrEmpty(sceneInfo.NetworkProtocol) && !string.IsNullOrEmpty(outerBindIp) && sceneInfo.OuterPort != 0)
            {
                // 设置Scene的网络、目前只支持KCP和TCP
                var networkProtocolType = Enum.Parse<NetworkProtocolType>(sceneInfo.NetworkProtocol);
                var serverNetworkComponent = scene.AddComponent<ServerNetworkComponent>();
                var address = NetworkHelper.ToIPEndPoint($"{outerBindIp}:{sceneInfo.OuterPort}");
                serverNetworkComponent.Initialize(networkProtocolType, NetworkTarget.Outer, address);
            }

            if (runEvent && sceneInfo.SceneType != null)
            {
                // 没有SceneType目前只有代码创建的Scene才会这样、目前只有Server的Scene是这样
                await EventSystem.Instance.PublishAsync(new OnCreateScene(sceneInfo, onSetNetworkComplete));
            }

            Scenes.Add(scene);
            return scene;
        }

        /// <summary>
        /// 一般用于创建临时Scene、如果不是必要不建议使用这个接口
        /// </summary>
        /// <param name="name"></param>
        /// <param name="server"></param>
        /// <param name="entityId"></param>
        /// <param name="sceneConfigId"></param>
        /// <param name="networkProtocol"></param>
        /// <param name="outerBindIp"></param>
        /// <param name="outerPort"></param>
        /// <param name="runEvent"></param>
        /// <param name="onSetNetworkComplete"></param>
        /// <returns></returns>
        public static async FTask<Scene> Create(string name, Server server, long entityId, uint sceneConfigId = 0, string networkProtocol = null, string outerBindIp = null, int outerPort = 0, Action<Session> onSetNetworkComplete = null, bool runEvent = true)
        {
            var sceneInfo = new SceneConfigInfo()
            {
                Name = name,
                EntityId = entityId,
                Id = sceneConfigId,
                NetworkProtocol = networkProtocol,
                OuterPort = outerPort,
                WorldId = ((EntityIdStruct)entityId).WordId
            };

            return await Create(server, outerBindIp, sceneInfo, onSetNetworkComplete, runEvent);
        }

        public static List<SceneConfigInfo> GetSceneInfoByRouteId(uint routeId)
        {
            var list = new List<SceneConfigInfo>();
            var allSceneConfig = ConfigTableManage.AllSceneConfig();

            foreach (var sceneConfigInfo in allSceneConfig)
            {
                if (sceneConfigInfo.RouteId != routeId)
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
