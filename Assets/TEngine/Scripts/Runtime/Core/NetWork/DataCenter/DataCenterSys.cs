using System.Collections.Generic;

namespace TEngine.Runtime
{
    /// <summary>
    /// 数据中心系统
    /// </summary>
    public class DataCenterSys : BehaviourSingleton<DataCenterSys>
    {
        private List<IDataCenterModule> m_listModule = new List<IDataCenterModule>();

        public override void Awake()
        {
            InitModule();
            base.Awake();
        }

        public override void Update()
        {
            var listModule = m_listModule;
            for (int i = 0; i < listModule.Count; i++)
            {
                listModule[i].OnUpdate();
            }
            base.Update();
        }

        public override void Destroy()
        {
            base.Destroy();
        }

        private void InitModule()
        {
            //InitModule(LoginDataMgr.Instance);
        }

        public void InitModule(IDataCenterModule module)
        {
            if (!m_listModule.Contains(module))
            {
                module.Init();
                m_listModule.Add(module);
            }
        }
    }
}