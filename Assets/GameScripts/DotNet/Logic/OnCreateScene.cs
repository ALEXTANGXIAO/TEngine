#if TENGINE_NET
using TEngine.Core.Network;

namespace TEngine.Logic;

/// <summary>
/// 当Scene创建时需要干什么
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

        switch (sceneConfigInfo.SceneType)
        {
            case "Addressable":
            {
                sceneConfigInfo.Scene.AddComponent<AddressableManageComponent>();
                break;
            }
        }
        Log.Info($"scene create: {self.SceneInfo.SceneType} {self.SceneInfo.Name} SceneId:{self.SceneInfo.Id} ServerId:{self.SceneInfo.RouteId} WorldId:{self.SceneInfo.WorldId}");

        await FTask.CompletedTask;
    }
}
#endif