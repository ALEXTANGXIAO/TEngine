using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace TEngine.Runtime
{
    /// <summary>
    /// 加载器，处理场景加载、对象预生成等需要在加载期处理的事务.
    /// </summary>
    public class LoadJobManager : UnitySingleton<LoadJobManager>
    {
        System.Action<float> _progressCallback;
        System.Action _onFinishCallback;
        Queue<LoadJob> _jobQueue = new Queue<LoadJob>();
        LoadJob _curJob = null;
        int _doneJobCount;
        int _jobCount;
        float _progress;

        /// <summary>
        /// 添加任务.
        /// </summary>
        /// <param name="job"></param>
        public void AddJob(LoadJob job)
        {
            _jobQueue.Enqueue(job);
        }

        /// <summary>
        /// 启动执行任务队列.
        /// </summary>
        /// <param name="progressCallback">处理进度回调，参数为0-1进度</param>
        /// <param name="onFinishCallback">完成回调。</param>
        public void Launch(System.Action<float> progressCallback, System.Action onFinishCallback)
        {
            _progress = 0f;
            _doneJobCount = 0;
            _jobCount = _jobQueue.Count;
            _progressCallback = progressCallback;
            _onFinishCallback = onFinishCallback;
            _curJob = _jobQueue.Dequeue();
            _curJob.Start();
        }

        void Update()
        {
            if (_curJob != null)
            {
                _progress = (_doneJobCount + _curJob.Process()) / _jobCount;
                if (_curJob.IsDone)
                {
                    if (_jobQueue.Count > 0)
                    {
                        _curJob = _jobQueue.Dequeue();
                        _curJob.Start();
                    }
                    else
                        _curJob = null;

                    ++_doneJobCount;
                }

                if (_progressCallback != null)
                    _progressCallback(_progress);
            }
            else
            {
                SingletonMgr.Release(gameObject);
                _progressCallback = null;

                if (_onFinishCallback != null)
                    _onFinishCallback();
            }
        }

        /// <summary>
        /// 异步加载场景。
        /// </summary>
        /// <param name="sceneName">场景名称。</param>
        /// <param name="progressCallback">加载函数回调。</param>
        /// <param name="callback">加载完成回调。</param>
        /// <param name="mode">场景加载模式。</param>
        public void LoadSceneAsync(string sceneName, System.Action<float> progressCallback = null, System.Action callback = null, LoadSceneMode mode = LoadSceneMode.Single)
        {
            MonoUtility.StartCoroutine(LoadJobManager.Instance.LoadJobManagerLoadScene(sceneName, progressCallback, callback, mode));
        }

        private IEnumerator LoadJobManagerLoadScene(string sceneName, System.Action<float> progressCallback = null, System.Action callback = null,
            LoadSceneMode mode = LoadSceneMode.Single)
        {
            bool isLoadEnd = false;
            Instance.AddJob(new SceneLoadJob(sceneName, LoadSceneMode.Single));
            Instance.Launch(progressCallback, () =>
            {
                callback?.Invoke();
                isLoadEnd = true;
            });
            yield return null;
            while (!isLoadEnd)
            {
                yield return null;
            }
        }
    }
}