namespace TEngineCore
{
    /// <summary>
    /// ECS Actor
    /// </summary>
    public class ECSActor : ECSComponent
    {
        public string Name;
        public UnityEngine.GameObject gameObject;
        public UnityEngine.Transform transform;
        public uint ActorID;
    }
}