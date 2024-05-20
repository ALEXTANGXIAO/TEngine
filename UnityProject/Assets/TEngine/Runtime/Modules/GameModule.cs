using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TEngine
{
    /// <summary>
    /// 游戏模块。
    /// </summary>
    public partial class GameModule : MonoBehaviour
    {
        private static readonly Dictionary<Type, Module> _moduleMaps = new Dictionary<Type, Module>(ModuleImpSystem.DesignModuleCount);

        private static GameObject _gameModuleRoot;
        
        #region 框架模块
        /// <summary>
        /// 获取游戏基础模块。
        /// </summary>
        public static RootModule Base
        {
            get => _base ??= Get<RootModule>();
            private set => _base = value;
        }

        private static RootModule _base;

        /// <summary>
        /// 获取调试模块。
        /// </summary>
        public static DebuggerModule Debugger
        {
            get => _debugger ??= Get<DebuggerModule>();
            private set => _debugger = value;
        }


        private static DebuggerModule _debugger;

        /// <summary>
        /// 获取有限状态机模块。
        /// </summary>
        public static FsmModule Fsm => _fsm ??= Get<FsmModule>();

        private static FsmModule _fsm;

        /// <summary>
        /// 流程管理模块。
        /// </summary>
        public static ProcedureModule Procedure => _procedure ??= Get<ProcedureModule>();

        private static ProcedureModule _procedure;

        /// <summary>
        /// 获取对象池模块。
        /// </summary>
        public static ObjectPoolModule ObjectPool => _objectPool ??= Get<ObjectPoolModule>();

        private static ObjectPoolModule _objectPool;

        /// <summary>
        /// 获取资源模块。
        /// </summary>
        public static ResourceModule Resource => _resource ??= Get<ResourceModule>();

        private static ResourceModule _resource;

        /// <summary>
        /// 获取音频模块。
        /// </summary>
        public static AudioModule Audio => _audio ??= Get<AudioModule>();

        private static AudioModule _audio;

        /// <summary>
        /// 获取配置模块。
        /// </summary>
        public static SettingModule Setting => _setting ??= Get<SettingModule>();

        private static SettingModule _setting;
        
        /// <summary>
        /// 获取UI模块。
        /// </summary>
        public static UIModule UI => _ui ??= Get<UIModule>();

        private static UIModule _ui;
        
        /// <summary>
        /// 获取多语言模块。
        /// </summary>
        public static LocalizationModule Localization => _localization ??= Get<LocalizationModule>();

        private static LocalizationModule _localization;
        
        /// <summary>
        /// 获取场景模块。
        /// </summary>
        public static SceneModule Scene => _scene ??= Get<SceneModule>();
        
        private static SceneModule _scene;
        
        /// <summary>
        /// 获取计时器模块。
        /// </summary>
        public static TimerModule Timer => _timer ??= Get<TimerModule>();
        
        private static TimerModule _timer;
        
        /// <summary>
        /// 资源组件拓展。
        /// </summary>
        public static ResourceExtComponent ResourceExt => _resourceExt ??= Get<ResourceExtComponent>();
        
        private static ResourceExtComponent _resourceExt;
        #endregion

        /// <summary>
        /// 获取游戏框架模块类。
        /// </summary>
        /// <typeparam name="T">游戏框架模块类。</typeparam>
        /// <returns>游戏框架模块实例。</returns>
        public static T Get<T>() where T : Module
        {
            Type type = typeof(T);

            if (_moduleMaps.TryGetValue(type, out var ret))
            {
                return ret as T;
            }

            T module = ModuleSystem.GetModule<T>();

            Log.Assert(condition: module != null, $"{typeof(T)} is null");

            _moduleMaps.Add(type, module);

            return module;
        }

        private void Start()
        {
            Log.Info("GameModule Active");
            gameObject.name = $"[{nameof(GameModule)}]";
            _gameModuleRoot = gameObject;
            DontDestroyOnLoad(gameObject);
        }

        public static void Shutdown(ShutdownType shutdownType)
        {
            Log.Info("GameModule Shutdown");
            if (_gameModuleRoot != null)
            {
                Destroy(_gameModuleRoot);
                _gameModuleRoot = null;
            }
            _moduleMaps.Clear();
            
            _base = null;
            _debugger = null;
            _fsm = null;
            _procedure = null;
            _objectPool = null;
            _resource = null;
            _audio = null;
            _setting = null;
            _ui = null;
            _localization = null;
            _scene = null;
            _timer = null;
            _resourceExt = null;
        }

        #region HandlePlayModeStateChanged
        private void OnEnable()
        {
#if UNITY_EDITOR
            EditorApplication.playModeStateChanged += HandlePlayModeStateChanged;
#endif
        }

        private void OnDisable()
        {
#if UNITY_EDITOR
            EditorApplication.playModeStateChanged -= HandlePlayModeStateChanged;
#endif
        }

#if UNITY_EDITOR
        void HandlePlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode)
            {
				ModuleImpSystem.Shutdown();
                ModuleSystem.Shutdown(ShutdownType.Quit);
            }
        }
#endif
        #endregion

    }
}