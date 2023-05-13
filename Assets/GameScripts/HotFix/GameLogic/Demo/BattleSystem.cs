using System.Collections;
using GameLogic;
using TEngine;
using UnityEngine;
using UniFramework.Pooling;
using UniFramework.Utility;
using AudioType = TEngine.AudioType;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

/// <summary>
/// 战斗房间
/// </summary>
public class BattleRoom
{
	private enum ESteps
	{
		None,
		Ready,
		Spawn,
		WaitSpawn,
		WaitWave,
		GameOver,
	}

	private Spawner _entitySpawner;
	private GameObject _roomRoot;

	// 关卡参数
	private const int EnemyCount = 10;
	private const int EnemyScore = 10;
	private const int AsteroidScore = 1;
	private readonly Vector3 _spawnValues = new Vector3(6, 0, 20);
	private readonly string[] _entityLocations = new string[]
	{
		"asteroid01", "asteroid02", "asteroid03", "enemy_ship"
	};

	private ESteps _steps = ESteps.None;
	private int _totalScore = 0;
	private int _waveSpawnCount = 0;

	private UniTimer _startWaitTimer = UniTimer.CreateOnceTimer(1f);
	private UniTimer _spawnWaitTimer = UniTimer.CreateOnceTimer(0.75f);
	private UniTimer _waveWaitTimer = UniTimer.CreateOnceTimer(4f);

	public BattleRoom()
	{
		Utility.Unity.AddUpdateListener(UpdateRoom);
	}

	~BattleRoom()
	{
		Utility.Unity.RemoveUpdateListener(UpdateRoom);
		DestroyRoom();
	}

	/// <summary>
	/// 销毁房间
	/// </summary>
	public void DestroyRoom()
	{
		// 加载背景音乐
		GameModule.Audio.Stop(AudioType.Music, true);

		if (_entitySpawner != null)
			_entitySpawner.DestroyAll(true);

		if (_roomRoot != null)
			Object.Destroy(_roomRoot);

		GameModule.UI.CloseWindow<UIBattleWindow>();
	}

	/// <summary>
	/// 更新房间
	/// </summary>
	public void UpdateRoom()
	{
		if (_steps == ESteps.None || _steps == ESteps.GameOver)
			return;

		if (_steps == ESteps.Ready)
		{
			if (_startWaitTimer.Update(Time.deltaTime))
			{
				_steps = ESteps.Spawn;
			}
		}

		if (_steps == ESteps.Spawn)
		{
			var enemyLocation = _entityLocations[Random.Range(0, 4)];
			Vector3 spawnPosition = new Vector3(Random.Range(-_spawnValues.x, _spawnValues.x), _spawnValues.y, _spawnValues.z);
			Quaternion spawnRotation = Quaternion.identity;

			if (enemyLocation == "enemy_ship")
			{
				// 生成敌人实体
				var handle = _entitySpawner.SpawnSync(enemyLocation, _roomRoot.transform, spawnPosition, spawnRotation);
				var entity = handle.GameObj.GetComponent<EntityEnemy>();
				entity.InitEntity(handle);
			}
			else
			{
				// 生成小行星实体
				var handle = _entitySpawner.SpawnSync(enemyLocation, _roomRoot.transform, spawnPosition, spawnRotation);
				var entity = handle.GameObj.GetComponent<EntityAsteroid>();
				entity.InitEntity(handle);
			}

			_waveSpawnCount++;
			if (_waveSpawnCount >= EnemyCount)
			{
				_steps = ESteps.WaitWave;
			}
			else
			{
				_steps = ESteps.WaitSpawn;
			}
		}

		if (_steps == ESteps.WaitSpawn)
		{
			if (_spawnWaitTimer.Update(Time.deltaTime))
			{
				_spawnWaitTimer.Reset();
				_steps = ESteps.Spawn;
			}
		}

		if (_steps == ESteps.WaitWave)
		{
			if (_waveWaitTimer.Update(Time.deltaTime))
			{
				_waveWaitTimer.Reset();
				_waveSpawnCount = 0;
				_steps = ESteps.Spawn;
			}
		}
	}

