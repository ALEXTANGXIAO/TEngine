using System;
using System.Collections.Concurrent;
using System.Threading;
using TEngine.Core.Network;
#pragma warning disable CS8765
#pragma warning disable CS8601
#pragma warning disable CS8618

namespace TEngine
{
    public sealed class ThreadSynchronizationContext : SynchronizationContext
    {
        public readonly int ThreadId;
        private Action _actionHandler;
        private readonly ConcurrentQueue<Action> _queue = new();
        public static ThreadSynchronizationContext Main { get; } = new(Environment.CurrentManagedThreadId);

        public ThreadSynchronizationContext(int threadId)
        {
            ThreadId = threadId;
        }

        public void Update()
        {
            while (_queue.TryDequeue(out _actionHandler))
            {
                try
                {
                    _actionHandler();
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }

        public override void Post(SendOrPostCallback callback, object state)
        {
            Post(() => callback(state));
        }

        public void Post(Action action)
        {
            _queue.Enqueue(action);
        }
    }
}