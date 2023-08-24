#if TENGINE_NET
using System.Text;
using TEngine.Core.Network;

#pragma warning disable CS8604
#pragma warning disable CS8602
#pragma warning disable CS8600
#pragma warning disable CS8618

namespace TEngine.Core;

public enum ProtoBufOpCodeType
{
    None = 0,
    Outer = 1,
    Inner = 2,
    InnerBson = 3,
}

public sealed class OpcodeInfo
{
    public uint Code;
    public string Name;
}

public sealed class ProtoBufExporter
{
    private uint _aMessage;
    private uint _aRequest;
    private uint _aResponse;
    private uint _aRouteMessage;
    private uint _aRouteRequest;
    private uint _aRouteResponse;
    private string _serverTemplate;
    private string _clientTemplate;

    private readonly List<OpcodeInfo> _opcodes = new();

    public ProtoBufExporter()
    {
        Console.OutputEncoding = Encoding.UTF8;

        if (!Directory.Exists(Define.ProtoBufServerDirectory))
        {
            Directory.CreateDirectory(Define.ProtoBufServerDirectory);
        }

        if (!Directory.Exists(Define.ProtoBufClientDirectory))
        {
            Directory.CreateDirectory(Define.ProtoBufClientDirectory);
        }
        
        if (!Directory.Exists($"{Define.ProtoBufDirectory}Outer"))
        {
            Directory.CreateDirectory($"{Define.ProtoBufDirectory}Outer");
        }
                
        if (!Directory.Exists($"{Define.ProtoBufDirectory}Inner"))
        {
            Directory.CreateDirectory($"{Define.ProtoBufDirectory}Inner");
        }
        
        if (!Directory.Exists($"{Define.ProtoBufDirectory}Bson"))
        {
            Directory.CreateDirectory($"{Define.ProtoBufDirectory}Bson");
        }

        var tasks = new Task[2];
        tasks[0] = Task.Run(RouteType);
        tasks[1] = Task.Run(async () =>
        {
            LoadTemplate();
            await Start(ProtoBufOpCodeType.Outer);
            await Start(ProtoBufOpCodeType.Inner);
            await Start(ProtoBufOpCodeType.InnerBson);
        });
        Task.WaitAll(tasks);
    }

