# TEngine
TEngine
<p align="center">
    <img src="http://1.12.241.46:8081/temp/TEngine512.png" alt="logo" width="256" height="256">
</p>

<h3 align="center">TEngine</h3>

<p align="center">
  Unity框架解决方案
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
  <a href="http://1.12.241.46:5000/"><strong>框架文档 »</strong></a>
  <br>
  ·
  <br>
  <a href="https://github.com/ALEXTANGXIAO/TEngine">框架首页 »</a>
</p>



# TEngine v1.0.0

TEngine
```Json
TEngine项目结构

Assets
├── TResources        // TResources资源加载和打包目录
├── ConfigStruct      // 生成的配置表结构体
├── TEngine           // TEngine
└── Scripts           // 脚本资源

com.tx.tengine
├── Tools~            // 常用工具
├── Config~           // 转表工具
├── FileServer~       // 基于Node.js热更服务器，有条件用OSS
├── Runtime           // TEngine 脚本资源
└── TEnginePackage~   // TEngine 差异化插件，如UI、网络、热更等

com.tx.tengine/Runtime
├── ClientSaveData      // 本地化储存
├── 3rd                 // 三方插件（Json库、Protobuf）
├── Config              // Config配置表
├── Editor              // Editor
├── Event               // Event事件驱动系统
├── ECS                 // ECS架构
├── FileSystem          // FileSystem
├── Json                // Json库文件
├── Game                // 核心逻辑
├── Unitity             // Unitity工具类
├── Res                 // Res资源加载模块
└── Core                // TEngine核心
    ├── BaseLogicSys    // 基础系统模块，依赖TEngine实现生命周期
    ├── MemPoolMgr      // 内存缓存池
    ├── TEngineRedux    // DVA/Redux
    ├── TSingleton      // 单例以及单例管理器
    └── TEngine         // 主入口

可定制化模块
Assets/TEngine/Runtime/UI
├── Editor            // 脚本从预制体自动生成UI代码
├── Extend           // 转表工具
├── Res               // 基于Node.js热更服务器，有条件用OSS
//TODO

```