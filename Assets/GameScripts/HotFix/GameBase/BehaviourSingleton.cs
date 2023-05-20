﻿using System;
using System.Collections.Generic;

namespace TEngine
{
    /// <summary>
    /// 通过LogicSys来驱动且具备Unity完整生命周期的单例（不继承MonoBehaviour）。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class BehaviourSingleton<T> : BaseBehaviourSingleton where T : BaseBehaviourSingleton, new()
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (null == _instance)
                {
                    _instance = new T();
                    Log.Assert(_instance != null);
                    _instance.Awake();
                    RegSingleton(_instance);
                }

                return _instance;
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

        /// <summary>
        /// 帧更新。
        /// <remarks>需要UpdateAttribute。</remarks>
        /// </summary>
        public virtual void Update()
        {
        }

        /// <summary>
        /// 后帧更新。
        /// <remarks>需要LateUpdateAttribute。</remarks>
        /// </summary>
        public virtual void LateUpdate()
        {
        }
        
        /// <summary>
        /// 物理帧更新。
        /// <remarks>需要FixedUpdateAttribute。</remarks>
        /// </summary>
        public virtual void FixedUpdate()
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

        public virtual void OnDrawGizmos()
        {
        }
    }

    public class BehaviourSingleSystem : BaseLogicSys<BehaviourSingleSystem>
    {
        private readonly List<BaseBehaviourSingleton> _listInst = new List<BaseBehaviourSingleton>();
        private readonly List<BaseBehaviourSingleton> _listStart = new List<BaseBehaviourSingleton>();
        private readonly List<BaseBehaviourSingleton> _listUpdate = new List<BaseBehaviourSingleton>();
        private readonly List<BaseBehaviourSingleton> _listLateUpdate = new List<BaseBehaviourSingleton>();
        private readonly List<BaseBehaviourSingleton> _listFixedUpdate = new List<BaseBehaviourSingleton>();

        public void RegSingleton(BaseBehaviourSingleton inst)
        {
            Log.Assert(!_listInst.Contains(inst));
            _listInst.Add(inst);
            _listStart.Add(inst);
            if (HadAttribute<UpdateAttribute>(inst.GetType()))
            {
                _listUpdate.Add(inst);
            }
            if (HadAttribute<LateUpdateAttribute>(inst.GetType()))
            {
                _listLateUpdate.Add(inst);
            }
            if (HadAttribute<FixedUpdateAttribute>(inst.GetType()))
            {
                _listFixedUpdate.Add(inst);
            }
        }

        public void UnRegSingleton(BaseBehaviourSingleton inst)
        {
            if (inst == null)
            {
                Log.Error($"BaseBehaviourSingleton Is Null");
                return;
            }

            Log.Assert(_listInst.Contains(inst));
            if (_listInst.Contains(inst))
            {
                _listInst.Remove(inst);
            }

            if (_listStart.Contains(inst))
            {
                _listStart.Remove(inst);
            }

            if (_listUpdate.Contains(inst))
            {
                _listUpdate.Remove(inst);
            }

            if (_listLateUpdate.Contains(inst))
            {
                _listLateUpdate.Remove(inst);
            }

            inst.Destroy();
            inst = null;
        }

        public override void OnUpdate()
        {
            var listStart = _listStart;
            var listToUpdate = _listUpdate;
            var listToLateUpdate = _listLateUpdate;
            if (listStart.Count > 0)
            {
                for (int i = 0; i < listStart.Count; i++)
                {
                    var inst = listStart[i];
                    Log.Assert(!inst.IsStart);

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
            var listLateUpdate = _listLateUpdate;
            var listLateUpdateCnt = listLateUpdate.Count;
            for (int i = 0; i < listLateUpdateCnt; i++)
            {
                var inst = listLateUpdate[i];

                TProfiler.BeginFirstSample(inst.GetType().FullName);
                inst.LateUpdate();
                TProfiler.EndFirstSample();
            }
        }
        
        public override void OnFixedUpdate()
        {
            var listFixedUpdate = _listFixedUpdate;
            var listFixedUpdateCnt = listFixedUpdate.Count;
            for (int i = 0; i < listFixedUpdateCnt; i++)
            {
                var inst = listFixedUpdate[i];

                TProfiler.BeginFirstSample(inst.GetType().FullName);
                inst.FixedUpdate();
                TProfiler.EndFirstSample();
            }
        }

        public override void OnDestroy()
        {
            for (int i = 0; i < _listInst.Count; i++)
            {
                var inst = _listInst[i];
                inst.Destroy();
            }
        }

        public override void OnApplicationPause(bool pause)
        {
            for (int i = 0; i < _listInst.Count; i++)
            {
                var inst = _listInst[i];
                if (pause)
                {
                    inst.OnPause();
                }
                else
                {
                    inst.OnResume();
                }
            }
        }

        public override void OnDrawGizmos()
        {
            for (int i = 0; i < _listInst.Count; i++)
            {
                var inst = _listInst[i];
                inst.OnDrawGizmos();
            }
        }
        
        private bool HadAttribute<T>(Type type) where T:Attribute
        {
            T attribute = Attribute.GetCustomAttribute(type, typeof(T)) as T;

            return attribute != null;
        }
    }
}