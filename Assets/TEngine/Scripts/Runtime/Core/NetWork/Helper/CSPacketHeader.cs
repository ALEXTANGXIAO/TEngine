using System;
using ProtoBuf;

namespace TEngine.Runtime
{
    [Serializable, ProtoContract(Name = @"CSPacketHeader")]
    public sealed class CSPacketHeader : PacketHeaderBase
    {
        public override PacketType PacketType
        {
            get
            {
                return PacketType.ClientToServer;
            }
        }
    }
}