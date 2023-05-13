using System.Collections.Generic;
using TEngine;
using UniFramework.Pooling;

public partial class GameApp
{
    private List<ILogicSys> _listLogicMgr;
    
    private void Init()
    {
        _listLogicMgr = new List<ILogicSys>();
        RegisterAllSystem();
        InitSystemSetting();
    }
    
    /// <summary>
    /// 设置一些通用的系统属性。
    /// </summary>
    private void InitSystemSetting()
    {
        // 初始化对象池系统
        UniPooling.Initalize();
        AddLogicSys(BehaviourSingleSystem.Instance);
    }

    /// <summary>
    /// 注册所有逻辑系统
    /// </summary>
    private void RegisterAllSystem()
    {
        
    }
    
    /// <summary>
    /// 注册逻辑系统。
    /// </summary>
    /// <param name="logicSys">ILogicSys</param>
    /// <returns></returns>
    protected bool AddLogicSys(ILogicSys logicSys)
    {
        if (_listLogicMgr.Contains(logicSys))
        {
            Log.Fatal("Repeat add logic system: {0}", logicSys.GetType().Name);
            return false;
        }

        if (!logicSys.OnInit())
        {
            Log.Fatal("{0} Init failed", logicSys.GetType().Name);
            return false;
        }

        _listLogicMgr.Add(logicSys);

        return true;
    }
}