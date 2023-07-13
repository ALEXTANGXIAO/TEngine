#if TENGINE_NET
using CommandLine;
using TEngine.Core;

#pragma warning disable CS8618

namespace TEngine;

public enum AppType
{
    Game,

    Export,
    
    /// <summary>
    /// 每台物理机一个守护进程，用来启动该物理机上的所有进程。
    /// </summary>
    Watcher,
}


public enum Mode
{
    /// <summary>
    /// Develop:所有Server都在一个进程中,Release:每个Server都在独立的进程中。
    /// </summary>
    Release,

    Develop,
}

public class CommandLineOptions
{
    /// <summary>
    /// 进程Id
    /// </summary>
    [Option("AppId", Required = false, Default = (uint)0, HelpText = "Enter an AppId such as 1")]
    public uint AppId { get; set; }

    /// <summary>
    /// App类型
    /// Game - 游戏服务器App
    /// Export - 导表App
    /// </summary>
    [Option("AppType", Required = false, Default = AppType.Game, HelpText = "AppType enum")]
    public AppType AppType { get; set; }

    /// <summary>
    /// 服务器运行模式
    /// Develop - 开发模式（所有Server都在一个进程中）
    /// Release - 发布模式（每个Server都在独立的进程中）
    /// </summary>
    [Option("Mode", Required = false, Default = Mode.Develop, HelpText = "Mode enum")]
    public Mode Mode { get; set; }
    
    [Option("LogLevel", Required = false, Default = 2)]
    public int LogLevel { get; set; }

#if TENGINE_NET
    /// <summary>
    /// 导表的类型
    /// </summary>
    [Option("ExcelExportType", Required = false, Default = ExportType.None, HelpText = "Increment,All")]
    public ExportType ExportType { get; set; }
#endif
}
#endif