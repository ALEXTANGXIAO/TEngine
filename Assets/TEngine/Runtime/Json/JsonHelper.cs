using LitJson;
using System;

namespace TEngine
{
    public class JsonHelper : Singleton<JsonHelper>
    {
        public JsonHelper()
        {
        }

        public object Deserialize(Type type, string json)
        {
            return JsonMapper.ToObject(json,type);
        }
        public T Deserialize<T>(string json)
        {
            return JsonMapper.ToObject<T>(json);
        }

        public JsonData Deserialize(string json)
        {
            return JsonMapper.ToObject(json);
        }

        public string Serialize(JsonData jsonData)
        {
            return JsonMapper.ToJson(jsonData);
        }

        public string Serialize(object jsonData)
        {
            return JsonMapper.ToJson(jsonData);
        }
    }
}