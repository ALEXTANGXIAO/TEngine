using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TEngine.Net;
using UI;

namespace TEngine
{
    partial class GameEntry
    {
        void RegisterSystem()
        {
            TEngineEntry.Instance.AddLogicSys(DataCenterSys.Instance);
            TEngineEntry.Instance.AddLogicSys(BehaviourSingleSystem.Instance);
            TEngineEntry.Instance.AddLogicSys(UISys.Instance);
        }
    }
}
