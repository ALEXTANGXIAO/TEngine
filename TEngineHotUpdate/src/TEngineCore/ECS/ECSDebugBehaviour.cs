using System;
using System.Collections.Generic;

namespace TEngineCore
{
    public enum ECSDebugType
    {
        Entity,
        System,
        Component
    }

    [Serializable]
    public class ECSCmptDebugKeyInfo
    {
        public string m_name;
        public string val;
    }

    [Serializable]
    public class ECSCmptDebugInfo
    {
        public string m_name;
        public ECSDebugType Type = ECSDebugType.Component;
        public List<ECSCmptDebugKeyInfo> m_info = new List<ECSCmptDebugKeyInfo>();
    }

    public class ECSDebugBehaviour : UnityEngine.MonoBehaviour
    {
        public List<ECSCmptDebugInfo> m_ECSInfo = new List<ECSCmptDebugInfo>();
        public ECSCmptDebugInfo AddDebugCmpt(string cmptName)
        {
            var cmptInfo = m_ECSInfo.Find((item) => { return item.m_name == cmptName; });
            if (cmptInfo == null)
            {
                cmptInfo = new ECSCmptDebugInfo();
                cmptInfo.m_name = cmptName;
                m_ECSInfo.Add(cmptInfo); ;
            }

            return cmptInfo;
        }

        public void RmvDebugCmpt(string cmptName)
        {
            m_ECSInfo.RemoveAll((item) => { return item.m_name == cmptName; });
        }

        public void SetDebugInfo(string cmptName, string key, string val)
        {
            var cmptInfo = AddDebugCmpt(cmptName);
            var entry = cmptInfo.m_info.Find((t) => { return t.m_name == key; });
            if (entry == null)
            {
                entry = new ECSCmptDebugKeyInfo();
                entry.m_name = key;
                cmptInfo.m_info.Add(entry);
            }

            entry.val = val;
        }

        public void RemoveAllDebugInfo(string cmpName)
        {
            var cmpInfo = m_ECSInfo.Find((item) => { return item.m_name == cmpName; });
            if (cmpInfo != null)
            {
                cmpInfo.m_info.Clear();
            }
        }
    }
}