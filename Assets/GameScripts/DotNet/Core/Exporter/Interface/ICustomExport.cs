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
                if (!Directory.Exists(Define.ClientCustomExportDirectory))
                {
                    Directory.CreateDirectory(Define.ClientCustomExportDirectory);
                }
                
                File.WriteAllText($"{Define.ClientCustomExportDirectory}/{fileName}", fileContent);
                return;
            }
            case CustomExportType.Server:
            {
                if (!Directory.Exists(Define.ServerCustomExportDirectory))
                {
                    Directory.CreateDirectory(Define.ServerCustomExportDirectory);
                }
                
                File.WriteAllText($"{Define.ServerCustomExportDirectory}/{fileName}", fileContent);
                return;
            }
        }
    }
}
#endif
