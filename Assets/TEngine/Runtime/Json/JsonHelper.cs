using UnityEngine;
using LitJson;
using TEngine;

public class JsonHelper : Singleton<JsonHelper>
{
    public JsonHelper()
    {
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

    public string Serialize(Object jsonData)
    {
        return JsonMapper.ToJson(jsonData);
    }
}