using System;
using System.Collections;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace TEngine
{
    /// <summary>
    /// PlayerPrefs 数据管理类 统一管理数据的存储和读取，最下面有例子
    /// </summary>
    public class PlayerPrefsDataMgr:TSingleton<PlayerPrefsDataMgr>
    {
        private StringBuilder m_strBuilder = new StringBuilder();
        private static readonly string m_split = "_";
        /// <summary>
        /// 存储数据
        /// </summary>
        /// <param name="data">数据对象</param>
        /// <param name="keyName">数据对象的唯一key 自己控制</param>
        public void SaveData(object data, string keyName ="0")
        {
            Type dataType = data.GetType();

            FieldInfo[] infos = dataType.GetFields();

            m_strBuilder.Length = 0;
            FieldInfo info;
            for (int i = 0; i < infos.Length; i++)
            {
                info = infos[i];

                m_strBuilder.Append(string.Format("{0}_{1}_{2}_{3}", keyName, dataType.Name, info.FieldType.Name, info.Name));
                SaveValue(info.GetValue(data), m_strBuilder.ToString());
            }
            m_strBuilder.Length = 0;
            PlayerPrefs.Save();
        }

        private void SaveValue(object value, string keyName)
        {
            Type fieldType = value.GetType();

            if (fieldType == typeof(int))
            {
                int rValue = (int)value;
                rValue += 10;
                PlayerPrefs.SetInt(keyName, rValue);
            }
            else if (fieldType == typeof(float))
            {
                PlayerPrefs.SetFloat(keyName, (float)value);
            }
            else if (fieldType == typeof(string))
            {
                PlayerPrefs.SetString(keyName, value.ToString());
            }
            else if (fieldType == typeof(bool))
            {
                PlayerPrefs.SetInt(keyName, (bool)value ? 1 : 0);
            }

            else if (typeof(IList).IsAssignableFrom(fieldType))
            {
                IList list = value as IList;
                PlayerPrefs.SetInt(keyName, list.Count);
                int index = 0;
                foreach (object obj in list)
                {
                    SaveValue(obj, keyName + index);
                    ++index;
                }
            }
            else if (typeof(IDictionary).IsAssignableFrom(fieldType))
            {
                IDictionary dic = value as IDictionary;
                PlayerPrefs.SetInt(keyName, dic.Count);
                int index = 0;
                foreach (object key in dic.Keys)
                {
                    SaveValue(key, keyName + "_key_" + index);
                    SaveValue(dic[key], keyName + "_value_" + index);
                    ++index;
                }
            }
            else
            {
                SaveData(value, keyName);
            }
        }

        /// <summary>
        /// 读取数据
        /// </summary>
        /// <param name="type">想要读取数据的 数据类型Type</param>
        /// <param name="keyName">数据对象的唯一key 自己控制</param>
        /// <returns></returns>
        public object LoadData(Type type, string keyName = "0")
        {
            object data = Activator.CreateInstance(type);

            FieldInfo[] infos = type.GetFields();
            m_strBuilder.Length = 0;
            FieldInfo info;
            for (int i = 0; i < infos.Length; i++)
            {
                info = infos[i];

                m_strBuilder.Append(
                    string.Format("{0}_{1}_{2}_{3}", keyName, type.Name, info.FieldType.Name, info.Name));
                info.SetValue(data, LoadValue(info.FieldType, m_strBuilder.ToString()));
            }
            m_strBuilder.Length = 0;
            return data;
        }

        /// <summary>
        /// 得到单个数据的方法
        /// </summary>
        /// <param name="fieldType">字段类型 用于判断 用哪个api来读取</param>
        /// <param name="keyName">用于获取具体数据</param>
        /// <returns></returns>
        private object LoadValue(Type fieldType, string keyName)
        {
            if (fieldType == typeof(int))
            {
                //解密 减10
                return PlayerPrefs.GetInt(keyName, 0) - 10;
            }
            else if (fieldType == typeof(float))
            {
                return PlayerPrefs.GetFloat(keyName, 0);
            }
            else if (fieldType == typeof(string))
            {
                return PlayerPrefs.GetString(keyName, "");
            }
            else if (fieldType == typeof(bool))
            {
                return PlayerPrefs.GetInt(keyName, 0) == 1 ? true : false;
            }
            else if (typeof(IList).IsAssignableFrom(fieldType))
            {
                int count = PlayerPrefs.GetInt(keyName, 0);

                IList list = Activator.CreateInstance(fieldType) as IList;
                for (int i = 0; i < count; i++)
                {
                    list.Add(LoadValue(fieldType.GetGenericArguments()[0], keyName + i));
                }
                return list;
            }
            else if (typeof(IDictionary).IsAssignableFrom(fieldType))
            {
                int count = PlayerPrefs.GetInt(keyName, 0);
                IDictionary dictionary = Activator.CreateInstance(fieldType) as IDictionary;
                Type[] kvType = fieldType.GetGenericArguments();
                for (int i = 0; i < count; i++)
                {
                    dictionary.Add(LoadValue(kvType[0], keyName + "_key_" + i),
                             LoadValue(kvType[1], keyName + "_value_" + i));
                }
                return dictionary;
            }
            else
            {
                return LoadData(fieldType, keyName);
            }
        }
    }
}

// Example
// class Player
// {
//     public int level;
//     public string name;
//     public float exp;
//     public List<Player> FriendList;
// }

// Player p = new Player();
// p.level = 100;
// p.exp = 9123123f;
// p.name = "123";
// p.FriendList = new List<Player>();

// PlayerPrefsDataMgr.Instance.SaveData(p, "1002");

// var player = PlayerPrefsDataMgr.Instance.LoadData(typeof(Player), "1002");