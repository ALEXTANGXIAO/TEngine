using TEngine;
using UnityEngine;

public class EcsDemoApp : MonoBehaviour
{
    public GameObject @object;

    void Start()
    {
        var entity = EcsSystem.Instance.Create<Entity>();
        EcsActor actor = entity.AddComponent<EcsActor>();
        actor.Name = typeof(EcsActor).ToString();
        actor.gameObject = Instantiate(@object);
        actor.transform = actor.gameObject.GetComponent<Transform>();
        entity.AddComponent<EcsComponent>();

        Debug.Log(entity.ToString());
    }

    void Update()
    {
        EcsSystem.Instance.Update();
    }
}
