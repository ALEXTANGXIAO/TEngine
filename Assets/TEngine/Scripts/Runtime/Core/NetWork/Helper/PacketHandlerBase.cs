namespace TEngine.Runtime
{
    /// <summary>
    /// PacketHandler处理，TEngine.Runtime程序集内无需手动注册，实现即可
    /// </summary>
    public abstract class PacketHandlerBase : IPacketHandler
    {
        public abstract int Id
        {
            get;
        }

        public abstract void Handle(object sender, Packet packet);
    }
}