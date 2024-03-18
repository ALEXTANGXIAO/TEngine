using System.Collections.Generic;
using UnityEngine;
using TEngine;

namespace GameMain
{
    public class UIDefine
    {
        public static readonly string UILoadUpdate = "UILoadUpdate";
        public static readonly string UILoadTip = "UILoadTip";
    }

    public static class UILoadMgr
    {
        private static readonly Dictionary<string, System.Type> _uiMap = new Dictionary<string, System.Type>();

        private static bool _isInit = false;

        /// <summary>
        /// 初始化。
        /// </summary>
        public static void Initialize()
        {
            if (_isInit)
            {
                return;
            }

            _uiMap.Add(UIDefine.UILoadUpdate, typeof(UILoadUpdate));
            _uiMap.Add(UIDefine.UILoadTip, typeof(UILoadTip));
            GameModule.UI.ShowUI<UILoadUpdate>();
            _isInit = true;
        }

        /// <summary>
        /// show ui
        /// </summary>
        /// <param name="uiInfo">对应的ui</param>
        /// <param name="param">参数</param>
        public static void Show(string uiInfo, object param = null)
        {
            if (string.IsNullOrEmpty(uiInfo))
                return;

            if (!_uiMap.ContainsKey(uiInfo))
            {
                Log.Error($"ui not exist: {uiInfo}");
                return;
            }

            GameModule.UI.ShowUI(_uiMap[uiInfo], param);
        }

        /// <summary>
        /// 隐藏ui管理器。
        /// </summary>
        public static void HideAll()
        {
            GameModule.UI.CloseUI<UILoadTip>();
            GameModule.UI.CloseUI<UILoadUpdate>();
        }
    }
}