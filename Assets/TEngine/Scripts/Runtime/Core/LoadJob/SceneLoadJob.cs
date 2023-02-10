using UnityEngine;
using UnityEngine.SceneManagement;

namespace TEngine.Runtime
{
    /// <summary>
    /// 场景加载任务
    /// </summary>
    public class SceneLoadJob : LoadJob
    {
        AsyncOperation _asyncOp;
        string _sceneName;
        LoadSceneMode _loadMode;

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="sceneName">场景名</param>
        public SceneLoadJob(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
        {
            _sceneName = sceneName;
            _loadMode = mode;
        }

        /// <summary>
        /// 开始执行加载任务，强制卸载未引用的AB资源，开始LoadScene
        /// </summary>
        public override void Start()
        {
            //经讨论对象池清理策略交由项目组控制更合理
            ResMgr.Instance.UnloadUnusedAssetBundle();
            _asyncOp = ResMgr.Instance.LoadScene(_sceneName, _loadMode);
        }

        /// <summary>
        /// 处理加载任务
        /// </summary>
        /// <returns></returns>
        public override float Process()
        {
            if (_asyncOp != null)
            {
                if (_asyncOp.isDone)
                {
                    _isDone = true;

                    return 1f;
                }
                else
                    return _asyncOp.progress / 0.9f;
            }
            else
                return 0f;
        }
    }
}
