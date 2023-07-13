using System;
using TEngine.Core;
#pragma warning disable CS8625
#pragma warning disable CS8618

namespace TEngine
{
    public sealed class TimerAction : IDisposable
    {
        public long Id;
        public long Time;
        public object Callback;
        public TimerType TimerType;
        
        public static TimerAction Create()
        {
            var timerAction = Pool<TimerAction>.Rent();
            timerAction.Id = IdFactory.NextRunTimeId();
            return timerAction;
        }
        
        public void Dispose()
        {
            Id = 0;
            Time = 0;
            Callback = null;
            TimerType = TimerType.None;
            Pool<TimerAction>.Return(this);
        }
    }
}