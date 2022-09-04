using System.Collections.Generic;

namespace TEngine.Runtime
{
    /// <summary>
    /// 实现UnitySingleton的OnUpdate
    /// </summary>
    internal class UpdateInstance : BehaviourSingleton<UpdateInstance>
    {
        public List<IUnitySingleton> UnitySingletons;

        public UpdateInstance()
        {
            UnitySingletons = new List<IUnitySingleton>();
        }

        public void Retain(IUnitySingleton unitySingleton)
        {
            if (UnitySingletons.Contains(unitySingleton))
            {
                Log.Fatal($"Repeat Retain UnitySingleton{unitySingleton}");
            }
            else
            {
                UnitySingletons.Add(unitySingleton);

                UnitySingletons.Sort((x, y) => -x.GetPriority().CompareTo(y.GetPriority()));
            }
        }

        public void Release(IUnitySingleton unitySingleton)
        {
            if (UnitySingletons.Contains(unitySingleton))
            {
                UnitySingletons.Remove(unitySingleton);
            }
        }

        public void ReleaseAll()
        {
            var count = UnitySingletons.Count;
            for (int i = 0; i < count; i++)
            {
                Release(UnitySingletons[i]);
            }
        }

        public override void Update()
        {
            var count = UnitySingletons.Count;
            for (int i = 0; i < count; i++)
            {
                UnitySingletons[i].OnUpdate(UnityEngine.Time.deltaTime, UnityEngine.Time.unscaledDeltaTime);
            }
        }

        public override void LateUpdate()
        {
            base.LateUpdate();
        }

        public override void Destroy()
        {
            base.Destroy();
        }
    }
}