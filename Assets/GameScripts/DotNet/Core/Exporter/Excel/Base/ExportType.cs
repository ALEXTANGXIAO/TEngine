#if TENGINE_NET
namespace TEngine.Core;

public enum ExportType
{
    None = 0,
    ProtoBuf = 1,               // 导出ProtoBuf
    AllExcelIncrement = 2,      // 所有-增量导出Excel
    AllExcel = 3,               // 所有-全量导出Excel
    Max,                        // 这个一定放最后
}
#endif
