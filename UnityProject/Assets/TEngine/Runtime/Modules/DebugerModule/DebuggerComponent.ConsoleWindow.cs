using System;
using System.Collections.Generic;
using UnityEngine;

namespace TEngine
{
    public sealed partial class DebuggerModule : Module
    {
        [Serializable]
        private sealed class ConsoleWindow : IDebuggerWindow
        {
            private readonly Queue<LogNode> _logNodes = new Queue<LogNode>();

            private SettingModule _settingModule = null;
            private Vector2 _logScrollPosition = Vector2.zero;
            private Vector2 _stackScrollPosition = Vector2.zero;
            private int _infoCount = 0;
            private int _warningCount = 0;
            private int _errorCount = 0;
            private int _fatalCount = 0;
            private LogNode _selectedNode = null;
            private bool _lastLockScroll = true;
            private bool _lastInfoFilter = true;
            private bool _lastWarningFilter = true;
            private bool _lastErrorFilter = true;
            private bool _lastFatalFilter = true;

            [SerializeField]
            private bool m_LockScroll = true;

            [SerializeField]
            private int m_MaxLine = 100;

            [SerializeField]
            private bool m_InfoFilter = true;

            [SerializeField]
            private bool m_WarningFilter = true;

            [SerializeField]
            private bool m_ErrorFilter = true;

            [SerializeField]
            private bool m_FatalFilter = true;

            [SerializeField]
            private Color32 m_InfoColor = Color.white;

            [SerializeField]
            private Color32 m_WarningColor = Color.yellow;

            [SerializeField]
            private Color32 m_ErrorColor = Color.red;

            [SerializeField]
            private Color32 m_FatalColor = new Color(0.7f, 0.2f, 0.2f);

            public bool LockScroll
            {
                get => m_LockScroll;
                set => m_LockScroll = value;
            }

            public int MaxLine
            {
                get => m_MaxLine;
                set => m_MaxLine = value;
            }

            public bool InfoFilter
            {
                get => m_InfoFilter;
                set => m_InfoFilter = value;
            }

            public bool WarningFilter
            {
                get => m_WarningFilter;
                set => m_WarningFilter = value;
            }

            public bool ErrorFilter
            {
                get => m_ErrorFilter;
                set => m_ErrorFilter = value;
            }

            public bool FatalFilter
            {
                get => m_FatalFilter;
                set => m_FatalFilter = value;
            }

            public int InfoCount => _infoCount;

            public int WarningCount => _warningCount;

            public int ErrorCount => _errorCount;

            public int FatalCount => _fatalCount;

            public Color32 InfoColor
            {
                get => m_InfoColor;
                set => m_InfoColor = value;
            }

            public Color32 WarningColor
            {
                get => m_WarningColor;
                set => m_WarningColor = value;
            }

            public Color32 ErrorColor
            {
                get => m_ErrorColor;
                set => m_ErrorColor = value;
            }

            public Color32 FatalColor
            {
                get => m_FatalColor;
                set => m_FatalColor = value;
            }

            public void Initialize(params object[] args)
            {
                _settingModule = ModuleSystem.GetModule<SettingModule>();
                if (_settingModule == null)
                {
                    Log.Fatal("Setting component is invalid.");
                    return;
                }

                Application.logMessageReceived += OnLogMessageReceived;
                m_LockScroll = _lastLockScroll = _settingModule.GetBool("Debugger.Console.LockScroll", true);
                m_InfoFilter = _lastInfoFilter = _settingModule.GetBool("Debugger.Console.InfoFilter", true);
                m_WarningFilter = _lastWarningFilter = _settingModule.GetBool("Debugger.Console.WarningFilter", true);
                m_ErrorFilter = _lastErrorFilter = _settingModule.GetBool("Debugger.Console.ErrorFilter", true);
                m_FatalFilter = _lastFatalFilter = _settingModule.GetBool("Debugger.Console.FatalFilter", true);
            }

            public void Shutdown()
            {
                Application.logMessageReceived -= OnLogMessageReceived;
                Clear();
            }

            public void OnEnter()
            {
            }

            public void OnLeave()
            {
            }

            public void OnUpdate(float elapseSeconds, float realElapseSeconds)
            {
                if (_lastLockScroll != m_LockScroll)
                {
                    _lastLockScroll = m_LockScroll;
                    _settingModule.SetBool("Debugger.Console.LockScroll", m_LockScroll);
                }

                if (_lastInfoFilter != m_InfoFilter)
                {
                    _lastInfoFilter = m_InfoFilter;
                    _settingModule.SetBool("Debugger.Console.InfoFilter", m_InfoFilter);
                }

                if (_lastWarningFilter != m_WarningFilter)
                {
                    _lastWarningFilter = m_WarningFilter;
                    _settingModule.SetBool("Debugger.Console.WarningFilter", m_WarningFilter);
                }

                if (_lastErrorFilter != m_ErrorFilter)
                {
                    _lastErrorFilter = m_ErrorFilter;
                    _settingModule.SetBool("Debugger.Console.ErrorFilter", m_ErrorFilter);
                }

                if (_lastFatalFilter != m_FatalFilter)
                {
                    _lastFatalFilter = m_FatalFilter;
                    _settingModule.SetBool("Debugger.Console.FatalFilter", m_FatalFilter);
                }
            }

