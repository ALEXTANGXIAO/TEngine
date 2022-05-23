using UnityEngine;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Linq;

/*******************************************************************************
 *      开启一个Loom进程
       Loom.RunAsync(() =>
       {
          aucThread = new Thread(ReceiveMsg);
          aucThread.Start();
       } 
       进程调用主线程方法
        MainPack pack = (MainPack)MainPack.Descriptor.Parser.ParseFrom(buffer, 0, len);
        Loom.QueueOnMainThread((param) =>
       {
             UdpHandleResponse(pack);
       }, null);
 
 *******************************************************************************/
namespace TEngineCore
{
    // <summary>
    // 从子线程调用Unity的方法
    // </summary>
    public class Loom : MonoBehaviour
    {
        public Dictionary<string, CancellationTokenSource> TokenSourcesDictionary = new Dictionary<string, CancellationTokenSource>();
        public static int MaxThreads = 8;
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
            initialized = true;
        }

        protected void OnDestroy()
        {

        }

        private static bool initialized;

        public static void Initialize()
        {
            if (!initialized)
            {
                if (!Application.isPlaying)
                {
                    return;
                }
                initialized = true;

                var obj = new GameObject("Loom");

                _current = obj.AddComponent<Loom>();

                GameObject tEngine = SingletonMgr.Root;

                if (tEngine != null)
                {
                    obj.transform.SetParent(tEngine.transform);
                }
            }
        }

        public struct NoDelayedQueueItem
        {
            public Action<object> action;
            public object param;
        }

        private List<NoDelayedQueueItem> _actions = new List<NoDelayedQueueItem>();

        public struct DelayedQueueItem
        {
            public float time;
            public Action<object> action;
            public object param;
        }

        private List<DelayedQueueItem> _delayed = new List<DelayedQueueItem>();

        List<DelayedQueueItem> _currentDelayed = new List<DelayedQueueItem>();

        public static void QueueOnMainThread(Action<object> taction, object tparam)
        {
            QueueOnMainThread(taction, tparam, 0f);
        }
        public static void QueueOnMainThread(Action<object> taction, object tparam, float time)
        {
            if (time != 0f)
            {
                lock (Current._delayed)
                {
                    Current._delayed.Add(new DelayedQueueItem { time = Time.time + time, action = taction, param = tparam });
                }
            }
            else
            {
                lock (Current._actions)
                {
                    Current._actions.Add(new NoDelayedQueueItem { action = taction, param = tparam });
                }
            }
        }

        public static Thread RunAsync(string actionName,Action action)
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

        List<NoDelayedQueueItem> _currentActions = new List<NoDelayedQueueItem>();

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
                    _currentActions[i].action(_currentActions[i].param);
                }
            }

            if (_delayed.Count > 0)
            {
                lock (_delayed)
                {
                    _currentDelayed.Clear();
                    _currentDelayed.AddRange(_delayed.Where(d => d.time <= Time.time));
                    for (int i = 0; i < _currentDelayed.Count; i++)
                    {
                        _delayed.Remove(_currentDelayed[i]);
                    }
                }

                for (int i = 0; i < _currentDelayed.Count; i++)
                {
                    _currentDelayed[i].action(_currentDelayed[i].param);
                }
            }
        }
    }
}