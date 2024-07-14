using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TEngine
{
    /// <summary>
    /// 调试器模块。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed partial class DebuggerModule : Module
    {
        /// <summary>
        /// 默认调试器漂浮框大小。
        /// </summary>
        internal static readonly Rect DefaultIconRect = new Rect(10f, 10f, 60f, 60f);

        /// <summary>
        /// 默认调试器窗口大小。
        /// </summary>
        internal static readonly Rect DefaultWindowRect = new Rect(10f, 10f, 640f, 480f);

        /// <summary>
        /// 默认调试器窗口缩放比例。
        /// </summary>
        internal static readonly float DefaultWindowScale = 1.5f;

        private static TextEditor s_TextEditor = null;
        private IDebuggerManager _debuggerManager = null;
        private Rect m_DragRect = new Rect(0f, 0f, float.MaxValue, 25f);
        private Rect m_IconRect = DefaultIconRect;
        private Rect m_WindowRect = DefaultWindowRect;
        private float m_WindowScale = DefaultWindowScale;

        [SerializeField]
        private GUISkin m_Skin = null;

        [SerializeField]
        private DebuggerActiveWindowType m_ActiveWindow = DebuggerActiveWindowType.AlwaysOpen;

        public DebuggerActiveWindowType ActiveWindowType => m_ActiveWindow;

        [SerializeField]
        private bool m_ShowFullWindow = false;

        [SerializeField]
        private ConsoleWindow m_ConsoleWindow = new ConsoleWindow();

        private SystemInformationWindow m_SystemInformationWindow = new SystemInformationWindow();
        private EnvironmentInformationWindow m_EnvironmentInformationWindow = new EnvironmentInformationWindow();
        private ScreenInformationWindow m_ScreenInformationWindow = new ScreenInformationWindow();
        private GraphicsInformationWindow m_GraphicsInformationWindow = new GraphicsInformationWindow();
        private InputSummaryInformationWindow m_InputSummaryInformationWindow = new InputSummaryInformationWindow();
        private InputTouchInformationWindow m_InputTouchInformationWindow = new InputTouchInformationWindow();
        private InputLocationInformationWindow m_InputLocationInformationWindow = new InputLocationInformationWindow();
        private InputAccelerationInformationWindow m_InputAccelerationInformationWindow = new InputAccelerationInformationWindow();
        private InputGyroscopeInformationWindow m_InputGyroscopeInformationWindow = new InputGyroscopeInformationWindow();
        private InputCompassInformationWindow m_InputCompassInformationWindow = new InputCompassInformationWindow();
        private PathInformationWindow m_PathInformationWindow = new PathInformationWindow();
        private SceneInformationWindow m_SceneInformationWindow = new SceneInformationWindow();
        private TimeInformationWindow m_TimeInformationWindow = new TimeInformationWindow();
        private QualityInformationWindow m_QualityInformationWindow = new QualityInformationWindow();
        private ProfilerInformationWindow m_ProfilerInformationWindow = new ProfilerInformationWindow();
        private RuntimeMemorySummaryWindow m_RuntimeMemorySummaryWindow = new RuntimeMemorySummaryWindow();
        private RuntimeMemoryInformationWindow<Object> m_RuntimeMemoryAllInformationWindow = new RuntimeMemoryInformationWindow<Object>();
        private RuntimeMemoryInformationWindow<Texture> m_RuntimeMemoryTextureInformationWindow = new RuntimeMemoryInformationWindow<Texture>();
        private RuntimeMemoryInformationWindow<Mesh> m_RuntimeMemoryMeshInformationWindow = new RuntimeMemoryInformationWindow<Mesh>();
        private RuntimeMemoryInformationWindow<Material> m_RuntimeMemoryMaterialInformationWindow = new RuntimeMemoryInformationWindow<Material>();
        private RuntimeMemoryInformationWindow<Shader> m_RuntimeMemoryShaderInformationWindow = new RuntimeMemoryInformationWindow<Shader>();
        private RuntimeMemoryInformationWindow<AnimationClip> m_RuntimeMemoryAnimationClipInformationWindow = new RuntimeMemoryInformationWindow<AnimationClip>();
        private RuntimeMemoryInformationWindow<AudioClip> m_RuntimeMemoryAudioClipInformationWindow = new RuntimeMemoryInformationWindow<AudioClip>();
        private RuntimeMemoryInformationWindow<Font> m_RuntimeMemoryFontInformationWindow = new RuntimeMemoryInformationWindow<Font>();
        private RuntimeMemoryInformationWindow<TextAsset> m_RuntimeMemoryTextAssetInformationWindow = new RuntimeMemoryInformationWindow<TextAsset>();
        private RuntimeMemoryInformationWindow<ScriptableObject> m_RuntimeMemoryScriptableObjectInformationWindow = new RuntimeMemoryInformationWindow<ScriptableObject>();
        private ObjectPoolInformationWindow m_ObjectPoolInformationWindow = new ObjectPoolInformationWindow();
        private MemoryPoolPoolInformationWindow _mMemoryPoolPoolInformationWindow = new MemoryPoolPoolInformationWindow();
        // private NetworkInformationWindow m_NetworkInformationWindow = new NetworkInformationWindow();
        private SettingsWindow m_SettingsWindow = new SettingsWindow();
        private OperationsWindow m_OperationsWindow = new OperationsWindow();

        private FpsCounter m_FpsCounter = null;

        /// <summary>
        /// 获取或设置调试器窗口是否激活。
        /// </summary>
        public bool ActiveWindow
        {
            get => _debuggerManager.ActiveWindow;
            set
            {
                _debuggerManager.ActiveWindow = value;
                enabled = value;
            }
        }

        /// <summary>
        /// 获取或设置是否显示完整调试器界面。
        /// </summary>
        public bool ShowFullWindow
        {
            get => m_ShowFullWindow;
            set
            {
                if (_eventSystem != null)
                {
                    _eventSystem.SetActive(!value,ref _eventSystemActive);
                }
                m_ShowFullWindow = value;
            }
        }

        /// <summary>
        /// 获取或设置调试器漂浮框大小。
        /// </summary>
        public Rect IconRect
        {
            get => m_IconRect;
            set => m_IconRect = value;
        }

        /// <summary>
        /// 获取或设置调试器窗口大小。
        /// </summary>
        public Rect WindowRect
        {
            get => m_WindowRect;
            set => m_WindowRect = value;
        }

        /// <summary>
        /// 获取或设置调试器窗口缩放比例。
        /// </summary>
        public float WindowScale
        {
            get => m_WindowScale;
            set => m_WindowScale = value;
        }

        private SettingModule _settingModule = null;

        private bool _eventSystemActive = true;
        private GameObject _eventSystem;
        /// <summary>
        /// 游戏框架模块初始化。
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            s_TextEditor = new TextEditor();
            _eventSystem = GameObject.Find("UIRoot/EventSystem");
            _debuggerManager = ModuleImpSystem.GetModule<IDebuggerManager>();
            if (_debuggerManager == null)
            {
                Log.Fatal("Debugger manager is invalid.");
                return;
            }

            m_FpsCounter = new FpsCounter(0.5f);
        }

        private void OnDestroy()
        {
            if (_settingModule == null)
            {
                Log.Fatal("Setting component is invalid.");
                return;
            }
            
            _settingModule.Save();
        }

        private void Initialize()
        {
            _settingModule = ModuleSystem.GetModule<SettingModule>();
            if (_settingModule == null)
            {
                Log.Fatal("Setting component is invalid.");
                return;
            }
            
            var lastIconX = _settingModule.GetFloat("Debugger.Icon.X", DefaultIconRect.x);
            var  lastIconY = _settingModule.GetFloat("Debugger.Icon.Y", DefaultIconRect.y);
            var lastWindowX = _settingModule.GetFloat("Debugger.Window.X", DefaultWindowRect.x);
            var lastWindowY = _settingModule.GetFloat("Debugger.Window.Y", DefaultWindowRect.y);
            var lastWindowWidth = _settingModule.GetFloat("Debugger.Window.Width", DefaultWindowRect.width);
            var lastWindowHeight = _settingModule.GetFloat("Debugger.Window.Height", DefaultWindowRect.height);
            m_WindowScale = _settingModule.GetFloat("Debugger.Window.Scale", DefaultWindowScale);
            m_WindowRect = new Rect(lastIconX, lastIconY, DefaultIconRect.width, DefaultIconRect.height);
            m_WindowRect = new Rect(lastWindowX, lastWindowY, lastWindowWidth, lastWindowHeight);
        }

        private void Start()
        {
            Initialize();
            RegisterDebuggerWindow("Console", m_ConsoleWindow);
            RegisterDebuggerWindow("Information/System", m_SystemInformationWindow);
            RegisterDebuggerWindow("Information/Environment", m_EnvironmentInformationWindow);
            RegisterDebuggerWindow("Information/Screen", m_ScreenInformationWindow);
            RegisterDebuggerWindow("Information/Graphics", m_GraphicsInformationWindow);
            RegisterDebuggerWindow("Information/Input/Summary", m_InputSummaryInformationWindow);
            RegisterDebuggerWindow("Information/Input/Touch", m_InputTouchInformationWindow);
            RegisterDebuggerWindow("Information/Input/Location", m_InputLocationInformationWindow);
            RegisterDebuggerWindow("Information/Input/Acceleration", m_InputAccelerationInformationWindow);
            RegisterDebuggerWindow("Information/Input/Gyroscope", m_InputGyroscopeInformationWindow);
            RegisterDebuggerWindow("Information/Input/Compass", m_InputCompassInformationWindow);
            RegisterDebuggerWindow("Information/Other/Scene", m_SceneInformationWindow);
            RegisterDebuggerWindow("Information/Other/Path", m_PathInformationWindow);
            RegisterDebuggerWindow("Information/Other/Time", m_TimeInformationWindow);
            RegisterDebuggerWindow("Information/Other/Quality", m_QualityInformationWindow);
            RegisterDebuggerWindow("Profiler/Summary", m_ProfilerInformationWindow);
            RegisterDebuggerWindow("Profiler/Memory/Summary", m_RuntimeMemorySummaryWindow);
            RegisterDebuggerWindow("Profiler/Memory/All", m_RuntimeMemoryAllInformationWindow);
            RegisterDebuggerWindow("Profiler/Memory/Texture", m_RuntimeMemoryTextureInformationWindow);
            RegisterDebuggerWindow("Profiler/Memory/Mesh", m_RuntimeMemoryMeshInformationWindow);
            RegisterDebuggerWindow("Profiler/Memory/Material", m_RuntimeMemoryMaterialInformationWindow);
            RegisterDebuggerWindow("Profiler/Memory/Shader", m_RuntimeMemoryShaderInformationWindow);
            RegisterDebuggerWindow("Profiler/Memory/AnimationClip", m_RuntimeMemoryAnimationClipInformationWindow);
            RegisterDebuggerWindow("Profiler/Memory/AudioClip", m_RuntimeMemoryAudioClipInformationWindow);
            RegisterDebuggerWindow("Profiler/Memory/Font", m_RuntimeMemoryFontInformationWindow);
            RegisterDebuggerWindow("Profiler/Memory/TextAsset", m_RuntimeMemoryTextAssetInformationWindow);
            RegisterDebuggerWindow("Profiler/Memory/ScriptableObject", m_RuntimeMemoryScriptableObjectInformationWindow);
            RegisterDebuggerWindow("Profiler/Object Pool", m_ObjectPoolInformationWindow);
            RegisterDebuggerWindow("Profiler/Reference Pool", _mMemoryPoolPoolInformationWindow);
            // RegisterDebuggerWindow("Profiler/Network", m_NetworkInformationWindow);
            RegisterDebuggerWindow("Other/Settings", m_SettingsWindow);
            RegisterDebuggerWindow("Other/Operations", m_OperationsWindow);

            switch (m_ActiveWindow)
            {
                case DebuggerActiveWindowType.AlwaysOpen:
                    ActiveWindow = true;
                    break;

                case DebuggerActiveWindowType.OnlyOpenWhenDevelopment:
                    ActiveWindow = Debug.isDebugBuild;
                    break;

                case DebuggerActiveWindowType.OnlyOpenInEditor:
                    ActiveWindow = Application.isEditor;
                    break;

                default:
                    ActiveWindow = false;
                    break;
            }
        }

        private void Update()
        {
            m_FpsCounter.Update(GameTime.deltaTime, GameTime.unscaledDeltaTime);
        }

        private void OnGUI()
        {
            if (_debuggerManager == null || !_debuggerManager.ActiveWindow)
            {
                return;
            }

            GUISkin cachedGuiSkin = GUI.skin;
            Matrix4x4 cachedMatrix = GUI.matrix;

            GUI.skin = m_Skin;
            GUI.matrix = Matrix4x4.Scale(new Vector3(m_WindowScale, m_WindowScale, 1f));

            if (m_ShowFullWindow)
            {
                m_WindowRect = GUILayout.Window(0, m_WindowRect, DrawWindow, "<b>TENGINE DEBUGGER</b>");
            }
            else
            {
                m_IconRect = GUILayout.Window(0, m_IconRect, DrawDebuggerWindowIcon, "<b>DEBUGGER</b>");
            }

            GUI.matrix = cachedMatrix;
            GUI.skin = cachedGuiSkin;
        }

        /// <summary>
        /// 注册调试器窗口。
        /// </summary>
        /// <param name="path">调试器窗口路径。</param>
        /// <param name="debuggerWindow">要注册的调试器窗口。</param>
        /// <param name="args">初始化调试器窗口参数。</param>
        public void RegisterDebuggerWindow(string path, IDebuggerWindow debuggerWindow, params object[] args)
        {
            _debuggerManager.RegisterDebuggerWindow(path, debuggerWindow, args);
        }

        /// <summary>
        /// 解除注册调试器窗口。
        /// </summary>
        /// <param name="path">调试器窗口路径。</param>
        /// <returns>是否解除注册调试器窗口成功。</returns>
        public bool UnregisterDebuggerWindow(string path)
        {
            return _debuggerManager.UnregisterDebuggerWindow(path);
        }

        /// <summary>
        /// 获取调试器窗口。
        /// </summary>
        /// <param name="path">调试器窗口路径。</param>
        /// <returns>要获取的调试器窗口。</returns>
        public IDebuggerWindow GetDebuggerWindow(string path)
        {
            return _debuggerManager.GetDebuggerWindow(path);
        }

        /// <summary>
        /// 选中调试器窗口。
        /// </summary>
        /// <param name="path">调试器窗口路径。</param>
        /// <returns>是否成功选中调试器窗口。</returns>
        public bool SelectDebuggerWindow(string path)
        {
            return _debuggerManager.SelectDebuggerWindow(path);
        }

        /// <summary>
        /// 还原调试器窗口布局。
        /// </summary>
        public void ResetLayout()
        {
            IconRect = DefaultIconRect;
            WindowRect = DefaultWindowRect;
            WindowScale = DefaultWindowScale;
        }

        /// <summary>
        /// 获取记录的所有日志。
        /// </summary>
        /// <param name="results">要获取的日志。</param>
        public void GetRecentLogs(List<LogNode> results)
        {
            m_ConsoleWindow.GetRecentLogs(results);
        }

        /// <summary>
        /// 获取记录的最近日志。
        /// </summary>
        /// <param name="results">要获取的日志。</param>
        /// <param name="count">要获取最近日志的数量。</param>
        public void GetRecentLogs(List<LogNode> results, int count)
        {
            m_ConsoleWindow.GetRecentLogs(results, count);
        }

        private void DrawWindow(int windowId)
        {
            GUI.DragWindow(m_DragRect);
            DrawDebuggerWindowGroup(_debuggerManager.DebuggerWindowRoot);
        }

        private void DrawDebuggerWindowGroup(IDebuggerWindowGroup debuggerWindowGroup)
        {
            if (debuggerWindowGroup == null)
            {
                return;
            }

            List<string> names = new List<string>();
            string[] debuggerWindowNames = debuggerWindowGroup.GetDebuggerWindowNames();
            for (int i = 0; i < debuggerWindowNames.Length; i++)
            {
                names.Add(Utility.Text.Format("<b>{0}</b>", debuggerWindowNames[i]));
            }

            if (debuggerWindowGroup == _debuggerManager.DebuggerWindowRoot)
            {
                names.Add("<b>Close</b>");
            }

            int toolbarIndex = GUILayout.Toolbar(debuggerWindowGroup.SelectedIndex, names.ToArray(), GUILayout.Height(30f), GUILayout.MaxWidth(Screen.width));
            if (toolbarIndex >= debuggerWindowGroup.DebuggerWindowCount)
            {
                ShowFullWindow = false;
                return;
            }

            if (debuggerWindowGroup.SelectedWindow == null)
            {
                return;
            }

            if (debuggerWindowGroup.SelectedIndex != toolbarIndex)
            {
                debuggerWindowGroup.SelectedWindow.OnLeave();
                debuggerWindowGroup.SelectedIndex = toolbarIndex;
                debuggerWindowGroup.SelectedWindow.OnEnter();
            }

            IDebuggerWindowGroup subDebuggerWindowGroup = debuggerWindowGroup.SelectedWindow as IDebuggerWindowGroup;
            if (subDebuggerWindowGroup != null)
            {
                DrawDebuggerWindowGroup(subDebuggerWindowGroup);
            }

            debuggerWindowGroup?.SelectedWindow?.OnDraw();
        }

        private void DrawDebuggerWindowIcon(int windowId)
        {
            GUI.DragWindow(m_DragRect);
            GUILayout.Space(5);
            Color32 color = Color.white;
            m_ConsoleWindow.RefreshCount();
            if (m_ConsoleWindow.FatalCount > 0)
            {
                color = m_ConsoleWindow.GetLogStringColor(LogType.Exception);
            }
            else if (m_ConsoleWindow.ErrorCount > 0)
            {
                color = m_ConsoleWindow.GetLogStringColor(LogType.Error);
            }
            else if (m_ConsoleWindow.WarningCount > 0)
            {
                color = m_ConsoleWindow.GetLogStringColor(LogType.Warning);
            }
            else
            {
                color = m_ConsoleWindow.GetLogStringColor(LogType.Log);
            }

            string title = Utility.Text.Format("<color=#{0:x2}{1:x2}{2:x2}{3:x2}><b>FPS: {4:F2}</b></color>", color.r, color.g, color.b, color.a, m_FpsCounter.CurrentFps);
            if (GUILayout.Button(title, GUILayout.Width(100f), GUILayout.Height(40f)))
            {
                ShowFullWindow = true;
            }
        }

        private static void CopyToClipboard(string content)
        {
            s_TextEditor.text = content;
            s_TextEditor.OnFocus();
            s_TextEditor.Copy();
            s_TextEditor.text = string.Empty;
        }
    }
}
