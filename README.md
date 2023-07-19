# TEngine
<p align="center">
    <img src="Books/src/TEngine512.png" alt="logo" width="384" height="384">
</p>

<h3 align="center"><strong>TEngine<strong></h3>

<p align="center">
  <strong>Unity框架解决方案<strong>
    <br>
  <a style="text-decoration:none">
    <img src="https://img.shields.io/badge/Unity%20Ver-2021.3.20++-blue.svg?style=flat-square" alt="status" />
  </a>
  <a style="text-decoration:none">
    <img src="https://img.shields.io/github/license/ALEXTANGXIAO/TEngine" alt="license" />
  </a>
  <a style="text-decoration:none">
    <img src="https://img.shields.io/github/last-commit/ALEXTANGXIAO/TEngine" alt="last" />
  </a>
  <a style="text-decoration:none">
    <img src="https://img.shields.io/github/issues/ALEXTANGXIAO/TEngine" alt="issue" />
  </a>
  <a style="text-decoration:none">
    <img src="https://img.shields.io/github/languages/top/ALEXTANGXIAO/TEngine" alt="topLanguage" />
  </a>
  <br>
  
  <br>
</p>


# <strong>TEngine v3.0.0

#### TEngine是一个简单(新手友好)且强大的Unity框架,对于需要一套上手快、文档清晰、高性能且可拓展性极强的开发者或者团队来说是一个很好的游戏开发框架解决方案。


## <a href="http://1.12.241.46:5000/"><strong>文档快速入门 »</strong></a>
## <a href="https://github.com/ALEXTANGXIAO/TEngineServer"><strong>服务端GitHub入口 »</strong></a>

## <strong>为什么要使用TEngine
0. 五分钟即可上手整套开发流程，代码整洁，思路清晰，功能强大。模块化和可定制度高，模块间的耦合度极低，您可以随时把您不需要的模块进行移除。
1. 强大的日志系统Log，可以编辑器/真机输出日志和日志文件到可持久化目录，捕捉到Excepion的时候自定义上传。
```Csharp
public void Test()
{
  Log.Info("some things");
  Log.Assert(bool:condition,"assert log");
  Log.Error("error some things");
...
}
[TEngine][INFO][2022-05-20 23:44:19 871] - DevicePerformanceLevel 设备性能评级:High
```
2. <strong>高效的事件系统GameEventMgr，可以指定事件ID/事件String监听和分发事件。通过事件来驱动模块，如战斗的角色身上的事件流、UI和网络以及Model的数据流、开发中的绝大部分情况都可以通过事件来进行驱动。(配合UI模块或者拓展的战斗模块实现MVE[Model - View - Event]事件驱动架构)
```Csharp

public static int Hellp = StringId.StringToHash("Hellp.Hellp");

class A
{
   public A()
   {
     //添加事件监听string
     GameEvent.AddEventListener("TEngine很好用",TodoSomeThings);
     //添加事件监听int 事件ID
     GameEvent.AddEventListener(Hellp,TodoSomeThings2);
   }
}

class B
{
  private void SaySomeThings()
  {
      //发送事件流
      GameEvent.Send("TEngine很好用");
      GameEvent.Send(Hellp);
  }
}

【举个例子：游戏中血量扣除的时候，服务器发来了一个减少Hp的消息包，
我们可以在收到这个消息包的时候发送一个事件流，在玩家头顶的HP进度
条组件/左上角Hp的UI血条组件添加一个监听事件，各个模块负责自己监听后的逻辑】
Server -> SendMessage(ReduceHP)

class ClientHandle
{
  private void HandleMessage(MainPack mainpack)
  {
    ...
    HpPack hpPack = mainpack.hpPack;
    int playerId = mainpack.playerId;
    var player = PlayerMgr.Instance.GetPlayer(playerId);
    if(player != null){
      player.Event.Send("Hpchange",hpPack);       //局部的事件管理器
      GameEvent.Send("Hpchange",hpPack);  //全局事件中心
    }
  }
}

class PlayerHp
{
  public ECSEventCmpt Event { get; set; }
  PlayerHp(){
    Event.AddEventListener<HpPack>(Hellp,HandleUpChange);
  }
}

class PlayerHpUI
{
  PlayerHpUI(){
    GameEvent.AddEventListener<HpPack>(Hellp,HandleUpChange);
  }
}
```
3. 健壮的资源模块TResources,开发者只用关注一个接口便可以通用的在编辑器进行资源加载或者真机加载AB，包括打包AB等等。通过类AssetConfig统一配置本地资源加载和AB打包的路径(AssetRootPath),AB加密偏移也在底层处理，更安全的保护了您的项目，测试无法被AssetStudio解密，并且经测试AB加密偏移性能几乎无影响。   
增加了AB资源计数的概念，提供了AB桥接器IResourceHelper，你可以拓展此接口实现自己的资源管理器以便于接入XAssets或者是YooAssets等等。

<strong>4.救世的<a href="https://github.com/focus-creative-games/hybridclr"><strong>HybirdCLR(huatuo)</strong></a>热更新(目前支持大部分Unity2020-2021 lts等版本)

```
//项目结构
Assets
├── AssetRaw            // 热更资源目录
├── Atlas               // 自动生成图集目录
├── HybridCLRData       // hybridclr相关目录
├── TEngine             // 框架核心目录
└── GameScripts         // 程序集目录
    ├── Editor          // 编辑器程序集
    ├── Main            // 主程序程序集(启动器与流程)
    └── HotFix          // 游戏热更程序集目录 [Folder]
        ├── GameBase    // 游戏基础框架程序集 [Dll]
        ├── GameProto   // 游戏配置协议程序集 [Dll]  
        ├── BattleCore  // 游戏核心战斗程序集 [Dll] 
        └── GameLogic   // 游戏业务逻辑程序集 [Dll]
            ├── GameApp.cs                  // 热更主入口
            └── GameApp_RegisterSystem.cs   // 热更主入口注册系统   
```

 - 必要：项目使用了以下第三方插件，请自行购买导入：
   - /Unity/Assets/Plugins/Sirenix
---


---
## <strong>优质开源项目推荐

### <a href="https://github.com/tuyoogame/YooAsset"><strong>YooAsset</strong></a> - YooAsset是一套商业级经历百万DAU游戏验证的资源管理系统。
#### <a href="https://github.com/JasonXuDeveloper/JEngine"><strong>JEngine</strong></a> - 使Unity开发的游戏支持热更新的解决方案。

#### <a href="https://github.com/focus-creative-games/hybridclr"><strong>HybridCLR</strong></a> - 特性完整、零成本、高性能、低内存的近乎完美的Unity全平台原生c#热更方案
    
## <strong>交流群
### <a href="http://qm.qq.com/cgi-bin/qm/qr?_wv=1027&k=MzOcQIzGVLQ5AC5LHaqqA3h_F6lZ_DX4&authKey=LctqAWGHkJ7voQvuj1oaSe5tsGrc1XmQG3U4QniieGUlxY3lC7FtDIpEvPOX0vT8&noverify=0&group_code=862987645">群   号：862987645 </strong></a>
