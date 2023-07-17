#if TENGINE_NET
using TEngine.Core.Network;

namespace TEngine.Logic;

[Flags]
public enum SceneType: long
{
    None = 0,
    Gate = 1,               // Gate
    Addressable = 1 << 2,   // 负责进程间消息通信
    Map = 1 << 3,           // 游戏场景服
    Chat = 1 << 4,          // 游戏聊天服
}

/// <summary>
/// 场景创建回调。
/// <remarks>常用于定义场景需要添加的组件。</remarks>
/// </summary>
public class OnCreateScene : AsyncEventSystem<TEngine.OnCreateScene>
{
    public override async FTask Handler(TEngine.OnCreateScene self)
    {
        // 服务器是以Scene为单位的、所以Scene下有什么组件都可以自己添加定义
        // OnCreateScene这个事件就是给开发者使用的
        // 比如Address协议这里、我就是做了一个管理Address地址的一个组件挂在到Address这个Scene下面了
        // 比如Map下你需要一些自定义组件、你也可以在这里操作
        var sceneConfigInfo = self.SceneInfo;

        switch (sceneConfigInfo.SceneType.Parse<SceneType>())
        {
            case SceneType.Addressable:
            {
                sceneConfigInfo.Scene.AddComponent<AddressableManageComponent>();
                break;
            }
            case SceneType.Gate:
            {
                sceneConfigInfo.Scene.AddComponent<AccountComponent>();
                break;
            }
        }
        Log.Info($"scene create: {self.SceneInfo.SceneType} {self.SceneInfo.Name} SceneId:{self.SceneInfo.Id} ServerId:{self.SceneInfo.RouteId} WorldId:{self.SceneInfo.WorldId}");

        await FTask.CompletedTask;
    }
}
#endif