using TEngine;
using UnityEngine;

namespace GameLogic.BattleDemo
{
    /// <summary>
    /// 实体创建预数据。
    /// </summary>
    public class EntityCreateData:IMemory
    {
        public ActorEntityType actorEntityType;
        
        public bool HasBornPos = false;
        
        public Vector3 BornPos;
        
        public Vector3 BornForward;
        
        /// <summary>
        /// 设置出生点。
        /// </summary>
        /// <param name="bornPos"></param>
        /// <param name="forward"></param>
        public void SetBornPos(Vector3 bornPos, Vector3 forward)
        {
            HasBornPos = true;
            BornPos = bornPos;
            BornForward = forward;
        }

        public void Clear()
        {
            actorEntityType = ActorEntityType.None;
            HasBornPos = false;
            BornPos = Vector3.zero;
            BornForward = Vector3.zero;
        }
    }
}