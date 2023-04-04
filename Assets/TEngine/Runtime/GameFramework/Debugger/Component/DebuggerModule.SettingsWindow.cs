using UnityEngine;

namespace TEngine
{
    public sealed partial class DebuggerModule : GameFrameworkComponent
    {
        private sealed class SettingsWindow : ScrollableDebuggerWindowBase
        {
            private DebuggerModule _mDebuggerModule = null;
            private SettingComponent m_SettingComponent = null;
            private float m_LastIconX = 0f;
            private float m_LastIconY = 0f;
            private float m_LastWindowX = 0f;
            private float m_LastWindowY = 0f;
            private float m_LastWindowWidth = 0f;
            private float m_LastWindowHeight = 0f;
            private float m_LastWindowScale = 0f;

            public override void Initialize(params object[] args)
            {
                _mDebuggerModule = GameEntry.GetComponent<DebuggerModule>();
                if (_mDebuggerModule == null)
                {
                    Log.Fatal("Debugger component is invalid.");
                    return;
                }

                m_SettingComponent = GameEntry.GetComponent<SettingComponent>();
                if (m_SettingComponent == null)
                {
                    Log.Fatal("Setting component is invalid.");
                    return;
                }

                m_LastIconX = m_SettingComponent.GetFloat("Debugger.Icon.X", DefaultIconRect.x);
                m_LastIconY = m_SettingComponent.GetFloat("Debugger.Icon.Y", DefaultIconRect.y);
                m_LastWindowX = m_SettingComponent.GetFloat("Debugger.Window.X", DefaultWindowRect.x);
                m_LastWindowY = m_SettingComponent.GetFloat("Debugger.Window.Y", DefaultWindowRect.y);
                m_LastWindowWidth = m_SettingComponent.GetFloat("Debugger.Window.Width", DefaultWindowRect.width);
                m_LastWindowHeight = m_SettingComponent.GetFloat("Debugger.Window.Height", DefaultWindowRect.height);
                _mDebuggerModule.WindowScale = m_LastWindowScale = m_SettingComponent.GetFloat("Debugger.Window.Scale", DefaultWindowScale);
                _mDebuggerModule.IconRect = new Rect(m_LastIconX, m_LastIconY, DefaultIconRect.width, DefaultIconRect.height);
                _mDebuggerModule.WindowRect = new Rect(m_LastWindowX, m_LastWindowY, m_LastWindowWidth, m_LastWindowHeight);
            }

            public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
            {
                if (m_LastIconX != _mDebuggerModule.IconRect.x)
                {
                    m_LastIconX = _mDebuggerModule.IconRect.x;
                    m_SettingComponent.SetFloat("Debugger.Icon.X", _mDebuggerModule.IconRect.x);
                }

                if (m_LastIconY != _mDebuggerModule.IconRect.y)
                {
                    m_LastIconY = _mDebuggerModule.IconRect.y;
                    m_SettingComponent.SetFloat("Debugger.Icon.Y", _mDebuggerModule.IconRect.y);
                }

                if (m_LastWindowX != _mDebuggerModule.WindowRect.x)
                {
                    m_LastWindowX = _mDebuggerModule.WindowRect.x;
                    m_SettingComponent.SetFloat("Debugger.Window.X", _mDebuggerModule.WindowRect.x);
                }

                if (m_LastWindowY != _mDebuggerModule.WindowRect.y)
                {
                    m_LastWindowY = _mDebuggerModule.WindowRect.y;
                    m_SettingComponent.SetFloat("Debugger.Window.Y", _mDebuggerModule.WindowRect.y);
                }

                if (m_LastWindowWidth != _mDebuggerModule.WindowRect.width)
                {
                    m_LastWindowWidth = _mDebuggerModule.WindowRect.width;
                    m_SettingComponent.SetFloat("Debugger.Window.Width", _mDebuggerModule.WindowRect.width);
                }

                if (m_LastWindowHeight != _mDebuggerModule.WindowRect.height)
                {
                    m_LastWindowHeight = _mDebuggerModule.WindowRect.height;
                    m_SettingComponent.SetFloat("Debugger.Window.Height", _mDebuggerModule.WindowRect.height);
                }

                if (m_LastWindowScale != _mDebuggerModule.WindowScale)
                {
                    m_LastWindowScale = _mDebuggerModule.WindowScale;
                    m_SettingComponent.SetFloat("Debugger.Window.Scale", _mDebuggerModule.WindowScale);
                }
            }

