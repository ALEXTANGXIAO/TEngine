using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEngineCore.Net
{
    /// <summary>
    /// 数据中心系统
    /// </summary>
    public class DataCenterSys : BaseLogicSys<DataCenterSys>
    {
        private List<IDataCenterModule> m_listModule = new List<IDataCenterModule>();

        public override bool OnInit()
        {
            RegCmdHandle();
            InitModule();
            return true;
        }

        private void RegCmdHandle()
        {
            
        }

        void InitModule()
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
