#if TENGINE_NET
using TEngine.Core.Network;

namespace TEngine.Logic;

public class H_C2G_LoginAddressRequestHandler : MessageRPC<H_C2G_LoginAddressRequest, H_G2C_LoginAddressResponse>
{
    protected override async FTask Run(Session session, H_C2G_LoginAddressRequest request, H_G2C_LoginAddressResponse response, Action reply)
    {
        // 什么是可寻址消息
        // 此服务器是一个分布式框架、所以肯定会有多个服务器相互通信
        // 游戏里一个玩家这里统称Unit、随着游戏场景的越来越多、为了让游戏服务器能承载更多人
        // 所以大家都会把服务器给分成多个、比如一个地图一个服务器、从而能提升服务器的负载能力、也更方便开发、类似微服务
        // 但这样就会有几个问题出现了:
        // 1、客户端是直接连接到服务器、如果切换服务器的时候客户端就会掉线、怎么解决不让用户掉线
        // 2、从A服务器到B服务器后、如果能正常发送到B服务器而不是发送到A服务器中
        // 为了解决上面的问题、大多服务器架构都采用了一个中转服务器（Gate）来收发消息
        // 比如客户端一直连接的一个服务器这里统称Gate
        // 那网络通讯的管道模型是客户端->Gate->其他服务器、客户端接收消息是其他服务器->Gate->客户端
        // 这样的好处是无论玩家在什么服务器只需要改变Gate到其他服务器的连接就可以了、中间客户端是一直连接到Gate的所以客户端不会掉线
        // 这个问题解决了、但还有一个问题就是Gate跟其他服务器的连接会随着玩家的逻辑变动
        // 所以框架提供的可寻址消息（Address）消息、使用Address消息后会自动寻找到Unit的正确位置并发送到、不需要开发者再处理这个逻辑了
        // 下面就是一个例子
        // 1、首选分配一个可用、负载比较低的服务器给这个Unit、我这里就在ServerConfig.xsl表里拿一个MAP了、但实际开发过程可能比这个要复杂
        // 我这里就简单些一个做为演示、其实这些逻辑开发者完全可以自己封装一个接口来做。
        // 在ServerConfig.xsl里找到MAP的进程、看到ID是3072通过这个Id在SceneConfig.xsl里找到对应的Scene的EntityId
        var sceneEntityId = Helper.AddressableSceneHelper.GetSceneEntityIdByRouteId(3072);
        
        // 2、在InnerMessage里定义一个协议、用于Gate跟Map通讯的协议I_G2M_LoginAddress
        var loginAddressResponse = (I_M2G_LoginAddressResponse)await MessageHelper.CallInnerRoute(session.Scene,
            sceneEntityId,
            new I_G2M_LoginAddressRequest()
            {
                AddressId = session.Id,
                GateRouteId = session.RuntimeId,
            });
        if (loginAddressResponse.ErrorCode != 0)
        {
            Log.Error($"注册到Map的Address发生错误 ErrorCode:{loginAddressResponse.ErrorCode}");
            return;
        }
        // 3、可寻址消息组件、挂载了这个组件可以接收和发送Addressable消息
        session.AddComponent<AddressableRouteComponent>();
    }
}
#endif