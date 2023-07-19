using System;
using System.IO;
using System.Text;

namespace TEngine.Core
{
    public static class ByteHelper
    {
        private static readonly string[] Suffix = { "Byte", "KB", "MB", "GB", "TB" };

        public static long ReadInt64(FileStream stream)
        {
            var buffer = new byte[8];
            stream.Read(buffer, 0, 8);
            return BitConverter.ToInt64(buffer, 0);
        }

        public static int ReadInt32(FileStream stream)
        {
            var buffer = new byte[4];
            stream.Read(buffer, 0, 4);
            return BitConverter.ToInt32(buffer, 0);
        }

        public static long ReadInt64(MemoryStream stream)
        {
            var buffer = new byte[8];
            stream.Read(buffer, 0, 8);
            return BitConverter.ToInt64(buffer, 0);
        }

        public static int ReadInt32(MemoryStream stream)
        {
            var buffer = new byte[4];
            stream.Read(buffer, 0, 4);
            return BitConverter.ToInt32(buffer, 0);
        }

        public static string ToHex(this byte b)
        {
            return b.ToString("X2");
        }

        public static string ToHex(this byte[] bytes)
        {
            var stringBuilder = new StringBuilder();
            foreach (var b in bytes)
            {
                stringBuilder.Append(b.ToString("X2"));
            }

            return stringBuilder.ToString();
        }

        public static string ToHex(this byte[] bytes, string format)
        {
            var stringBuilder = new StringBuilder();
            foreach (var b in bytes)
            {
                stringBuilder.Append(b.ToString(format));
            }

            return stringBuilder.ToString();
        }

        public static string ToHex(this byte[] bytes, int offset, int count)
        {
            var stringBuilder = new StringBuilder();
            for (var i = offset; i < offset + count; ++i)
            {
                stringBuilder.Append(bytes[i].ToString("X2"));
            }

            return stringBuilder.ToString();
        }

        public static string ToStr(this byte[] bytes)
        {
            return Encoding.Default.GetString(bytes);
        }

        public static string ToStr(this byte[] bytes, int index, int count)
        {
            return Encoding.Default.GetString(bytes, index, count);
        }

        public static string Utf8ToStr(this byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes);
        }

        public static string Utf8ToStr(this byte[] bytes, int index, int count)
        {
            return Encoding.UTF8.GetString(bytes, index, count);
        }

        public static void WriteTo(this byte[] bytes, int offset, uint num)
        {
            bytes[offset]     = (byte)(num & 0xff);
            bytes[offset + 1] = (byte)((num & 0xff00)     >> 8);
            bytes[offset + 2] = (byte)((num & 0xff0000)   >> 16);
            bytes[offset + 3] = (byte)((num & 0xff000000) >> 24);
        }

        public static void WriteTo(this byte[] bytes, int offset, int num)
        {
            bytes[offset]     = (byte)(num & 0xff);
            bytes[offset + 1] = (byte)((num & 0xff00)     >> 8);
            bytes[offset + 2] = (byte)((num & 0xff0000)   >> 16);
            bytes[offset + 3] = (byte)((num & 0xff000000) >> 24);
        }

        public static void WriteTo(this byte[] bytes, int offset, byte num)
        {
            bytes[offset] = num;
        }

        public static void WriteTo(this byte[] bytes, int offset, short num)
        {
            bytes[offset]     = (byte)(num & 0xff);
            bytes[offset + 1] = (byte)((num & 0xff00) >> 8);
        }

        public static void WriteTo(this byte[] bytes, int offset, ushort num)
        {
            bytes[offset]     = (byte)(num & 0xff);
            bytes[offset + 1] = (byte)((num & 0xff00) >> 8);
        }

        public static string ToReadableSpeed(this long byteCount)
        {
            var    i        = 0;
            double dblSByte = byteCount;
            if (byteCount <= 1024)
            {
                return $"{dblSByte:0.##}{Suffix[i]}";
            }

            for (i = 0; byteCount / 1024 > 0; i++, byteCount /= 1024)
            {
                dblSByte = byteCount / 1024.0;
            }

            return $"{dblSByte:0.##}{Suffix[i]}";
        }

        public static string ToReadableSpeed(this ulong byteCount)
        {
            var    i        = 0;
            double dblSByte = byteCount;

            if (byteCount <= 1024)
            {
                return $"{dblSByte:0.##}{Suffix[i]}";
            }

            for (i = 0; byteCount / 1024 > 0; i++, byteCount /= 1024)
            {
                dblSByte = byteCount / 1024.0;
            }

            return $"{dblSByte:0.##}{Suffix[i]}";
        }

        /// <summary>
        /// 合并一个数组
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="otherBytes"></param>
        /// <returns></returns>
        public static byte[] MergeBytes(byte[] bytes, byte[] otherBytes)
        {
            var result = new byte[bytes.Length + otherBytes.Length];
            bytes.CopyTo(result, 0);
            otherBytes.CopyTo(result, bytes.Length);
            return result;
        }
    }
}