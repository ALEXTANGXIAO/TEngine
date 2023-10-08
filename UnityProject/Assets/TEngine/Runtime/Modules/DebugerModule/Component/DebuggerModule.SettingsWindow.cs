using System;
using UnityEngine;

namespace TEngine
{
    public sealed partial class DebuggerModule : Module
    {
        private sealed class SettingsWindow : ScrollableDebuggerWindowBase
        {
            private DebuggerModule _mDebuggerModule = null;
            private SettingModule _mSettingModule = null;
            private float m_LastIconX = 0f;
            private float m_LastIconY = 0f;
            private float m_LastWindowX = 0f;
            private float m_LastWindowY = 0f;
            private float m_LastWindowWidth = 0f;
            private float m_LastWindowHeight = 0f;
            private float m_LastWindowScale = 0f;

            public override void Initialize(params object[] args)
            {
                _mDebuggerModule = ModuleSystem.GetModule<DebuggerModule>();
                if (_mDebuggerModule == null)
                {
                    Log.Fatal("Debugger component is invalid.");
                    return;
                }

                _mSettingModule = ModuleSystem.GetModule<SettingModule>();
                if (_mSettingModule == null)
                {
                    Log.Fatal("Setting component is invalid.");
                    return;
                }

                m_LastIconX = _mSettingModule.GetFloat("Debugger.Icon.X", DefaultIconRect.x);
                m_LastIconY = _mSettingModule.GetFloat("Debugger.Icon.Y", DefaultIconRect.y);
                m_LastWindowX = _mSettingModule.GetFloat("Debugger.Window.X", DefaultWindowRect.x);
                m_LastWindowY = _mSettingModule.GetFloat("Debugger.Window.Y", DefaultWindowRect.y);
                m_LastWindowWidth = _mSettingModule.GetFloat("Debugger.Window.Width", DefaultWindowRect.width);
                m_LastWindowHeight = _mSettingModule.GetFloat("Debugger.Window.Height", DefaultWindowRect.height);
                _mDebuggerModule.WindowScale = m_LastWindowScale = _mSettingModule.GetFloat("Debugger.Window.Scale", DefaultWindowScale);
                _mDebuggerModule.IconRect = new Rect(m_LastIconX, m_LastIconY, DefaultIconRect.width, DefaultIconRect.height);
                _mDebuggerModule.WindowRect = new Rect(m_LastWindowX, m_LastWindowY, m_LastWindowWidth, m_LastWindowHeight);
            }

            public override void OnUpdate(float elapseSeconds, float realElapseSeconds)
            {
                if (Math.Abs(m_LastIconX - _mDebuggerModule.IconRect.x) > 0.01f)
                {
                    m_LastIconX = _mDebuggerModule.IconRect.x;
                    _mSettingModule.SetFloat("Debugger.Icon.X", _mDebuggerModule.IconRect.x);
                }

                if (Math.Abs(m_LastIconY - _mDebuggerModule.IconRect.y) > 0.01f)
                {
                    m_LastIconY = _mDebuggerModule.IconRect.y;
                    _mSettingModule.SetFloat("Debugger.Icon.Y", _mDebuggerModule.IconRect.y);
                }

                if (Math.Abs(m_LastWindowX - _mDebuggerModule.WindowRect.x) > 0.01f)
                {
                    m_LastWindowX = _mDebuggerModule.WindowRect.x;
                    _mSettingModule.SetFloat("Debugger.Window.X", _mDebuggerModule.WindowRect.x);
                }

                if (Math.Abs(m_LastWindowY - _mDebuggerModule.WindowRect.y) > 0.01f)
                {
                    m_LastWindowY = _mDebuggerModule.WindowRect.y;
                    _mSettingModule.SetFloat("Debugger.Window.Y", _mDebuggerModule.WindowRect.y);
                }

                if (Math.Abs(m_LastWindowWidth - _mDebuggerModule.WindowRect.width) > 0.01f)
                {
                    m_LastWindowWidth = _mDebuggerModule.WindowRect.width;
                    _mSettingModule.SetFloat("Debugger.Window.Width", _mDebuggerModule.WindowRect.width);
                }

                if (Math.Abs(m_LastWindowHeight - _mDebuggerModule.WindowRect.height) > 0.01f)
                {
                    m_LastWindowHeight = _mDebuggerModule.WindowRect.height;
                    _mSettingModule.SetFloat("Debugger.Window.Height", _mDebuggerModule.WindowRect.height);
                }

                if (Math.Abs(m_LastWindowScale - _mDebuggerModule.WindowScale) > 0.01f)
                {
                    m_LastWindowScale = _mDebuggerModule.WindowScale;
                    _mSettingModule.SetFloat("Debugger.Window.Scale", _mDebuggerModule.WindowScale);
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
                        if (Math.Abs(width - _mDebuggerModule.WindowRect.width) > 0.01f)
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
                        if (Math.Abs(height - _mDebuggerModule.WindowRect.height) > 0.01f)
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
                        if (Math.Abs(scale - _mDebuggerModule.WindowScale) > 0.01f)
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

            public override void OnLeave()
            {
                _mSettingModule.Save();
            }
        }
    }
}
