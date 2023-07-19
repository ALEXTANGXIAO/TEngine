using UnityEngine;

namespace GameLogic.BattleDemo
{
    /// <summary>
    /// 表现层实体。
    /// </summary>
    public abstract class EntityVisual:MonoBehaviour
    {
        public EntityLogic Entity { protected set; get;}
        
        
    }
}