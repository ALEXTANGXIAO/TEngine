using System.Collections.Generic;
using TEngineCore.ListJson;
using UnityEngine;

namespace TEngineCore
{
    public abstract class BaseClientData
    {
        private string m_configName;

        public void Init(string configName)
        {
            m_configName = configName;
            Load();
        }

        public void Load()
        {
            string fullName = GetSaveUniqPrefix() + m_configName;
            var jsonString = PlayerPrefs.GetString(fullName);
            if (!string.IsNullOrEmpty(jsonString))
            {
                JsonData json = JsonHelper.Instance.Deserialize(jsonString);
                if (json != null)
                {
                    Deserialize(json);
                }
            }
        }

        public void Save()
        {
            string fullName = GetSaveUniqPrefix() + m_configName;
            JsonData jsonData = new JsonData();
            Serialize(jsonData);

            var jsonTex = JsonHelper.Instance.Serialize(jsonData);
            if (!string.IsNullOrEmpty(jsonTex))
            {
                PlayerPrefs.SetString(fullName, jsonTex);
                PlayerPrefs.Save();
            }
        }

        protected abstract void Serialize(JsonData json);

        protected abstract void Deserialize(JsonData json);

        private string GetSaveUniqPrefix()
        {
            string hashPath = UnityUtil.GetHashCodeByString(Application.dataPath).ToString();
            string uniqInstance = SystemInfo.deviceUniqueIdentifier;
            string uniqKey = hashPath + uniqInstance;
            return uniqKey;
        }
    }

    public class SystemSaveData: BaseClientData
    {
        public int[] settingParams;
        public uint test;
        public float m_cameraDistance;

        public SystemSaveData()
        {
            settingParams = new int[(int)SystemSaveType.Max];
            settingParams[(int) SystemSaveType.Lod] = 0;
        }

        public enum SystemSaveType
        {
            Lod,        // 同屏人数
            MusicOn,    // 打开音乐
            SoundOn,    // 打开音效
            Max,
        }

        protected override void Serialize(JsonData json)
        {
            if (json == null)
            {
                return;
            }
        }


        protected override void Deserialize(JsonData json)
        {
            if (json == null)
            {
                return;
            }
        }
    }

    public class ClientSaveData : TSingleton<ClientSaveData>
    {
        private Dictionary<string, BaseClientData> m_dictSaveData = new Dictionary<string, BaseClientData>();

        public T GetSaveData<T>() where T : BaseClientData, new()
        {
            string typeName = typeof(T).Name;
            BaseClientData ret;
            if (!m_dictSaveData.TryGetValue(typeName, out ret))
            {
                ret = new T();
                ret.Init(typeName);
                m_dictSaveData.Add(typeName, ret);
            }
            return (T)ret;
        }

        public void SaveAllData()
        {
            var enumerator = m_dictSaveData.GetEnumerator();
            while (enumerator.MoveNext())
            {
                enumerator.Current.Value.Save();
            }
        }

        public SystemSaveData CurrentSystemSaveData
        {
            get
            {
                return GetSaveData<SystemSaveData>();
            }
        }
    }
}