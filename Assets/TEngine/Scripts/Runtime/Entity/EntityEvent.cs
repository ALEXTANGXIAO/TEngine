namespace TEngine.Runtime.Entity
{
    public class EntityEvent
    {
        public static int ShowEntitySuccess = StringId.StringToHash("EntityEvent.ShowEntitySuccess");
        public static int ShowEntityFailure = StringId.StringToHash("EntityEvent.ShowEntityFailure");
        public static int ShowEntityUpdate = StringId.StringToHash("EntityEvent.ShowEntityUpdate");
        public static int ShowEntityDependency = StringId.StringToHash("EntityEvent.ShowEntityDependency");
        public static int HideEntityComplete = StringId.StringToHash("EntityEvent.HideEntityComplete");
    }
}

