using System;
using System.Collections.Generic;
using TEngine.DataStructure;
using TEngine.Core;
#pragma warning disable CS8625

namespace TEngine
{
    public class TimerSchedulerCore
    {
        private long _minTime;
        private readonly Func<long> _now;
        private readonly Queue<long> _timeOutTime = new();
        private readonly Queue<long> _timeOutTimerIds = new();
        private readonly Dictionary<long, TimerAction> _timers = new();
        private readonly SortedOneToManyList<long, long> _timeId = new();
        
        public TimerSchedulerCore(Func<long> now)
        {
            _now = now;
        }
        
        public void Update()
        {
            try
            {
                var currentTime = _now();
                
                if (_timeId.Count == 0)
                {
                    return;
                }

                if (currentTime < _minTime)
                {
                    return;
                }

                _timeOutTime.Clear();
                _timeOutTimerIds.Clear();

                foreach (var (key, _) in _timeId)
                {
                    if (key > currentTime)
                    {
                        _minTime = key;
                        break;
                    }

                    _timeOutTime.Enqueue(key);
                }

                while (_timeOutTime.TryDequeue(out var time))
                {
                    foreach (var timerId in _timeId[time])
                    {
                        _timeOutTimerIds.Enqueue(timerId);
                    }

                    _timeId.RemoveKey(time);
                }

                while (_timeOutTimerIds.TryDequeue(out var timerId))
                {
                    if (!_timers.TryGetValue(timerId, out var timer))
                    {
                        continue;
                    }

                    _timers.Remove(timer.Id);

                    switch (timer.TimerType)
                    {
                        case TimerType.OnceWaitTimer:
                        {
                            var tcs = (FTask<bool>) timer.Callback;
                            timer.Dispose();
                            tcs.SetResult(true);
                            break;
                        }
                        case TimerType.OnceTimer:
                        {
                            var action = (Action) timer.Callback;
                            timer.Dispose();

                            if (action == null)
                            {
                                Log.Error($"timer {timer.ToJson()}");
                                break;
                            }

                            action();
                            break;
                        }
                        case TimerType.RepeatedTimer:
                        {
                            var action = (Action) timer.Callback;
                            AddTimer(_now() + timer.Time, timer);

                            if (action == null)
                            {
                                Log.Error($"timer {timer.ToJson()}");
                                break;
                            }

                            action();
                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
        
        private void AddTimer(long tillTime, TimerAction timer)
        {
            _timers.Add(timer.Id, timer);
            _timeId.Add(tillTime, timer.Id);

            if (tillTime < _minTime)
            {
                _minTime = tillTime;
            }
        }

        public async FTask<bool> WaitFrameAsync()
        {
            return await WaitAsync(1);
        }
        
        public async FTask<bool> WaitAsync(long time, FCancellationToken cancellationToken = null)
        {
            return await WaitTillAsync(_now() + time, cancellationToken);
        }
        
        public async FTask<bool> WaitTillAsync(long tillTime, FCancellationToken cancellationToken = null)
        {
            if (_now() > tillTime)
            {
                return true;
            }

            var tcs = FTask<bool>.Create();
            var timerAction = TimerAction.Create();
            var timerId = timerAction.Id;
            timerAction.Callback = tcs;
            timerAction.TimerType = TimerType.OnceWaitTimer;

            void CancelActionVoid()
            {
                if (!_timers.ContainsKey(timerId))
                {
                    return;
                }

                Remove(timerId);
                tcs.SetResult(false);
            }

            bool b;
            try
            {
                cancellationToken?.Add(CancelActionVoid);
                AddTimer(tillTime, timerAction);
                b = await tcs;
            }
            finally
            {
                cancellationToken?.Remove(CancelActionVoid);
            }

            return b;
        }
        
        public long NewFrameTimer(Action action)
        {
            return RepeatedTimer(100, action);
        }

        public long RepeatedTimer(long time, Action action)
        {
            if (time <= 0)
            {
                throw new Exception("repeated time <= 0");
            }

            var tillTime = _now() + time;
            var timer = TimerAction.Create();
            timer.TimerType = TimerType.RepeatedTimer;
            timer.Time = time;
            timer.Callback = action;
            AddTimer(tillTime, timer);
            return timer.Id;
        }
        
        public long RepeatedTimer<T>(long time, T timerHandlerType) where T : struct
        {
            void RepeatedTimerVoid()
            {
                EventSystem.Instance.Publish(timerHandlerType);
            }

            return RepeatedTimer(time, RepeatedTimerVoid);
        }
        
        public long OnceTimer(long time, Action action)
        {
            return OnceTillTimer(_now() + time, action);
        }
        
        public long OnceTimer<T>(long time, T timerHandlerType) where T : struct
        {
            void OnceTimerVoid()
            {
                EventSystem.Instance.Publish(timerHandlerType);
            }

            return OnceTimer(time, OnceTimerVoid);
        }
        
        public long OnceTillTimer(long tillTime, Action action)
        {
            if (tillTime < _now())
            {
                Log.Error($"new once time too small tillTime:{tillTime} Now:{_now()}");
            }

            var timer = TimerAction.Create();
            timer.TimerType = TimerType.OnceTimer;
            timer.Callback = action;
            AddTimer(tillTime, timer);
            return timer.Id;
        }
        
        public long OnceTillTimer<T>(long tillTime, T timerHandlerType) where T : struct
        {
            void OnceTillTimerVoid()
            {
                EventSystem.Instance.Publish(timerHandlerType);
            }

            return OnceTillTimer(tillTime, OnceTillTimerVoid);
        }
        
        public void RemoveByRef(ref long id)
        {
            Remove(id);
            id = 0;
        }

        public void Remove(long id)
        {
            if (id == 0 || !_timers.Remove(id, out var timer))
            {
                return;
            }

            timer?.Dispose();
        }
    }
}