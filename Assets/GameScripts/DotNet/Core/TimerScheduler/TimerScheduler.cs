using TEngine.Core;
#if TENGINE_UNITY
using UnityEngine;
#endif
namespace TEngine
{
    public sealed class TimerScheduler : Singleton<TimerScheduler>, IUpdateSingleton
    {
        public readonly TimerSchedulerCore Core = new TimerSchedulerCore(() => TimeHelper.Now);
#if TENGINE_UNITY
        public readonly TimerSchedulerCore Unity = new TimerSchedulerCore(() => (long) (Time.time * 1000));
#endif
        public void Update()
        {
            Core.Update();
#if TENGINE_UNITY
            Unity.Update();
#endif
        }
    }
}