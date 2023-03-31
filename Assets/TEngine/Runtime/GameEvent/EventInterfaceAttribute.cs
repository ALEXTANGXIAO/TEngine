using System;

namespace TEngine
{
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
         private EEventGroup _mGroup;
         public EventInterface(EEventGroup group)
         {
             _mGroup = group;
         }
    }
}
