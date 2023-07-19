using TEngine;

namespace GameLogic.BattleDemo
{
    /// <summary>
    /// 逻辑层组件实体。
    /// </summary>
    public abstract class EntityLogicComponent: Entity
    {
        public EntityLogic Owner => (EntityLogic)Parent;
    }
}