using System.Collections.Generic;

namespace TEngine
{
    /// <summary>
    /// 通过LogicSys来驱动且具备Unity完整生命周期的单例（不继承MonoBehaviour）
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BehaviourSingleton<T> : BaseBehaviourSingleton where T : BaseBehaviourSingleton, new()
    {
        private static T sInstance;
        public static T Instance
        {
            get
            {
                if (null == sInstance)
                {
                    sInstance = new T();
                    TLogger.LogAssert(sInstance != null);
                    sInstance.Awake();
                    RegSingleton(sInstance);
                }

                return sInstance;
            }
        }

        private static void RegSingleton(BaseBehaviourSingleton inst)
        {
            BehaviourSingleSystem.Instance.RegSingleton(inst);
        }
    }

    public class BaseBehaviourSingleton
    {
        public bool IsStart = false;

        public virtual void Active()
        {
        }

        public virtual void Awake()
        {
        }

        public virtual bool IsHaveLateUpdate()
        {
            return false;
        }

        public virtual void Start()
        {
        }

        public virtual void Update()
        {
        }

        public virtual void LateUpdate()
        {
        }

        public virtual void Destroy()
        {
        }

        public virtual void OnPause()
        {
        }

        public virtual void OnResume()
        {
        }
    }

    public class BehaviourSingleSystem : BaseLogicSys<BehaviourSingleSystem>
    {
        List<BaseBehaviourSingleton> m_listInst = new List<BaseBehaviourSingleton>();
        List<BaseBehaviourSingleton> m_listStart = new List<BaseBehaviourSingleton>();
        List<BaseBehaviourSingleton> m_listUpdate = new List<BaseBehaviourSingleton>();
        List<BaseBehaviourSingleton> m_listLateUpdate = new List<BaseBehaviourSingleton>();

        public void RegSingleton(BaseBehaviourSingleton inst)
        {
            TLogger.LogAssert(!m_listInst.Contains(inst));
            m_listInst.Add(inst);
            m_listStart.Add(inst);
        }

        public void UnRegSingleton(BaseBehaviourSingleton inst)
        {
            if (inst == null)
            {
                TLogger.LogError($"BaseBehaviourSingleton Is Null");
                return;
            }
            TLogger.LogAssert(m_listInst.Contains(inst));
            if (m_listInst.Contains(inst))
            {
                m_listInst.Remove(inst);
            }
            if (m_listStart.Contains(inst))
            {
                m_listStart.Remove(inst);
            }
            if (m_listUpdate.Contains(inst))
            {
                m_listUpdate.Remove(inst);
            }
            if (m_listLateUpdate.Contains(inst))
            {
                m_listLateUpdate.Remove(inst);
            }
            inst.Destroy();
            inst = null;
        }

        public override void OnUpdate()
        {
            var listStart = m_listStart;
            var listToUpdate = m_listUpdate;
            var listToLateUpdate = m_listLateUpdate;
            if (listStart.Count > 0)
            {
                for (int i = 0; i < listStart.Count; i++)
                {
                    var inst = listStart[i];
                    TLogger.LogAssert(!inst.IsStart);

                    inst.IsStart = true;
                    inst.Start();
                    listToUpdate.Add(inst);

                    if (inst.IsHaveLateUpdate())
                    {
                        listToLateUpdate.Add(inst);
                    }
                }

                listStart.Clear();
            }

            var listUpdateCnt = listToUpdate.Count;
            for (int i = 0; i < listUpdateCnt; i++)
            {
                var inst = listToUpdate[i];

                TProfiler.BeginFirstSample(inst.GetType().FullName);
                inst.Update();
                TProfiler.EndFirstSample();
            }
        }

        public override void OnLateUpdate()
        {
            var listLateUpdate = m_listLateUpdate;
            var listLateUpdateCnt = listLateUpdate.Count;
            for (int i = 0; i < listLateUpdateCnt; i++)
            {
                var inst = listLateUpdate[i];

                TProfiler.BeginFirstSample(inst.GetType().FullName);
                inst.LateUpdate();
                TProfiler.EndFirstSample();
            }
        }

        public override void OnDestroy()
        {
            for (int i = 0; i < m_listInst.Count; i++)
            {
                var inst = m_listInst[i];
                inst.Destroy();
            }
        }

        public override void OnPause()
        {
            for (int i = 0; i < m_listInst.Count; i++)
            {
                var inst = m_listInst[i];
                inst.OnPause();
            }
        }

        public override void OnResume()
        {
            for (int i = 0; i < m_listInst.Count; i++)
            {
                var inst = m_listInst[i];
                inst.OnResume();
            }
        }
    }
}
