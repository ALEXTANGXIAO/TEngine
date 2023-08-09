#if TENGINE_NET
namespace TEngine.Core;

public static class Define
{
#if TENGINE_NET

    #region ProtoBuf

    public static readonly char[] SplitChars = { ' ', '\t' };
    /// <summary>
    /// ProtoBuf文件夹
    /// </summary>
    public static string ProtoBufDirectory;
    /// <summary>
    /// 服务端生成文件夹
    /// </summary>
    public static string ProtoBufServerDirectory;
    /// <summary>
    /// 客户端生成文件夹
    /// </summary>
    public static string ProtoBufClientDirectory;
    /// <summary>
    /// 代码模板路径
    /// </summary>
    public static string ProtoBufTemplatePath;

    #endregion

    #region Excel

    /// <summary>
    /// 配置文件根目录
    /// </summary>
    public static string ExcelProgramPath;
    /// <summary>
    /// 版本文件Excel
    /// </summary>
    public static string ExcelVersionFile;
    /// <summary>
    /// 服务器代码生成文件夹
    /// </summary>
    public static string ExcelServerFileDirectory;
    /// <summary>
    /// 客户端代码生成文件夹
    /// </summary>
    public static string ExcelClientFileDirectory;
    /// <summary>
    /// 服务器二进制数据文件夹
    /// </summary>
    public static string ExcelServerBinaryDirectory;
    /// <summary>
    /// 客户端二进制数据文件夹
    /// </summary>
    public static string ExcelClientBinaryDirectory;
    /// <summary>
    /// 服务器Json数据文件夹
    /// </summary>
    public static string ExcelServerJsonDirectory;
    /// <summary>
    /// 客户端Json数据文件夹
    /// </summary>
    public static string ExcelClientJsonDirectory;
    /// <summary>
    /// 服务器自定义导出代码
    /// </summary>
    public static string ServerCustomExportDirectory;
    /// <summary>
    /// 客户端自定义导出代码
    /// </summary>
    public static string ClientCustomExportDirectory;
    /// <summary>
    /// 自定义导出代码存放的程序集
    /// </summary>
    public static string CustomExportAssembly;
    /// <summary>
    /// 导表支持的类型
    /// </summary>
    public static readonly HashSet<string> ColTypeSet = new HashSet<string>()
    {
        "", "0", "bool", "byte", "short", "ushort", "int", "uint", "long", "ulong", "float", "string", "AttrConfig",
        "IntDictionaryConfig", "StringDictionaryConfig",
        "short[]", "int[]", "long[]", "float[]", "string[]","uint[]"
    };
    /// <summary>
    /// Excel生成代码模板的位置
    /// </summary>
    public static string ExcelTemplatePath;
    /// <summary>
    /// 代码模板
    /// </summary>
    public static string ExcelTemplate
    {
        get
        {
            return _template ??= File.ReadAllText(ExcelTemplatePath);
        }
    }
    private static string _template;

    #endregion

    #region Network

    public static int SessionIdleCheckerInterval;
    public static int SessionIdleCheckerTimeout;

    #endregion

#endif
}
#endif