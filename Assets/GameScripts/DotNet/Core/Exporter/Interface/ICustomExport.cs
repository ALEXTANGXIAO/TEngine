#if TENGINE_NET
namespace TEngine.Core;

public interface ICustomExport
{
    void Run();
}
public abstract class ACustomExport : ICustomExport
{
    protected enum CustomExportType
    {
        Client,Server
    }
    
    public abstract void Run();

    protected void Write(string fileName, string fileContent, CustomExportType customExportType)
    {
        switch (customExportType)
        {
            case CustomExportType.Client:
            {
                if (!Directory.Exists(ExcelDefine.ClientCustomExportDirectory))
                {
                    Directory.CreateDirectory(ExcelDefine.ClientCustomExportDirectory);
                }
                
                File.WriteAllText($"{ExcelDefine.ClientCustomExportDirectory}/{fileName}", fileContent);
                return;
            }
            case CustomExportType.Server:
            {
                if (!Directory.Exists(ExcelDefine.ServerCustomExportDirectory))
                {
                    Directory.CreateDirectory(ExcelDefine.ServerCustomExportDirectory);
                }
                
                File.WriteAllText($"{ExcelDefine.ServerCustomExportDirectory}/{fileName}", fileContent);
                return;
            }
        }
    }
}
#endif
