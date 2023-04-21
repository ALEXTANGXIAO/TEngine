using System;
using ProtoBuf;

namespace GameLogic
{
    [Serializable, ProtoContract(Name = @"HeartBeat")]
    public class HeartBeat : PacketBase
    {
        public HeartBeat()
        {
        }

        public override int Id => 1;

        public override void Clear()
        {
        }
    }
}