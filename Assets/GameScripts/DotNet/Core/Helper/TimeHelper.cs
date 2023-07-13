using System;

namespace TEngine.Core
{
    public static class TimeHelper
    {
        public const long Hour = 3600000;                       // 小时毫秒值 60 * 60 * 1000
        public const long Minute = 60000;                       // 分钟毫秒值 60 * 1000
        public const long OneDay = 86400000;                    // 天毫秒值 24 * 60 * 60 * 1000
        private const long Epoch = 621355968000000000L;         // 1970年1月1日的Ticks
        
        private static readonly DateTime Dt1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static long Now => (DateTime.UtcNow.Ticks - Epoch) / 10000;

#if TENGINE_UNITY
        public static long TimeDiff;
        public static long ServerNow => Now + TimeDiff;
#endif

        public static long Transition(DateTime d)
        {
            return (d.Ticks - Epoch) / 10000;
        }
    
        public static DateTime Transition(long timeStamp)
        {
            return Dt1970.AddTicks(timeStamp);
        }
    
        public static DateTime TransitionLocal(long timeStamp)
        {
            return Dt1970.AddTicks(timeStamp).ToLocalTime();
        }
    }
}