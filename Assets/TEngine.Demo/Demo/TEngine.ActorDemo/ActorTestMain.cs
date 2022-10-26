using System.Collections.Generic;
using TEngine.Runtime;
using TEngine.Runtime.Actor;
using UnityEngine;

public class ActorTestMain : MonoBehaviour
{
    private uint _actorId = 1000;
    
    void Start()
    {
        //Demo示例，监听TEngine流程加载器OnStartGame事件
        //抛出这个事件说明框架流程加载完成（热更新，初始化等）
        GameEvent.AddEventListener(TEngineEvent.OnStartGame,OnStartGame);
    }

    private void OnStartGame()
    {
        Log.Debug("TEngineEvent.OnStartGame");

        
        //激活ActorManager
        ActorManager.Instance.Active();
        
        //注册Actor类型
        ActorManager.Instance.RegisterActorType(ActorType.ActorPlayer,typeof(PlayerActor));

        //创建Actor
        var actor = ActorManager.Instance.CreateGameActor(ActorType.ActorPlayer, _actorId++, true);

        //附加组件（ActorGameObjet可视化）
        actor.Attach<ModelComponent>();
        
        actor.Attach<FsmComponent>();
        
        actor.Attach<AnimatorComponent>();
    }
}