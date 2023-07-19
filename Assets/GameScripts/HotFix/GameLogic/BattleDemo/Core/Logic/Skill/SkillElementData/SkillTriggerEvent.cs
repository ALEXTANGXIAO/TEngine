using System;

namespace GameLogic.BattleDemo
{
    [Serializable]
    public enum SkillTriggerEvent
    {
        /// <summary>
        /// 无触发。
        /// </summary>
        NoneEvent,
        
        /// <summary>
        /// 时间点触发。
        /// </summary>
        TimeEvent,

        /// <summary>
        /// 施法结束触发。
        /// </summary>
        AnimStopEvent,
    }
}