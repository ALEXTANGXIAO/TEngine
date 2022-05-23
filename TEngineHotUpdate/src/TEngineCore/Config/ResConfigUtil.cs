using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace TEngineCore
{
    public class ResConfigUtil 
    {
        private static StringBuilder m_strBuilder = new StringBuilder();
        private static readonly string m_split = "_";

        #region 读取接口
        public static List<T> ReadConfigListRes<T>(string fileName)
        {
            string resPath = string.Format("Config/{0}.json",fileName);
            TextAsset jsonStr = TResources.Load<TextAsset>(resPath);
            if (jsonStr == null)
            {
                TLogger.LogError("读取Json配置数据失败：{0}", fileName);
                return null;
            }

            List<T> list = new List<T>();
            var jsonData = JsonHelper.Instance.Deserialize<List<T>>(jsonStr.text);
            list = jsonData;
            return list;
        }

        public static Dictionary<string, T> ReadConfigRes<T>(string fileName)
        {
            string resPath = string.Format("Config/{0}.json", fileName);
            TextAsset jsonStr = TResources.Load<TextAsset>(resPath);
            if (jsonStr == null)
            {
                TLogger.LogError("读取Json配置数据失败：{0}", fileName);
                return null;
            }

            Dictionary<string, T> dic = new Dictionary<string, T>();
            var jsonData = JsonHelper.Instance.Deserialize<Dictionary<string, T>>(jsonStr.text);
            dic = jsonData;
            return dic;
        }

        public static Dictionary<int, T> ReadConfigResIntKey<T>(string fileName)
        {
            string resPath = string.Format("Config/{0}.json", fileName);
            TextAsset jsonStr = TResources.Load<TextAsset>(resPath);
            if (jsonStr == null)
            {
                TLogger.LogError("读取Json配置数据失败：{0}", fileName);
                return null;
            }

            Dictionary<int, T> dic = new Dictionary<int, T>();
            var jsonData = JsonHelper.Instance.Deserialize<Dictionary<int, T>>(jsonStr.text);
            dic = jsonData;
            return dic;
        }

        #endregion

        public static UInt64 Make64Key(uint key1, uint key2)
        {
            return (((UInt64)key1) << 32) | key2;
        }

        public static string MakeStringKey(uint key1, uint key2, uint key3)
        {
            m_strBuilder.Length = 0;
            m_strBuilder.Append(key1);
            m_strBuilder.Append(m_split);
            m_strBuilder.Append(key2);
            m_strBuilder.Append(m_split);
            m_strBuilder.Append(key3);
            return m_strBuilder.ToString();
        }

        public static string MakeStringKey(string key1, uint key2)
        {
            m_strBuilder.Length = 0;
            m_strBuilder.Append(key1);
            m_strBuilder.Append(m_split);
            m_strBuilder.Append(key2);
            return m_strBuilder.ToString();
        }

        public static string MakeStringKey(string key1, string key2)
        {
            m_strBuilder.Length = 0;
            m_strBuilder.Append(key1);
            m_strBuilder.Append(m_split);
            m_strBuilder.Append(key2);
            return m_strBuilder.ToString();
        }
    }
}
/*
 *
===》 example 《===
public class BufferMgr : Singleton<BufferMgr>
{
    private Dictionary<string, BuffConfig> m_dictBaseConfig = new Dictionary<string, BuffConfig>();

    public BufferMgr()
    {
        m_dictBaseConfig = ResConfigUtil.ReadConfigRes<BuffConfig>("BuffConfig");
    }

    public Dictionary<string, BuffConfig> GetBuffConfig()
    {
        return m_dictBaseConfig;
    }
}
 *
 */