	/// <summary>
	/// 加载房间
	/// </summary>
	public IEnumerator LoadRoom()
	{
		// 创建房间根对象
		_roomRoot = new GameObject("BattleRoom");

		// 加载背景音乐
		GameModule.Audio.Play(AudioType.Music, "music_background", true);

		// 创建游戏对象发生器
		_entitySpawner = UniPooling.CreateSpawner("DefaultPackage");

		// 创建游戏对象池
		yield return _entitySpawner.CreateGameObjectPoolAsync("player_ship");
		yield return _entitySpawner.CreateGameObjectPoolAsync("player_bullet");
		yield return _entitySpawner.CreateGameObjectPoolAsync("enemy_ship");
		yield return _entitySpawner.CreateGameObjectPoolAsync("enemy_bullet");
		yield return _entitySpawner.CreateGameObjectPoolAsync("asteroid01");
		yield return _entitySpawner.CreateGameObjectPoolAsync("asteroid02");
		yield return _entitySpawner.CreateGameObjectPoolAsync("asteroid03");
		yield return _entitySpawner.CreateGameObjectPoolAsync("explosion_asteroid");
		yield return _entitySpawner.CreateGameObjectPoolAsync("explosion_enemy");
		yield return _entitySpawner.CreateGameObjectPoolAsync("explosion_player");

		// 创建玩家实体对象
		var handle = _entitySpawner.SpawnSync("player_ship", _roomRoot.transform);
		var entity = handle.GameObj.GetComponent<EntityPlayer>();
		entity.InitEntity(handle);

		// 显示战斗界面
		yield return GameModule.UI.ShowUIAsync<UIBattleWindow>();

		// 监听游戏事件
		GameEvent.AddEventListener<Vector3,Quaternion>(ActorEventDefine.PlayerDead,OnPlayerDead);
		GameEvent.AddEventListener<Vector3,Quaternion>(ActorEventDefine.EnemyDead,OnEnemyDead);
		GameEvent.AddEventListener<Vector3,Quaternion>(ActorEventDefine.AsteroidExplosion,OnAsteroidExplosion);
		GameEvent.AddEventListener<Vector3,Quaternion>(ActorEventDefine.PlayerFireBullet,OnPlayerFireBullet);
		GameEvent.AddEventListener<Vector3,Quaternion>(ActorEventDefine.EnemyFireBullet,OnEnemyFireBullet);

		_steps = ESteps.Ready;
	}

	#region 接收事件

	private void OnPlayerDead(Vector3 position,Quaternion rotation)
	{
		// 创建爆炸效果
		var handle = _entitySpawner.SpawnSync("explosion_player", _roomRoot.transform, position, rotation);
		var entity = handle.GameObj.GetComponent<EntityEffect>();
		entity.InitEntity(handle);

		_steps = ESteps.GameOver;
		GameEvent.Send(ActorEventDefine.GameOver);
	}

	private void OnEnemyDead(Vector3 position,Quaternion rotation)
	{
		// 创建爆炸效果
		var handle = _entitySpawner.SpawnSync("explosion_enemy", _roomRoot.transform, position, rotation);
		var entity = handle.GameObj.GetComponent<EntityEffect>();
		entity.InitEntity(handle);

		_totalScore += EnemyScore;
		GameEvent.Send(ActorEventDefine.ScoreChange,_totalScore);
	}

	private void OnAsteroidExplosion(Vector3 position,Quaternion rotation)
	{
		// 创建爆炸效果
		var handle = _entitySpawner.SpawnSync("explosion_asteroid", _roomRoot.transform, position, rotation);
		var entity = handle.GameObj.GetComponent<EntityEffect>();
		entity.InitEntity(handle);

		_totalScore += AsteroidScore;
		GameEvent.Send(ActorEventDefine.ScoreChange,_totalScore);
	}

	private void OnPlayerFireBullet(Vector3 position,Quaternion rotation)
	{
		// 创建子弹实体
		var handle = _entitySpawner.SpawnSync("player_bullet", _roomRoot.transform, position, rotation);
		var entity = handle.GameObj.GetComponent<EntityBullet>();
		entity.InitEntity(handle);
	}

	private void OnEnemyFireBullet(Vector3 position,Quaternion rotation)
	{
		// 创建子弹实体
        var handle = _entitySpawner.SpawnSync("enemy_bullet", _roomRoot.transform,position,rotation);
        var entity = handle.GameObj.GetComponent<EntityBullet>();
        entity.InitEntity(handle);
	}

	#endregion
}