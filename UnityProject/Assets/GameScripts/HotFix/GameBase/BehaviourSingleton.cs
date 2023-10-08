using System;
using System.Collections.Generic;

namespace TEngine
{
    /// <summary>
    /// 通过LogicSys来驱动且具备Unity完整生命周期的单例（不继承MonoBehaviour）。
    /// <remarks>Update、FixUpdate以及LateUpdate这些敏感帧更新需要加上对应的Attribute以最优化性能。</remarks>
    /// </summary>
    /// <typeparam name="T">完整生命周期的类型。</typeparam>
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

    #region Attribute

    /// <summary>
    /// 帧更新属性。
    /// <remarks>适用于BehaviourSingleton。</remarks>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class UpdateAttribute : Attribute
    {
    }

    /// <summary>
    /// 物理帧更新属性。
    /// <remarks>适用于BehaviourSingleton。</remarks>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class FixedUpdateAttribute : Attribute
    {
    }

    /// <summary>
    /// 后帧更新属性。
    /// <remarks>适用于BehaviourSingleton。</remarks>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class LateUpdateAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class RoleLoginAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class RoleLogoutAttribute : Attribute
    {
    }

    #endregion

    /// <summary>
    /// 基础Behaviour单例。
    /// <remarks>（抽象类）</remarks>
    /// </summary>
    public abstract class BaseBehaviourSingleton
    {
        /// <summary>
        /// 是否已经Start。
        /// </summary>
        public bool IsStart = false;

        public virtual void Awake()
        {
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

    /// <summary>
    /// 通过LogicSys来驱动且具备Unity完整生命周期的驱动系统（不继承MonoBehaviour）。
    /// </summary>
    public sealed class BehaviourSingleSystem : BaseLogicSys<BehaviourSingleSystem>
    {
        private readonly List<BaseBehaviourSingleton> _listInst = new List<BaseBehaviourSingleton>();
        private readonly List<BaseBehaviourSingleton> _listStart = new List<BaseBehaviourSingleton>();
        private readonly List<BaseBehaviourSingleton> _listUpdate = new List<BaseBehaviourSingleton>();
        private readonly List<BaseBehaviourSingleton> _listLateUpdate = new List<BaseBehaviourSingleton>();
        private readonly List<BaseBehaviourSingleton> _listFixedUpdate = new List<BaseBehaviourSingleton>();

        /// <summary>
        /// 注册单例。
        /// <remarks>调用Instance时自动调用。</remarks>
        /// </summary>
        /// <param name="inst">单例实例。</param>
        internal void RegSingleton(BaseBehaviourSingleton inst)
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
            int count = listStart.Count;
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    var inst = listStart[i];
                    Log.Assert(!inst.IsStart);

                    inst.IsStart = true;
                    inst.Start();
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
            int count = _listInst.Count;
            for (int i = 0; i < count; i++)
            {
                var inst = _listInst[i];
                inst.Destroy();
            }
        }

        public override void OnApplicationPause(bool pause)
        {
            int count = _listInst.Count;
            for (int i = 0; i < count; i++)
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
            int count = _listInst.Count;
            for (int i = 0; i < count; i++)
            {
                var inst = _listInst[i];
                inst.OnDrawGizmos();
            }
        }

        private bool HadAttribute<T>(Type type) where T : Attribute
        {
            T attribute = Attribute.GetCustomAttribute(type, typeof(T)) as T;
            return attribute != null;
        }
    }
}