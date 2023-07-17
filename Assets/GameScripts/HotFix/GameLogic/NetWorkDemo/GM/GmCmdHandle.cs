using System.Collections.Generic;
using TEngine;

namespace GameLogic
{
    public class GmCmdHandle
    {
        public void Init()
        {
            RegGmCmd("gc", Gc);
        }
        
        public void RegGmCmd(string cmd, HandleGM func)
        {
            ClientGm.Instance.RegGmCmd(cmd, func);
        }

        public void ShowText(string format, params object[] args)
        {
            Log.Debug(format, args);
            string retStr = string.Format(format, args);
            GameModule.UI.ShowUIAsync<GMPanel>(retStr);
        }

        //////////////////////////////
        /// GM实际处理代码
        //////////////////////////////

        private void Gc(List<string> paras)
        {
            GameModule.Resource.ForceUnloadUnusedAssets(true);
        }
    }
}