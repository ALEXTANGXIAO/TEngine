using TEngine;

namespace GameLogic.BattleDemo
{
    /// <summary>
    /// 逻辑层实体。
    /// </summary>
    public abstract class EntityLogic : Entity
    {
        /// <summary>
        /// 逻辑层实体类型。
        /// </summary>
        /// <returns></returns>
        public abstract ActorEntityType GetActorEntityType();
        
        /// <summary>
        /// 是否是战斗起始的Actor。
        /// <remarks>,比如双方参与战斗的玩家，或者技能编辑器里的Caster。</remarks>
        /// </summary>
        public bool IsStartActor;

        public EntityCreateData CreateData { private set; get; }
        
        public virtual string GetActorName()
        {
            return string.Empty;
        }

        #region 缓存常用组件
        public ActorData ActorData { protected set; get; }
        
        public BuffComponent BuffComponent { protected set; get; }
        public SkillCasterComponent SkillCaster { protected set; get; }
        #endregion

        #region 生命周期

        internal bool LogicCreate(EntityCreateData entityCreateData)
        {
            CreateData = entityCreateData;
            OnLogicCreate();
            return true;
        }
        
        protected virtual void OnLogicCreate()
        {
            
        }

        internal void LogicDestroy()
        {
            OnLogicDestroy();
            if (CreateData != null)
            {
                MemoryPool.Release(CreateData);
            }
        }
        
        protected virtual void OnLogicDestroy()
        {
            
        }
        #endregion
    }
}