using System;

namespace TEngine.Core
{
    public static class IdFactory
    {
        private static readonly long Epoch1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks / 10000;
        private static readonly long Epoch2023 = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks / 10000 - Epoch1970;
        private static readonly long EpochThisYear = new DateTime(DateTime.Now.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks / 10000 - Epoch1970;
        
        private static uint _lastRunTimeIdTime;
        private static uint _lastRunTimeIdSequence;

        private static uint _lastEntityIdTime;
        private static uint _lastEntityIdSequence;

        public static long NextRunTimeId()
        {
            var time = (uint) ((TimeHelper.Now - EpochThisYear) / 1000);

            if (time > _lastRunTimeIdTime)
            {
                _lastRunTimeIdTime = time;
                _lastRunTimeIdSequence = 0;
            }
            else if (++_lastRunTimeIdSequence > uint.MaxValue - 1)
            {
                ++_lastRunTimeIdTime;
                _lastRunTimeIdSequence = 0;
            }

            return new RuntimeIdStruct(_lastRunTimeIdTime, _lastRunTimeIdSequence);
        }

        public static long NextEntityId(uint locationId)
        {
            var time = (uint)((TimeHelper.Now - Epoch2023) / 1000);

            if (time > _lastEntityIdTime)
            {
                _lastEntityIdTime = time;
                _lastEntityIdSequence = 0;
            }
            else if (++_lastEntityIdSequence > EntityIdStruct.MaskSequence - 1)
            {
                ++_lastEntityIdTime;
                _lastEntityIdSequence = 0;
            }

            return new EntityIdStruct(locationId, _lastEntityIdTime, _lastEntityIdSequence);
        }

        public static uint GetRouteId(long entityId)
        {
            return (ushort)(entityId >> 16 & EntityIdStruct.MaskRouteId);
        }

        public static ushort GetAppId(long entityId)
        {
            return (ushort)(entityId >> 26 & RouteIdStruct.MaskAppId);
        }

        public static int GetWordId(long entityId)
        {
            return (ushort)(entityId >> 16 & RouteIdStruct.MaskWordId);
        }
    }
}

