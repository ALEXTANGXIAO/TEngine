using System;
using System.Collections.Generic;
using UnityEngine;

namespace TEngine.Runtime.Actor
{
    [Serializable]
    public class ActorComponentDebugKeyInfo
    {
        public string name;
        public string val;
    }

    [Serializable]
    public class ActorComponentDebugInfo
    {
        public string name;
        public List<ActorComponentDebugKeyInfo> infos = new List<ActorComponentDebugKeyInfo>();
    }

    /// <summary>
    /// 用来调试查看actor信息
    /// </summary>
    public class ActorDebugerBehaviour : MonoBehaviour
    {
        public List<ActorComponentDebugInfo> componentInfos = new List<ActorComponentDebugInfo>();

        public event Action OnGizmosSelect;
        public event Action OnGizmos;

        public ActorComponentDebugInfo AddDebugComponent(string componentName)
        {
            var componentInfo = componentInfos.Find(item => item.name == componentName);
            if (componentInfo == null)
            {
                componentInfo = new ActorComponentDebugInfo();
                componentInfo.name = componentName;
                componentInfos.Add(componentInfo); ;
            }

            return componentInfo;
        }

        public void RmvDebugComponent(string componentName)
        {
            componentInfos.RemoveAll((item) => { return item.name == componentName; });
        }

        public void SetDebugInfo(string componentName, string key, string val)
        {
            var componentInfo = AddDebugComponent(componentName);
            var entry = componentInfo.infos.Find(t => t.name == key);
            if (entry == null)
            {
                entry = new ActorComponentDebugKeyInfo();
                entry.name = key;
                componentInfo.infos.Add(entry);
            }

            entry.val = val;
        }

        public void RemoveAllDebugInfo(string cmpName)
        {
            var cmpInfo = componentInfos.Find((item) => item.name == cmpName);
            if (cmpInfo != null)
            {
                cmpInfo.infos.Clear();
            }
        }

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            if (OnGizmosSelect!=null)
            {
                OnGizmosSelect();
            }
        }

        void OnDrawGizmos()
        {
            if (OnGizmos != null)
            {
                OnGizmos();
            }
        }
#endif
    }
}
