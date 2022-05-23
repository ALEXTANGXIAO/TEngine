using System.IO;
using JetBrains.Annotations;

namespace TEngineCore.Net
{
    public class ProtoUtils
    {
        /// <summary>
        /// 序列化 MainPack -> byte[]
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="mainPack"></param>
        /// <returns></returns>
        [CanBeNull]
        public static byte[] Serialize<T>(T mainPack) where T : class
        {
            try
            {
                using (var stream = new System.IO.MemoryStream())
                {
                    ProtoBuf.Serializer.Serialize(stream, mainPack);
                    return stream.ToArray();
                }
            }
            catch (IOException ex)
            {
                TLogger.LogError($"[Serialize] Error：{ex.Message}, {ex.Data["StackTrace"]}");
                return null;
            }
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="buffer"></param>
        /// <returns></returns>
        [CanBeNull]
        public static T DeSerialize<T>(byte[] buffer) where T : class
        {
            try
            {
                return ProtoBuf.Serializer.Deserialize(typeof(T), new System.IO.MemoryStream(buffer)) as T;
            }
            catch (IOException ex)
            {
                TLogger.LogError(($"[DeSerialize] 错误：{ex.Message}, {ex.Data["StackTrace"]}"));
                return null;
            }
        }
    }
}
