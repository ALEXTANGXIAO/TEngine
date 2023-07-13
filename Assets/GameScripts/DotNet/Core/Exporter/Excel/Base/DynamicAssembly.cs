#if TENGINE_NET
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using ProtoBuf;

#pragma warning disable CS8600
#pragma warning disable CS8601

namespace TEngine.Core;

public static class DynamicAssembly
{
    public static Assembly Load(string path)
    {
        var fileList = new List<string>();

        // 找到所有需要加载的CS文件

        foreach (string file in Directory.GetFiles(path))
        {
            if (Path.GetExtension(file) != ".cs")
            {
                continue;
            }

            fileList.Add(file);
        }

        var syntaxTreeList = new List<SyntaxTree>();

        foreach (var file in fileList)
        {
            using var fileStream = new StreamReader(file);
            var cSharp = CSharpSyntaxTree.ParseText(fileStream.ReadToEnd());
            syntaxTreeList.Add(cSharp);
        }

        AssemblyMetadata assemblyMetadata;
        MetadataReference metadataReference;
        var currentDomain = AppDomain.CurrentDomain;
        var assemblyName = Path.GetRandomFileName();
        var assemblyArray = currentDomain.GetAssemblies();
        var metadataReferenceList = new List<MetadataReference>();

        // 注册引用

        foreach (var domainAssembly in assemblyArray)
        {
            if (string.IsNullOrEmpty(domainAssembly.Location))
            {
                continue;
            }

            assemblyMetadata = AssemblyMetadata.CreateFromFile(domainAssembly.Location);
            metadataReference = assemblyMetadata.GetReference();
            metadataReferenceList.Add(metadataReference);
        }

        // 添加ProtoEntity支持

        assemblyMetadata = AssemblyMetadata.CreateFromFile(typeof(AProto).Assembly.Location);
        metadataReference = assemblyMetadata.GetReference();
        metadataReferenceList.Add(metadataReference);

        // 添加ProtoBuf.net支持

        assemblyMetadata = AssemblyMetadata.CreateFromFile(typeof(ProtoMemberAttribute).Assembly.Location);
        metadataReference = assemblyMetadata.GetReference();
        metadataReferenceList.Add(metadataReference);

        CSharpCompilation compilation = CSharpCompilation.Create(assemblyName, syntaxTreeList, metadataReferenceList,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        using var ms = new MemoryStream();

        var result = compilation.Emit(ms);
        if (!result.Success)
        {
            foreach (var resultDiagnostic in result.Diagnostics)
            {
                Exporter.LogError(resultDiagnostic.GetMessage());
            }

            throw new Exception("failures");
        }

        ms.Seek(0, SeekOrigin.Begin);
        return Assembly.Load(ms.ToArray());
    }
    
    public static DynamicConfigDataType GetDynamicInfo(Assembly dynamicAssembly, string tableName)
    {
        var dynamicConfigDataType = new DynamicConfigDataType
        {
            ConfigDataType = GetConfigType(dynamicAssembly, $"{tableName}Data"),
            ConfigType = GetConfigType(dynamicAssembly, $"{tableName}")
        };

        dynamicConfigDataType.ConfigData = CreateInstance(dynamicConfigDataType.ConfigDataType);
        var listPropertyType = dynamicConfigDataType.ConfigDataType.GetProperty("List");

        if (listPropertyType == null)
        {
            throw new Exception("No Property named Add was found");
        }

        dynamicConfigDataType.Obj = listPropertyType.GetValue(dynamicConfigDataType.ConfigData);
        dynamicConfigDataType.Method = listPropertyType.PropertyType.GetMethod("Add");

        if (dynamicConfigDataType.Method == null)
        {
            throw new Exception("No method named Add was found");
        }

        return dynamicConfigDataType;
    }
    
    private static Type GetConfigType(Assembly dynamicAssembly, string typeName)
    {
        var configType = dynamicAssembly.GetType($"TEngine.{typeName}");
        
        if (configType == null)
        {
            throw new FileNotFoundException($"TEngine.{typeName} not found");
        }
        
        return configType;
        // return dynamicAssembly.GetType($"TEngine.{typeName}");
    }
    
    public static AProto CreateInstance(Type configType)
    {
        var config = (AProto) Activator.CreateInstance(configType);
        
        if (config == null)
        {
            throw new Exception($"{configType.Name} is Activator.CreateInstance error");
        }

        return config;
    }
}
#endif