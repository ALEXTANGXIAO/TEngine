namespace TEngine.Runtime
{
    /// <summary>
    /// 网络消息包基类。
    /// </summary>
    public abstract class Packet:IMemory
    {
        public abstract void Clear();
    }
}