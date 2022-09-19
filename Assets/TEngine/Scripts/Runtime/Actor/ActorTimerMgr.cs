using System;
using System.Collections.Generic;
using UnityEngine;

namespace TEngine.Runtime.Actor
{
    public enum TimerType
    {
        TimerTime = 0,
        TimerFrameUpdate,
        TimerFrameLateUpdate,
        TimerFrameOnceUpdate,
        TimerUnscaledTime,
    }

    public class GameTimer
    {
        public GameTimer Next;
        public GameTimer Prev;
        public string DebugSourceName;
        public Action CallAction;
        public bool Destroyed;
        public bool Loop;
        public float Interval;
        public float TriggerTime;
        public bool InExeQueue;
        public TimerType timerType;

        public static bool IsNull(GameTimer gameTimer)
        {
            return gameTimer == null || gameTimer.Destroyed;
        }
    }

    public class GameTimerList
    {
        public GameTimer Head;
        public GameTimer Tail;

        public int Count = 0;

        public bool IsEmpty
        {
            get { return Head == null; }
        }

        public void AddTail(GameTimer node)
        {
            var tail = Tail;
            if (tail != null)
            {
                tail.Next = node;
                node.Prev = tail;
            }
            else
            {
                Head = node;
            }

            Tail = node;
            AddCount();
        }


        private void AddCount()
        {
            Count++;
        }

        public void DecCount(int sub = 1)
        {
            Count -= sub;
            Log.Assert(Count >= 0);
        }

        public void AddSorted(GameTimer node, float triggerTime)
        {
            Log.Assert(node.Prev == null);
            Log.Assert(node.Next == null);

            node.TriggerTime = triggerTime;

            var head = Head;
            while (head != null)
            {
                if (head.TriggerTime >= triggerTime)
                {
                    break;
                }

                head = head.Next;
            }

            if (head != null)
            {
                var prev = head.Prev;
                if (prev != null)
                {
                    prev.Next = node;
                }

                node.Prev = prev;

                node.Next = head;
                head.Prev = node;

                if (prev == null)
                {
                    Head = node;
                }

                AddCount();
            }
            else
            {
                AddTail(node);
            }
        }

        public void Remove(GameTimer node)
        {
            var prev = node.Prev;
            var next = node.Next;

            if (prev != null)
            {
                prev.Next = next;
            }

            if (next != null)
            {
                next.Prev = prev;
            }

            node.Next = null;
            node.Prev = null;

            if (Head == node)
            {
                Head = next;
            }

            if (Tail == node)
            {
                Tail = prev;
            }

            DecCount();
        }

        public void Clear()
        {
            Head = null;
            Tail = null;
            Count = 0;
        }
    }

    public class ActorTimerMgr : BehaviourSingleton<ActorTimerMgr>
    {
        private GameTimerList _runningList = new GameTimerList();
        private GameTimerList _frameUpdateList = new GameTimerList();
        private GameTimerList _frameLateUpdateList = new GameTimerList();
        private GameTimerList _frameOnceUpdateList = new GameTimerList();
        private GameTimerList _unscaleRunningList = new GameTimerList();

        public override void Awake()
        {
            DoCreateLoopTimer("TimerDebug", 10f, () =>
            {
                var totalCount = _frameUpdateList.Count + _frameLateUpdateList.Count +
                                 +_frameOnceUpdateList.Count + _runningList.Count + _unscaleRunningList.Count;

                int maxTriggerCount = 2000;
                if (totalCount > maxTriggerCount)
                {
                    Log.Fatal("Timer is overflow: {0}", totalCount);
                }
            });
        }

        #region 创建定时器接口

        public bool CreateLoopTimer(ref GameTimer result, string source, float interval, Action timerAction)
        {
            if (!GameTimer.IsNull(result))
            {
                return false;
            }

            result = DoCreateLoopTimer(source, interval, timerAction);
            return true;
        }

        private GameTimer DoCreateLoopTimer(string source, float interval, Action timerAction)
        {
            interval = Math.Max(interval, 0.001f);
            var timer = AllocTimer(source, timerAction);
            timer.timerType = TimerType.TimerTime;
            timer.Loop = true;
            timer.Interval = interval;
            var triggerTime = Time.time + interval;
            _runningList.AddSorted(timer, triggerTime);
            return timer;
        }

