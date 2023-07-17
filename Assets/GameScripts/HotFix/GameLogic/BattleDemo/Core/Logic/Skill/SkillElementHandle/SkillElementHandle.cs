using GameConfig.Battle;
using TEngine;

namespace GameLogic.BattleDemo
{
    /// <summary>
    /// 每个元素的状态。
    /// </summary>
    public enum SkillElementStatus
    {
        /// <summary>
        /// 元素状态初始化。
        /// </summary>
        ELEM_STATUS_INIT,
        
        /// <summary>
        /// 元素状态运行中。
        /// </summary>
        ELEM_STATUS_RUN,
        
        /// <summary>
        /// 元素状态停止。
        /// </summary>
        ELEM_STATUS_STOP
    }
    
    public abstract class SkillElementHandle
    {
        private static uint m_nextHandleGID = 1;
        
        public uint m_handleGID;
        public EntityLogic CasterActor;
        protected uint m_skillId;
        protected SkillAttrDamageData[] m_damageAttr;

        protected SkillPlayData m_playData;
        public SkillElementStatus Status = SkillElementStatus.ELEM_STATUS_INIT;
        
        public void Destroy()
        {
            if (Status == SkillElementStatus.ELEM_STATUS_RUN)
            {
                Stop();
            }

            OnDestroy();
        }
        
        /// <summary>
        /// 初始化接口
        /// </summary>
        public void Init(SkillPlayData playData, SkillAttrDamageData[] damageAttr)
        {
            m_handleGID = m_nextHandleGID++;
            CasterActor = playData.CasterActor;
            m_skillId = playData.skillId;
            m_playData = playData;
            m_damageAttr = damageAttr;

            Status = SkillElementStatus.ELEM_STATUS_INIT;
            OnInit();
        }

        /// <summary>
        /// 触发Element开始。
        /// </summary>
        /// <param name="playData">技能播放数据。</param>
        /// <param name="eventType">技能触发类型。</param>
        public void Start(SkillPlayData playData, SkillTriggerEvent eventType)
        {
            if (Status != SkillElementStatus.ELEM_STATUS_INIT)
            {
                Log.Error("invalid status skillId[{0}] element Type[{1}]", m_skillId, GetType().Name);
                return;
            }

            Status = SkillElementStatus.ELEM_STATUS_RUN;

            //如果是重复触发的机制，则不需要开始就触发。
            OnStart(playData, eventType);
        }
        
        /// <summary>
        /// 触发Element结束。
        /// </summary>
        public void Stop()
        {
            if (Status == SkillElementStatus.ELEM_STATUS_STOP)
            {
                return;
            }

            if (Status != SkillElementStatus.ELEM_STATUS_RUN)
            {
                Status = SkillElementStatus.ELEM_STATUS_STOP;
                return;
            }

            Status = SkillElementStatus.ELEM_STATUS_STOP;
            OnStop();
        }
        
        #region override function

        /// <summary>
        /// 检查配置是否正常
        /// </summary>
        /// <returns></returns>
        public virtual string CheckElementConfig(ref string errField)
        {
            return null;
        }

        /// <summary>
        /// 初始化一些数，在加入到技能列表的时候触发
        /// </summary>
        protected virtual void OnInit()
        {
        }

        /// <summary>
        /// 触发销毁。
        /// </summary>
        protected virtual void OnDestroy()
        {
        }

        /// <summary>
        /// 触发开始
        /// </summary>
        /// <param name="playData">触发开始的消息类型</param>
        /// <param name="eventType">触发开始的消息类型</param>
        protected virtual void OnStart(SkillPlayData playData, SkillTriggerEvent eventType)
        {
        }

        /// <summary>
        /// 触发结束。
        /// </summary>
        protected virtual void OnStop()
        {
        }

        /// <summary>
        /// 调试绘制。
        /// </summary>
        public virtual void OnDrawGizmos()
        {
        }
        #endregion
    }
}