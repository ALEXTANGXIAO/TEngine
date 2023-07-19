using System.Runtime.InteropServices;

namespace TEngine.Core
{
    // +----------------------+---------------------------+
    // | AppId(8) 最多255个进程 | WordId(10) 最多1023个世界 
    // +----------------------+---------------------------+

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct RouteIdStruct
    {
        public ushort AppId;
        public ushort WordId;

        public const int MaskAppId = 0xFF;
        public const int MaskWordId = 0x3FF;
        
        public RouteIdStruct(ushort appId, ushort wordId)
        {
            AppId = appId;
            WordId = wordId;
        }

        public static implicit operator uint(RouteIdStruct routeId)
        {
            uint result = 0;
            result |= routeId.WordId;
            result |= (uint)routeId.AppId << 10;
            return result;
        }

        public static implicit operator RouteIdStruct(uint routeId)
        {
            var idStruct = new RouteIdStruct()
            {
                WordId = (ushort)(routeId & MaskWordId)
            };

            routeId >>= 10;
            idStruct.AppId = (ushort)(routeId & MaskAppId);
            return idStruct;
        }
        
        public override string ToString()
        {
            return $"AppId:{this.AppId}|WordId:{this.WordId}";
        }
    }
}