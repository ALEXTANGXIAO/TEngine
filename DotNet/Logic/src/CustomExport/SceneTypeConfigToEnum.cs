#if TENGINE_NET
using System.Text;
using TEngine.Core;
using TEngine.Helper;

namespace TEngine.Model;

/// <summary>
/// 将场景类型配置表转换为枚举和字典的自定义导出类。
/// </summary>
public sealed class SceneTypeConfigToEnum : ACustomExport
{
    /// <summary>
    /// 执行自定义导出操作。
    /// </summary>
    public override void Run()
    {
        // 获取场景配置表的完整路径
        using var excelPackage = ExcelHelper.LoadExcel(Define.SceneConfigPath);
        var sceneType = new Dictionary<string, string>();
        var sceneSubType = new Dictionary<string, string>();
        // 获取场景类型配置工作表
        var sceneTypeConfig = excelPackage.Workbook.Worksheets["SceneTypeConfig"];
        // 遍历场景类型配置表的行
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
        // 获取场景子类型配置工作表
        var sceneSubTypeConfig = excelPackage.Workbook.Worksheets["SceneSubTypeConfig"];
        // 遍历场景子类型配置表的行 
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
        // 如果存在场景类型或场景子类型，执行导出操作
        if (sceneType.Count > 0 || sceneSubType.Count > 0)
        {
            Write(CustomExportType.Server, sceneType, sceneSubType);
        }
    }

    private void Write(CustomExportType customExportType, Dictionary<string, string> sceneTypes, Dictionary<string, string> sceneSubType)
    {
        var strBuilder = new StringBuilder();
        var dicBuilder = new StringBuilder();

        // 添加命名空间和注释头部
        strBuilder.AppendLine("namespace TEngine\n{");
        strBuilder.AppendLine("\t// 生成器自动生成，请不要手动编辑。");
        // 生成场景类型的静态类
        strBuilder.AppendLine("\tpublic static class SceneType\n\t{");
        dicBuilder.AppendLine("\n\t\tpublic static readonly Dictionary<string, int> SceneTypeDic = new Dictionary<string, int>()\n\t\t{");

        // 遍历场景类型字典，生成场景类型的常量和字典项
        foreach (var (sceneTypeId, sceneTypeStr) in sceneTypes)
        {
            dicBuilder.AppendLine($"\t\t\t{{ \"{sceneTypeStr}\", {sceneTypeId} }},");
            strBuilder.AppendLine($"\t\tpublic const int {sceneTypeStr} = {sceneTypeId};");
        }

        // 添加场景类型字典尾部，合并到主字符串构建器中
        dicBuilder.AppendLine("\t\t};");
        strBuilder.Append(dicBuilder);
        strBuilder.AppendLine("\t}\n");

        // 生成场景子类型的静态类
        strBuilder.AppendLine("\t// 生成器自动生成，请不要手动编辑。");
        strBuilder.AppendLine("\tpublic static class SceneSubType\n\t{");
        // 清空字典构建器，用于生成场景子类型的字典项
        dicBuilder.Clear();
        dicBuilder.AppendLine("\n\t\tpublic static readonly Dictionary<string, int> SceneSubTypeDic = new Dictionary<string, int>()\n\t\t{");
        // 遍历场景子类型字典，生成场景子类型的常量和字典项
        foreach (var (sceneSubTypeId, sceneSubTypeStr) in sceneSubType)
        {
            dicBuilder.AppendLine($"\t\t\t{{ \"{sceneSubTypeStr}\", {sceneSubTypeId} }},");
            strBuilder.AppendLine($"\t\tpublic const int {sceneSubTypeStr} = {sceneSubTypeId};");
        }

        // 添加场景子类型字典尾部，合并到主字符串构建器中
        dicBuilder.AppendLine("\t\t};");
        strBuilder.Append(dicBuilder);
        strBuilder.AppendLine("\t}\n}");

        // 调用外部方法将生成的代码写入文件
        Write("SceneType.cs", strBuilder.ToString(), customExportType);
    }
}
#endif