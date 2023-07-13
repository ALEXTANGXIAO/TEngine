#if TENGINE_NET
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using TEngine.Core;
using Microsoft.Extensions.Configuration;
#pragma warning disable CS8601

#pragma warning disable CS8618

namespace TEngine.Core;

public sealed class Exporter
{
    public void Start()
    {
        Console.OutputEncoding = Encoding.UTF8;
        var exportType = Define.Options.ExportType;

        if (exportType != ExportType.None)
        {
            return;
        }

        LogInfo("请输入你想要做的操作:");
        LogInfo("1:导出网络协议（ProtoBuf）");
        LogInfo("2:增量导出Excel（包含常量枚举）");
        LogInfo("3:全量导出Excel（包含常量枚举）");

        var keyChar = Console.ReadKey().KeyChar;
            
        if (!int.TryParse(keyChar.ToString(), out var key) || key is < 1 or >= (int) ExportType.Max)
        {
            Console.WriteLine("");
            LogInfo("无法识别的导出类型请，输入正确的操作类型。");
            return;
        }
            
        LogInfo("");
        exportType = (ExportType) key;
        LoadConfig();
        
        switch (exportType)
        {
            case ExportType.ProtoBuf:
            {
                _ = new ProtoBufExporter();
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

    private void LoadConfig()
    {
        const string settingsName = "TEngineSettings.json";
        var currentDirectory = Directory.GetCurrentDirectory();

        if (!File.Exists(Path.Combine(currentDirectory, settingsName)))
        {
            throw new FileNotFoundException($"not found {settingsName} in OutputDirectory");
        }

        var configurationRoot = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(settingsName)
            .Build();
        // ProtoBuf文件所在的位置文件夹位置
        ProtoBufDefine.ProtoBufDirectory = configurationRoot["Export:ProtoBufDirectory:Value"];
        // ProtoBuf生成到服务端的文件夹位置
        ProtoBufDefine.ServerDirectory = configurationRoot["Export:ProtoBufServerDirectory:Value"];
        // ProtoBuf生成到客户端的文件夹位置
        ProtoBufDefine.ClientDirectory = configurationRoot["Export:ProtoBufClientDirectory:Value"];
        // ProtoBuf生成代码模板的位置
        ProtoBufDefine.ProtoBufTemplatePath = configurationRoot["Export:ProtoBufTemplatePath:Value"];
        // Excel配置文件根目录
        ExcelDefine.ProgramPath = configurationRoot["Export:ExcelProgramPath:Value"];
        // Excel版本文件的位置
        ExcelDefine.ExcelVersionFile = configurationRoot["Export:ExcelVersionFile:Value"];
        // Excel生成服务器代码的文件夹位置
        ExcelDefine.ServerFileDirectory = configurationRoot["Export:ExcelServerFileDirectory:Value"];
        // Excel生成客户端代码文件夹位置
        ExcelDefine.ClientFileDirectory = configurationRoot["Export:ExcelClientFileDirectory:Value"];
        // Excel生成服务器二进制数据文件夹位置
        ExcelDefine.ServerBinaryDirectory = configurationRoot["Export:ExcelServerBinaryDirectory:Value"];
        // Excel生成客户端二进制数据文件夹位置
        ExcelDefine.ClientBinaryDirectory = configurationRoot["Export:ExcelClientBinaryDirectory:Value"];
        // Excel生成服务器Json数据文件夹位置
        ExcelDefine.ServerJsonDirectory = configurationRoot["Export:ExcelServerJsonDirectory:Value"];
        // Excel生成客户端Json数据文件夹位置
        ExcelDefine.ClientJsonDirectory = configurationRoot["Export:ExcelClientJsonDirectory:Value"];
        // Excel生成代码模板的位置
        ExcelDefine.ExcelTemplatePath = configurationRoot["Export:ExcelTemplatePath:Value"];
        // 服务器自定义导出代码文件夹位置
        ExcelDefine.ServerCustomExportDirectory = configurationRoot["Export:ServerCustomExportDirectory:Value"];
        // 客户端自定义导出代码文件夹位置
        ExcelDefine.ClientCustomExportDirectory = configurationRoot["Export:ClientCustomExportDirectory:Value"];
    }

    public static void LogInfo(string msg)
    {
        Console.WriteLine(msg);
    }

    public static void LogError(string msg)
    {
        ConsoleColor color = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"{msg}\n{new StackTrace(1, true)}");
        Console.ForegroundColor = color;
    }

    public static void LogError(Exception e)
    {
        ConsoleColor color = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(e.Data.Contains("StackTrace") ? $"{e.Data["StackTrace"]}\n{e}" : e.ToString());
        Console.ForegroundColor = color;
    }
}
#endif
