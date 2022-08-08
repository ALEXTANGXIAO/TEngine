using TEngine.EntityModule;
using UnityEngine;

public class GameObjectEntity :Entity
{
    public GameObject gameObject;
    public Transform tranform;

    public void Bind(GameObject gameObject)
    {
        this.gameObject = gameObject;
        tranform = this.gameObject.transform;
    }

    public override void Awake()
    {
        base.Awake();
    }

    public override void OnDestroy()
    {
        if (gameObject != null)
        {
            Object.Destroy(gameObject);
            gameObject = null;
        }
        tranform = null;
        base.OnDestroy();
    }
}
