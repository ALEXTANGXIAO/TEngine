using System;
using Newtonsoft.Json;
#pragma warning disable CS8603

namespace TEngine.Core
{
    public static class JsonHelper
    {
        public static string ToJson<T>(this T t)
        {
            return JsonConvert.SerializeObject(t);
        }
        
        public static object Deserialize(this string json, Type type, bool reflection = true)
        {
            return JsonConvert.DeserializeObject(json, type);
        }
        
        public static T Deserialize<T>(this string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static T Clone<T>(T t)
        {
            return t.ToJson().Deserialize<T>();
        }
    }
}