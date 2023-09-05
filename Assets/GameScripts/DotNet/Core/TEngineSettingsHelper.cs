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
        Define.ProtoBufDirectory = FileHelper.GetFullPath(root["Export:ProtoBufDirectory:Value"]);
        // ProtoBuf生成到服务端的文件夹位置
        Define.ProtoBufServerDirectory = FileHelper.GetFullPath(root["Export:ProtoBufServerDirectory:Value"]);
        // ProtoBuf生成到客户端的文件夹位置
        Define.ProtoBufClientDirectory = FileHelper.GetFullPath(root["Export:ProtoBufClientDirectory:Value"]);
        // ProtoBuf生成代码模板的位置
        Define.ProtoBufTemplatePath = FileHelper.GetFullPath(root["Export:ProtoBufTemplatePath:Value"]);
    }

    private static void LoadExcelConfig(IConfigurationRoot root)
    {
        // Excel配置文件根目录
        Define.ExcelProgramPath = FileHelper.GetFullPath(root["Export:ExcelProgramPath:Value"]);
        // Excel版本文件的位置
        Define.ExcelVersionFile = FileHelper.GetFullPath(root["Export:ExcelVersionFile:Value"]);
        // Excel生成服务器代码的文件夹位置
        Define.ExcelServerFileDirectory = FileHelper.GetFullPath(root["Export:ExcelServerFileDirectory:Value"]);
        // Excel生成客户端代码文件夹位置
        Define.ExcelClientFileDirectory = FileHelper.GetFullPath(root["Export:ExcelClientFileDirectory:Value"]);
        // Excel生成服务器二进制数据文件夹位置
        Define.ExcelServerBinaryDirectory = FileHelper.GetFullPath(root["Export:ExcelServerBinaryDirectory:Value"]);
        // Excel生成客户端二进制数据文件夹位置
        Define.ExcelClientBinaryDirectory = FileHelper.GetFullPath(root["Export:ExcelClientBinaryDirectory:Value"]);
        // Excel生成服务器Json数据文件夹位置
        Define.ExcelServerJsonDirectory = FileHelper.GetFullPath(root["Export:ExcelServerJsonDirectory:Value"]);
        // Excel生成客户端Json数据文件夹位置
        Define.ExcelClientJsonDirectory = FileHelper.GetFullPath(root["Export:ExcelClientJsonDirectory:Value"]);
        // Excel生成代码模板的位置
        Define.ExcelTemplatePath = FileHelper.GetFullPath(root["Export:ExcelTemplatePath:Value"]);
        // 服务器自定义导出代码文件夹位置
        Define.ServerCustomExportDirectory = FileHelper.GetFullPath(root["Export:ServerCustomExportDirectory:Value"]);
        // 客户端自定义导出代码
        Define.ClientCustomExportDirectory = FileHelper.GetFullPath(root["Export:ClientCustomExportDirectory:Value"]);
        // SceneConfig.xlsx的位置
        Define.SceneConfigPath = FileHelper.GetFullPath(root["Export:SceneConfigPath:Value"]);
        // 自定义导出代码存放的程序集
        Define.CustomExportAssembly = FileHelper.GetFullPath(root["Export:CustomExportAssembly:Value"]);
    }
}
#endif