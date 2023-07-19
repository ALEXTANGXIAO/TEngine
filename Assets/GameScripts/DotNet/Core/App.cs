#if TENGINE_NET
using CommandLine;
using TEngine.Core;
using NLog;

namespace TEngine
{
    public static class App
    {
        public static void Init()
        {
            try
            {
                // 设置默认的线程的同步上下文
                SynchronizationContext.SetSynchronizationContext(ThreadSynchronizationContext.Main);
                // 解析命令行参数
                Parser.Default.ParseArguments<CommandLineOptions>(Environment.GetCommandLineArgs())
                    .WithNotParsed(error => throw new Exception("Command line format error!"))
                    .WithParsed(option => AppDefine.Options = option);
                // 加载框架配置
                TEngineSettingsHelper.Initialize();
                // 检查启动参数
                switch (AppDefine.Options.AppType)
                {
                    case AppType.Game:
                    {
                        break;
                    }
                    case AppType.Export:
                    {
                        new Exporter().Start();
                        return;
                    }
                    default:
                    {
                        throw new NotSupportedException($"AppType is {AppDefine.Options.AppType} Unrecognized!");
                    }
                }

                // 根据不同的运行模式来选择日志的方式
                switch (AppDefine.Options.Mode)
                {
                    case Mode.Develop:
                    {
                        LogManager.Configuration.RemoveRuleByName("ConsoleTrace");
                        LogManager.Configuration.RemoveRuleByName("ConsoleDebug");
                        LogManager.Configuration.RemoveRuleByName("ConsoleInfo");
                        LogManager.Configuration.RemoveRuleByName("ConsoleWarn");
                        LogManager.Configuration.RemoveRuleByName("ConsoleError");

                        LogManager.Configuration.RemoveRuleByName("ServerDebug");
                        LogManager.Configuration.RemoveRuleByName("ServerTrace");
                        LogManager.Configuration.RemoveRuleByName("ServerInfo");
                        LogManager.Configuration.RemoveRuleByName("ServerWarn");
                        LogManager.Configuration.RemoveRuleByName("ServerError");
                        break;
                    }
                    case Mode.Release:
                    {
                        LogManager.Configuration.RemoveRuleByName("ConsoleTrace");
                        LogManager.Configuration.RemoveRuleByName("ConsoleDebug");
                        LogManager.Configuration.RemoveRuleByName("ConsoleInfo");
                        LogManager.Configuration.RemoveRuleByName("ConsoleWarn");
                        LogManager.Configuration.RemoveRuleByName("ConsoleError");
                        break;
                    }
                }

                // 初始化SingletonSystemCenter这个一定要放到最前面
                // 因为SingletonSystem会注册AssemblyManager的OnLoadAssemblyEvent和OnUnLoadAssemblyEvent的事件
                // 如果不这样、会无法把程序集的单例注册到SingletonManager中
                SingletonSystem.Initialize();
                // 加载核心程序集
                AssemblyManager.Initialize();
                
                Log.Info($"Start Server Param => {Parser.Default.FormatCommandLine(AppDefine.Options)}");
            }
            catch (Exception exception)
            {
                Log.Error(exception);
            }
        }

        public static async FTask Start()
        {
            switch (AppDefine.Options.Mode)
            {
                case Mode.Develop:
                {
                    // 开发模式默认所有Server都在一个进程中、方便调试、但网络还都是独立的
                    var serverConfigInfos = ConfigTableManage.AllServerConfig();

                    foreach (var serverConfig in serverConfigInfos)
                    {
                        await Server.Create(serverConfig.Id);
                    }

                    return;
                }
                case Mode.Release:
                {
                    // 发布模式只会启动启动参数传递的Server、也就是只会启动一个Server
                    // 您可以做一个Server专门用于管理启动所有Server的工作
                    await Server.Create(AppDefine.Options.AppId);
                    return;
                }
            }
        }

        public static void Close()
        {
            SingletonSystem.Dispose();
            AssemblyManager.Dispose();
        }
    }
}
#endif