    private async Task Start(ProtoBufOpCodeType opCodeType)
    {
        List<string> files = new List<string>();
        var opCodeName = "";
        OpcodeInfo opcodeInfo = null;
        _opcodes.Clear();
        var file = new StringBuilder();
        var saveDirectory = new Dictionary<string, string>();

        switch (opCodeType)
        {
            case ProtoBufOpCodeType.Outer:
            {
                _aMessage = Opcode.OuterMessage;
                _aRequest = Opcode.OuterRequest;
                _aResponse = Opcode.OuterResponse;
                _aRouteMessage = Opcode.OuterRouteMessage;
                _aRouteRequest = Opcode.OuterRouteRequest;
                _aRouteResponse = Opcode.OuterRouteResponse;
                opCodeName = "OuterOpcode";
                saveDirectory.Add(Define.ProtoBufServerDirectory, _serverTemplate);
                saveDirectory.Add(Define.ProtoBufClientDirectory, _clientTemplate);
                files.Add($"{Define.ProtoBufDirectory}OuterMessage.proto");
                files.AddRange(Directory.GetFiles($"{Define.ProtoBufDirectory}Outer").ToList());
                break;
            }
            case ProtoBufOpCodeType.Inner:
            {
                // 预留1000个协议号给框架内部协议用
                _aMessage = Opcode.InnerMessage + 1000;
                _aRequest = Opcode.InnerRequest + 1000;
                _aResponse = Opcode.InnerResponse + 1000;
                _aRouteMessage = Opcode.InnerRouteMessage + 1000;
                _aRouteRequest = Opcode.InnerRouteRequest + 1000;
                _aRouteResponse = Opcode.InnerRouteResponse + 1000;
                opCodeName = "InnerOpcode";
                saveDirectory.Add(Define.ProtoBufServerDirectory, _serverTemplate);
                files.Add($"{Define.ProtoBufDirectory}InnerMessage.proto");
                files.AddRange(Directory.GetFiles($"{Define.ProtoBufDirectory}Inner").ToList());
                break;
            }
            case ProtoBufOpCodeType.InnerBson:
            {
                // 预留1000个协议号给框架内部协议用
                _aMessage = Opcode.InnerBsonMessage + 1000;
                _aRequest = Opcode.InnerBsonRequest + 1000;
                _aResponse = Opcode.InnerBsonResponse + 1000;
                _aRouteMessage = Opcode.InnerBsonRouteMessage + 1000;
                _aRouteRequest = Opcode.InnerBsonRouteRequest + 1000;
                _aRouteResponse = Opcode.InnerBsonRouteResponse + 1000;
                opCodeName = "InnerBsonOpcode";
                saveDirectory.Add(Define.ProtoBufServerDirectory, _serverTemplate);
                files.Add($"{Define.ProtoBufDirectory}InnerBsonMessage.proto");
                files.AddRange(Directory.GetFiles($"{Define.ProtoBufDirectory}Bson").ToList());
                break;
            }
        }

        #region GenerateProtoFiles
        foreach (var filePath in files)
        {
            var parameter = "";
            var className = "";
            var isMsgHead = false;
            string responseTypeStr = null;
            string customRouteType = null;
            var protoFileText = await File.ReadAllTextAsync(filePath);

            foreach (var line in protoFileText.Split('\n'))
            {
                var currentLine = line.Trim();

                if (string.IsNullOrWhiteSpace(currentLine))
                {
                    continue;
                }

                if (currentLine.StartsWith("///"))
                {
                    file.AppendFormat("	/// <summary>\r\n" + "	/// {0}\r\n" + "	/// </summary>\r\n", currentLine.TrimStart(new[] { '/', '/', '/' }));
                    continue;
                }

                if (currentLine.StartsWith("message"))
                {
                    isMsgHead = true;
                    opcodeInfo = new OpcodeInfo();
                    file.AppendLine("\t[ProtoContract]");
                    className = currentLine.Split(Define.SplitChars, StringSplitOptions.RemoveEmptyEntries)[1];
                    var splits = currentLine.Split(new[] { "//" }, StringSplitOptions.RemoveEmptyEntries);

                    if (splits.Length > 1)
                    {
                        var parameterArray = currentLine.Split(new[] { "//" }, StringSplitOptions.RemoveEmptyEntries)[1].Trim().Split(',');
                        parameter = parameterArray[0].Trim();

                        switch (parameterArray.Length)
                        {
                            case 2:
                                responseTypeStr = parameterArray[1].Trim();
                                break;
                            case 3:
                            {
                                customRouteType = parameterArray[1].Trim();

                                if (parameterArray.Length == 3)
                                {
                                    responseTypeStr = parameterArray[2].Trim();
                                }

                                break;
                            }
                        }
                    }
                    else
                    {
                        parameter = "";
                    }

                    file.Append(string.IsNullOrWhiteSpace(parameter)
                            ? $"\tpublic partial class {className} : AProto"
                            : $"\tpublic partial class {className} : AProto, {parameter}");
                    opcodeInfo.Name = className;
                    continue;
                }

                if (!isMsgHead)
                {
                    continue;
                }

                switch (currentLine)
                {
                    case "{":
                    {
                        file.AppendLine("\n\t{");

                        if (string.IsNullOrWhiteSpace(parameter) || parameter == "IMessage")
                        {
                            opcodeInfo.Code += ++_aMessage;
                            file.AppendLine($"\t\tpublic uint OpCode() {{ return {opCodeName}.{className}; }}");
                        }
                        else
                        {
                            if (responseTypeStr != null)
                            {
                                file.AppendLine("\t\t[ProtoIgnore]");
                                file.AppendLine($"\t\tpublic {responseTypeStr} ResponseType {{ get; set; }}");
                                responseTypeStr = null;
                            }
                            else
                            {
                                if (parameter.Contains("RouteRequest"))
                                {
                                    Exporter.LogError($"{opcodeInfo.Name} 没指定ResponseType");
                                }
                            }

                            file.AppendLine($"\t\tpublic uint OpCode() {{ return {opCodeName}.{className}; }}");

                            if (customRouteType != null)
                            {
                                file.AppendLine($"\t\tpublic long RouteTypeOpCode() {{ return (long)RouteType.{customRouteType}; }}");
                                customRouteType = null;
                            }
                            else if (parameter is "IAddressableRouteRequest" or "IAddressableRouteMessage")
                            {
                                file.AppendLine($"\t\tpublic long RouteTypeOpCode() {{ return CoreRouteType.Addressable; }}");
                            }
                            else if (parameter.EndsWith("BsonRouteMessage") || parameter.EndsWith("BsonRouteRequest"))
                            {
                                file.AppendLine($"\t\tpublic long RouteTypeOpCode() {{ return CoreRouteType.BsonRoute; }}");
                            }
                            else if (parameter is "IRouteMessage" or "IRouteRequest")
                            {
                                file.AppendLine($"\t\tpublic long RouteTypeOpCode() {{ return CoreRouteType.Route; }}");
                            }

                            switch (parameter)
                            {
                                case "IRequest":
                                case "IBsonRequest":
                                {
                                    opcodeInfo.Code += ++_aRequest;
                                    break;
                                }
                                case "IResponse":
                                case "IBsonResponse":
                                {
                                    opcodeInfo.Code += ++_aResponse;
                                    file.AppendLine("\t\t[ProtoMember(91, IsRequired = true)]");
                                    file.AppendLine("\t\tpublic uint ErrorCode { get; set; }");
                                    break;
                                }
                                default:
                                {
                                    if (parameter.EndsWith("RouteMessage") || parameter == "IRouteMessage")
                                    {
                                        opcodeInfo.Code += ++_aRouteMessage;
                                    }
                                    else if (parameter.EndsWith("RouteRequest") || parameter == "IRouteRequest")
                                    {
                                        opcodeInfo.Code += ++_aRouteRequest;
                                    }
                                    else if (parameter.EndsWith("RouteResponse") || parameter == "IRouteResponse")
                                    {
                                        opcodeInfo.Code += ++_aRouteResponse;
                                        file.AppendLine("\t\t[ProtoMember(91, IsRequired = true)]");
                                        file.AppendLine("\t\tpublic uint ErrorCode { get; set; }");
                                    }

                                    break;
                                }
                            }
                        }

                        _opcodes.Add(opcodeInfo);
                        continue;
                    }
                    case "}":
                    {
                        isMsgHead = false;
                        file.AppendLine("\t}");
                        continue;
                    }
                    case "":
                    {
                        continue;
                    }
                }

                if (currentLine.StartsWith("//"))
                {
                    file.AppendFormat("\t\t///<summary>\r\n" + "\t\t/// {0}\r\n" + "\t\t///</summary>\r\n", currentLine.TrimStart('/', '/'));
                    continue;
                }

                if (currentLine.StartsWith("repeated"))
                {
                    Repeated(file, currentLine);
                }
                else
                {
                    Members(file, currentLine);
                }
            }

            var csName = $"{Path.GetFileNameWithoutExtension(filePath)}.cs";

            foreach (var (directory, template) in saveDirectory)
            {
                var csFile = Path.Combine(directory, csName);
                var content = template.Replace("(Content)", file.ToString());
                await File.WriteAllTextAsync(csFile, content);
            }

            file.Clear();
        }
        #endregion
        
        #region GenerateOpCode
        file.Clear();
        file.AppendLine("namespace TEngine");
        file.AppendLine("{");
        file.AppendLine($"\tpublic static partial class {opCodeName}");
        file.AppendLine("\t{");

        foreach (var opcode in _opcodes)
        {
            file.AppendLine($"\t\t public const int {opcode.Name} = {opcode.Code};");
        }

        _opcodes.Clear();

        file.AppendLine("\t}");
        file.AppendLine("}");

        foreach (var (directory, _) in saveDirectory)
        {
            var csFile = Path.Combine(directory, $"{opCodeName}.cs");
            await File.WriteAllTextAsync(csFile, file.ToString());
        }

        #endregion
    }

