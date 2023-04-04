using System;
using UnityEngine;
using TEngine.Localization;

namespace TEngine
{
    /// <summary>
    /// 基础模块。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RootModule : GameFrameworkModuleBase
    {
        private const int DefaultDpi = 96;  // default windows dpi

        private float m_GameSpeedBeforePause = 1f;

        [SerializeField]
        private Language m_EditorLanguage = Language.Unspecified;

        [SerializeField]
        private string m_TextHelperTypeName = "TEngine.DefaultTextHelper";

        [SerializeField]
        private string m_VersionHelperTypeName = "TEngine.DefaultVersionHelper";

        [SerializeField]
        private string m_LogHelperTypeName = "TEngine.DefaultLogHelper";

        [SerializeField]
        private string m_CompressionHelperTypeName = "TEngine.DefaultCompressionHelper";

        [SerializeField]
        private string m_JsonHelperTypeName = "TEngine.DefaultJsonHelper";

        [SerializeField]
        private int m_FrameRate = 120;

        [SerializeField]
        private float m_GameSpeed = 1f;

        [SerializeField]
        private bool m_RunInBackground = true;

        [SerializeField]
        private bool m_NeverSleep = true;

        /// <summary>
        /// 获取或设置编辑器语言（仅编辑器内有效）。
        /// </summary>
        public Language EditorLanguage
        {
            get => m_EditorLanguage;
            set => m_EditorLanguage = value;
        }

        /// <summary>
        /// 获取或设置游戏帧率。
        /// </summary>
        public int FrameRate
        {
            get => m_FrameRate;
            set => Application.targetFrameRate = m_FrameRate = value;
        }

        /// <summary>
        /// 获取或设置游戏速度。
        /// </summary>
        public float GameSpeed
        {
            get => m_GameSpeed;
            set => Time.timeScale = m_GameSpeed = value >= 0f ? value : 0f;
        }

        /// <summary>
        /// 获取游戏是否暂停。
        /// </summary>
        public bool IsGamePaused => m_GameSpeed <= 0f;

        /// <summary>
        /// 获取是否正常游戏速度。
        /// </summary>
        public bool IsNormalGameSpeed => Math.Abs(m_GameSpeed - 1f) < 0.01f;

        /// <summary>
        /// 获取或设置是否允许后台运行。
        /// </summary>
        public bool RunInBackground
        {
            get => m_RunInBackground;
            set => Application.runInBackground = m_RunInBackground = value;
        }

        /// <summary>
        /// 获取或设置是否禁止休眠。
        /// </summary>
        public bool NeverSleep
        {
            get => m_NeverSleep;
            set
            {
                m_NeverSleep = value;
                Screen.sleepTimeout = value ? SleepTimeout.NeverSleep : SleepTimeout.SystemSetting;
            }
        }

        /// <summary>
        /// 游戏框架模块初始化。
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            
            GameTime.StartFrame();

            InitTextHelper();
            InitVersionHelper();
            InitLogHelper();
            Log.Info("TEngine Version: {0}", TEngine.Version.GameFrameworkVersion);
            Log.Info("Game Version: {0} ({1})", TEngine.Version.GameVersion, TEngine.Version.InternalGameVersion);
            Log.Info("Unity Version: {0}", Application.unityVersion);

            InitCompressionHelper();
            InitJsonHelper();

            Utility.Converter.ScreenDpi = Screen.dpi;
            if (Utility.Converter.ScreenDpi <= 0)
            {
                Utility.Converter.ScreenDpi = DefaultDpi;
            }
            
            Application.targetFrameRate = m_FrameRate;
            Time.timeScale = m_GameSpeed;
            Application.runInBackground = m_RunInBackground;
            Screen.sleepTimeout = m_NeverSleep ? SleepTimeout.NeverSleep : SleepTimeout.SystemSetting;

            Application.lowMemory += OnLowMemory;
        }

        private void Update()
        {
            GameTime.StartFrame();
            GameFrameworkEntry.Update(GameTime.deltaTime, GameTime.unscaledDeltaTime);
        }

        private void OnApplicationQuit()
        {
            Application.lowMemory -= OnLowMemory;
            StopAllCoroutines();
        }

        private void OnDestroy()
        {
            GameFrameworkEntry.Shutdown();
        }

        /// <summary>
        /// 暂停游戏。
        /// </summary>
        public void PauseGame()
        {
            if (IsGamePaused)
            {
                return;
            }

            m_GameSpeedBeforePause = GameSpeed;
            GameSpeed = 0f;
        }

        /// <summary>
        /// 恢复游戏。
        /// </summary>
        public void ResumeGame()
        {
            if (!IsGamePaused)
            {
                return;
            }

            GameSpeed = m_GameSpeedBeforePause;
        }

