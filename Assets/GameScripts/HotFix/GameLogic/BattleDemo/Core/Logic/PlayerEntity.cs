namespace GameLogic.BattleDemo
{
    public class PlayerEntity : EntityLogic
    {
        public override ActorEntityType GetActorEntityType()
        {
            return ActorEntityType.Player;
        }

        protected override void OnLogicCreate()
        {
            base.OnLogicCreate();
            ActorData = AddComponent<ActorData>();
            BuffComponent = AddComponent<BuffComponent>();
            SkillCaster = AddComponent<SkillCasterComponent>();
        }
    }
}