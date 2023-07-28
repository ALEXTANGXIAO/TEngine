#if TENGINE_NET
using System.Text;
using TEngine.Core;
using TEngine.Helper;

namespace TEngine.CustomExport;

public sealed class SceneTypeConfigToEnum : ACustomExport
{
    public override void Run()
    {
        var fullPath = FileHelper.GetFullPath("../../../Config/Excel/Server/SceneConfig.xlsx");
        using var excelPackage = ExcelHelper.LoadExcel(fullPath);
        var sceneType = new Dictionary<string, string>();
        var sceneSubType = new Dictionary<string, string>();
        var sceneTypeConfig = excelPackage.Workbook.Worksheets["SceneTypeConfig"];

        for (var row = 3; row <= sceneTypeConfig.Dimension.Rows; row++)
        {
            var sceneTypeId = sceneTypeConfig.GetCellValue(row, 1);
            var sceneTypeStr = sceneTypeConfig.GetCellValue(row, 2);

            if (string.IsNullOrEmpty(sceneTypeId) || string.IsNullOrEmpty(sceneTypeStr))
            {
                continue;
            }

            sceneType.Add(sceneTypeId, sceneTypeStr);
        }
            
        var sceneSubTypeConfig = excelPackage.Workbook.Worksheets["SceneSubTypeConfig"];
            
        for (var row = 3; row <= sceneSubTypeConfig.Dimension.Rows; row++)
        {
            var sceneSubTypeId = sceneSubTypeConfig.GetCellValue(row, 1);
            var sceneSubTypeStr = sceneSubTypeConfig.GetCellValue(row, 2);

            if (string.IsNullOrEmpty(sceneSubTypeId) || string.IsNullOrEmpty(sceneSubTypeStr))
            {
                continue;
            }

            sceneSubType.Add(sceneSubTypeId, sceneSubTypeStr);
        }

        if (sceneType.Count > 0 || sceneSubType.Count > 0)
        {
            Write(CustomExportType.Server, sceneType, sceneSubType);
        }
    }

    private void Write(CustomExportType customExportType, Dictionary<string, string> sceneTypes, Dictionary<string, string> sceneSubType)
    {
        var strBuilder = new StringBuilder();
        var dicBuilder = new StringBuilder();

        strBuilder.AppendLine("namespace TEngine\n{");
        strBuilder.AppendLine("\t// 生成器自动生成，请不要手动编辑。");
        strBuilder.AppendLine("\tpublic static class SceneType\n\t{");
        dicBuilder.AppendLine("\n\t\tpublic static readonly Dictionary<string, int> SceneTypeDic = new Dictionary<string, int>()\n\t\t{");

        foreach (var (sceneTypeId, sceneTypeStr) in sceneTypes)
        {
            dicBuilder.AppendLine($"\t\t\t{{ \"{sceneTypeStr}\", {sceneTypeId} }},");
            strBuilder.AppendLine($"\t\tpublic const int {sceneTypeStr} = {sceneTypeId};");
        }

        dicBuilder.AppendLine("\t\t};");
        strBuilder.Append(dicBuilder);
        strBuilder.AppendLine("\t}\n");
        
        strBuilder.AppendLine("\t// 生成器自动生成，请不要手动编辑。");
        strBuilder.AppendLine("\tpublic static class SceneSubType\n\t{");
        
        dicBuilder.Clear();
        dicBuilder.AppendLine("\n\t\tpublic static readonly Dictionary<string, int> SceneSubTypeDic = new Dictionary<string, int>()\n\t\t{");
        foreach (var (sceneSubTypeId, sceneSubTypeStr) in sceneSubType)
        {
            dicBuilder.AppendLine($"\t\t\t{{ \"{sceneSubTypeStr}\", {sceneSubTypeId} }},");
            strBuilder.AppendLine($"\t\tpublic const int {sceneSubTypeStr} = {sceneSubTypeId};");
        }
        dicBuilder.AppendLine("\t\t};");
        strBuilder.Append(dicBuilder);
        strBuilder.AppendLine("\t}\n}");
        
        Write("SceneType.cs", strBuilder.ToString(), customExportType);
    }
}
#endif