            protected override void OnDrawScrollableWindow()
            {
                GUILayout.Label("<b>Window Settings</b>");
                GUILayout.BeginVertical("box");
                {
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Label("Position:", GUILayout.Width(60f));
                        GUILayout.Label("Drag window caption to move position.");
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    {
                        float width = _mDebuggerModule.WindowRect.width;
                        GUILayout.Label("Width:", GUILayout.Width(60f));
                        if (GUILayout.RepeatButton("-", GUILayout.Width(30f)))
                        {
                            width--;
                        }
                        width = GUILayout.HorizontalSlider(width, 100f, Screen.width - 20f);
                        if (GUILayout.RepeatButton("+", GUILayout.Width(30f)))
                        {
                            width++;
                        }
                        width = Mathf.Clamp(width, 100f, Screen.width - 20f);
                        if (width != _mDebuggerModule.WindowRect.width)
                        {
                            _mDebuggerModule.WindowRect = new Rect(_mDebuggerModule.WindowRect.x, _mDebuggerModule.WindowRect.y, width, _mDebuggerModule.WindowRect.height);
                        }
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    {
                        float height = _mDebuggerModule.WindowRect.height;
                        GUILayout.Label("Height:", GUILayout.Width(60f));
                        if (GUILayout.RepeatButton("-", GUILayout.Width(30f)))
                        {
                            height--;
                        }
                        height = GUILayout.HorizontalSlider(height, 100f, Screen.height - 20f);
                        if (GUILayout.RepeatButton("+", GUILayout.Width(30f)))
                        {
                            height++;
                        }
                        height = Mathf.Clamp(height, 100f, Screen.height - 20f);
                        if (height != _mDebuggerModule.WindowRect.height)
                        {
                            _mDebuggerModule.WindowRect = new Rect(_mDebuggerModule.WindowRect.x, _mDebuggerModule.WindowRect.y, _mDebuggerModule.WindowRect.width, height);
                        }
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    {
                        float scale = _mDebuggerModule.WindowScale;
                        GUILayout.Label("Scale:", GUILayout.Width(60f));
                        if (GUILayout.RepeatButton("-", GUILayout.Width(30f)))
                        {
                            scale -= 0.01f;
                        }
                        scale = GUILayout.HorizontalSlider(scale, 0.5f, 4f);
                        if (GUILayout.RepeatButton("+", GUILayout.Width(30f)))
                        {
                            scale += 0.01f;
                        }
                        scale = Mathf.Clamp(scale, 0.5f, 4f);
                        if (scale != _mDebuggerModule.WindowScale)
                        {
                            _mDebuggerModule.WindowScale = scale;
                        }
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    {
                        if (GUILayout.Button("0.5x", GUILayout.Height(60f)))
                        {
                            _mDebuggerModule.WindowScale = 0.5f;
                        }
                        if (GUILayout.Button("1.0x", GUILayout.Height(60f)))
                        {
                            _mDebuggerModule.WindowScale = 1f;
                        }
                        if (GUILayout.Button("1.5x", GUILayout.Height(60f)))
                        {
                            _mDebuggerModule.WindowScale = 1.5f;
                        }
                        if (GUILayout.Button("2.0x", GUILayout.Height(60f)))
                        {
                            _mDebuggerModule.WindowScale = 2f;
                        }
                        if (GUILayout.Button("2.5x", GUILayout.Height(60f)))
                        {
                            _mDebuggerModule.WindowScale = 2.5f;
                        }
                        if (GUILayout.Button("3.0x", GUILayout.Height(60f)))
                        {
                            _mDebuggerModule.WindowScale = 3f;
                        }
                        if (GUILayout.Button("3.5x", GUILayout.Height(60f)))
                        {
                            _mDebuggerModule.WindowScale = 3.5f;
                        }
                        if (GUILayout.Button("4.0x", GUILayout.Height(60f)))
                        {
                            _mDebuggerModule.WindowScale = 4f;
                        }
                    }
                    GUILayout.EndHorizontal();

                    if (GUILayout.Button("Reset Layout", GUILayout.Height(30f)))
                    {
                        _mDebuggerModule.ResetLayout();
                    }
                }
                GUILayout.EndVertical();
            }
        }
    }
}
