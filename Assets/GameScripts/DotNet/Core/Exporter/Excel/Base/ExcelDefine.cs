#if TENGINE_NET
namespace TEngine.Core;

public static class ExcelDefine
{
    /// <summary>
    /// 项目跟目录路径
    /// </summary>
    private const string ProjectPath = "../../..";
    /// <summary>
    /// 配置文件根目录
    /// </summary>
    public static string ProgramPath = $"{ProjectPath}/Config/Excel/";
    /// <summary>
    /// 版本文件Excel
    /// </summary>
    public static string ExcelVersionFile = $"{ProgramPath}Version.txt";
    /// <summary>
    /// 服务器代码生成文件夹
    /// </summary>
    public static string ServerFileDirectory = $"{ProjectPath}/Server/TEngine.Model/Generate/ConfigTable/Entity/";
    /// <summary>
    /// 客户端代码生成文件夹
    /// </summary>
    public static string ClientFileDirectory = $"{ProjectPath}/Client/Unity/Assets/Scripts/TEngine/TEngine.Model/Generate/ConfigTable/Entity/";
    /// <summary>
    /// 服务器二进制数据文件夹
    /// </summary>
    public static string ServerBinaryDirectory = $"{ProjectPath}/Config/Binary/";
    /// <summary>
    /// 客户端二进制数据文件夹
    /// </summary>
    public static string ClientBinaryDirectory = $"{ProjectPath}/Client/Unity/Assets/Bundles/Config/";
    /// <summary>
    /// 服务器Json数据文件夹
    /// </summary>
    public static string ServerJsonDirectory = $"{ProjectPath}/Config/Json/Server/";
    /// <summary>
    /// 客户端Json数据文件夹
    /// </summary>
    public static string ClientJsonDirectory = $"{ProjectPath}/Config/Json/Client/";
    /// <summary>
    /// 服务器自定义导出代码
    /// </summary>
    public static string ServerCustomExportDirectory = $"{ProjectPath}/Server/TEngine.Model/Generate/CustomExport/";
    /// <summary>
    /// 客户端自定义导出代码
    /// </summary>
    public static string ClientCustomExportDirectory = $"{ProjectPath}/Client/Unity/Assets/Scripts/TEngine/TEngine.Model/Generate/CustomExport/";
    /// <summary>
    /// 导表支持的类型
    /// </summary>
    public static readonly HashSet<string> ColTypeSet = new HashSet<string>()
    {
        "", "0", "bool", "byte", "short", "ushort", "int", "uint", "long", "ulong", "float", "string", "AttrConfig",
        "short[]", "int[]", "long[]", "float[]", "string[]"
    };
    /// <summary>
    /// Excel生成代码模板的位置
    /// </summary>
    public static string ExcelTemplatePath = $"{ProjectPath}/Config/Template/ExcelTemplate.txt";
    /// <summary>
    /// 代码模板
    /// </summary>
    public static string ExcelTemplate
    {
        get
        {
            return _template ??= File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"{ProjectPath}/Config/Template/ExcelTemplate.txt"));
        }
    }
    private static string _template;
}

#endif