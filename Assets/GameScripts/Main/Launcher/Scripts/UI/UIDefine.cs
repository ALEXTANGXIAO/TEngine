using System.Collections.Generic;
using TEngine;

namespace GameMain
{
    public class UIDefine
    {
        public static readonly string UILoadUpdate = "UILoadUpdate";
        public static readonly string UILoadTip = "UILoadTip";

        /// <summary>
        /// 注册ui
        /// </summary>
        /// <param name="list"></param>
        public static void RegisterUI(Dictionary<string, string> list)
        {
            if (list == null)
            {
                Log.Error("[UIManager]list is null");
                return;
            }

            if (!list.ContainsKey(UILoadUpdate))
            {
                list.Add(UILoadUpdate, $"AssetLoad/{UILoadUpdate}");
            }

            if (!list.ContainsKey(UILoadTip))
            {
                list.Add(UILoadTip, $"AssetLoad/{UILoadTip}");
            }
        }
    }
}