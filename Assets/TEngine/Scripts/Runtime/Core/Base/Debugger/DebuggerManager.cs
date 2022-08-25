using System;
using UnityEngine;

namespace TEngine.Runtime
{
    /// <summary>
    /// 调试器管理器。
    /// </summary>
    internal sealed partial class DebuggerManager : BehaviourSingleton<DebuggerManager>, IDebuggerManager
    {
        private readonly DebuggerWindowGroup m_DebuggerWindowRoot;
        private bool m_ActiveWindow;

        /// <summary>
        /// 初始化调试器管理器的新实例。
        /// </summary>
        public DebuggerManager()
        {
            m_DebuggerWindowRoot = new DebuggerWindowGroup();
            m_ActiveWindow = false;
        }

        /// <summary>
        /// 获取或设置调试器窗口是否激活。
        /// </summary>
        public bool ActiveWindow
        {
            get
            {
                return m_ActiveWindow;
            }
            set
            {
                m_ActiveWindow = value;
            }
        }

        /// <summary>
        /// 调试器窗口根结点。
        /// </summary>
        public IDebuggerWindowGroup DebuggerWindowRoot
        {
            get
            {
                return m_DebuggerWindowRoot;
            }
        }

        /// <summary>
        /// 调试器管理器轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        public override void Update()
        {
            if (!m_ActiveWindow)
            {
                return;
            }

            m_DebuggerWindowRoot.OnUpdate(Time.deltaTime, Time.unscaledDeltaTime); ;
        }

        /// <summary>
        /// 关闭并清理调试器管理器。
        /// </summary>
        public override void Destroy()
        {
            m_ActiveWindow = false;
            m_DebuggerWindowRoot.Shutdown();
            base.Destroy();
        }

        /// <summary>
        /// 注册调试器窗口。
        /// </summary>
        /// <param name="path">调试器窗口路径。</param>
        /// <param name="debuggerWindow">要注册的调试器窗口。</param>
        /// <param name="args">初始化调试器窗口参数。</param>
        public void RegisterDebuggerWindow(string path, IDebuggerWindow debuggerWindow, params object[] args)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new Exception("Path is invalid.");
            }

            if (debuggerWindow == null)
            {
                throw new Exception("Debugger window is invalid.");
            }

            m_DebuggerWindowRoot.RegisterDebuggerWindow(path, debuggerWindow);
            debuggerWindow.Initialize(args);
        }

        /// <summary>
        /// 解除注册调试器窗口。
        /// </summary>
        /// <param name="path">调试器窗口路径。</param>
        /// <returns>是否解除注册调试器窗口成功。</returns>
        public bool UnregisterDebuggerWindow(string path)
        {
            return m_DebuggerWindowRoot.UnregisterDebuggerWindow(path);
        }

        /// <summary>
        /// 获取调试器窗口。
        /// </summary>
        /// <param name="path">调试器窗口路径。</param>
        /// <returns>要获取的调试器窗口。</returns>
        public IDebuggerWindow GetDebuggerWindow(string path)
        {
            return m_DebuggerWindowRoot.GetDebuggerWindow(path);
        }

        /// <summary>
        /// 选中调试器窗口。
        /// </summary>
        /// <param name="path">调试器窗口路径。</param>
        /// <returns>是否成功选中调试器窗口。</returns>
        public bool SelectDebuggerWindow(string path)
        {
            return m_DebuggerWindowRoot.SelectDebuggerWindow(path);
        }
    }
}
