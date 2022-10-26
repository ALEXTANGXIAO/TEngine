using System.Collections;
using System.Collections.Generic;
using TEngine.Runtime;
using TEngine.Runtime.UIModule;
using UnityEngine;

public class UITestMain : MonoBehaviour
{
    void Start()
    {
        //Demo示例，监听TEngine流程加载器OnStartGame事件
        //抛出这个事件说明框架流程加载完成（热更新，初始化等）
        GameEvent.AddEventListener(TEngineEvent.OnStartGame,OnStartGame);
    }

    private int _loopTime = 0;
    /// <summary>
    /// OnStartGame
    /// </summary>
    private void OnStartGame()
    {
        // 激活UI系统
        UISys.Instance.Active();

        UISys.Mgr.ShowWindow<TestUI>();

        TimerMgr.Instance.AddTimer((args =>
        {
            _loopTime++;
            GameEvent.Send(TestUI.TestEvent);
            if (_loopTime > 7)
            {
                UISys.Mgr.CloseWindow<TestUI>();
            }
        }), 1f, true);
    }
}
