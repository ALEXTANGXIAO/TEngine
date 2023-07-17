using TEngine;

namespace GameLogic.BattleDemo
{
    /// <summary>
    /// 技能释放组件。
    /// </summary>
    public class SkillCasterComponent:EntityLogicComponent
    {

        /// <summary>
        /// 播放技能。
        /// </summary>
        /// <param name="skillId">技能Id。</param>
        /// <param name="target">目标。</param>
        /// <param name="checkCd">是否检测CD。</param>
        /// <param name="forceCaster">是否强制释放。</param>
        /// <returns>是否播放成功。</returns>
        internal void PlaySkill(int skillId, EntityLogic target = null, bool forceCaster = false, bool checkCd = true)
        {
            Log.Assert(skillId > 0, $"ActorName: {Owner.GetActorName()}");
            Log.Debug("Start Play SKill[{0}]", skillId);
            
            var skillBaseConfig = ConfigLoader.Instance.Tables.TbSkill.Get(skillId);
            
            if (skillBaseConfig == null)
            {
                Log.Error("GetSkillBaseConfig Failed, invalid skillID: {0}", skillId);
                return;
            }
            
            
        }
    }
}