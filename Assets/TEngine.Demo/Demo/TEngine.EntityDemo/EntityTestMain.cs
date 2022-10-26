using System;
using TEngine.Demo.TEngine.EntityDemo;
using TEngine.Runtime;
using TEngine.Runtime.Entity;
using UnityEngine;
using LightType = TEngine.Demo.TEngine.EntityDemo.LightType;

public class EntityTestMain : MonoBehaviour
{
    void Start()
    {
        //Demo示例，监听TEngine流程加载器OnStartGame事件
        //抛出这个事件说明框架流程加载完成（热更新，初始化等）
        GameEvent.AddEventListener(TEngineEvent.OnStartGame,OnStartGame);
    }

    private void OnStartGame()
    {
        Log.Debug("TEngineEvent.OnStartGame");

        for (int i = 1; i < 10; i++)
        {
            //实体数据创建
            EntityData entityData= EntityData.Create();
            entityData.Position = new Vector3(i, i, 0);
            entityData.Rotation = Quaternion.identity;
            //实体系统创建实体，自动创建实体组
            EntitySystem.Instance.CreateEntity<EntityCube>("Cube",entityData);
        }
        
        LightEntityMgr.Instance.CreateLight(LightType.DirectionalLight,Vector3.zero, Quaternion.identity);
    }
    
    /// <summary>
    /// 实体Cube
    /// </summary>
    public class EntityCube:EntityLogicEx
    {
        protected override void OnAttached(EntityLogic childEntity, Transform parentTransform, object userData)
        {
            base.OnAttached(childEntity, parentTransform, userData);
            Log.Info("OnAttached");
        }

        /// <summary>
        /// 实体显示事件
        /// </summary>
        /// <param name="userData">EntityData</param>
        protected override void OnShow(object userData)
        {
            base.OnShow(userData);
            Log.Info("OnShow");
        }

        protected override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);
        }
    }
}
