using System.Collections.Generic;

namespace TEngine.Runtime
{
    public interface IUpdateSystem
    {
        int GetPriority();
        
        void OnUpdate(float elapseSeconds, float realElapseSeconds);
    }
    
    /// <summary>
    /// 实现UnitySingleton的OnUpdate
    /// </summary>
    internal class UpdateInstance : BehaviourSingleton<UpdateInstance>
    {
        public List<IUpdateSystem> UpdateSystems;

        public UpdateInstance()
        {
            UpdateSystems = new List<IUpdateSystem>();
        }

        public void Retain(IUpdateSystem updateSystem)
        {
            if (UpdateSystems.Contains(updateSystem))
            {
                Log.Fatal($"Repeat Retain UnitySingleton{updateSystem}");
            }
            else
            {
                UpdateSystems.Add(updateSystem);

                UpdateSystems.Sort((x, y) => -x.GetPriority().CompareTo(y.GetPriority()));
            }
        }

        public void Release(IUpdateSystem updateSystem)
        {
            if (UpdateSystems.Contains(updateSystem))
            {
                UpdateSystems.Remove(updateSystem);
            }
        }

        public void ReleaseAll()
        {
            var count = UpdateSystems.Count;
            for (int i = count-1; i >= 0; i--)
            {
                Release(UpdateSystems[i]);
            }
        }

        public override void Update()
        {
            var count = UpdateSystems.Count;
            for (int i = 0; i < count; i++)
            {
                UpdateSystems[i].OnUpdate(UnityEngine.Time.deltaTime, UnityEngine.Time.unscaledDeltaTime);
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