    private async Task RouteType()
    {
        var routeTypeFile = $"{Define.ProtoBufDirectory}RouteType.Config";
        var protoFileText = await File.ReadAllTextAsync(routeTypeFile);
        var routeTypeFileSb = new StringBuilder();
        routeTypeFileSb.AppendLine("namespace TEngine.Core.Network\n{");
        routeTypeFileSb.AppendLine("\t// Route协议定义(需要定义1000以上、因为1000以内的框架预留)\t");
        routeTypeFileSb.AppendLine("\tpublic enum RouteType : long\n\t{");

        foreach (var line in protoFileText.Split('\n'))
        {
            var currentLine = line.Trim();

            if (currentLine.StartsWith("//"))
            {
                continue;
            }

            var splits = currentLine.Split(new[] { "//" }, StringSplitOptions.RemoveEmptyEntries);
            var routeTypeStr = splits[0].Split("=", StringSplitOptions.RemoveEmptyEntries);
            routeTypeFileSb.Append($"\t\t{routeTypeStr[0].Trim()} = {routeTypeStr[1].Trim()},");

            if (splits.Length > 1)
            {
                routeTypeFileSb.Append($" // {splits[1].Trim()}\n");
            }
            else
            {
                routeTypeFileSb.Append('\n');
            }
        }

        routeTypeFileSb.AppendLine("\t}\n}");
        var file = routeTypeFileSb.ToString();
        await File.WriteAllTextAsync($"{Define.ProtoBufServerDirectory}RouteType.cs", file);
        await File.WriteAllTextAsync($"{Define.ProtoBufClientDirectory}RouteType.cs", file);
    }

