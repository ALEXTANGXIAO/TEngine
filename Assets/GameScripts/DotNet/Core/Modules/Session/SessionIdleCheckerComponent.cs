#if TENGINE_NET
using TEngine.Core.Network;

namespace TEngine.Core;

public class SessionIdleCheckerComponent: Entity
{
    private long _timeOut;
    private long _timerId;
    private long _selfRuntimeId;
    private Session _session;

    public override void Dispose()
    {
        Stop();
        _timeOut = 0;
        _selfRuntimeId = 0;
        _session = null;
        base.Dispose();
    }

    public void Start(int interval, int timeOut)
    {
        _timeOut = timeOut;
        _session = (Session)Parent;
        _selfRuntimeId = RuntimeId;
        _timerId = TimerScheduler.Instance.Core.RepeatedTimer(interval, Check);
    }

    public void Stop()
    {
        if (_timerId == 0)
        {
            return;
        }

        TimerScheduler.Instance.Core.RemoveByRef(ref _timerId);
    }

    private void Check()
    {
        if (_selfRuntimeId != RuntimeId)
        {
            Stop();
        }

        var timeNow = TimeHelper.Now;

        if (timeNow - _session.LastReceiveTime < _timeOut)
        {
            return;
        }

        Log.Warning($"session timeout id:{Id} timeNow:{timeNow} _session.LastReceiveTime:{_session.LastReceiveTime} _timeOut:{_timeOut}");
        _session.Dispose();
    }
}
#endif