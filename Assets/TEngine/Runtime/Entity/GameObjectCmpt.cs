namespace TEngine.EntityModule
{
    /// <summary>
    /// Entity Actor
    /// </summary>
    public class GameObjectCmpt :EntityComponent
    {
        public string Name;
        public UnityEngine.GameObject gameObject;
        public UnityEngine.Transform transform;

        public override void OnDestroy()
        {
            if (gameObject != null)
            {
                UnityEngine.GameObject.Destroy(gameObject);
            }
        }
    }
}