using TEngine;

namespace GameLogic
{
    public static class ActorEventDefine
    {
        public static readonly int ScoreChange = StringId.StringToHash("ActorEventDefine.ScoreChange");
        public static readonly int GameOver = StringId.StringToHash("ActorEventDefine.GameOver");
        public static readonly int EnemyDead = StringId.StringToHash("ActorEventDefine.EnemyDead");
        public static readonly int PlayerDead = StringId.StringToHash("ActorEventDefine.PlayerDead");
        public static readonly int AsteroidExplosion = StringId.StringToHash("ActorEventDefine.AsteroidExplosion");
        public static readonly int EnemyFireBullet = StringId.StringToHash("ActorEventDefine.EnemyFireBullet");
        public static readonly int PlayerFireBullet = StringId.StringToHash("ActorEventDefine.PlayerFireBullet");
    }
}