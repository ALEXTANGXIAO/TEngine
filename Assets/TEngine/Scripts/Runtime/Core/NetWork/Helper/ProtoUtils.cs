using System.Diagnostics;
using System.IO;
using System.Text;

namespace TEngine.Runtime
{
    public class ProtoUtils
    {
        /// <summary>
        /// 序列化 MainPack -> byte[]
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="mainPack"></param>
        /// <returns></returns>
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
                Log.Error($"[Serialize] Error：{ex.Message}, {ex.Data["StackTrace"]}");
                return null;
            }
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static T DeSerialize<T>(byte[] buffer) where T : class
        {
            try
            {
                return ProtoBuf.Serializer.Deserialize(typeof(T), new System.IO.MemoryStream(buffer)) as T;
            }
            catch (IOException ex)
            {
                Log.Error(($"[DeSerialize] Error：{ex.Message}, {ex.Data["StackTrace"]}"));
                return null;
            }
        }

        private static StringBuilder _stringBuilder = new StringBuilder();

        public static string GetBuffer(byte[] buffer,int count = -1)
        {
            _stringBuilder.Length = 0;
            _stringBuilder.Append("[");

            var length = count <= 0 ? buffer.Length : count;
            
            for (int i = 0; i < length; i++)
            {
                if (i == length - 1)
                {
                    _stringBuilder.Append(buffer[i]);
                    _stringBuilder.Append("]");
                }
                else
                {
                    _stringBuilder.Append(buffer[i]);
                    _stringBuilder.Append(",");
                }
            }

            return _stringBuilder.ToString();
        }

        [Conditional("UNITY_EDITOR")][Conditional("ENABLE_LOG_BUFFER")]
        public static void PrintBuffer(byte[] buffer,int count = -1)
        {
            Log.Debug(GetBuffer(buffer,count));
        }
    }
}