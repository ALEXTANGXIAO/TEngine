using System.Runtime.InteropServices;

namespace TEngine.Core
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct EntityIdStruct
    {
        // +-------------------+----------------------+-------------------------+----------------------------------+
        // |  time(30) 最大34年 | AppId(8) 最多255个进程 | WordId(10) 最多1023个世界 | sequence(16) 每毫秒每个进程能生产65535个
        // +-------------------+----------------------+-------------------------+----------------------------------+
        // 这样算下来、每个世界都有255个进程、就算WOW的游戏一个区也用不到255个进程
        // 计算下每个世界255个进程也就是1023 * 255 = 260865个进程、完全够用了
        // 但有一个问题是游戏最大只能有1023个区了、但可以通过战区手段来解决这个问题
        
        public uint Time{ get; private set; }
        public uint Sequence{ get; private set; }
        public uint RouteId { get; private set; }

        public ushort AppId => (ushort)(RouteId >> 10 & RouteIdStruct.MaskAppId);
        public ushort WordId=> (ushort)(RouteId & RouteIdStruct.MaskWordId);
        
        public const int MaskRouteId = 0x3FFFF;
        public const int MaskSequence = 0xFFFF;

        public EntityIdStruct(uint routeId, uint time, uint sequence)
        {
            Time = time;
            Sequence = sequence;
            RouteId = routeId;
        }

        public static implicit operator long(EntityIdStruct entityIdStruct)
        {
            ulong result = 0;
            result |= entityIdStruct.Sequence;
            result |= (ulong)entityIdStruct.RouteId << 16;
            result |= (ulong)entityIdStruct.Time << 34;
            return (long)result;
        }

        public static implicit operator EntityIdStruct(long id)
        {
            var result = (ulong) id;
            var idStruct = new EntityIdStruct()
            {
                Sequence = (uint) (result & MaskSequence)
            };
            result >>= 16;
            idStruct.RouteId = (uint) (result & 0x3FFFF);
            result >>= 18;
            idStruct.Time = (uint) result;
            return idStruct;
        }
    }
}