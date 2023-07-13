#if TENGINE_NET
namespace TEngine.Logic;

public static class Entry
{
    public static async FTask Start()
    {
        await FTask.CompletedTask;
    }
}
#endif