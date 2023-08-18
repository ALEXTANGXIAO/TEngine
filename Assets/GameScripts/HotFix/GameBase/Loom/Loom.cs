#if !UNITY_WEBGL
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Linq;

/*******************************************************************************
        //开启一个Loom进程
       Loom.RunAsync(() =>
       {
          aucThread = new Thread(ReceiveMsg);
          aucThread.Start();
       }

       //进程调用主线程方法
        MainPack pack = (MainPack)MainPack.Descriptor.Parser.ParseFrom(buffer, 0, len);
        Loom.QueueOnMainThread((param) =>
       {
             UdpHandleResponse(pack);
       }, null);

 *******************************************************************************/
namespace GameBase
{
    /// <summary>
    /// Loom多线程通信。
    /// <remarks></remarks>
    /// </summary>
    public class Loom : MonoBehaviour
    {
        public Dictionary<string, CancellationTokenSource> TokenSourcesDictionary = new Dictionary<string, CancellationTokenSource>();
        private static readonly int MaxThreads = 8;
        private static int _numThreads;
        private static Loom _current;

        public static Loom Current
        {
            get
            {
                Initialize();
                return _current;
            }
        }

        public void Awake()
        {
            _current = this;
            _initialized = true;
        }

        protected void OnDestroy()
        {
        }

        private static bool _initialized;

        private static void Initialize()
        {
            if (!_initialized)
            {
                if (!Application.isPlaying)
                {
                    return;
                }

                _initialized = true;

                var obj = new GameObject("[Loom]");

                _current = obj.AddComponent<Loom>();

                DontDestroyOnLoad(obj);
            }
        }

        public struct NoDelayedQueueItem
        {
            public Action<object> Action;
            public object Param;
        }

        private readonly List<NoDelayedQueueItem> _actions = new List<NoDelayedQueueItem>();

        public struct DelayedQueueItem
        {
            public float Time;
            public Action<object> Action;
            public object Param;
        }

        private readonly List<DelayedQueueItem> _delayed = new List<DelayedQueueItem>();

        private readonly List<DelayedQueueItem> _currentDelayed = new List<DelayedQueueItem>();

        public static void QueueOnMainThread(Action<object> taction, object param, float time = 0f)
        {
            if (time != 0f)
            {
                lock (Current._delayed)
                {
                    Current._delayed.Add(new DelayedQueueItem { Time = Time.time + time, Action = taction, Param = param });
                }
            }
            else
            {
                lock (Current._actions)
                {
                    Current._actions.Add(new NoDelayedQueueItem { Action = taction, Param = param });
                }
            }
        }

        public static Thread RunAsync(Action action)
        {
            Initialize();
            while (_numThreads >= MaxThreads)
            {
                Thread.Sleep(100);
            }

            Interlocked.Increment(ref _numThreads);
            ThreadPool.QueueUserWorkItem(RunAction, action);
            return null;
        }

        private static void RunAction(object action)
        {
            try
            {
                ((Action)action)();
            }
            catch
            {
                // ignored
            }
            finally
            {
                Interlocked.Decrement(ref _numThreads);
            }
        }


        void OnDisable()
        {
            if (_current == this)
            {
                _current = null;
            }
        }

        private readonly List<NoDelayedQueueItem> _currentActions = new List<NoDelayedQueueItem>();

        void Update()
        {
            if (_actions.Count > 0)
            {
                lock (_actions)
                {
                    _currentActions.Clear();
                    _currentActions.AddRange(_actions);
                    _actions.Clear();
                }

                for (int i = 0; i < _currentActions.Count; i++)
                {
                    _currentActions[i].Action(_currentActions[i].Param);
                }
            }

            if (_delayed.Count > 0)
            {
                lock (_delayed)
                {
                    _currentDelayed.Clear();
                    _currentDelayed.AddRange(_delayed.Where(d => d.Time <= Time.time));
                    for (int i = 0; i < _currentDelayed.Count; i++)
                    {
                        _delayed.Remove(_currentDelayed[i]);
                    }
                }

                for (int i = 0; i < _currentDelayed.Count; i++)
                {
                    _currentDelayed[i].Action(_currentDelayed[i].Param);
                }
            }
        }
    }
}
#endif