        /// <summary>
        /// 重置为正常游戏速度。
        /// </summary>
        public void ResetNormalGameSpeed()
        {
            if (IsNormalGameSpeed)
            {
                return;
            }

            GameSpeed = 1f;
        }

        internal void Shutdown()
        {
            Destroy(gameObject);
        }

        private void InitTextHelper()
        {
            if (string.IsNullOrEmpty(m_TextHelperTypeName))
            {
                return;
            }

            Type textHelperType = Utility.Assembly.GetType(m_TextHelperTypeName);
            if (textHelperType == null)
            {
                Log.Error("Can not find text helper type '{0}'.", m_TextHelperTypeName);
                return;
            }

            Utility.Text.ITextHelper textHelper = (Utility.Text.ITextHelper)Activator.CreateInstance(textHelperType);
            if (textHelper == null)
            {
                Log.Error("Can not create text helper instance '{0}'.", m_TextHelperTypeName);
                return;
            }

            Utility.Text.SetTextHelper(textHelper);
        }

        private void InitVersionHelper()
        {
            if (string.IsNullOrEmpty(m_VersionHelperTypeName))
            {
                return;
            }

            Type versionHelperType = Utility.Assembly.GetType(m_VersionHelperTypeName);
            if (versionHelperType == null)
            {
                throw new GameFrameworkException(Utility.Text.Format("Can not find version helper type '{0}'.", m_VersionHelperTypeName));
            }

            Version.IVersionHelper versionHelper = (Version.IVersionHelper)Activator.CreateInstance(versionHelperType);
            if (versionHelper == null)
            {
                throw new GameFrameworkException(Utility.Text.Format("Can not create version helper instance '{0}'.", m_VersionHelperTypeName));
            }

            Version.SetVersionHelper(versionHelper);
        }

        private void InitLogHelper()
        {
            if (string.IsNullOrEmpty(m_LogHelperTypeName))
            {
                return;
            }

            Type logHelperType = Utility.Assembly.GetType(m_LogHelperTypeName);
            if (logHelperType == null)
            {
                throw new GameFrameworkException(Utility.Text.Format("Can not find log helper type '{0}'.", m_LogHelperTypeName));
            }

            GameFrameworkLog.ILogHelper logHelper = (GameFrameworkLog.ILogHelper)Activator.CreateInstance(logHelperType);
            if (logHelper == null)
            {
                throw new GameFrameworkException(Utility.Text.Format("Can not create log helper instance '{0}'.", m_LogHelperTypeName));
            }

            GameFrameworkLog.SetLogHelper(logHelper);
        }

        private void InitCompressionHelper()
        {
            if (string.IsNullOrEmpty(m_CompressionHelperTypeName))
            {
                return;
            }

            Type compressionHelperType = Utility.Assembly.GetType(m_CompressionHelperTypeName);
            if (compressionHelperType == null)
            {
                Log.Error("Can not find compression helper type '{0}'.", m_CompressionHelperTypeName);
                return;
            }

            Utility.Compression.ICompressionHelper compressionHelper = (Utility.Compression.ICompressionHelper)Activator.CreateInstance(compressionHelperType);
            if (compressionHelper == null)
            {
                Log.Error("Can not create compression helper instance '{0}'.", m_CompressionHelperTypeName);
                return;
            }

            Utility.Compression.SetCompressionHelper(compressionHelper);
        }

        private void InitJsonHelper()
        {
            if (string.IsNullOrEmpty(m_JsonHelperTypeName))
            {
                return;
            }

            Type jsonHelperType = Utility.Assembly.GetType(m_JsonHelperTypeName);
            if (jsonHelperType == null)
            {
                Log.Error("Can not find JSON helper type '{0}'.", m_JsonHelperTypeName);
                return;
            }

            Utility.Json.IJsonHelper jsonHelper = (Utility.Json.IJsonHelper)Activator.CreateInstance(jsonHelperType);
            if (jsonHelper == null)
            {
                Log.Error("Can not create JSON helper instance '{0}'.", m_JsonHelperTypeName);
                return;
            }

            Utility.Json.SetJsonHelper(jsonHelper);
        }

        private void OnLowMemory()
        {
            Log.Warning("Low memory reported...");

            ObjectPoolModule objectPoolModule = GameEntry.GetModule<ObjectPoolModule>();
            if (objectPoolModule != null)
            {
                objectPoolModule.ReleaseAllUnused();
            }

            ResourceModule ResourceModule = GameEntry.GetModule<ResourceModule>();
            if (ResourceModule != null)
            {
                ResourceModule.ForceUnloadUnusedAssets(true);
            }
        }
    }
}
