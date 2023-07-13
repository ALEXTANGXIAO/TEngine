#if TENGINE_NET
namespace TEngine;

public class MachineConfigInfo
{
    public uint Id;
    public string OuterIP;
    public string OuterBindIP;
    public string InnerBindIP;
    public int ManagementPort;
}
#endif
