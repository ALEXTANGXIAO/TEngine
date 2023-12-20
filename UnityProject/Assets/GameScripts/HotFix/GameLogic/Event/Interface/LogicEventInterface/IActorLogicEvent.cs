using System;
using TEngine;

namespace GameLogic
{
    /// <summary>
    /// 示例货币枚举。
    /// </summary>
    public enum CurrencyType
    {
        None,
        Gold,
        Diamond,
    }
    
    /// <summary>
    /// 示意逻辑层事件。
    /// <remarks> 优化抛出事件，通过接口约束事件参数。</remarks>
    /// <remarks> example: GameEvent.Get<IActorLogicEvent>().OnMainPlayerCurrencyChange(CurrencyType.Gold,oldVal,newVal); </remarks>
    /// </summary>
    [EventInterface(EEventGroup.GroupLogic)]
    interface IActorLogicEvent
    {
        void OnMainPlayerDataChange();
        
        void OnMainPlayerLevelChange();

        void OnMainPlayerGoldChange(uint oldVal, uint newVal);

        void OnMainPlayerDiamondChange(uint oldVal, uint newVal);
        
        void OnMainPlayerBindDiamondChange(uint oldVal, uint newVal);
        
        void OnMainPlayerCurrencyChange(CurrencyType type, uint oldVal, uint newVal);
        
        void OnMainPlayerExpChange(ulong oldVal, ulong newVal);
    }
}
