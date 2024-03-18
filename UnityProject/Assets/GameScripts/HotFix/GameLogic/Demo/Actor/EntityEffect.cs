using GameLogic;
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
		PoolManager.Instance.PushGameObject(this.gameObject);
	}
}