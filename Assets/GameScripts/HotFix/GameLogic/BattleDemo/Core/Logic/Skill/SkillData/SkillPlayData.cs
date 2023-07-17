using System.Collections.Generic;
using GameConfig.Battle;
using TEngine;

namespace GameLogic.BattleDemo
{
    public enum SkillPlayStatus
    {
        /// <summary>
        /// 初始状态。
        /// </summary>
        PlayInit,

        /// <summary>
        /// 技能施法前摇。
        /// <remarks>播放动作阶段。</remarks>
        /// </summary>
        PlayingAim,

        /// <summary>
        /// 播放技能阶段，该阶段同时只能有一个技能播放。
        /// </summary>
        PlayingFront,

        /// <summary>
        /// 后台播放阶段，前台播放完后，可能还有一些元素要继续生效，这个时候转为后台播放阶段。
        /// 同时玩家可以释放新的技能。
        /// </summary>
        PlayingBack,

        /// <summary>
        /// 播放完毕，等待播放.
        /// </summary>
        PlayingToFree
    }
    
    /// <summary>
    /// 技能播放的数据。
    /// </summary>
    public class SkillPlayData:IMemory
    {
        /// <summary>
        /// 技能内存Id,代表该玩家当前的唯一技能Id。
        /// </summary>
        public uint skillGid = 0;

        /// <summary>
        /// 技能的配置Id。
        /// </summary>
        public uint skillId;

        /// <summary>
        /// 技能的配置。
        /// </summary>
        public SkillBaseConfig skillBaseConfig;
        
        /// <summary>
        /// 技能表现ID.
        /// </summary>
        public int skillDisplayId;

        /// <summary>
        /// 技能表现数据。
        /// </summary>
        public SkillDisplayData skillDisplayData;
        
        /// <summary>
        /// 是否已经创建过visual表现层。
        /// </summary>
        public bool HasVisualPlayData = false;
        
        /// <summary>
        /// 开始时间。
        /// </summary>
        public float startTime;

        /// <summary>
        /// 开始技能进入后台的时间。
        /// </summary>
        public float startBackTime;
        
        private SkillPlayStatus _status = SkillPlayStatus.PlayInit;
        
        /// <summary>
        /// 播放状态
        /// </summary>
        public SkillPlayStatus Status
        {
            set
            {
                if (_status != value)
                {
                    _status = value;
                    if (_status == SkillPlayStatus.PlayingBack)
                    {
                        startBackTime = GameTime.time;
                    }
                }
            }
            get => _status;
        }

        public bool IsFrontStatus => _status == SkillPlayStatus.PlayingAim || _status == SkillPlayStatus.PlayingFront;

        public bool IsRunningStatus => _status == SkillPlayStatus.PlayingFront || _status == SkillPlayStatus.PlayingBack;

        private EntityLogic _casterActor = null;
        private SkillCasterComponent _skillCaster = null;
        
        /// <summary>
        /// 获取技能施法者。
        /// </summary>
        public EntityLogic CasterActor
        {
            get => _casterActor;

            set
            {
                _casterActor = value;
                _skillCaster = _casterActor.SkillCaster;
            }
        }

        /// <summary>
        /// 获取施法者的运行时ID。
        /// </summary>
        public long CasterId
        {
            get
            {
                if (_casterActor != null)
                {
                    return _casterActor.RuntimeId;
                }

                return 0;
            }
        }
        
        /// <summary>
        /// 目标对象。
        /// </summary>
        public EntityLogic targetActor;

        /// <summary>
        /// 获取技能播放模块。
        /// </summary>
        internal SkillCasterComponent SkillCaster => _skillCaster;
        
        /// <summary>
        /// 处理动画元素。
        /// </summary>
        internal SkillAnimationHandle animHandle;

        /// <summary>
        /// 技能元素处理列表。
        /// </summary>
        internal List<SkillElementHandle> handleList = new List<SkillElementHandle>();

        public void Clear()
        {
            skillId = 0;
            skillGid = 0;
            skillDisplayId = 0;
            skillDisplayData = null;
            skillBaseConfig = null;
            
            Status = SkillPlayStatus.PlayInit;
            startTime = 0;
            startBackTime = 0;

            CasterActor = null;
            targetActor = null;
            DestroyAllElement();
        }
        
        private void DestroyAllElement()
        {
            //销毁所有的ElementHandle
            foreach (var elemHandle in handleList)
            {
                elemHandle?.Destroy();
            }
            handleList.Clear();
            animHandle = null;
        }
        
        /// <summary>
        /// 增加技能元素处理。
        /// </summary>
        /// <param name="handle">技能元素处理。</param>
        /// <returns>是否增加成功。</returns>
        internal bool AddElementHandle(SkillElementHandle handle)
        {
            string errField = null;
            string checkResult = handle.CheckElementConfig(ref errField);
            if (!string.IsNullOrEmpty(checkResult))
            {
                Log.Warning("skill Element config[{0}] error: {1}, RandomSkillLibraryId[{2}]", handle.GetType().ToString(), checkResult, skillId);
                return false;
            }
            handleList.Add(handle);
            return true;
        }
        
        /// <summary>
        /// 创建表现层技能对象。
        /// </summary>
        internal void CreateVisualObject()
        {
            if (HasVisualPlayData)
            {
                return;
            }
            HasVisualPlayData = true;
            //发送给visual事件
            //TODO
        }

        /// <summary>
        /// 销毁表现层技能对象。
        /// </summary>
        internal void DestroyVisualObject()
        {
            if (HasVisualPlayData && _casterActor != null)
            {
                HasVisualPlayData = false;
                //TODO
            }
        }
    }
}