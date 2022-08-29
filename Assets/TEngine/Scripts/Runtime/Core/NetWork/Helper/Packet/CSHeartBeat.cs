using System;
using ProtoBuf;

namespace TEngine.Runtime
{
    [Serializable, ProtoContract(Name = @"CSHeartBeat")]
    public class CSHeartBeat : CSPacketBase
    {
        public CSHeartBeat()
        {
        }

        public override int Id => 1;

        public override void Clear()
        {
        }
    }
}
