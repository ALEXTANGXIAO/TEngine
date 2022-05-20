using System;
using System.Collections.Generic;
using UnityEditorInternal;


namespace Huatuo.Editor
{
    [Serializable]
    internal struct HuatuoVersion
    {
        public string HuatuoTag;
        public string Il2cppTag;
        public string Il2cppUrl;
        public string HuatuoUrl;
        public string InstallTime;
        public long Timestamp;
        public string CacheDir;
    }
    
    internal struct InstallVersion
    {
        public EFILE_NAME huatuoType;
        public string il2cppTag;
        public string huatuoTag;
    }


    [Serializable]
    public struct CommitItem
    {
        public string sha;
        public string url;
        public string message;
        public string html_url;
        public string comments_url;
        public string GetShaShort()
        {
            return sha.Substring(0, 6);
        }
    }

    public struct ItemSerial<T>
    {
        public List<T> items;
    }

    [Serializable]
    public struct TagItem
    {
        public string name;
        public string zipball_url;
        public string tarball_url;
        public string node_id;
        public CommitItem commit;
    }


    [Serializable]
    public struct RemoteConfig
    {
        public List<string> unity_version;
        public List<string> huatuo_version;
        public List<string> il2cpp_version;
        public List<string> il2cpp_min_version;
        public List<string> huatuo_deprecated_version;
        public List<string> il2cpp_deprecated_version;
        public List<string> il2cpp_recommend_version;
        public string huatuo_recommend_version;
        public string huatuo_min_version;
    }

    /// <summary>
    /// 这个类提供了Huatuo和IL2CPP相关的版本信息
    /// </summary>
    [Serializable]
    internal class HuatuoRemoteConfig
    {
        public string ver = "";
        public string hash = "";
        public List<string> unity_version;
        public List<string> huatuo_version;
        public List<string> il2cpp_version;
        public string il2cpp_recommend_version;
        public string huatuo_recommend_version;
        public string huatuo_min_version;
        
        public HuatuoRemoteConfig(RemoteConfig rc)
        {
            unity_version = rc.unity_version;
            huatuo_recommend_version = rc.huatuo_recommend_version;
            InitHuatuoVersion(rc);
            InitIl2CppVersion(rc);
            InitIl2CppRecommendVersion(rc);
        }
        
        private bool BigThanMinVersion(string version, string minVersion)
        {
            var a = version.Split('.');
            var min = minVersion.Split('.');
            for (int i = 0; i < a.Length && i < min.Length; i++)
            {
                var nA = Convert.ToInt32(a[i]);
                var nMin = Convert.ToInt32(min[i]);
                if (nMin > nA)
                {
                    return false;
                }
            }
            return true;
        }
        
        private void InitHuatuoVersion(RemoteConfig rc)
        {
            var ret = new List<string>();
            if (rc.huatuo_version != null)
            {
                foreach (var item in rc.huatuo_version)
                {
                    if (rc.huatuo_deprecated_version.Contains(item))
                    {
                        continue;
                    }
                    if (!BigThanMinVersion(item, rc.huatuo_min_version))
                    {
                        continue;
                    }
                    ret.Add(item);
                }
            }
            huatuo_version = ret;
        }
        
        private void InitIl2CppVersion(RemoteConfig rc)
        {
            var ret = new List<string>();
            if (rc.il2cpp_version != null)
            {
                foreach (string version in rc.il2cpp_version)
                {
                    if (version.StartsWith(HTEditorConfig.UnityVersionDigits))
                    {
                        ret.Add(version);
                    }
                }
            }
            il2cpp_version = ret;
        }
        
        private void InitIl2CppRecommendVersion(RemoteConfig rc)
        {
            foreach (var version in rc.il2cpp_recommend_version)
            {
                if (version.StartsWith(HTEditorConfig.UnityVersionDigits))
                {
                    il2cpp_recommend_version = version;
                    break;
                }
            }
        }

        /// <summary>
        /// 判断两个版本是否一致
        /// </summary>
        public bool Compare(HuatuoRemoteConfig other)
        {
            if (other == null)
            {
                return false;
            }
            if (ver == other.ver && hash == other.hash)
            {
                return true;
            }

            return HTEditorUtility.CompareVersions(ver, other.ver) >= 0;
        }
    }
}
