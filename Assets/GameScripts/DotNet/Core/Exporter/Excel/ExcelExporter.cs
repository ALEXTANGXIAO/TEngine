#if TENGINE_NET
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Text.RegularExpressions;
using TEngine.CustomExport;
using TEngine.DataStructure;
using Newtonsoft.Json;
using OfficeOpenXml;
using TEngine.Helper;
using static System.String;

namespace TEngine.Core;

using TableDictionary = SortedDictionary<string, List<int>>;

public sealed class ExcelExporter
{
    private Dictionary<string, long> _versionDic;
    private readonly Regex _regexName = new Regex("^[a-zA-Z][a-zA-Z0-9_]*$");
    private readonly HashSet<string> _loadFiles = new HashSet<string> {".xlsx", ".xlsm", ".csv"};
    private readonly OneToManyList<string, ExportInfo> _tables = new OneToManyList<string, ExportInfo>();
    private readonly ConcurrentDictionary<string, ExcelTable> _excelTables = new ConcurrentDictionary<string, ExcelTable>();
    private readonly ConcurrentDictionary<string, ExcelWorksheet> _worksheets = new ConcurrentDictionary<string, ExcelWorksheet>();

    static ExcelExporter()
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    }
    
    public ExcelExporter(ExportType exportType)
    {
        var versionFilePath = Define.ExcelVersionFile;
        
        switch (exportType)
        {
            case ExportType.AllExcelIncrement:
            {
                break;
            }
            case ExportType.AllExcel:
            {
                if (File.Exists(versionFilePath))
                {
                    File.Delete(versionFilePath);
                }

                FileHelper.ClearDirectoryFile(Define.ExcelServerFileDirectory);
                FileHelper.ClearDirectoryFile(Define.ExcelClientFileDirectory);
                break;
            }
        }

        Find();
        Parsing();
        ExportToBinary();
        File.WriteAllText(versionFilePath, JsonConvert.SerializeObject(_versionDic));
        CustomExport();
    }
    
    private static void CustomExport()
    {
        // 清除文件夹
        FileHelper.ClearDirectoryFile(Define.ServerCustomExportDirectory);
        FileHelper.ClearDirectoryFile(Define.ClientCustomExportDirectory);
        // 找到程序集
        var assemblyLoadContext = new AssemblyLoadContext("ExporterDll", true);
        var dllBytes = File.ReadAllBytes($"{Define.CustomExportAssembly}.dll");
        var pdbBytes = File.ReadAllBytes($"{Define.CustomExportAssembly}.pdb");
        var assembly = assemblyLoadContext.LoadFromStream(new MemoryStream(dllBytes), new MemoryStream(pdbBytes));
        // 加载程序集
        AssemblyManager.LoadAssembly(int.MaxValue, assembly);
        
        // 执行自定义导出
        
        var task = new List<Task>();
        
        foreach (var type in AssemblyManager.ForEach(typeof(ICustomExport)))
        {
            var customExport = (ICustomExport)Activator.CreateInstance(type);

            if (customExport != null)
            {
                task.Add(Task.Run(customExport.Run));
            }
        }
        
        // 添加生成SceneType的自定义导出
        task.Add(Task.Run(new SceneTypeConfigToEnum().Run));

        Task.WaitAll(task.ToArray());
    }
    
    /// <summary>
    /// 查找配置文件
    /// </summary>
    private void Find()
    {
        var versionFilePath = Define.ExcelVersionFile;
        
        if(File.Exists(versionFilePath))
        {
            var versionJson = File.ReadAllText(versionFilePath);
            _versionDic = JsonConvert.DeserializeObject<Dictionary<string, long>>(versionJson);
        }
        else
        {
            _versionDic = new Dictionary<string, long>();
        }
        
        var dir = new DirectoryInfo(Define.ExcelProgramPath);
        var excelFiles = dir.GetFiles("*", SearchOption.AllDirectories);

        if (excelFiles.Length <= 0)
        {
            return;
        }

        foreach (var excelFile in excelFiles)
        {
            // 过滤掉非指定后缀的文件

            if (!_loadFiles.Contains(excelFile.Extension))
            {
                continue;
            }

            var lastIndexOf = excelFile.Name.LastIndexOf(".", StringComparison.Ordinal);

            if (lastIndexOf < 0)
            {
                continue;
            }

            var fullName = excelFile.FullName;
            var excelName = excelFile.Name.Substring(0, lastIndexOf);
            var path = fullName.Substring(0, fullName.Length - excelFile.Name.Length);

            // 过滤#和~开头文件和文件夹带有#的所有文件

            if (excelName.StartsWith("#", StringComparison.Ordinal) ||
                excelName.StartsWith("~", StringComparison.Ordinal) ||
                path.Contains("#", StringComparison.Ordinal))
            {
                continue;
            }

            if (!_regexName.IsMatch(excelName))
            {
                Exporter.LogError($"{excelName} 配置文件名非法");
                continue;
            }

            _tables.Add(excelName.Split('_')[0], new ExportInfo()
            {
                Name = excelName, FileInfo = excelFile
            });
        }
        
        var removeTables = new List<string>();

        foreach (var (tableName, tableList) in _tables)
        {
            var isNeedExport = false;

            foreach (var exportInfo in tableList)
            {
                var timer = TimeHelper.Transition(exportInfo.FileInfo.LastWriteTime);

                if (!isNeedExport)
                {
                    if (_versionDic.TryGetValue(exportInfo.Name, out var lastWriteTime))
                    {
                        isNeedExport = lastWriteTime != timer;
                    }
                    else
                    {
                        isNeedExport = true;
                    }
                }

                _versionDic[exportInfo.Name] = timer;
            }

            if (!isNeedExport)
            {
                removeTables.Add(tableName);
            }
        }
        
        foreach (var removeTable in removeTables)
        {
            _tables.Remove(removeTable);
        }
        
        foreach (var (_, exportInfo) in _tables)
        {
            exportInfo.Sort((x, y) => Compare(x.Name, y.Name, StringComparison.Ordinal));
        }
    }

    /// <summary>
    /// 生成配置文件
    /// </summary>
    private void Parsing()
    {
        var generateTasks = new List<Task>();

        foreach (var (tableName, tableList) in _tables)
        {
            var task = Task.Run(() =>
            {
                var writeToClassTask = new List<Task>();
                var excelTable = new ExcelTable(tableName);

                // 筛选需要导出的列

                foreach (var exportInfo in tableList)
                {
                    try
                    {
                        var serverColInfoList = new List<int>();
                        var clientColInfoList = new List<int>();
                        var worksheet = LoadExcel(exportInfo.FileInfo.FullName, true);

                        for (var col = 3; col <= worksheet.Columns.EndColumn; col++)
                        {
                            // 列名字第一个字符是#不参与导出

                            var colName = worksheet.GetCellValue(5, col);
                            if (colName.StartsWith("#", StringComparison.Ordinal))
                            {
                                continue;
                            }

                            // 数值列不参与导出

                            var numericalCol = worksheet.GetCellValue(3, col);
                            if (numericalCol != "" && numericalCol != "0")
                            {
                                continue;
                            }

                            var serverType = worksheet.GetCellValue(1, col);
                            var clientType = worksheet.GetCellValue(2, col);
                            var isExportServer = !IsNullOrEmpty(serverType) && serverType != "0";
                            var isExportClient = !IsNullOrEmpty(clientType) && clientType != "0";

                            if (!isExportServer && !isExportClient)
                            {
                                continue;
                            }

                            if (isExportServer && isExportClient & serverType != clientType)
                            {
                                Exporter.LogError($"配置表 {exportInfo.Name} {col} 列 [{colName}] 客户端类型 {clientType} 和 服务端类型 {serverType} 不一致");
                                continue;
                            }

                            if (!Define.ColTypeSet.Contains(serverType) ||
                                !Define.ColTypeSet.Contains(clientType))
                            {
                                Exporter.LogError($"配置表 {exportInfo.Name} {col} 列 [{colName}] 客户端类型 {clientType}, 服务端类型 {serverType} 不合法");
                                continue;
                            }

                            if (!_regexName.IsMatch(colName))
                            {
                                Exporter.LogError($"配置表 {exportInfo.Name} {col} 列 [{colName}] 列名非法");
                                continue;
                            }

                            serverColInfoList.Add(col);

                            if (isExportClient)
                            {
                                clientColInfoList.Add(col);
                            }
                        }

                        if (clientColInfoList.Count > 0)
                        {
                            excelTable.ClientColInfos.Add(exportInfo.FileInfo.FullName, clientColInfoList);
                        }

                        if (serverColInfoList.Count > 0)
                        {
                            excelTable.ServerColInfos.Add(exportInfo.FileInfo.FullName, serverColInfoList);
                        }
                    }
                    catch (Exception e)
                    {
                        Exporter.LogError($"Config : {tableName}, Name : {exportInfo.Name}, Error : {e}");
                    }
                }

                // 生成cs文件

                writeToClassTask.Add(Task.Run(() =>
                {
                    WriteToClass(excelTable.ServerColInfos, Define.ExcelServerFileDirectory, true);
                }));

                writeToClassTask.Add(Task.Run(() =>
                {
                    WriteToClass(excelTable.ClientColInfos, Define.ExcelClientFileDirectory, false);
                }));

                Task.WaitAll(writeToClassTask.ToArray());
                _excelTables.TryAdd(tableName, excelTable);
            });

            generateTasks.Add(task);
        }

        Task.WaitAll(generateTasks.ToArray());
    }

    /// <summary>
    /// 写入到cs
    /// </summary>
    /// <param name="colInfos"></param>
    /// <param name="exportPath"></param>
    /// <param name="isServer"></param>
    private void WriteToClass(TableDictionary colInfos, string exportPath, bool isServer)
    {
        if (colInfos.Count <= 0)
        {
            return;
        }

        var index = 0;
        var fileBuilder = new StringBuilder();
        var colNameSet = new HashSet<string>();

        if (colInfos.Count == 0)
        {
            return;
        }

        var csName = Path.GetFileNameWithoutExtension(colInfos.First().Key)?.Split('_')[0];

        foreach (var (tableName, cols) in colInfos)
        {
            if (cols == null || cols.Count == 0)
            {
                continue;
            }

            var excelWorksheet = LoadExcel(tableName, false);

            foreach (var colIndex in cols)
            {
                var colName = excelWorksheet.GetCellValue(5, colIndex);

                if (colNameSet.Contains(colName))
                {
                    continue;
                }

                colNameSet.Add(colName);

                string colType;

                if (isServer)
                {
                    colType = excelWorksheet.GetCellValue(1, colIndex);

                    if (IsNullOrEmpty(colType) || colType == "0")
                    {
                        colType = excelWorksheet.GetCellValue(2, colIndex);
                    }
                }
                else
                {
                    colType = excelWorksheet.GetCellValue(2, colIndex);
                }

                var remarks = excelWorksheet.GetCellValue(4, colIndex);

                fileBuilder.Append($"\n\t\t[ProtoMember({++index}, IsRequired  = true)]\n");
                fileBuilder.Append(
                    IsArray(colType,out var t)
                        ? $"\t\tpublic {colType} {colName} {{ get; set; }} = Array.Empty<{t}>(); // {remarks}"
                        : $"\t\tpublic {colType} {colName} {{ get; set; }} // {remarks}");
            }
        }
        
        var template = Define.ExcelTemplate;
        
        if (fileBuilder.Length > 0)
        {
            if (!Directory.Exists(exportPath))
            {
                Directory.CreateDirectory(exportPath);
            }

            var content = template.Replace("(namespace)", "TEngine")
                .Replace("(ConfigName)", csName)
                .Replace("(Fields)", fileBuilder.ToString());
            File.WriteAllText(Path.Combine(exportPath, $"{csName}.cs"), content);
        }
    }
    
    /// <summary>
    /// 把数据和实体类转换二进制导出到文件中
    /// </summary>
    private void ExportToBinary()
    {
        var exportToBinaryTasks = new List<Task>();
        var dynamicServerAssembly = DynamicAssembly.Load(Define.ExcelServerFileDirectory);
        var dynamicClientAssembly = DynamicAssembly.Load(Define.ExcelClientFileDirectory);
        
        foreach (var (tableName, tableList) in _tables)
        {
            var task = Task.Run(() =>
            {
                var idCheck = new HashSet<string>();
                var excelTable = _excelTables[tableName];
                var csName = Path.GetFileNameWithoutExtension(tableName);
                var serverColInfoCount = excelTable.ServerColInfos.Sum(d=>d.Value.Count);
                var serverDynamicInfo = serverColInfoCount == 0 ? null : DynamicAssembly.GetDynamicInfo(dynamicServerAssembly, csName);
                var clientColInfoCount = excelTable.ClientColInfos.Sum(d=>d.Value.Count);
                var clientDynamicInfo = clientColInfoCount == 0 ? null : DynamicAssembly.GetDynamicInfo(dynamicClientAssembly, csName);

                for (var i = 0; i < tableList.Count; i++)
                {
                    var tableListName = tableList[i];

                    try
                    {
                        var fileInfoFullName = tableListName.FileInfo.FullName;
                        var excelWorksheet = LoadExcel(fileInfoFullName, false);
                        var rows = excelWorksheet.Dimension.Rows;
                        excelTable.ServerColInfos.TryGetValue(fileInfoFullName, out var serverCols);
                        excelTable.ClientColInfos.TryGetValue(fileInfoFullName, out var clientCols);

                        for (var row = 7; row <= rows; row++)
                        {
                            if (excelWorksheet.GetCellValue(row, 1).StartsWith("#", StringComparison.Ordinal))
                            {
                                continue;
                            }
                            
                            var id = excelWorksheet.GetCellValue(row, 3);

                            if (idCheck.Contains(id))
                            {
                                Exporter.LogError($"{tableListName.Name} 存在重复Id {id} 行号 {row}");
                                continue;
                            }

                            idCheck.Add(id);
                            var isLast = row == rows && (i == tableList.Count - 1);
                            GenerateBinary(fileInfoFullName, excelWorksheet, serverDynamicInfo, serverCols, id, row, isLast, true);
                            GenerateBinary(fileInfoFullName, excelWorksheet, clientDynamicInfo, clientCols, id, row, isLast, false);
                        }
                    }
                    catch (Exception e)
                    {
                        Exporter.LogError($"Table:{tableListName} error! \n{e}");
                        throw;
                    }
                }

                if (serverDynamicInfo?.ConfigData != null)
                {
                    var bytes = ProtoBufHelper.ToBytes(serverDynamicInfo.ConfigData);
                    var serverBinaryDirectory = Define.ExcelServerBinaryDirectory;
                    
                    if (!Directory.Exists(serverBinaryDirectory))
                    {
                        Directory.CreateDirectory(serverBinaryDirectory);
                    }
                    
                    File.WriteAllBytes(Path.Combine(serverBinaryDirectory, $"{csName}Data.bytes"), bytes);

                    if (serverDynamicInfo.Json.Length > 0)
                    {
                        var serverJsonDirectory = Define.ExcelServerJsonDirectory;
                        using var sw = new StreamWriter(Path.Combine(serverJsonDirectory, $"{csName}Data.Json"));
                        sw.WriteLine("{\"List\":[");
                        sw.Write(serverDynamicInfo.Json.ToString());
                        sw.WriteLine("]}");
                    }
                }
                
                if (clientDynamicInfo?.ConfigData != null)
                {
                    var bytes = ProtoBufHelper.ToBytes(clientDynamicInfo.ConfigData);
                    var clientBinaryDirectory = Define.ExcelClientBinaryDirectory;
                    
                    if (!Directory.Exists(clientBinaryDirectory))
                    {
                        Directory.CreateDirectory(clientBinaryDirectory);
                    }
                    
                    File.WriteAllBytes(Path.Combine(clientBinaryDirectory, $"{csName}Data.bytes"), bytes);
                
                    if (clientDynamicInfo.Json.Length > 0)
                    {
                        var clientJsonDirectory = Define.ExcelClientJsonDirectory;
                        using var sw = new StreamWriter(Path.Combine(clientJsonDirectory, $"{csName}Data.Json"));
                        sw.WriteLine("{\"List\":[");
                        sw.Write(clientDynamicInfo.Json.ToString());
                        sw.WriteLine("]}");
                    }
                }
            });
            exportToBinaryTasks.Add(task);
        }

        Task.WaitAll(exportToBinaryTasks.ToArray());
    }

    private void GenerateBinary(string fileInfoFullName, ExcelWorksheet excelWorksheet, DynamicConfigDataType dynamicInfo, List<int> cols, string id, int row, bool isLast, bool isServer)
    {
        if (cols == null || IsNullOrEmpty(id) || cols.Count <= 0 || dynamicInfo?.ConfigType == null)
        {
            return;
        }

        var config = DynamicAssembly.CreateInstance(dynamicInfo.ConfigType);

        for (var i = 0; i < cols.Count; i++)
        {
            string colType;
            var colIndex = cols[i];
            var colName = excelWorksheet.GetCellValue(5, colIndex);
            var value = excelWorksheet.GetCellValue(row, colIndex);
            
            if (isServer)
            {
                colType = excelWorksheet.GetCellValue(1, colIndex);
                    
                if (IsNullOrEmpty(colType) || colType == "0")
                {
                    colType = excelWorksheet.GetCellValue(2, colIndex);
                }
            }
            else
            {
                colType = excelWorksheet.GetCellValue(2, colIndex);
            }

            try
            {
                SetNewValue(dynamicInfo.ConfigType.GetProperty(colName), config, colType, value);
            }
            catch (Exception e)
            {
                Exporter.LogError($"Error Table {fileInfoFullName} Col:{colName} colType:{colType} Row:{row} value:{value} {e}");
                throw;
            }
        }
        
        dynamicInfo.Method.Invoke(dynamicInfo.Obj, new object[] {config});
                
        var json = JsonConvert.SerializeObject(config);

        if (isLast)
        {
            dynamicInfo.Json.AppendLine(json);
        }
        else
        {
            dynamicInfo.Json.AppendLine($"{json},");
        }
    }

    public ExcelWorksheet LoadExcel(string name, bool isAddToDic)
    {
        if (_worksheets.TryGetValue(name, out var worksheet))
        {
            return worksheet;
        }

        worksheet = ExcelHelper.LoadExcel(name).Workbook.Worksheets[0];

        if (isAddToDic)
        {
            _worksheets.TryAdd(name, worksheet);
        }
        
        Exporter.LogInfo(name);
        return worksheet;
    }

    private void SetNewValue(PropertyInfo propertyInfo, AProto config, string type, string value)
    {
        if (IsNullOrWhiteSpace(value))
        {
            return;
        }

        switch (type)
        {
            case "short":
            {
                propertyInfo.SetValue(config, Convert.ToInt16(value));
                return;
            }
            case "ushort":
            {
                propertyInfo.SetValue(config, Convert.ToUInt16(value));
                return;
            }
            case "uint":
            {
                propertyInfo.SetValue(config, Convert.ToUInt32(value));
                return;
            }
            case "int":
            {
                propertyInfo.SetValue(config, Convert.ToInt32(value));
                return;
            }
            case "decimal":
            {
                propertyInfo.SetValue(config, Convert.ToDecimal(value));
                return;
            }
            case "string":
            {
                try
                {
                    propertyInfo.SetValue(config, value);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
                
                return;
            }
            case "bool":
            {
                // 空字符串

                value = value.ToLower();

                if (IsNullOrEmpty(value))
                {
                    propertyInfo.SetValue(config, false);
                }
                else if (bool.TryParse(value, out bool b))
                {
                    propertyInfo.SetValue(config, b);
                }
                else if (int.TryParse(value, out int v))
                {
                    propertyInfo.SetValue(config, v != 0);
                }
                else
                {
                    propertyInfo.SetValue(config, false);
                }

                return;
            }
            case "ulong":
            {
                propertyInfo.SetValue(config, Convert.ToUInt64(value));
                return;
            }
            case "long":
            {
                propertyInfo.SetValue(config, Convert.ToInt64(value));
                return;
            }
            case "double":
            {
                propertyInfo.SetValue(config, Convert.ToDouble(value));
                return;
            }
            case "float":
            {
                propertyInfo.SetValue(config, Convert.ToSingle(value));
                return;
            }
            case "int32[]":
            case "int[]":
            {
                if (value != "0")
                {
                    propertyInfo.SetValue(config, value.Split(",").Select(d => Convert.ToInt32(d)).ToArray());
                }

                return;
            }
            case "long[]":
            {
                if (value != "0")
                {
                    propertyInfo.SetValue(config, value.Split(",").Select(d => Convert.ToInt64(d)).ToArray());
                }

                return;
            }
            case "double[]":
            {
                if (value != "0")
                {
                    propertyInfo.SetValue(config, value.Split(",").Select(d => Convert.ToDouble(d)).ToArray());
                }

                return;
            }
            case "string[]":
            {
                if (value == "0")
                {
                    return;
                }

                var list = value.Split(",").ToArray();

                for (var i = 0; i < list.Length; i++)
                {
                    list[i] = list[i].Replace("\"", "");
                }

                propertyInfo.SetValue(config, value.Split(",").ToArray());

                return;
            }
            case "float[]":
            {
                if (value != "0")
                {
                    propertyInfo.SetValue(config, value.Split(",").Select(d => Convert.ToSingle(d)).ToArray());
                }

                return;
            }
            default:
                throw new NotSupportedException($"不支持此类型: {type}");
        }
    }

    private bool IsArray(string type, out string t)
    {
        t = null;
        var index = type.IndexOf("[]", StringComparison.Ordinal);

        if (index >= 0)
        {
            t = type.Remove(index, 2);
        }

        return index >= 0;
    }
}
#endif
