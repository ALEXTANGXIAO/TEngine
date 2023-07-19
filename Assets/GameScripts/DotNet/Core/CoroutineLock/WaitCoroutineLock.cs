using System;
using TEngine.Core;

namespace TEngine.Core
{
    public struct CoroutineLockTimeout
    {
        public long LockId;
        public WaitCoroutineLock WaitCoroutineLock;
    }
    
    public sealed class OnCoroutineLockTimeout : EventSystem<CoroutineLockTimeout>
    {
        public override void Handler(CoroutineLockTimeout self)
        {
            if (self.LockId != self.WaitCoroutineLock.LockId)
            {
                return;
            }

            var coroutineLockQueue = self.WaitCoroutineLock.CoroutineLockQueue;
            var coroutineLockQueueType = coroutineLockQueue.CoroutineLockQueueType;
            var key = coroutineLockQueue.Key;
            Log.Error($"coroutine lock timeout CoroutineLockQueueType:{coroutineLockQueueType.Name} Key:{key} Tag:{self.WaitCoroutineLock.Tag}");
        }
    }

    public sealed class WaitCoroutineLock : IDisposable
    {
        private long _timerId;
        public bool IsDisposed => LockId == 0;
        public string Tag { get; private set; }
        public long LockId { get; private set; }
        public FTask<WaitCoroutineLock> Tcs { get; private set; }
        public CoroutineLockQueue CoroutineLockQueue{ get; private set; }

        public static WaitCoroutineLock Create(CoroutineLockQueue coroutineLockQueue, string tag, int timeOut)
        {
            var lockId = IdFactory.NextRunTimeId();
            var waitCoroutineLock = Pool<WaitCoroutineLock>.Rent();

            waitCoroutineLock.Tag = tag;
            waitCoroutineLock.LockId = lockId;
            waitCoroutineLock.CoroutineLockQueue = coroutineLockQueue;
            waitCoroutineLock.Tcs = FTask<WaitCoroutineLock>.Create();
            waitCoroutineLock._timerId = TimerScheduler.Instance.Core.OnceTimer(timeOut, new CoroutineLockTimeout()
            {
                LockId = lockId, WaitCoroutineLock = waitCoroutineLock
            });

            return waitCoroutineLock;
        }

        public void Dispose()
        {
            if (IsDisposed)
            {
                Log.Error("WaitCoroutineLock is Dispose");
                return;
            }
            
            Release(CoroutineLockQueue);
            
            LockId = 0;
            Tcs = null;
            Tag = null;
            CoroutineLockQueue = null;
            
            if (_timerId != 0)
            {
                TimerScheduler.Instance.Core.RemoveByRef(ref _timerId);
            }
            
            Pool<WaitCoroutineLock>.Return(this);
        }

        private static void Release(CoroutineLockQueue coroutineLockQueue)
        {
            // 放到下一帧执行释放锁、如果不这样、会导致逻辑的顺序不正常
            ThreadSynchronizationContext.Main.Post(coroutineLockQueue.Release);
        }

        public void SetResult()
        {
            if (Tcs == null)
            {
                throw new NullReferenceException("SetResult tcs is null");
            }
            
            Tcs.SetResult(this);
        }
    }
}