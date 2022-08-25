using UnityEngine;

namespace TEngine.Runtime
{
    public class HotUpdateMgr : UnitySingleton<HotUpdateMgr>
    {
        [SerializeField]
        private TextAsset m_BuildInfoTextAsset = null;

        private BuildInfo m_BuildInfo = null;

        public BuildInfo BuildInfo
        {
            get
            {
                if (m_BuildInfo == null)
                {
                    InitBuildInfo();
                }
                return m_BuildInfo;
            }
        }

        public void InitBuildInfo()
        {
            if (m_BuildInfoTextAsset == null || string.IsNullOrEmpty(m_BuildInfoTextAsset.text))
            {
                Log.Info("Build info can not be found or empty.");
                return;
            }

            m_BuildInfo = Utility.Json.ToObject<BuildInfo>(m_BuildInfoTextAsset.text);
            if (m_BuildInfo == null)
            {
                Log.Warning("Parse build info failure.");
                return;
            }
        }
    }
}