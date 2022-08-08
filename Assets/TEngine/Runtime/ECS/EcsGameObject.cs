namespace TEngine
{
    /// <summary>
    /// Ecs Actor
    /// </summary>
    public class EcsGameObject : EcsComponent
    {
        public string Name;
        public UnityEngine.GameObject gameObject;
        public UnityEngine.Transform transform;
    }
}