using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace TEngine.Runtime
{
    /// <summary>
    /// 默认游戏配置辅助器。
    /// </summary>
    public class DefaultSettingHelper : SettingHelperBase
    {
        private const string SettingFileName = "TEngineSetting.dat";

        private string _filePath = null;
        private DefaultSetting _settings = null;

        /// <summary>
        /// 获取游戏配置项数量。
        /// </summary>
        public override int Count
        {
            get
            {
                return _settings != null ? _settings.Count : 0;
            }
        }

        /// <summary>
        /// 获取游戏配置存储文件路径。
        /// </summary>
        public string FilePath
        {
            get
            {
                return _filePath;
            }
        }

        /// <summary>
        /// 获取游戏配置。
        /// </summary>
        public DefaultSetting Setting
        {
            get
            {
                return _settings;
            }
        }
        /// <summary>
        /// 加载游戏配置。
        /// </summary>
        /// <returns>是否加载游戏配置成功。</returns>
        public override bool Load()
        {
            try
            {
                if (!File.Exists(_filePath))
                {
                    return true;
                }

                using (FileStream fileStream = new FileStream(_filePath, FileMode.Open, FileAccess.Read))
                {
                    Deserialize(fileStream);
                    return true;
                }
            }
            catch (Exception exception)
            {
                Log.Warning("Load settings failure with exception '{0}'.", exception);
                return false;
            }
        }

        /// <summary>
        /// 保存游戏配置。
        /// </summary>
        /// <returns>是否保存游戏配置成功。</returns>
        public override bool Save()
        {
            try
            {
                using (FileStream fileStream = new FileStream(_filePath, FileMode.Create, FileAccess.Write))
                {
                    return Serialize(fileStream, _settings);
                }
            }
            catch (Exception exception)
            {
                Log.Warning("Save settings failure with exception '{0}'.", exception);
                return false;
            }
        }

        /// <summary>
        /// 获取所有游戏配置项的名称。
        /// </summary>
        /// <returns>所有游戏配置项的名称。</returns>
        public override string[] GetAllSettingNames()
        {
            return _settings.GetAllSettingNames();
        }

        /// <summary>
        /// 获取所有游戏配置项的名称。
        /// </summary>
        /// <param name="results">所有游戏配置项的名称。</param>
        public override void GetAllSettingNames(List<string> results)
        {
            _settings.GetAllSettingNames(results);
        }

        /// <summary>
        /// 检查是否存在指定游戏配置项。
        /// </summary>
        /// <param name="settingName">要检查游戏配置项的名称。</param>
        /// <returns>指定的游戏配置项是否存在。</returns>
        public override bool HasSetting(string settingName)
        {
            return _settings.HasSetting(settingName);
        }

        /// <summary>
        /// 移除指定游戏配置项。
        /// </summary>
        /// <param name="settingName">要移除游戏配置项的名称。</param>
        /// <returns>是否移除指定游戏配置项成功。</returns>
        public override bool RemoveSetting(string settingName)
        {
            return _settings.RemoveSetting(settingName);
        }

        /// <summary>
        /// 清空所有游戏配置项。
        /// </summary>
        public override void RemoveAllSettings()
        {
            _settings.RemoveAllSettings();
        }

        /// <summary>
        /// 从指定游戏配置项中读取布尔值。
        /// </summary>
        /// <param name="settingName">要获取游戏配置项的名称。</param>
        /// <returns>读取的布尔值。</returns>
        public override bool GetBool(string settingName)
        {
            return _settings.GetBool(settingName);
        }

        /// <summary>
        /// 从指定游戏配置项中读取布尔值。
        /// </summary>
        /// <param name="settingName">要获取游戏配置项的名称。</param>
        /// <param name="defaultValue">当指定的游戏配置项不存在时，返回此默认值。</param>
        /// <returns>读取的布尔值。</returns>
        public override bool GetBool(string settingName, bool defaultValue)
        {
            return _settings.GetBool(settingName, defaultValue);
        }

        /// <summary>
        /// 向指定游戏配置项写入布尔值。
        /// </summary>
        /// <param name="settingName">要写入游戏配置项的名称。</param>
        /// <param name="value">要写入的布尔值。</param>
        public override void SetBool(string settingName, bool value)
        {
            _settings.SetBool(settingName, value);
        }

        /// <summary>
        /// 从指定游戏配置项中读取整数值。
        /// </summary>
        /// <param name="settingName">要获取游戏配置项的名称。</param>
        /// <returns>读取的整数值。</returns>
        public override int GetInt(string settingName)
        {
            return _settings.GetInt(settingName);
        }

        /// <summary>
        /// 从指定游戏配置项中读取整数值。
        /// </summary>
        /// <param name="settingName">要获取游戏配置项的名称。</param>
        /// <param name="defaultValue">当指定的游戏配置项不存在时，返回此默认值。</param>
        /// <returns>读取的整数值。</returns>
        public override int GetInt(string settingName, int defaultValue)
        {
            return _settings.GetInt(settingName, defaultValue);
        }

        /// <summary>
        /// 向指定游戏配置项写入整数值。
        /// </summary>
        /// <param name="settingName">要写入游戏配置项的名称。</param>
        /// <param name="value">要写入的整数值。</param>
        public override void SetInt(string settingName, int value)
        {
            _settings.SetInt(settingName, value);
        }

        /// <summary>
        /// 从指定游戏配置项中读取浮点数值。
        /// </summary>
        /// <param name="settingName">要获取游戏配置项的名称。</param>
        /// <returns>读取的浮点数值。</returns>
        public override float GetFloat(string settingName)
        {
            return _settings.GetFloat(settingName);
        }

        /// <summary>
        /// 从指定游戏配置项中读取浮点数值。
        /// </summary>
        /// <param name="settingName">要获取游戏配置项的名称。</param>
        /// <param name="defaultValue">当指定的游戏配置项不存在时，返回此默认值。</param>
        /// <returns>读取的浮点数值。</returns>
        public override float GetFloat(string settingName, float defaultValue)
        {
            return _settings.GetFloat(settingName, defaultValue);
        }

        /// <summary>
        /// 向指定游戏配置项写入浮点数值。
        /// </summary>
        /// <param name="settingName">要写入游戏配置项的名称。</param>
        /// <param name="value">要写入的浮点数值。</param>
        public override void SetFloat(string settingName, float value)
        {
            _settings.SetFloat(settingName, value);
        }

        /// <summary>
        /// 从指定游戏配置项中读取字符串值。
        /// </summary>
        /// <param name="settingName">要获取游戏配置项的名称。</param>
        /// <returns>读取的字符串值。</returns>
        public override string GetString(string settingName)
        {
            return _settings.GetString(settingName);
        }

        /// <summary>
        /// 从指定游戏配置项中读取字符串值。
        /// </summary>
        /// <param name="settingName">要获取游戏配置项的名称。</param>
        /// <param name="defaultValue">当指定的游戏配置项不存在时，返回此默认值。</param>
        /// <returns>读取的字符串值。</returns>
        public override string GetString(string settingName, string defaultValue)
        {
            return _settings.GetString(settingName, defaultValue);
        }

        /// <summary>
        /// 向指定游戏配置项写入字符串值。
        /// </summary>
        /// <param name="settingName">要写入游戏配置项的名称。</param>
        /// <param name="value">要写入的字符串值。</param>
        public override void SetString(string settingName, string value)
        {
            _settings.SetString(settingName, value);
        }

        /// <summary>
        /// 从指定游戏配置项中读取对象。
        /// </summary>
        /// <typeparam name="T">要读取对象的类型。</typeparam>
        /// <param name="settingName">要获取游戏配置项的名称。</param>
        /// <returns>读取的对象。</returns>
        public override T GetObject<T>(string settingName)
        {
            return Utility.Json.ToObject<T>(GetString(settingName));
        }

        /// <summary>
        /// 从指定游戏配置项中读取对象。
        /// </summary>
        /// <param name="objectType">要读取对象的类型。</param>
        /// <param name="settingName">要获取游戏配置项的名称。</param>
        /// <returns>读取的对象。</returns>
        public override object GetObject(Type objectType, string settingName)
        {
            return Utility.Json.ToObject(objectType, GetString(settingName));
        }

        /// <summary>
        /// 从指定游戏配置项中读取对象。
        /// </summary>
        /// <typeparam name="T">要读取对象的类型。</typeparam>
        /// <param name="settingName">要获取游戏配置项的名称。</param>
        /// <param name="defaultObj">当指定的游戏配置项不存在时，返回此默认对象。</param>
        /// <returns>读取的对象。</returns>
        public override T GetObject<T>(string settingName, T defaultObj)
        {
            string json = GetString(settingName, null);
            if (json == null)
            {
                return defaultObj;
            }

            return Utility.Json.ToObject<T>(json);
        }

        /// <summary>
        /// 从指定游戏配置项中读取对象。
        /// </summary>
        /// <param name="objectType">要读取对象的类型。</param>
        /// <param name="settingName">要获取游戏配置项的名称。</param>
        /// <param name="defaultObj">当指定的游戏配置项不存在时，返回此默认对象。</param>
        /// <returns>读取的对象。</returns>
        public override object GetObject(Type objectType, string settingName, object defaultObj)
        {
            string json = GetString(settingName, null);
            if (json == null)
            {
                return defaultObj;
            }

            return Utility.Json.ToObject(objectType, json);
        }

        /// <summary>
        /// 向指定游戏配置项写入对象。
        /// </summary>
        /// <typeparam name="T">要写入对象的类型。</typeparam>
        /// <param name="settingName">要写入游戏配置项的名称。</param>
        /// <param name="obj">要写入的对象。</param>
        public override void SetObject<T>(string settingName, T obj)
        {
            SetString(settingName, Utility.Json.ToJson(obj));
        }

        /// <summary>
        /// 向指定游戏配置项写入对象。
        /// </summary>
        /// <param name="settingName">要写入游戏配置项的名称。</param>
        /// <param name="obj">要写入的对象。</param>
        public override void SetObject(string settingName, object obj)
        {
            SetString(settingName, Utility.Json.ToJson(obj));
        }

        private void Awake()
        {
            _filePath = Utility.Path.GetRegularPath(Path.Combine(Application.persistentDataPath, SettingFileName));
            _settings = new DefaultSetting();
        }
        
        private static readonly byte[] Header = new byte[] { (byte)'T', (byte)'E', (byte)'G' };
        
        /// <summary>从指定流反序列化数据。</summary>
        /// <param name="stream">指定流。</param>
        /// <returns>反序列化的数据。</returns>
        private DefaultSetting Deserialize(Stream stream)
        {
            byte[] header = Header;
            byte num1 = (byte) stream.ReadByte();
            byte num2 = (byte) stream.ReadByte();
            byte num3 = (byte) stream.ReadByte();
            if ((int) num1 != (int) header[0] || (int) num2 != (int) header[1] || (int) num3 != (int) header[2])
                throw new Exception(Utility.Text.Format("Header is invalid, need '{0}{1}{2}', current '{3}{4}{5}'.", (char) header[0], (char) header[1], (char) header[2], (char) num1, (char) num2, (char) num3));
            _settings.Deserialize(stream);
            return _settings;
        }
        
        /// <summary>序列化数据到目标流中。</summary>
        /// <param name="stream">目标流。</param>
        /// <param name="data">要序列化的数据。</param>
        /// <returns>是否序列化数据成功。</returns>
        private bool Serialize(Stream stream, DefaultSetting data)
        {
            byte[] header = Header;
            stream.WriteByte(header[0]);
            stream.WriteByte(header[1]);
            stream.WriteByte(header[2]);
            _settings.Serialize(stream);
            return true;
        }
    }
}
