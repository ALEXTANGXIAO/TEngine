using TEngine;
using UnityEngine;

public class EntityEffect : MonoBehaviour
{
	public float DelayDestroyTime = 1f;

	public void InitEntity()
	{
		Invoke(nameof(DelayDestroy), DelayDestroyTime);
	}
	private void DelayDestroy()
	{
		GameModule.Resource.FreeGameObject(this.gameObject);
	}
}