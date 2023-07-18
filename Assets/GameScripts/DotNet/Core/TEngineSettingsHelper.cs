#if TENGINE_NET
using Microsoft.Extensions.Configuration;
#pragma warning disable CS8604
#pragma warning disable CS8601
#pragma warning disable CS8618

namespace TEngine.Core;

public static class TEngineSettingsHelper
{
    public static void Initialize()
    {
        const string settingsName = "TEngineSettings.json";
        var currentDirectory = Directory.GetCurrentDirectory();

        if (!File.Exists(Path.Combine(currentDirectory, settingsName)))
        {
            throw new FileNotFoundException($"not found {settingsName} in OutputDirectory");
        }

        var configurationRoot = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile(settingsName).Build();
        // 加载网络配置
        LoadNetworkConfig(configurationRoot);
        // 加载ProtoBuf配置
        LoadProtoConfig(configurationRoot);
        // 加载Excel配置
        LoadExcelConfig(configurationRoot);
    }

    private static void LoadNetworkConfig(IConfigurationRoot root)
    {
        Define.SessionIdleCheckerInterval = Convert.ToInt32(root["Network:SessionIdleCheckerInterval:Value"]);
        Define.SessionIdleCheckerTimeout = Convert.ToInt32(root["Network:SessionIdleCheckerTimeout:Value"]);
    }

    private static void LoadProtoConfig(IConfigurationRoot root)
    {
        // ProtoBuf文件所在的位置文件夹位置
        Define.ProtoBufDirectory = root["Export:ProtoBufDirectory:Value"].GetFullPath();
        // ProtoBuf生成到服务端的文件夹位置
        Define.ProtoBufServerDirectory = root["Export:ProtoBufServerDirectory:Value"].GetFullPath();
        // ProtoBuf生成到客户端的文件夹位置
        Define.ProtoBufClientDirectory = root["Export:ProtoBufClientDirectory:Value"].GetFullPath();
        // ProtoBuf生成代码模板的位置
        Define.ProtoBufTemplatePath = root["Export:ProtoBufTemplatePath:Value"].GetFullPath();
    }

    private static void LoadExcelConfig(IConfigurationRoot root)
    {
        // Excel配置文件根目录
        Define.ExcelProgramPath = root["Export:ExcelProgramPath:Value"].GetFullPath();
        // Excel版本文件的位置
        Define.ExcelVersionFile = root["Export:ExcelVersionFile:Value"].GetFullPath();
        // Excel生成服务器代码的文件夹位置
        Define.ExcelServerFileDirectory = root["Export:ExcelServerFileDirectory:Value"].GetFullPath();
        // Excel生成客户端代码文件夹位置
        Define.ExcelClientFileDirectory = root["Export:ExcelClientFileDirectory:Value"].GetFullPath();
        // Excel生成服务器二进制数据文件夹位置
        Define.ExcelServerBinaryDirectory = root["Export:ExcelServerBinaryDirectory:Value"].GetFullPath();
        // Excel生成客户端二进制数据文件夹位置
        Define.ExcelClientBinaryDirectory = root["Export:ExcelClientBinaryDirectory:Value"].GetFullPath();
        // Excel生成服务器Json数据文件夹位置
        Define.ExcelServerJsonDirectory = root["Export:ExcelServerJsonDirectory:Value"].GetFullPath();
        // Excel生成客户端Json数据文件夹位置
        Define.ExcelClientJsonDirectory = root["Export:ExcelClientJsonDirectory:Value"].GetFullPath();
        // Excel生成代码模板的位置
        Define.ExcelTemplatePath = root["Export:ExcelTemplatePath:Value"].GetFullPath();
        // 服务器自定义导出代码文件夹位置
        Define.ServerCustomExportDirectory = root["Export:ServerCustomExportDirectory:Value"].GetFullPath();
        // 客户端自定义导出代码
        Define.ClientCustomExportDirectory = root["Export:ClientCustomExportDirectory:Value"].GetFullPath();
        // 自定义导出代码存放的程序集
        Define.CustomExportAssembly = root["Export:CustomExportAssembly:Value"].GetFullPath();
    }
    
    private static string GetFullPath(this string relativePath)
    {
        return Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), relativePath));
    }
}
#endif