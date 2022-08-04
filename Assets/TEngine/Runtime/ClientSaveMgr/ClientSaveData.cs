using System.Text;
using Newtonsoft.Json;
using UnityEngine;

namespace TEngine
{
    public abstract class BaseClientData<T> where T : class
    {
        public T Value;

        private string m_configName;

        public void Init(string configName)
        {
            m_configName = configName;
            Load();
        }

        public void Load()
        {
            string fullName = ClientSaveData.GetSaveUniqPrefix() + m_configName;
            var jsonString = PlayerPrefs.GetString(fullName);
            if (!string.IsNullOrEmpty(jsonString))
            {
                Value = ClientSaveData.Deserialize<T>(jsonString);
            }
        }

        public void Save()
        {
            string fullName = ClientSaveData.GetSaveUniqPrefix() + m_configName;
            var jsonTex = ClientSaveData.Serialize<T>(Value);
            if (!string.IsNullOrEmpty(jsonTex))
            {
                PlayerPrefs.SetString(fullName, jsonTex);
                PlayerPrefs.Save();
            }
        }

        protected abstract string Serialize(T type);

        protected abstract T Deserialize(string value);
    }

    public class ClientSaveData
    {
        private StringBuilder _stringBuilder = new StringBuilder();

        public static string GetSaveUniqPrefix()
        {
            string hashPath = UnityUtil.GetHashCodeByString(Application.dataPath).ToString();
            string uniqInstance = SystemInfo.deviceUniqueIdentifier;
            string uniqKey = hashPath + uniqInstance;
            return uniqKey;
        }

        public static string Serialize<T>(T type) where T : class
        {
            var ret = JsonConvert.SerializeObject(type);

            return ret;
        }

        public static T Deserialize<T>(string json) where T : class
        {
            var ret = JsonConvert.DeserializeObject<T>(json);

            return ret;
        }

        public static T Load<T>(string keyName, int userId = 0) where T : class
        {
            T ret = default(T);

            string typeName = typeof(T).Name + keyName;

            string fullName = GetSaveUniqPrefix() + typeName + userId;

            var jsonString = PlayerPrefs.GetString(fullName);

            if (!string.IsNullOrEmpty(jsonString))
            {
                ret = Deserialize<T>(jsonString);
            }
            return ret;
        }

        public static bool Save<T>(string keyName, T type, int userId = 0) where T : class
        {
            var jsonTex = Serialize<T>(type);

            if (!string.IsNullOrEmpty(jsonTex))
            {
                TLogger.LogInfoSuccessd(jsonTex);

                return Save<T>(keyName, jsonTex, userId);
            }

            return false;
        }

        public static bool Save<T>(string keyName, string json, int userId = 0) where T : class
        {
            var ret = false;

            string typeName = typeof(T).Name + keyName;

            string fullName = GetSaveUniqPrefix() + typeName + userId;

            if (!string.IsNullOrEmpty(json))
            {
                PlayerPrefs.SetString(fullName, json);

                PlayerPrefs.Save();

                ret = true;
            }

            return ret;
        }
    }
}