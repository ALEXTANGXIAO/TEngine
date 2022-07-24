using System.Collections.Generic;

namespace TEngine.UI
{
    public class UIDefine
    {
        public static readonly string UILoadUpdate = "UILoadUpdate";
        public static readonly string UILoadTip = "UILoadTip";

        /// <summary>
        /// 注册ui
        /// </summary>
        /// <param name="list"></param>
        public static void RegisitUI(Dictionary<string, string> list)
        {
            if (list == null)
            {
                TLogger.LogError("[UIManager]list is null");
                return;
            }

            if (!list.ContainsKey(UILoadUpdate))
            {
                list.Add(UILoadUpdate, $"{FileSystem.AssetFolder}/{UILoadUpdate}");
            }

            if (!list.ContainsKey(UILoadTip))
            {
                list.Add(UILoadTip, $"{FileSystem.AssetFolder}/{UILoadTip}");
            }
        }
    }
}