    private void Repeated(StringBuilder file, string newline)
    {
        try
        {
            var index = newline.IndexOf(";", StringComparison.Ordinal);
            newline = newline.Remove(index);
            var property = newline.Split(Define.SplitChars, StringSplitOptions.RemoveEmptyEntries);
            var type = property[1];
            var name = property[2];
            var memberIndex = int.Parse(property[4]);
            type = ConvertType(type);

            file.AppendLine($"\t\t[ProtoMember({memberIndex})]");
            file.AppendLine($"\t\tpublic List<{type}> {name} = new List<{type}>();");
        }
        catch (Exception e)
        {
            Exporter.LogError($"{newline}\n {e}");
        }
    }

    private void Members(StringBuilder file, string currentLine)
    {
        try
        {
            var index = currentLine.IndexOf(";", StringComparison.Ordinal);
            currentLine = currentLine.Remove(index);
            var property = currentLine.Split(Define.SplitChars, StringSplitOptions.RemoveEmptyEntries);
            var type = property[0];
            var name = property[1];
            var memberIndex = int.Parse(property[3]);
            var typeCs = ConvertType(type);
            string defaultValue = GetDefault(typeCs);

            file.AppendLine($"\t\t[ProtoMember({memberIndex})]");
            file.AppendLine($"\t\tpublic {typeCs} {name} {{ get; set; }}");
        }
        catch (Exception e)
        {
            Exporter.LogError($"{currentLine}\n {e}");
        }
    }

    private string ConvertType(string type)
    {
        return type switch
        {
            "int[]" => "int[] { }",
            "int32[]" => "int[] { }",
            "int64[]" => "long[] { }",
            "int32" => "int",
            "uint32" => "uint",
            "int64" => "long",
            "uint64" => "ulong",
            _ => type
        };
    }

    private string GetDefault(string type)
    {
        type = type.Trim();

        switch (type)
        {
            case "byte":
            case "short":
            case "int":
            case "long":
            case "float":
            case "double":
                return "0";
            case "bool":
                return "false";
            default:
                return "null";
        }
    }

    private void LoadTemplate()
    {
        string[] lines = File.ReadAllLines(Define.ProtoBufTemplatePath, Encoding.UTF8);

        StringBuilder serverSb = new StringBuilder();
        StringBuilder clientSb = new StringBuilder();

        int flag = 0;
        foreach (string line in lines)
        {
            string trim = line.Trim();

            if (trim.StartsWith("#if") && trim.Contains("SERVER"))
            {
                flag = 1;
                continue;
            }
            else if (trim.StartsWith("#else"))
            {
                flag = 2;
                continue;
            }
            else if (trim.StartsWith($"#endif"))
            {
                flag = 0;
                continue;
            }

            switch (flag)
            {
                case 1: // 服务端
                {
                    serverSb.AppendLine(line);
                    break;
                }
                case 2: // 客户端
                {
                    clientSb.AppendLine(line);
                    break;
                }
                default: // 双端
                {
                    serverSb.AppendLine(line);
                    clientSb.AppendLine(line);
                    break;
                }
            }
        }

        _serverTemplate = serverSb.ToString();
        _clientTemplate = clientSb.ToString();
    }
}
#endif