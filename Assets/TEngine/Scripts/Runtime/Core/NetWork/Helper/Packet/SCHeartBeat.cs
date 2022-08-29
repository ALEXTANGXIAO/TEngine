using ProtoBuf;
using System;

namespace TEngine.Runtime
{
    [Serializable, ProtoContract(Name = @"SCHeartBeat")]
    public class SCHeartBeat : SCPacketBase
    {
        public SCHeartBeat()
        {
        }

        public override int Id => 2;
      

        public override void Clear()
        {
        }
    }
}