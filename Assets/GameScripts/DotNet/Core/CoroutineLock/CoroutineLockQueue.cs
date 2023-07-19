using System;
using System.Collections.Generic;

namespace TEngine.Core
{
    public sealed class CoroutineLockQueue : IDisposable
    {
        public long Key { get; private set; }
        public CoroutineLockQueueType CoroutineLockQueueType { get; private set; }
        private readonly Queue<WaitCoroutineLock> _waitCoroutineLocks = new Queue<WaitCoroutineLock>();

        public static CoroutineLockQueue Create(long key, int time, CoroutineLockQueueType coroutineLockQueueType)
        {
            var coroutineLockQueue = Pool<CoroutineLockQueue>.Rent();
            coroutineLockQueue.Key = key;
            coroutineLockQueue.CoroutineLockQueueType = coroutineLockQueueType;
            return coroutineLockQueue;
        }

        public void Dispose()
        {
            Key = 0;
            CoroutineLockQueueType = null;
            Pool<CoroutineLockQueue>.Return(this);
        }

        public async FTask<WaitCoroutineLock> Lock(string tag, int time)
        {
#if TENGINE_DEVELOP
            if (_waitCoroutineLocks.Count >= 100)
            {
                // 当等待队列超过100个、表示这个协程锁可能有问题、打印一个警告方便排查错误
                Log.Warning($"too much waitCoroutineLock CoroutineLockQueueType:{CoroutineLockQueueType.Name} Key:{Key} Count: {_waitCoroutineLocks.Count} ");
            }
#endif
            var waitCoroutineLock = WaitCoroutineLock.Create(this, tag, time);
            _waitCoroutineLocks.Enqueue(waitCoroutineLock);
            return await waitCoroutineLock.Tcs;
        }

        public void Release()
        {
            if (_waitCoroutineLocks.Count == 0)
            {
                CoroutineLockQueueType.Remove(Key);
                return;
            }
            
            while (_waitCoroutineLocks.TryDequeue(out var waitCoroutineLock))
            {
                if (waitCoroutineLock.IsDisposed)
                {
                    continue;
                }
                
                waitCoroutineLock.SetResult();
                break;
            }
        }
    }
}