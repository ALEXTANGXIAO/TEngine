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



# <strong>TEngine v1.0.0

#### TEngine是一个简单(新手友好)且强大的Unity框架,对于需要一套上手快、文档清晰、高性能且可拓展性极强的开发者或者团队来说是一个很好的游戏开发框架解决方案。

## <a href="http://1.12.241.46:5000/"><strong>文档快速入门 »</strong></a>

## <strong>为什么要使用TEngine
0. 五分钟即可上手整套开发流程，代码整洁，思路清晰，功能强大。模块化和可定制度高，模块间的耦合度极低，您可以随时把您不需要的模块进行移除。
1. 强大的日志系统TLogger，可以编辑器/真机输出日志和日志文件到可持久化目录，捕捉到Excepion的时候自定义上传。
```Csharp
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
      //跨类发送事件
      GameEventMgr.Instance.Send("TEngine很好用");
      GameEventMgr.Instance.Send(Hellp);
  }
}

【举个例子：游戏中血量扣除的时候，服务器发来了一个减少Hp的消息包，我们可以在收到这个消息包的时候发送一个事件流，在玩家头顶的HP进度条组件/左上角Hp的UI血条组件添加一个监听事件，各个模块负责自己监听后的逻辑】
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
      player.Event.Send("Hpchange",hpPack);     //角色Player局部的事件管理器
      GameEventMgr.Instance.("Hpchange",hpPack);
    }
  }
}

class PlayerHp
{
  public PlayerEvent Event = new PlayerEvent();
  PlayerHp(){
    Event.Instance.AddEventListener<HpPack>(Hellp,HandleUpChange);
  }
}

class PlayerHpUI
{
  PlayerHpUI(){
    GameEventMgr.Instance.AddEventListener<HpPack>(Hellp,HandleUpChange);
  }
}
```
3. 健壮的资源模块TResources,开发者只用关注一个接口便可以通用的在编辑器进行资源加载或者真机加载AB，包括打包AB等等。通过类AssetConfig统一配置本地资源加载和AB打包的路径(AssetRootPath)。增加了资源计数的概念，后续在运行时更好的支持内存以及资源管理。
```Csharp
public static class TResources{
  public static T Load<T>(string path) where T : UnityEngine.Object
  {
      return ResMgr.Instance.Load<T>(path);
  }

  public static void LoadAsync(string path, Action<AssetData> callBack, bool withSubAsset = false)
  {
      ResMgr.Instance.GetAssetAtPathAsync(path, withSubAsset, callBack);
  }
}

```
## <strong>4.救世的huatuo热更新！！！！！！！！，TEngine内部集成了huatuo脚手架，可以一键改造Unity的IL2CPP，且备份好原本的文件,并且支持内部更新huatuo到最新版(2022.5.19日huatuo的安卓和IOS单元测试已全部通过)(目前支持Unity2020.3.33f1等高版本，2019版本将在2022年6月份支持。)
5. 如果您需要，强大的ECS模块Entity-Component-System,可以配合JobSystem和Brust释放你的性能。
```Csharp
### ECS架构类似unity的gameObject->component模式, 但是ECS是面向数据的编程思想，不同于面向对象以及Unity常用的Mono模式。Mono模式在内存中是散列的，而面向数据的ECS则是在内存中线性分布，且支持多线程（JobSystem、Brust编译）因此性能远高于原生Unity-Mono。可实现千人同屏。
### 在ECS中gameObject=entity, component=component, system类执行, ECS跟gameObject模式基本流程是一样的, 只是ecs中的组件可以复用, 而gameObject的component则不能复用, 在创建上万个对象时, gameObject就得重新new出来对象和组件, 而ecs调用Destroy时是把entity或component压入对象池, 等待下一次复用.实际上对象没有被释放,所以性能远高于gameObject的原因

 * E-- Entity 实体，本质上是存放组件的容器
 * C -- Component 组件，游戏所需的所有数据结构
 * S -- System 系统，根据组件数据处理逻辑状态的管理器
 * Component 组件只能存放数据，不能实现任何处理状态相关的函数
 * System系统不可以自己去记录维护任何状态
 * 说的通俗点，就是组件放数据，系统来处理。这么做的好处，就是为了尽可能地让数据与逻辑进行解耦
 * 一个良好的数据结构设计，也会以增加CPU缓存命中的形式来提升性能表现
 * 举个例子，常见的组件包括而不仅限于:
 * 渲染组件 ：模型的顶点、材质等数据，保证我们能正确地渲染到world中
 * 位置组件 ：记录着实体在这个world的真实位置
 * 特效组件 ：不同的时机，可能会需要播放不同的粒子特效以增强视觉感受

 //例子
//ECS时间组件
public class TimerComponent : Component, IUpdate //继承IUpdate接口后就会每帧调用Update方法
{
    private DateTime dateTime;
    public override void Awake()
    {
        dateTime = DateTime.Now.AddSeconds(5);//在初始化时,把当前时间推到5秒后
    }
    public void Update()
    {
        if (DateTime.Now >= dateTime)//当5秒时间到, 则删除这个时间组件, 实际上是压入对象池
        {
            Destroy(this);
        }
    }
    public override void OnDestroy()//当销毁, 实际是压入对象池前调用一次
    {
    }
}

static void Main(string[] args)
{
    var entity = GSystem.Instance.Create<Entity>();//创建实体对象,这会在对象池中查询,如果对象池没有对象,则会new, 有则弹出entity
    entity.AddComponent<TimerComponent>();//添加时间组件,也是从对象池查询,没有则new, 有则弹出TimerComponent对象
    while (true)
    {
        Thread.Sleep(30);
        GSystem.Instance.Run();//每帧执行ecs系统
    }
}
```
6. 可选择的高效网络模块，支持TCP/UDP异步网络管理器+Protobuf(后续看看建议会增加C#+DotNetty+Protobuf的服务器或者GoLang+Protobuf的服务器案例)
7. 可选择的商业化的UI框架，配合强大的TResource您可以直接进行游戏的UI开发。与Evnet事件模块实现MVE事件流驱动(Model - View - Event)。支持按照命名规范(ScriptGenerator/About查看)拼完预制体后，右键ScriptGenerator/UIProprty直接生成该预制体的属性绑定代码！！！极大的加快了UI开发的工作流。(您无需新建额外的狗屎UIMonobehaviour再挂载到UI预制体上面，您只需要把ScriptGenerator生成的UI代码复制到同名的UI脚本里就OK了)
```CSharp
//示例工具直接生成的代码
using TEngine;
using UnityEngine;
using UnityEngine.UI;

	class TestUI : UIWindow
	{
	#region 脚本工具生成的代码
		private Image m_imgbg;
		private Text m_textTittle;
		private Text m_textVer;
		private Image m_imgLogo;
		private GameObject m_goLoading;
		private Button m_btntest;
		private ScrollRect m_scroll;
		private Transform m_tfTrans;
	protected override void ScriptGenerator()
	{
			m_imgbg = FindChildComponent<Image>("m_imgbg");
			m_textTittle = FindChildComponent<Text>("m_textTittle");
			m_textVer = FindChildComponent<Text>("m_textVer");
			m_imgLogo = FindChildComponent<Image>("m_imgLogo");
			m_goLoading = FindChild("m_goLoading").gameObject;
			m_btntest = FindChildComponent<Button>("m_btntest");
			m_scroll = FindChildComponent<ScrollRect>("m_scroll");
			m_tfTrans = FindChild("m_tfTrans");
			m_btntest.onClick.AddListener(OnClicktestBtn);
	}
	#endregion

	#region 事件
		private void OnClicktestBtn()
		{
		}
	#endregion

}

```

```
//项目结构
Assets
├── link.xml            // IL2CPP的防裁剪
├── TEngine             // 框架目录
├── TResources          // 资源文件目录(可以自己修改AssetConfig进行自定义)
└── HotUpdateScripts    // 热更脚本资源

TEngine
├── Config~             // 配置表和转表工具(一键转表生成C#结构体和Json配置)
├── FileServer~         // Node编写的资源文件系统，可以部署测试AB下载，生产环境最好还是用OSS
├── UIFrameWork~        // UI系统的Package包
└── Runtime             // TEngine核心代码
    ├── ClientSaveData  // 本地可持久化(非必要)       
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
## <strong>优质开源项目推荐
#### <a href="https://github.com/JasonXuDeveloper/JEngine"><strong>JEngine</strong></a> - 使Unity开发的游戏支持热更新的解决方案。

#### <a href="https://github.com/focus-creative-games/huatuo"><strong>Huatuo</strong></a> - 特性完整、零成本、高性能、低内存的近乎完美的Unity全平台原生c#热更方案
