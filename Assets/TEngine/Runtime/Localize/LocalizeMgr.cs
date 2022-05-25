using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace TEngine
{
    public enum Language
    {
        Chinese = 1,
        English = 2,
    }

    public struct LocalizationId
    {
        public int CurlocalizationId;
    }

    public class LocalizeConfigNode
    {
        public string Chinese;
        public string EngLish;
    }

    public class LocalizeMgr : TSingleton<LocalizeMgr>
    {
        private int _curLanguage;
        private Dictionary<int, string> _localDic = new Dictionary<int, string>();

        const string LocalizeIdPath = "Localization/LocalizationId.json";

        const string ConfigPath = "Localization/Localization.json";

        public void InitLocalize()
        {
            _localDic.Clear();
            string localizeIdJson = ResMgr.Instance.GetStringFromAsset(LocalizeIdPath);
            string localizeConfigJson = ResMgr.Instance.GetStringFromAsset(ConfigPath);
            if (localizeIdJson == null)
            {
                TLogger.LogError("当前国际化地区配置不存在，请检查配置");
                return;
            }

            if (localizeConfigJson == null)
            {
                TLogger.LogError("国际化配置表找不到Json文件，请检查配置");
                return;
            }

            LocalizationId id = JsonUtility.FromJson<LocalizationId>(localizeIdJson);

            _curLanguage = id.CurlocalizationId;

            SetLanguage(id.CurlocalizationId);

            Dictionary<int, LocalizeConfigNode> dic = DeserializeStringToDictionary<int, LocalizeConfigNode>(localizeConfigJson);
            foreach (var item in dic)
            {
                switch ((Language)_curLanguage)
                {
                    case Language.Chinese:
                        _localDic.Add(item.Key, item.Value.Chinese);
                        break;
                    case Language.English:
                        _localDic.Add(item.Key, item.Value.EngLish);
                        break;
                    default:
                        break;
                }
            }
        }

        public void SetLanguage(int type)
        {
            _curLanguage = type;
        }

        public string GetLocalizeStr(int localizeKey)
        {
            if (localizeKey <= 0)
            {
                TLogger.LogError($"国际化表内未配置key为： {localizeKey}，请检查配置");
                return null;
            }
            if (_localDic[localizeKey] == null || _localDic[localizeKey] == "")
            {
                TLogger.LogError($"当前语言为： {(Language)_curLanguage},国际化表内未配置key为： {localizeKey}   的value，请检查配置");
                return null;
            }
            return _localDic[localizeKey];
        }


        public int GetLocalId(string _value)
        {

            foreach (var item in _localDic)
            {
                if (item.Value == _value)
                {
                    return item.Key;
                }
            }
            return 0;
        }

        public int GetCurLanguage()
        {
            return (int)_curLanguage;
        }

        private static Dictionary<TKey, TValue> DeserializeStringToDictionary<TKey, TValue>(string jsonStr)
        {
            if (string.IsNullOrEmpty(jsonStr))
            {
                return new Dictionary<TKey, TValue>();
            }
            Dictionary<TKey, TValue> jsonDict = JsonConvert.DeserializeObject<Dictionary<TKey, TValue>>(jsonStr);

            return jsonDict;
        }
    }
}
