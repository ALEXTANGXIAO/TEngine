#if TENGINE_UNITY
using TEngine.Core;
using TEngine.Core.Network;

namespace TEngine.Logic
{
    public class SessionHeartbeatComponent : Entity
    {
        private long _timerId;
        private long _selfRunTimeId;
        private Session _session;
        private readonly PingRequest _pingRequest = new PingRequest();

        public int Ping { get; private set; }

        public override void Dispose()
        {
            Stop();
            Ping = 0;
            _session = null;
            _selfRunTimeId = 0;
            base.Dispose();
        }

        public void Start(int interval)
        {
            _session = (Session)Parent;
            _selfRunTimeId = RuntimeId;
            _timerId = TimerScheduler.Instance.Unity.RepeatedTimer(interval, () => RepeatedSend().Coroutine());
        }

        public void Stop()
        {
            if (_timerId == 0)
            {
                return;
            }

            TimerScheduler.Instance?.Unity.RemoveByRef(ref _timerId);
        }

        private async FTask RepeatedSend()
        {
            if (_selfRunTimeId != RuntimeId)
            {
                Stop();
            }

            var requestTime = TimeHelper.Now;
            var pingResponse = (PingResponse)await _session.Call(_pingRequest);

            if (pingResponse.ErrorCode != 0)
            {
                return;
            }

            var responseTime = TimeHelper.Now;
            Ping = (int)(responseTime - requestTime) / 2;
            TimeHelper.TimeDiff = pingResponse.Now + Ping - responseTime;
        }
    }
}
#endif