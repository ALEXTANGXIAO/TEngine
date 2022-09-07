# TEngine
<p align="center">
    <img src="http://1.12.241.46:8081/temp/TEngine512.png" alt="logo" width="384" height="384">
</p>

<h3 align="center"><strong>TEngine<strong></h3>

<p align="center">
  <strong>Unity框架解决方案<strong>
    <br>
  <a style="text-decoration:none">
    <img src="https://img.shields.io/badge/Unity%20Ver-2019.4.12++-blue.svg?style=flat-square" alt="status" />
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
  <a style="text-decoration:none">
    <img src="https://app.fossa.com/api/projects/git%2Bgithub.com%2FJasonXuDeveloper%2FJEngine.svg?type=shield" alt="status" />
  </a>
  <br>
  
  <br>
</p>


# <strong>TEngine v2.0.0

#### TEngine是一个简单(新手友好)且强大的Unity框架,对于需要一套上手快、文档清晰、高性能且可拓展性极强的开发者或者团队来说是一个很好的游戏开发框架解决方案。

## 2.0版本主线开发中！！！稳定版本请到分支v1.2.0

## <a href="http://1.12.241.46:5000/"><strong>文档快速入门 »</strong></a>
## <a href="https://github.com/ALEXTANGXIAO/TEngineServer"><strong>服务端GitHub入口 »</strong></a>

## <strong>为什么要使用TEngine
0. 五分钟即可上手整套开发流程，代码整洁，思路清晰，功能强大。模块化和可定制度高，模块间的耦合度极低，您可以随时把您不需要的模块进行移除。
1. 强大的日志系统TLogger，可以编辑器/真机输出日志和日志文件到可持久化目录，捕捉到Excepion的时候自定义上传。
```Csharp
public void Test()
{
  TLogger.LogInfo();
  TLogger.LogAssert();
  TLogger.LogInfoSuccessd();
...
}
[TLogger][INFO][2022-05-20 23:44:19 871] - DevicePerformanceLevel 设备性能评级:High
```
2. <strong>高效的事件系统GameEventMgr，可以指定事件ID/事件String监听和分发事件。通过事件来驱动模块，如战斗的角色身上的事件流、UI和网络以及Model的数据流、开发中的绝大部分情况都可以通过事件来进行驱动。(配合UI模块或者拓展的战斗模块实现MVE[Model - View - Event]事件驱动架构)
```Csharp

public static int Hellp = StringId.StringToHash("Hellp.Hellp");

class A
{
   public A()
   {
     //添加事件监听string
     GameEventMgr.Instance.AddEventListener("TEngine很好用",TodoSomeThings);
     //添加事件监听int 事件ID
     GameEventMgr.Instance.AddEventListener(Hellp,TodoSomeThings2);
   }
}

class B
{
  private void SaySomeThings()
  {
      //发送事件流
      GameEventMgr.Instance.Send("TEngine很好用");
      GameEventMgr.Instance.Send(Hellp);
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
      GameEventMgr.Instance.Send("Hpchange",hpPack);  //全局事件中心
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
    GameEventMgr.Instance.AddEventListener<HpPack>(Hellp,HandleUpChange);
  }
}
```
3. 健壮的资源模块TResources,开发者只用关注一个接口便可以通用的在编辑器进行资源加载或者真机加载AB，包括打包AB等等。通过类AssetConfig统一配置本地资源加载和AB打包的路径(AssetRootPath),AB加密偏移也在底层处理，更安全的保护了您的项目，测试无法被AssetStudio解密，并且经测试AB加密偏移性能几乎无影响。   
增加了AB资源计数的概念，提供了AB桥接器IResourceHelper，你可以拓展此接口实现自己的资源管理器以便于接入XAssets或者是YooAssets等等。

<strong>4.救世的<a href="https://github.com/focus-creative-games/hybridclr"><strong>HybirdCLR(huatuo)</strong></a>热更新！！！！！！！！(2022.5.19日huatuo的安卓和IOS单元测试已全部通过)(目前支持Unity2020.3.33f1等高版本，2019版本将在2022年6月份支持。)

6. 可选择的高效网络模块，支持TCP/UDP异步网络管理器+Protobuf(增加了C#+DotNetty+Protobuf的服务器服务器案例)

7. 可选择的商业化的UI框架，配合强大的TResource您可以直接进行游戏的UI开发。与Evnet事件模块实现MVE事件流驱动(Model - View - Event)。支持按照命名规范(ScriptGenerator/About查看)拼完预制体后，右键ScriptGenerator/UIProprty直接生成该预制体的属性绑定代码！！！极大的加快了UI开发的工作流。(您无需新建额外的狗屎UIMonobehaviour再挂载到UI预制体上面，您只需要把ScriptGenerator生成的UI代码复制到同名的UI脚本里就OK了)

```
//项目结构
Assets
├── link.xml            // IL2CPP的防裁剪
├── TEngine             // 框架目录
├── TResources          // 资源文件目录(可以自己修改AssetConfig进行自定义)
└── HotUpdateScripts    // 热更脚本资源(可以把TEngine下的Runtime脚本放入此处，让TEngine也处于热更域)

TEngine
├── Config~             // 配置表和转表工具(一键转表生成C#结构体和Json配置)
├── FileServer~         // Node编写的资源文件系统，可以部署测试AB下载，生产环境最好还是用OSS
├── UIFrameWork~        // UI系统的Package包
├── Editor              // TEngine编辑器核心代码
└── Runtime             // TEngine核心代码
    ├── PlayerPrefsDataMgr// 本地可持久化(非必要)       
    ├── Audio           // 音频模块(非必要)
    ├── Config          // 配置表加载器(非必要)
    ├── Mono            // Mono管理器
    ├── Unitity         // 工具类
    ├── Res             // 资源加载管理器
    ├── HotUpdate       // 热更新模块(非必要)
    ├── UI              // UI系统模块(非必要)
    ├── Net             // 网络模块(非必要)
    ├── ECS             // ECS模块(非必要)
    ├── Event           // Event事件模块
    └── Core            // 核心模块
```
---
## <strong>技术支持
 QQ群：967860570   
 欢迎大家提供意见和改进意见，不喜请友善提意见哈 谢谢~   
 如果您觉得感兴趣想期待关注一下或者有眼前一亮的模块，不妨给个Star~

---

## <strong>友情链接：

#### <a href="https://github.com/asdfg314284230/TengineBilibilDemo"><strong>TengineBilibilDemo</strong></a> - 基于Tengine实现的Bilibili直播间访问的Demo。

---
## <strong>优质开源项目推荐
#### <a href="https://github.com/JasonXuDeveloper/JEngine"><strong>JEngine</strong></a> - 使Unity开发的游戏支持热更新的解决方案。

#### <a href="https://github.com/focus-creative-games/hybridclr"><strong>HybridCLR</strong></a> - 特性完整、零成本、高性能、低内存的近乎完美的Unity全平台原生c#热更方案


[//]: # (## <strong>Buy me a coffee.)

[//]: # (您的赞助会让我们做得更快更好，如果觉得TEngine有帮助，不妨赞助我买杯咖啡吧~)

[//]: # (<p align="center">)

[//]: # (    <img src="http://1.12.241.46:8081/TEngine/微信.png" alt="logo" width="384" height="384">)

[//]: # (</p>)

[//]: # (<p align="center">)

[//]: # (    <img src="http://1.12.241.46:8081/TEngine/支付宝.jpg" alt="logo" width="384" height="384">)

[//]: # (</p>)