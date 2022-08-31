using System;
using ProtoBuf;

namespace TEngine.Runtime
{
    [Serializable, ProtoContract(Name = @"CSPacketHeader")]
    public sealed class CSPacketHeader : PacketHeaderBase
    {
       
    }
}