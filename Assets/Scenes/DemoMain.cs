using System.Collections;
using System.Collections.Generic;
using TEngine.Runtime;
using UnityEngine;

public class DemoMain : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //Demo示例，监听TEngine流程加载器OnStartGame事件
        //抛出这个事件说明框架流程加载完成（热更新，初始化等）
        GameEventMgr.Instance.AddEventListener(TEngineEvent.OnStartGame,OnStartGame);
    }

    private void OnStartGame()
    {
        Log.Debug("TEngineEvent.OnStartGame");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
