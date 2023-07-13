#if TENGINE_NET
namespace TEngine.Core;

public static class ProtoBufDefine
{
    public static readonly char[] SplitChars = {' ', '\t'};
    /// <summary>
    /// 项目跟目录路径
    /// </summary>
    private const string ProjectPath = "../../..";
    /// <summary>
    /// ProtoBuf文件夹
    /// </summary>
    public static string ProtoBufDirectory = $"{ProjectPath}/Config/ProtoBuf/";
    /// <summary>
    /// 服务端生成文件夹
    /// </summary>
    public static string ServerDirectory = $"{ProjectPath}/Server/TEngine.Model/Generate/NetworkProtocol/";
    /// <summary>
    /// 客户端生成文件夹
    /// </summary>
    public static string ClientDirectory = $"{ProjectPath}/Client/Unity/Assets/Scripts/TEngine/TEngine.Model/Generate/NetworkProtocol/";
    /// <summary>
    /// 代码模板路径
    /// </summary>
    public static string ProtoBufTemplatePath = $"{ProjectPath}/Config/Template/ProtoTemplate.txt";
}
#endif