            public void OnDraw()
            {
                RefreshCount();

                GUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("Clear All", GUILayout.Width(100f)))
                    {
                        Clear();
                    }
                    m_LockScroll = GUILayout.Toggle(m_LockScroll, "Lock Scroll", GUILayout.Width(90f));
                    GUILayout.FlexibleSpace();
                    m_InfoFilter = GUILayout.Toggle(m_InfoFilter, Utility.Text.Format("Info ({0})", _infoCount), GUILayout.Width(90f));
                    m_WarningFilter = GUILayout.Toggle(m_WarningFilter, Utility.Text.Format("Warning ({0})", _warningCount), GUILayout.Width(90f));
                    m_ErrorFilter = GUILayout.Toggle(m_ErrorFilter, Utility.Text.Format("Error ({0})", _errorCount), GUILayout.Width(90f));
                    m_FatalFilter = GUILayout.Toggle(m_FatalFilter, Utility.Text.Format("Fatal ({0})", _fatalCount), GUILayout.Width(90f));
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginVertical("box");
                {
                    if (m_LockScroll)
                    {
                        _logScrollPosition.y = float.MaxValue;
                    }

                    _logScrollPosition = GUILayout.BeginScrollView(_logScrollPosition);
                    {
                        bool selected = false;
                        foreach (LogNode logNode in _logNodes)
                        {
                            switch (logNode.LogType)
                            {
                                case LogType.Log:
                                    if (!m_InfoFilter)
                                    {
                                        continue;
                                    }
                                    break;

                                case LogType.Warning:
                                    if (!m_WarningFilter)
                                    {
                                        continue;
                                    }
                                    break;

                                case LogType.Error:
                                    if (!m_ErrorFilter)
                                    {
                                        continue;
                                    }
                                    break;

                                case LogType.Exception:
                                    if (!m_FatalFilter)
                                    {
                                        continue;
                                    }
                                    break;
                            }
                            if (GUILayout.Toggle(_selectedNode == logNode, GetLogString(logNode)))
                            {
                                selected = true;
                                if (_selectedNode != logNode)
                                {
                                    _selectedNode = logNode;
                                    _stackScrollPosition = Vector2.zero;
                                }
                            }
                        }
                        if (!selected)
                        {
                            _selectedNode = null;
                        }
                    }
                    GUILayout.EndScrollView();
                }
                GUILayout.EndVertical();

                GUILayout.BeginVertical("box");
                {
                    _stackScrollPosition = GUILayout.BeginScrollView(_stackScrollPosition, GUILayout.Height(100f));
                    {
                        if (_selectedNode != null)
                        {
                            Color32 color = GetLogStringColor(_selectedNode.LogType);
                            if (GUILayout.Button(Utility.Text.Format("<color=#{0:x2}{1:x2}{2:x2}{3:x2}><b>{4}</b></color>{6}{6}{5}", color.r, color.g, color.b, color.a, _selectedNode.LogMessage, _selectedNode.StackTrack, Environment.NewLine), "label"))
                            {
                                CopyToClipboard(Utility.Text.Format("{0}{2}{2}{1}", _selectedNode.LogMessage, _selectedNode.StackTrack, Environment.NewLine));
                            }
                        }
                    }
                    GUILayout.EndScrollView();
                }
                GUILayout.EndVertical();
            }

            private void Clear()
            {
                _logNodes.Clear();
            }

            public void RefreshCount()
            {
                _infoCount = 0;
                _warningCount = 0;
                _errorCount = 0;
                _fatalCount = 0;
                foreach (LogNode logNode in _logNodes)
                {
                    switch (logNode.LogType)
                    {
                        case LogType.Log:
                            _infoCount++;
                            break;

                        case LogType.Warning:
                            _warningCount++;
                            break;

                        case LogType.Error:
                            _errorCount++;
                            break;

                        case LogType.Exception:
                            _fatalCount++;
                            break;
                    }
                }
            }

            public void GetRecentLogs(List<LogNode> results)
            {
                if (results == null)
                {
                    Log.Error("Results is invalid.");
                    return;
                }

                results.Clear();
                foreach (LogNode logNode in _logNodes)
                {
                    results.Add(logNode);
                }
            }

            public void GetRecentLogs(List<LogNode> results, int count)
            {
                if (results == null)
                {
                    Log.Error("Results is invalid.");
                    return;
                }

                if (count <= 0)
                {
                    Log.Error("Count is invalid.");
                    return;
                }

                int position = _logNodes.Count - count;
                if (position < 0)
                {
                    position = 0;
                }

                int index = 0;
                results.Clear();
                foreach (LogNode logNode in _logNodes)
                {
                    if (index++ < position)
                    {
                        continue;
                    }

                    results.Add(logNode);
                }
            }

            private void OnLogMessageReceived(string logMessage, string stackTrace, LogType logType)
            {
                if (logType == LogType.Assert)
                {
                    logType = LogType.Error;
                }

                _logNodes.Enqueue(LogNode.Create(logType, logMessage, stackTrace));
                while (_logNodes.Count > m_MaxLine)
                {
                    MemoryPool.Release(_logNodes.Dequeue());
                }
            }

            private string GetLogString(LogNode logNode)
            {
                Color32 color = GetLogStringColor(logNode.LogType);
                return Utility.Text.Format("<color=#{0:x2}{1:x2}{2:x2}{3:x2}>[{4:HH:mm:ss.fff}][{5}] {6}</color>", color.r, color.g, color.b, color.a, logNode.LogTime.ToLocalTime(), logNode.LogFrameCount, logNode.LogMessage);
            }

            internal Color32 GetLogStringColor(LogType logType)
            {
                Color32 color = Color.white;
                switch (logType)
                {
                    case LogType.Log:
                        color = m_InfoColor;
                        break;

                    case LogType.Warning:
                        color = m_WarningColor;
                        break;

                    case LogType.Error:
                        color = m_ErrorColor;
                        break;

                    case LogType.Exception:
                        color = m_FatalColor;
                        break;
                }

                return color;
            }
        }
    }
}
