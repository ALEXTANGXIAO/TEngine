using System;
using System.Collections.Generic;
using TEngine;

namespace GameLogic.BattleDemo
{
    /// <summary>
    /// 实体类型。
    /// </summary>
    public enum ActorEntityType
    {
        None,
        Player,
        Monster,
        Pet,
        Npc,
    }
    
    /// <summary>
    /// 逻辑层实体管理器。
    /// </summary>
    public class EntityLogicMgr
    {
        private static readonly Dictionary<long, EntityLogic> EntityLogicPool = new Dictionary<long, EntityLogic>();
        private static readonly List<EntityLogic> ListEntityLogics = new List<EntityLogic>();
     
        public static event Action<EntityLogic> OnEntityCreate;
        public static event Action<EntityLogic> OnEntityDestroy;

        public static List<EntityLogic> GetAllActor(ref List<EntityLogic> temp)
        {
            if (temp == null)
            {
                temp = new List<EntityLogic>();
            }
            temp.AddRange(ListEntityLogics);
            return temp;
        }
        
        public static List<EntityLogic> GetTypeActor(ref List<EntityLogic> temp,ActorEntityType type)
        {
            if (temp == null)
            {
                temp = new List<EntityLogic>();
            }

            foreach (var actor in ListEntityLogics)
            {
                if (actor.GetActorEntityType() == type)
                {
                    temp.Add(actor);
                }
            }
            return temp;
        }

        public static EntityLogic CreateEntityLogic(EntityCreateData entityCreateData, bool isStartActor = false)
        {
            if (entityCreateData == null)
            {
                Log.Error("create actor failed, create data is null");
                return null;
            }
            var actor = CreateActorEntityObject(entityCreateData.actorEntityType);
            if (actor == null)
            {
                Log.Error("create actor failed, create data is {0}", entityCreateData);
                return null;
            }

            actor.IsStartActor = isStartActor;
            if (!actor.LogicCreate(entityCreateData))
            {
                DestroyActor(actor);
                return null;
            }

            if (OnEntityCreate != null)
            {
                OnEntityCreate(actor);
            }

            Log.Debug("entityLogic created: {0}", actor.GetActorName());
            return actor;
        }
        
        private static EntityLogic CreateActorEntityObject(ActorEntityType actorType)
        {
            EntityLogic entityLogic = null;

            switch (actorType)
            {
                case ActorEntityType.Player:
                {
                    entityLogic = Entity.Create<PlayerEntity>(GameApp.Instance.Scene);
                    break;
                }
                default:
                {
                    Log.Error("unknown actor type:{0}", actorType);
                    break;
                }
            }

            if (entityLogic != null)
            {
                EntityLogicPool.Add(entityLogic.RuntimeId, entityLogic);
                ListEntityLogics.Add(entityLogic);
            }
            return entityLogic;
        }
        

        public static bool DestroyActor(long runtimeId)
        {
            EntityLogicPool.TryGetValue(runtimeId, out EntityLogic entityLogic);
            if (entityLogic != null)
            {
                return DestroyActor(entityLogic);
            }
            return false;
        }
        
        public static bool DestroyActor(EntityLogic entityLogic)
        {
            Log.Debug("on destroy entityLogic {0}", entityLogic.RuntimeId);


            var runtimeId = entityLogic.RuntimeId;
            Log.Assert(EntityLogicPool.ContainsKey(runtimeId));

            if (OnEntityDestroy != null)
            {
                OnEntityDestroy(entityLogic);
            }

            entityLogic.LogicDestroy();
            EntityLogicPool.Remove(runtimeId);
            ListEntityLogics.Remove(entityLogic);
            return true;
        }
    }
}