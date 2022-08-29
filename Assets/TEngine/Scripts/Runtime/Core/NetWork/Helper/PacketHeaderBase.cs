namespace TEngine.Runtime
{
    public abstract class PacketHeaderBase : IPacketHeader, IMemory
    {
        public abstract PacketType PacketType
        {
            get;
        }

        public int Id
        {
            get;
            set;
        }

        public int PacketLength
        {
            get;
            set;
        }

        public bool IsValid
        {
            get
            {
                return PacketType != PacketType.Undefined && Id > 0 && PacketLength >= 0;
            }
        }

        public void Clear()
        {
            Id = 0;
            PacketLength = 0;
        }
    }
}