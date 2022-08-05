namespace TEngine
{
    /// <summary>
    /// Ecs Actor
    /// </summary>
    public class EcsActor : EcsComponent
    {
        public string Name;
        public UnityEngine.GameObject gameObject;
        public UnityEngine.Transform transform;
        public uint ActorID;
    }
}