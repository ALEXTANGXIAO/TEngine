using System.Runtime.InteropServices;

namespace TEngine.Core
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct RuntimeIdStruct
    {
        // +----------+------------+
        // | time(32) | sequence(32)
        // +----------+------------+

        private uint Time;
        private uint Sequence;

        public RuntimeIdStruct(uint time, uint sequence)
        {
            Time = time;
            Sequence = sequence;
        }

        public static implicit operator long(RuntimeIdStruct runtimeId)
        {
            ulong result = 0;
            result |= runtimeId.Sequence;
            result |= (ulong) runtimeId.Time << 32;
            return (long) result;
        }

        public static implicit operator RuntimeIdStruct(long id)
        {
            var result = (ulong) id;
            var idStruct = new RuntimeIdStruct()
            {
                Time = (uint) (result >> 32),
                Sequence = (uint) (result & 0xFFFFFFFF)
            };
            return idStruct;
        }
        
        public override string ToString()
        {
            return $"Time: {this.Time}, Sequence: {this.Sequence}";
        }
    }
}