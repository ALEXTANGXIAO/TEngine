using System.IO;
using System.Security.Cryptography;

namespace TEngine.Core
{
    public static class MD5Helper
    {
        public static string FileMD5(string filePath)
        {
            using var file = new FileStream(filePath, FileMode.Open);
            return FileMD5(file);
        }
        
        public static string FileMD5(FileStream fileStream)
        {
            var md5 = MD5.Create();
            return md5.ComputeHash(fileStream).ToHex("x2");
        }

        public static string BytesMD5(byte[] bytes)
        {
            var md5 = MD5.Create();
            bytes = md5.ComputeHash(bytes);
            return bytes.ToHex("x2");
        }
    }
}