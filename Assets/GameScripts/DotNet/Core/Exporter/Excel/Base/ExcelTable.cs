#if TENGINE_NET
namespace TEngine.Core;

public sealed class ExcelTable
{
    public readonly string Name;
    public readonly SortedDictionary<string, List<int>> ClientColInfos = new();
    public readonly SortedDictionary<string, List<int>> ServerColInfos = new();
    
    public ExcelTable(string name)
    {
        Name = name;
    }
}
#endif
