using System;

namespace TEngine.Runtime
{
    public enum DEventGroup
    {
        /// <summary>
        /// UI相关的交互
        /// </summary>
        GroupUI,   

        /// <summary>
        /// 逻辑层内部相关的交互
        /// </summary>
        GroupLogic,
    }

    [System.AttributeUsage(System.AttributeTargets.Interface)]
    public class DEventInterface : Attribute
     {
         public DEventGroup m_group;
         public DEventInterface(DEventGroup group)
         {
             m_group = group;
         }
    }
}
