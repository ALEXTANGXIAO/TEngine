#if TENGINE_NET
using TEngine.Core.Network;

namespace TEngine.Logic;

[Flags]
public enum SceneType: long
{
    /// <summary>
    /// 场景无。
    /// </summary>
    None = 0,
    
    /// <summary>
    /// 场景Gate。
    /// </summary>
    Gate = 1,
    
    /// <summary>
    /// 场景Addressable。
    /// <remarks>负责进程间消息通信。</remarks>
    /// </summary>
    Addressable = 1 << 2,
    
    /// <summary>
    /// 游戏场景服。
    /// </summary>
    Map = 1 << 3,
    
    /// <summary>
    /// 游戏聊天服。
    /// </summary>
    Chat = 1 << 4,
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
        var scene = self.Scene;
        switch (scene.SceneType.Parse<SceneType>())
        {
            case SceneType.Gate:
            {
                self.Scene.AddComponent<AccountComponent>();
                break;
            }
        }
        Log.Info($"scene create: {self.Scene.SceneType} {self.Scene.Name} SceneId:{self.Scene.Id} LocationId:{self.Scene.LocationId} WorldId:{self.Scene.World?.Id}");

        await FTask.CompletedTask;
    }
}
#endif