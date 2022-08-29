namespace TEngine.Runtime
{
    public abstract class SCPacketBase : PacketBase
    {
        public override PacketType PacketType
        {
            get
            {
                return PacketType.ServerToClient;
            }
        }
    }
}