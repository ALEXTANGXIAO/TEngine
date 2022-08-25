using System;
using Newtonsoft.Json;

namespace TEngine.Runtime
{
    public static partial class Utility
    {
        /// <summary>
        /// JSON 相关的实用函数。
        /// </summary>
        public static partial class Json
        {
            /// <summary>
            /// 序列化
            /// </summary>
            /// <param name="obj"></param>
            /// <returns></returns>
            public static string Serialize(object obj)
            {
                return ToJson(obj);
            }

            /// <summary>
            /// 反序列化
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="json"></param>
            /// <returns></returns>
            public static T DeSerialize<T>(string json)
            {
                return ToObject<T>(json);
            }

            /// <summary>
            /// 将对象序列化为 JSON 字符串。
            /// </summary>
            /// <param name="obj">要序列化的对象。</param>
            /// <returns>序列化后的 JSON 字符串。</returns>
            public static string ToJson(object obj)
            {
                try
                {
                    return JsonConvert.SerializeObject(obj);
                }
                catch (Exception exception)
                {
                    throw new Exception(string.Format("Can not convert to JSON with exception '{0}'.", exception), exception);
                }
            }

            /// <summary>
            /// 将 JSON 字符串反序列化为对象。
            /// </summary>
            /// <typeparam name="T">对象类型。</typeparam>
            /// <param name="json">要反序列化的 JSON 字符串。</param>
            /// <returns>反序列化后的对象。</returns>
            public static T ToObject<T>(string json)
            {
                try
                {
                    return JsonConvert.DeserializeObject<T>(json);
                }
                catch (Exception exception)
                {
                    throw new Exception(string.Format("Can not convert to object with exception '{0}'.", exception), exception);
                }
            }

            /// <summary>
            /// 将 JSON 字符串反序列化为对象。
            /// </summary>
            /// <param name="objectType">对象类型。</param>
            /// <param name="json">要反序列化的 JSON 字符串。</param>
            /// <returns>反序列化后的对象。</returns>
            public static object ToObject(Type objectType, string json)
            {
                if (objectType == null)
                {
                    throw new Exception("Object type is invalid.");
                }

                try
                {
                    return JsonConvert.DeserializeObject(json,objectType);
                }
                catch (Exception exception)
                {
                    throw new Exception(string.Format("Can not convert to object with exception '{0}'.", exception), exception);
                }
            }
        }
    }
}
