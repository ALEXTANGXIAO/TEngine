using TEngine;
using UnityEngine;

public class EntityBullet : MonoBehaviour
{
	public float MoveSpeed = 20f;
	public float DelayDestroyTime = 5f;
	
	private Rigidbody _rigidbody;

	public void InitEntity()
	{
		_rigidbody.velocity = this.transform.forward * MoveSpeed;
	}

	void Awake()
	{
		_rigidbody = this.transform.GetComponent<Rigidbody>();
	}
	void OnTriggerEnter(Collider other)
	{
		var name = other.gameObject.name;
		if (name.StartsWith("Boundary"))
			return;

		var goName = this.gameObject.name;
		if (goName.StartsWith("enemy_bullet"))
		{
			if (name.StartsWith("enemy") == false)
			{
				GameModule.Resource.FreeGameObject(this.gameObject);
			}
		}

		if (goName.StartsWith("player_bullet"))
		{
			if (name.StartsWith("player") == false)
			{
				GameModule.Resource.FreeGameObject(this.gameObject);
			}
		}
	}
	void OnTriggerExit(Collider other)
	{
		var name = other.gameObject.name;
		if (name.StartsWith("Boundary"))
		{
			GameModule.Resource.FreeGameObject(this.gameObject);
		}
	}
}