        public GameTimer CreateOnceTimer(string source, float elapse, Action timerAction)
        {
            var timer = AllocTimer(source, timerAction);
            timer.timerType = TimerType.TimerTime;
            var triggerTime = Time.time + elapse;
            _runningList.AddSorted(timer, triggerTime);
            return timer;
        }

        /// <summary>
        /// 修改定时器循环创建接口，改为更加安全的方式
        /// </summary>
        /// <param name="result"></param>
        /// <param name="source"></param>
        /// <param name="timerAction"></param>
        public bool CreateLoopFrameTimer(ref GameTimer result, string source, Action timerAction)
        {
            if (!GameTimer.IsNull(result))
            {
                return false;
            }

            result = DoCreateLoopFrameTimer(source, timerAction);
            return true;
        }

        private GameTimer DoCreateLoopFrameTimer(string source, Action timerAction)
        {
            var timer = AllocTimer(source, timerAction);
            timer.timerType = TimerType.TimerFrameUpdate;
            _frameUpdateList.AddTail(timer);
            return timer;
        }

        public GameTimer CreateOnceFrameTimer(string source, Action timerAction)
        {
            var timer = AllocTimer(source, timerAction);
            timer.timerType = TimerType.TimerFrameOnceUpdate;
            _frameOnceUpdateList.AddTail(timer);
            return timer;
        }

        public bool CreateLoopFrameLateTimer(ref GameTimer result, string source, Action timerAction)
        {
            if (!GameTimer.IsNull(result))
            {
                return false;
            }

            result = DoCreateLoopFrameLateTimer(source, timerAction);
            return true;
        }

        private GameTimer DoCreateLoopFrameLateTimer(string source, Action timerAction)
        {
            var timer = AllocTimer(source, timerAction);
            timer.timerType = TimerType.TimerFrameLateUpdate;
            _frameLateUpdateList.AddTail(timer);
            return timer;
        }

        /// <summary>
        /// 不受
        /// </summary>
        /// <param name="source"></param>
        /// <param name="interval"></param>
        /// <param name="timerAction"></param>
        /// <returns></returns>
        public GameTimer CreateUnscaleLoopTimer(string source, float interval, Action timerAction)
        {
            interval = Math.Max(interval, 0.001f);
            var timer = AllocTimer(source, timerAction);
            timer.timerType = TimerType.TimerUnscaledTime;
            timer.Loop = true;
            timer.Interval = interval;
            var triggerTime = Time.unscaledTime + interval;
            _unscaleRunningList.AddSorted(timer, triggerTime);
            return timer;
        }

        public GameTimer CreateUnscaleOnceTimer(string source, float elapse, Action timerAction)
        {
            var timer = AllocTimer(source, timerAction);
            timer.timerType = TimerType.TimerUnscaledTime;
            var triggerTime = Time.unscaledTime + elapse;
            _unscaleRunningList.AddSorted(timer, triggerTime);
            return timer;
        }

        #endregion

        #region 销毁定时器接口

        public void DestroyTimer(ref GameTimer gameTimer)
        {
            ProcessDestroyTimer(gameTimer);
            gameTimer = null;
        }

        private void ProcessDestroyTimer(GameTimer gameTimer)
        {
            if (gameTimer == null || gameTimer.Destroyed)
            {
                return;
            }

            DoDestroy(gameTimer);

            if (!gameTimer.InExeQueue)
            {
                var type = gameTimer.timerType;
                if (type == TimerType.TimerTime)
                {
                    _runningList.Remove(gameTimer);
                }
                else if (type == TimerType.TimerFrameUpdate)
                {
                    _frameUpdateList.Remove(gameTimer);
                }
                else if (type == TimerType.TimerFrameOnceUpdate)
                {
                    _frameOnceUpdateList.Remove(gameTimer);
                }
                else if (type == TimerType.TimerUnscaledTime)
                {
                    _unscaleRunningList.Remove(gameTimer);
                }
                else
                {
                    Log.Assert(type == TimerType.TimerFrameLateUpdate);
                    _frameLateUpdateList.Remove(gameTimer);
                }
            }
            else
            {
                Log.Debug("Free when in exuete queue");
            }
        }

        #endregion

        public override bool IsHaveLateUpdate()
        {
            return true;
        }

        public override void LateUpdate()
        {
            UpdateFrameTimer(_frameLateUpdateList);
        }

