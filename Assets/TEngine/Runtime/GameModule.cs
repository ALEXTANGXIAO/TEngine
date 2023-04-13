using System;
using System.Collections.Generic;
using TEngine;
using UnityEngine;

/// <summary>
/// 游戏模块
/// </summary>
public class GameModule:MonoBehaviour
{
    #region BaseModules
    /// <summary>
    /// 获取游戏基础模块。
    /// </summary>
    public static RootModule Base { get; private set; }

    /// <summary>
    /// 获取调试模块。
    /// </summary>
    public static DebuggerModule Debugger { get; private set; }

    /// <summary>
    /// 获取有限状态机模块。
    /// </summary>
    public static FsmModule Fsm { get; private set; }

    /// <summary>
    /// 获取对象池模块。
    /// </summary>
    public static ObjectPoolModule ObjectPool { get; private set; }

    /// <summary>
    /// 获取资源模块。
    /// </summary>
    public static ResourceModule Resource { get; private set; }
    
    /// <summary>
    /// 流程管理模块。
    /// </summary>
    public static ProcedureModule Procedure { get; private set; }

    /// <summary>
    /// 获取配置模块。
    /// </summary>
    public static SettingModule Setting { get; private set; }

    /// <summary>
    /// 获取UI模块。
    /// </summary>
    public static UIModule UI { get; private set; }
    
    /// <summary>
    /// 获取音频模块。
    /// </summary>
    public static AudioModule Audio { get; private set; }

    #endregion

    /// <summary>
    /// 初始化系统框架模块
    /// </summary>
    private static void InitFrameWorkModules()
    {
        Base = Get<RootModule>();
        Debugger = Get<DebuggerModule>();
        Fsm = Get<FsmModule>();
        ObjectPool = Get<ObjectPoolModule>();
        Resource = Get<ResourceModule>();
        Procedure = Get<ProcedureModule>();
        Setting = Get<SettingModule>();
        UI = Get<UIModule>();
        Audio = Get<AudioModule>();
    }

    private static void InitCustomModules()
    {
        
    }
  
    private static readonly Dictionary<Type, GameFrameworkModuleBase> Modules = new Dictionary<Type, GameFrameworkModuleBase>();

    public static T Get<T>()where T : GameFrameworkModuleBase
    {
        Type type = typeof(T);
        
        if (Modules.ContainsKey(type))
        {
            return Modules[type] as T;
        }
        
        T module = TEngine.GameEntry.GetModule<T>();
        
        Log.Assert(condition:module != null,$"{typeof(T)} is null");
        
        Modules.Add(type,module);

        return module;
    }

    private void Start()
    {
        Log.Info("GameModule Active");
        InitFrameWorkModules();
        InitCustomModules();
        DontDestroyOnLoad(gameObject);
        gameObject.name = $"[{nameof(GameModule)}]";
    }
}