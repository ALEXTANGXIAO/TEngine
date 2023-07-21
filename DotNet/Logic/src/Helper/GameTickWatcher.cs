namespace TEngine.Helper;

public class GameTickWatcher
{
    private long m_startTick = 0;

    public GameTickWatcher()
    {
        Refresh();
    }

    public void Refresh()
    {
        m_startTick = DateTime.Now.Ticks;
    }

    /// <summary>
    /// 计算用时。
    /// </summary>
    /// <returns></returns>
    public float ElapseTime()
    {
        long endTick = DateTime.Now.Ticks;
        return (endTick - m_startTick) / 10000f / 1000.0f;
    }

    /// <summary>
    /// 计算用时。
    /// </summary>
    /// <returns></returns>
    public void LogElapseTime(string tag)
    {
        Console.WriteLine($"计算用时：{tag} 耗时 {this.ElapseTime()}");
    }
}