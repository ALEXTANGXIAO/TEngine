using System;
using System.Collections.Generic;

namespace TEngine.Core
{
    public sealed class CoroutineLockQueueType
    {
        public readonly string Name;
        private readonly Dictionary<long, CoroutineLockQueue> _coroutineLockQueues = new Dictionary<long, CoroutineLockQueue>();

        private CoroutineLockQueueType() { }
        public CoroutineLockQueueType(string name)
        {
            Name = name;
        }

        public async FTask<WaitCoroutineLock> Lock(long key, string tag = null, int time = 10000)
        {
            if (_coroutineLockQueues.TryGetValue(key, out var coroutineLockQueue))
            {
                return await coroutineLockQueue.Lock(tag,time);
            }

            coroutineLockQueue = CoroutineLockQueue.Create(key, time, this);
            _coroutineLockQueues.Add(key, coroutineLockQueue);
            return WaitCoroutineLock.Create(coroutineLockQueue, tag, time);
        }

        public void Remove(long key)
        {
            if (_coroutineLockQueues.Remove(key, out var coroutineLockQueue))
            {
                coroutineLockQueue.Dispose();
            }
        }
    }
}