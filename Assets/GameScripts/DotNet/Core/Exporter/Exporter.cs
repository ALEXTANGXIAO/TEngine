#if TENGINE_NET
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using TEngine.Helper;
using Microsoft.Extensions.Configuration;
#pragma warning disable CS8601


namespace TEngine.Core;

/// <summary>
/// 数据导出器，用于执行导出操作。
/// </summary>
public sealed class Exporter
{
    /// <summary>
    /// 开始执行数据导出操作。
    /// </summary>
    public void Start()
    {
        Console.OutputEncoding = Encoding.UTF8;
        var exportType = AppDefine.Options.ExportType;

        if (exportType != ExportType.None)
        {
            return;
        }

        LogInfo("请输入你想要做的操作:");
        LogInfo("1:导出网络协议（ProtoBuf）");
        LogInfo("2:导出网络协议并重新生成OpCode（ProtoBuf）");
        LogInfo("3:增量导出Excel（包含常量枚举）");
        LogInfo("4:全量导出Excel（包含常量枚举）");

        var keyChar = Console.ReadKey().KeyChar;
            
        if (!int.TryParse(keyChar.ToString(), out var key) || key is < 1 or >= (int) ExportType.Max)
        {
            Console.WriteLine("");
            LogInfo("无法识别的导出类型请，输入正确的操作类型。");
            return;
        }
            
        LogInfo("");
        exportType = (ExportType) key;

        switch (exportType)
        {
            case ExportType.ProtoBuf:
            {
                _ = new ProtoBufExporter(false);
                break;
            }
            case ExportType.ProtoBufAndOpCodeCache:
            {
                _ = new ProtoBufExporter(true);
                break;
            }
            case ExportType.AllExcel:
            case ExportType.AllExcelIncrement:
            {
                _ = new ExcelExporter(exportType);
                break;
            }
        }
           
        LogInfo("操作完成,按任意键关闭程序");
        Console.ReadKey();
        Environment.Exit(0);
    }

    /// <summary>
    /// 输出信息到控制台。
    /// </summary>
    /// <param name="msg">要输出的信息。</param>
    public static void LogInfo(string msg)
    {
        Console.WriteLine(msg);
    }

    /// <summary>
    /// 输出错误信息到控制台。
    /// </summary>
    /// <param name="msg">要输出的错误信息。</param>
    public static void LogError(string msg)
    {
        ConsoleColor color = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"{msg}\n{new StackTrace(1, true)}");
        Console.ForegroundColor = color;
    }

    /// <summary>
    /// 输出异常信息到控制台。
    /// </summary>
    /// <param name="e">要输出的异常。</param>
    public static void LogError(Exception e)
    {
        ConsoleColor color = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(e.Data.Contains("StackTrace") ? $"{e.Data["StackTrace"]}\n{e}" : e.ToString());
        Console.ForegroundColor = color;
    }
}
#endif
