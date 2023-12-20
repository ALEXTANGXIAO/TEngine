using TEngine;

namespace GameLogic
{
    public static class ActorEventDefine
    {
        public static readonly int ScoreChange = RuntimeId.ToRuntimeId("ActorEventDefine.ScoreChange");
        public static readonly int GameOver = RuntimeId.ToRuntimeId("ActorEventDefine.GameOver");
        public static readonly int EnemyDead = RuntimeId.ToRuntimeId("ActorEventDefine.EnemyDead");
        public static readonly int PlayerDead = RuntimeId.ToRuntimeId("ActorEventDefine.PlayerDead");
        public static readonly int AsteroidExplosion = RuntimeId.ToRuntimeId("ActorEventDefine.AsteroidExplosion");
        public static readonly int EnemyFireBullet = RuntimeId.ToRuntimeId("ActorEventDefine.EnemyFireBullet");
        public static readonly int PlayerFireBullet = RuntimeId.ToRuntimeId("ActorEventDefine.PlayerFireBullet");
    }
}