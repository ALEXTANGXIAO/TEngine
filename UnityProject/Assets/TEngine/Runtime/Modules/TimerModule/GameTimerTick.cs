using UnityEngine;

namespace TEngine
{
    public class GameTimerTick
    {
        protected OnTick Handle;
        protected float LastTime;
        protected float Interval;
        protected bool ResetInterval = true;

        public GameTimerTick(float interval, OnTick tickHandle) => Init(interval, true, true, tickHandle);

        public GameTimerTick(float interval, bool immediately, OnTick tickHandle) => Init(interval, immediately, true, tickHandle);

        public GameTimerTick(float interval, bool immediately, bool resetInterval, OnTick tickHandle)
        {
            Init(interval, immediately, resetInterval, tickHandle);
        }

        private void Init(float interval, bool immediately, bool resetInterval, OnTick tickHandle)
        {
            Interval = interval;
            Handle = tickHandle;
            ResetInterval = resetInterval;
            if (immediately)
            {
                return;
            }

            LastTime = Time.time;
        }

        public void OnUpdate()
        {
            float time = Time.time;
            if (LastTime + Interval >= time)
            {
                return;
            }

            if (ResetInterval)
            {
                LastTime = time;
            }
            else if (LastTime == 0.0)
            {
                LastTime = time;
            }
            else
            {
                LastTime += Interval;
            }

            Handle();
        }

        public delegate void OnTick();
    }
}