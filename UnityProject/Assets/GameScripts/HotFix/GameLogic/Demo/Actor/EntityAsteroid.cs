using GameLogic;
using TEngine;
using UnityEngine;

public class EntityAsteroid : MonoBehaviour
{
	public float MoveSpeed = -5f;
	public float Tumble = 5f;

	private Rigidbody _rigidbody;

	public void InitEntity()
	{
		_rigidbody.velocity = this.transform.forward * MoveSpeed;
		_rigidbody.angularVelocity = Random.insideUnitSphere * Tumble;
	}

	void Awake()
	{
		_rigidbody = this.transform.GetComponent<Rigidbody>();
	}
	void OnTriggerEnter(Collider other)
	{
		var name = other.gameObject.name;
		if (name.StartsWith("player"))
		{
			GameEvent.Send(ActorEventDefine.AsteroidExplosion,this.transform.position, this.transform.rotation);
			PoolManager.Instance.PushGameObject(this.gameObject);
		}
	}
	void OnTriggerExit(Collider other)
	{
		var name = other.gameObject.name;
		if (name.StartsWith("Boundary"))
		{
			PoolManager.Instance.PushGameObject(this.gameObject);
		}
	}
}