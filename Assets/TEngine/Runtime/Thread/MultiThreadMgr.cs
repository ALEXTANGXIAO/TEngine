using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace TEngine
{
    /// <summary>
    /// 异步任务，交给子线程执行
    /// </summary>
    public abstract class AsyncTask
    {
        protected MultiThreadMgr MultiThreadMgr
        {
            get
            {
                return MultiThreadMgr.Instance;
            }
        }

        /// <summary>
        /// 执行任务
        /// </summary>
        public void Execute()
        {
            try
            {
                Run();
            }
            finally
            {
                if (!Loop)
                {
                    MultiThreadMgr.FinishTask(this);
                }
            }
        }

        /// <summary>
        /// 关闭执行
        /// </summary>
        public abstract void Close();

        /// <summary>
        /// 开始运行
        /// </summary>
        public abstract void Run();

        public bool Loop = false;
    }

    /// <summary>
    /// 多线程管理器
    /// </summary>
    public class MultiThreadMgr : UnitySingleton<MultiThreadMgr>
    {
        // 需要在主线程执行的操作
        static List<Action> actions = new List<Action>();
        static List<Action> runningActions = new List<Action>();
        static object obj = new object();

        // 异步任务队列
        static List<AsyncTask> taskList = new List<AsyncTask>();
        static Dictionary<AsyncTask,Thread> threads = new Dictionary<AsyncTask,Thread>();

        /// <summary>
        /// 在主线程中执行
        /// </summary>
        /// <param name="action">Action.</param>
        public void RunOnMainThread(Action action)
        {
            lock (obj)
            {
                actions.Add(action);
            }
        }

        /// <summary>
        /// 添加异步任务
        /// </summary>
        /// <param name="runnable">Runnable.</param>
        public void AddAsyncTask(AsyncTask runnable)
        {
            TLogger.LogInfo($"AddTask:{ runnable}");
            taskList.Add(runnable);
            Thread thread = new Thread(runnable.Execute);
            threads.Add(runnable,thread);
            thread.IsBackground = true;
            thread.Start();
        }

        /// <summary>
        /// 完成异步任务
        /// </summary>
        /// <param name="runnable">Runnable.</param>
        public void FinishTask(AsyncTask runnable)
        {
            runnable.Close();
            taskList.Remove(runnable);
            threads[runnable].Abort();
            threads.Remove(runnable);
            TLogger.LogInfo($"RemoveTask:{runnable},{taskList.Count}");
        }

        /// <summary>
        /// 主线程更新
        /// </summary>
        void Update()
        {
            lock (obj)
            {
                runningActions.Clear();
                runningActions.AddRange(actions);
                actions.Clear();
            }

            // 处理主线程事件
            if (runningActions.Count > 0)
            {
                foreach (Action action in runningActions)
                {
                    action();
                }
            }
            runningActions.Clear();
        }

        protected override void OnDestroy()
        {
            for (int i = 0; i < taskList.Count; i++)
            {
                taskList[i].Close();
            }

            var etr = threads.GetEnumerator();

            while (etr.MoveNext())
            {
                var thread = etr.Current.Value;
                thread.Abort();
            }

            etr.Dispose();

            threads.Clear();

            base.OnDestroy();
        }
    }
}