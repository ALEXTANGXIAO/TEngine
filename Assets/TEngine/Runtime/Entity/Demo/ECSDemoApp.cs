using System;
using TEngine.EntityModule;
using UnityEngine;

namespace TEngine
{
    public class EcsDemoApp : MonoBehaviour
    {
        public GameObject @object;

        void Start()
        {
            var entity = Entity.Create<Entity>();
            entity.AddComponent<TestUpdateCmpt>();
            GameObjectCmpt actor = entity.AddComponent<GameObjectCmpt>();
            actor.Name = typeof(GameObjectCmpt).ToString();
            actor.gameObject = Instantiate(@object);
            actor.transform = actor.gameObject.GetComponent<Transform>();
            entity.AddComponent<EntityComponent>();
            entity.CheckDebugInfo(actor.gameObject);
            Debug.Log(entity.ToString());

            var entity2 = Entity.Create<Entity>();
            GameObjectCmpt actor2 = entity2.AddComponent<GameObjectCmpt>();
            actor2.Name = typeof(GameObjectCmpt).ToString();
            actor2.gameObject = Instantiate(@object);
            actor2.transform = actor2.gameObject.GetComponent<Transform>();
            entity2.AddComponent<EntityComponent>();
            entity2.CheckDebugInfo(actor2.gameObject);
            Debug.Log(entity2.ToString());
            

           

            TimerMgr.Instance.AddTimer((args =>
            {
                GameEventMgr.Instance.Send<int,Action<EcsObject>>(EntityEvent.AttachToEntity,entity.InstanceId,(obj)=>
                {
                    TLogger.LogInfoSuccessd("Attach Success"+obj.HashCode);
                });
                Entity.Destroy(entity);
                Entity.Destroy(entity2);
            }), 3f,false);
        }


        void Update()
        {
            EntitySystem.Instance.Update();
        }
    }
}