        public override void Update()
        {
            TProfiler.BeginSample("UpdateTickTimer");
            UpdateTickTimer(false);
            TProfiler.EndSample();

            TProfiler.BeginSample("UpdateUnscaleTickTimer");
            UpdateTickTimer(true);
            TProfiler.EndSample();

            TProfiler.BeginSample("UpdateFrameTimer");
            UpdateFrameTimer(_frameUpdateList);
            TProfiler.EndSample();

            TProfiler.BeginSample("UpdateOnceFrameTimer");
            UpdateOnceFrameTimer(_frameOnceUpdateList);
            TProfiler.EndSample();
        }

        private void UpdateTickTimer(bool isUnscaled)
        {
            var runningList = isUnscaled ? _unscaleRunningList : _runningList;
            var head = runningList.Head;
            if (head != null)
            {
                var nowTime = isUnscaled ? Time.unscaledTime : Time.time;
                var node = head;

                int delCount = 0;
                while (node != null && node.TriggerTime <= nowTime)
                {
                    Log.Assert(!node.Destroyed);
                    node.InExeQueue = true;
                    node = node.Next;

                    delCount++;
                }

                if (head == node)
                {
                    Log.Assert(delCount <= 0);
                    return;
                }

                GameTimer waitExeHead = null;
                if (node != null)
                {
                    var prev = node.Prev;
                    prev.Next = null;

                    node.Prev = null;
                    runningList.Head = node;

                    runningList.DecCount(delCount);
                    Log.Assert(runningList.Count >= 0);

                    waitExeHead = head;
                }
                else
                {
                    waitExeHead = head;
                    runningList.Clear();
                }

                node = waitExeHead;
                while (node != null)
                {
                    var next = node.Next;
                    node.Next = null;
                    node.Prev = null;

                    if (!node.Destroyed)
                    {
                        TProfiler.BeginFirstSample(node.DebugSourceName);

                        node.CallAction();

                        TProfiler.EndFirstSample();

                        if (node.Loop && !node.Destroyed)
                        {
                            node.InExeQueue = false;
                            var triggerTime = node.Interval + nowTime;
                            runningList.AddSorted(node, triggerTime);
                        }
                        else
                        {
                            DoDestroy(node);
                        }
                    }
                    else
                    {
                        Log.Debug("destroy timer: {0}", node.DebugSourceName);
                    }

                    node = next;
                }
            }
        }

        private void PrintTimerListStatic(GameTimerList list)
        {
            Dictionary<string, int> dictStat = new Dictionary<string, int>();
            var node = list.Head;
            while (node != null)
            {
                var count = 0;
                dictStat.TryGetValue(node.DebugSourceName, out count);
                count++;

                dictStat[node.DebugSourceName] = count;
                node = node.Next;
            }

            var itr = dictStat.GetEnumerator();
            while (itr.MoveNext())
            {
                Log.Warning("{0}:{1}", itr.Current.Key, itr.Current.Value);
            }
            itr.Dispose();
        }

        private void UpdateFrameTimer(GameTimerList list)
        {
            if (list == null)
            {
                return;
            }

            var node = list.Head;
            while (node != null)
            {
                Log.Assert(!node.Destroyed, node.DebugSourceName);

                node.InExeQueue = true;

                TProfiler.BeginSample(node.DebugSourceName);

                if (node.CallAction != null)
                {
                    node.CallAction();
                }

                TProfiler.EndSample();

                node.InExeQueue = false;

                var next = node.Next;
                if (node.Destroyed)
                {
                    Log.Debug("timer is destroy when loop, free it");
                    list.Remove(node);
                }

                node = next;
            }
        }

        private void UpdateOnceFrameTimer(GameTimerList list)
        {
            var node = list.Head;
            list.Clear();

            while (node != null)
            {
                Log.Assert(!node.Destroyed);

                node.InExeQueue = true;

                TProfiler.BeginFirstSample(node.DebugSourceName);

                node.CallAction();

                TProfiler.EndFirstSample();

                var next = node.Next;
                DoDestroy(node);
                node = next;
            }
        }


        private void DoDestroy(GameTimer gameTimer)
        {
            gameTimer.Destroyed = true;
            gameTimer.CallAction = null;
        }

        private GameTimer AllocTimer(string source, Action timerAction)
        {
            var freeHead = new GameTimer();
            freeHead.CallAction = timerAction;
            freeHead.DebugSourceName = source;
            return freeHead;
        }
    }
}