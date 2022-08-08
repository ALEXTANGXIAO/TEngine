using TEngine;
using UnityEngine;

public class EcsDemoApp : MonoBehaviour
{
    public GameObject @object;

    void Start()
    {
        var entity = Entity.Create<Entity>();
        EcsGameObject actor = entity.AddComponent<EcsGameObject>();
        actor.Name = typeof(EcsGameObject).ToString();
        actor.gameObject = Instantiate(@object);
        actor.transform = actor.gameObject.GetComponent<Transform>();
        entity.AddComponent<EcsComponent>();
        entity.CheckDebugInfo(actor.gameObject);
        Debug.Log(entity.ToString());

        var entity2 = Entity.Create<Entity>();
        EcsGameObject actor2 = entity2.AddComponent<EcsGameObject>();
        actor2.Name = typeof(EcsGameObject).ToString();
        actor2.gameObject = Instantiate(@object);
        actor2.transform = actor2.gameObject.GetComponent<Transform>();
        entity2.AddComponent<EcsComponent>();
        entity2.CheckDebugInfo(actor2.gameObject);
        Debug.Log(entity2.ToString());
    }

    void Update()
    {
        EcsSystem.Instance.Update();
    }
}
