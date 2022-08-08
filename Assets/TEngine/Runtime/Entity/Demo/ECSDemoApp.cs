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
            var entity = Entity.Create<GameObjectEntity>();
            entity.AddComponent<TestUpdateCmpt>();
            entity.Bind(Instantiate(@object));
            Debug.Log(entity.ToString());

            var entity2 = Entity.Create<GameObjectEntity>();
            entity2.Bind(Instantiate(@object));
            Debug.Log(entity2.ToString());
           

            TimerMgr.Instance.AddTimer((args =>
            {
                GameEventMgr.Instance.Send<int,Action<EcsObject>>(EntityEvent.AttachToEntity,entity.InstanceId,(obj)=>
                {
                    TLogger.LogInfoSuccessd("Attach Success"+obj.HashCode);
                });
                Entity.Destroy(entity);
                Entity.Destroy(entity2);


                TimerMgr.Instance.AddTimer(objects =>
                {
                    Start();
                }, 3f);
            }), 3f,false);
        }


        void Update()
        {
            EntitySystem.Instance.Update();
        }
    }
}