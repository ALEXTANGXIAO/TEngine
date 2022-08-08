using System;
using System.Collections.Generic;

namespace TEngine.EntityModule
{
    public enum EcsDebugType
    {
        Entity,
        System,
        Component
    }

    [Serializable]
    public class EcsCmptDebugKeyInfo
    {
        public string m_name;
        public string val;
    }

    [Serializable]
    public class EcsCmptDebugInfo
    {
        public string m_name;
        public EcsDebugType Type = EcsDebugType.Component;
        public List<EcsCmptDebugKeyInfo> m_info = new List<EcsCmptDebugKeyInfo>();
    }

    public class EcsDebugBehaviour : UnityEngine.MonoBehaviour
    {
        public List<EcsCmptDebugInfo> m_EcsInfo = new List<EcsCmptDebugInfo>();
        public EcsCmptDebugInfo AddDebugCmpt(string cmptName)
        {
            var cmptInfo = m_EcsInfo.Find((item) => { return item.m_name == cmptName; });
            if (cmptInfo == null)
            {
                cmptInfo = new EcsCmptDebugInfo();
                cmptInfo.m_name = cmptName;
                m_EcsInfo.Add(cmptInfo); ;
            }

            return cmptInfo;
        }

        public void RmvDebugCmpt(string cmptName)
        {
            m_EcsInfo.RemoveAll((item) => { return item.m_name == cmptName; });
        }

        public void SetDebugInfo(string cmptName, string key, string val)
        {
            var cmptInfo = AddDebugCmpt(cmptName);
            var entry = cmptInfo.m_info.Find((t) => { return t.m_name == key; });
            if (entry == null)
            {
                entry = new EcsCmptDebugKeyInfo();
                entry.m_name = key;
                cmptInfo.m_info.Add(entry);
            }

            entry.val = val;
        }

        public void RemoveAllDebugInfo(string cmpName)
        {
            var cmpInfo = m_EcsInfo.Find((item) => { return item.m_name == cmpName; });
            if (cmpInfo != null)
            {
                cmpInfo.m_info.Clear();
            }
        }
    }
}