using TEngine;

namespace GameLogic
{
    [System.AttributeUsage(System.AttributeTargets.Class)]
    internal class EventInterfaceImpAttribute : BaseAttribute
    {
        private EEventGroup _eGroup;
        public EEventGroup EventGroup => _eGroup;

        public EventInterfaceImpAttribute(EEventGroup group)
        {
            _eGroup = group;
        }
    }
}