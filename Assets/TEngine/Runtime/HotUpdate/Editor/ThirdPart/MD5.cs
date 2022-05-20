using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Huatuo.Editor.ThirdPart
{
    public class MD5
    {
        private static HashAlgorithm algorithm_md5;
        private static HashAlgorithm algorithm_sha1;
        private static StringBuilder stringBuilder = new StringBuilder();

        /// <summary>
        /// 计算一串string的md5
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string ComputeStringMD5(string content)
        {
            if (algorithm_md5 == null)
                algorithm_md5 = System.Security.Cryptography.MD5.Create();

            var bytes = algorithm_md5.ComputeHash(Encoding.UTF8.GetBytes(content));
            return ByteArrayToHexString(bytes);
        }

        /// <summary>
        /// 计算一串string的sha1
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string ComputeStringSha1(string content)
        {
            if (algorithm_sha1 == null)
                algorithm_sha1 = SHA1.Create();

            var bytes = algorithm_sha1.ComputeHash(Encoding.UTF8.GetBytes(content));
            return ByteArrayToHexString(bytes);
        }

        /// <summary>
        /// 计算文件的 md5 值
        /// </summary>
        /// <param name="fileName">要计算 MD5 值的文件名和路径</param>
        /// <returns>MD5 值16进制字符串</returns>
        public static string ComputeFileMD5(string fileName)
        {
            if (!File.Exists(fileName))
                return string.Empty;

            FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            if (algorithm_md5 == null)
                algorithm_md5 = System.Security.Cryptography.MD5.Create();

            var bytes = algorithm_md5.ComputeHash(stream);

            stream.Close();
            stream.Dispose();

            return ByteArrayToHexString(bytes);
        }

        /// <summary>
        /// 计算文件的 sha1 值
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string ComputeFileSha1(string fileName)
        {
            if (!File.Exists(fileName))
                return string.Empty;

            FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            if (algorithm_sha1 == null)
                algorithm_sha1 = SHA1.Create();

            var bytes = algorithm_sha1.ComputeHash(stream);

            stream.Close();
            stream.Dispose();

            return ByteArrayToHexString(bytes);
        }

        /// <summary>
        /// 字节数组转换为16进制表示
        /// </summary>
        public static string ByteArrayToHexString(byte[] buf)
        {
            stringBuilder.Clear();
            if (buf != null)
            {
                foreach (var b in buf)
                {
                    stringBuilder.Append(b.ToString("X2"));
                }
            }

            var str = stringBuilder.ToString();
            stringBuilder.Clear();
            return str;
        }
    }
}
