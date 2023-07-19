#if TENGINE_NET
using System.Text;
using TEngine.Core;

namespace TEngine.Model;

public sealed class SceneTypeConfigToEnum : ACustomExport
{
    public override void Run()
    {
        var serverSceneType = new HashSet<string>();
        var instanceList = SceneConfigData.Instance.List;
        
        foreach (var sceneConfig in instanceList)
        {
            serverSceneType.Add(sceneConfig.SceneType);
        }

        if (serverSceneType.Count > 0)
        {
            Write(CustomExportType.Server, serverSceneType);
        }
    }

    private void Write(CustomExportType customExportType, HashSet<string> sceneTypes)
    {
        var index = 0;
        var strBuilder = new StringBuilder();
        var dicBuilder = new StringBuilder();

        strBuilder.AppendLine("namespace TEngine\n{");
        strBuilder.AppendLine("\t// 生成器自动生成，请不要手动编辑。");
        strBuilder.AppendLine("\tpublic class SceneType\n\t{");
        dicBuilder.AppendLine("\n\t\tpublic static readonly Dictionary<string, int> SceneDic = new Dictionary<string, int>()\n\t\t{");
        
        foreach (var str in sceneTypes)
        {
            index++;
            dicBuilder.AppendLine($"\t\t\t{{ \"{str}\", {index} }},");
            strBuilder.AppendLine($"\t\tpublic const int {str} = {index};");
        }

        dicBuilder.AppendLine("\t\t};");
        strBuilder.Append(dicBuilder);
        strBuilder.AppendLine("\t}\n}");
        Write("SceneType.cs", strBuilder.ToString(), customExportType);
    }
}
#endif