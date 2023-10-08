using System;

namespace TEngine
{
    /// <summary>
    /// 事件分组枚举。
    /// </summary>
    public enum EEventGroup
    {
        /// <summary>
        /// UI相关的交互。
        /// </summary>
        GroupUI,   

        /// <summary>
        /// 逻辑层内部相关的交互。
        /// </summary>
        GroupLogic,
    }

    [System.AttributeUsage(System.AttributeTargets.Interface)]
    public class EventInterface : Attribute
     {
         private EEventGroup _eGroup;
         public EventInterface(EEventGroup group)
         {
             _eGroup = group;
         }
    }
}
