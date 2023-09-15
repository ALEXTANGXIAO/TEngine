using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace TEngine
{
    public static class StringHelper
    {
        public static IEnumerable<byte> ToBytes(this string str)
        {
            byte[] byteArray = Encoding.Default.GetBytes(str);
            return byteArray;
        }

        public static byte[] ToByteArray(this string str)
        {
            byte[] byteArray = Encoding.Default.GetBytes(str);
            return byteArray;
        }

        public static byte[] ToUtf8(this string str)
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(str);
            return byteArray;
        }

        public static byte[] HexToBytes(this string hexString)
        {
            if (hexString.Length % 2 != 0)
            {
                throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "The binary key cannot have an odd number of digits: {0}",
                    hexString));
            }

            var hexAsBytes = new byte[hexString.Length / 2];
            for (int index = 0; index < hexAsBytes.Length; index++)
            {
                string byteValue = "";
                byteValue += hexString[index * 2];
                byteValue += hexString[index * 2 + 1];
                hexAsBytes[index] = byte.Parse(byteValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }

            return hexAsBytes;
        }

        public static string Fmt(this string text, params object[] args)
        {
            return string.Format(text, args);
        }

        public static string ListToString<T>(this List<T> list)
        {
            StringBuilder sb = new StringBuilder();
            foreach (T t in list)
            {
                sb.Append(t);
                sb.Append(",");
            }

            return sb.ToString();
        }

        public static string ArrayToString<T>(this T[] args)
        {
            if (args == null)
            {
                return "";
            }

            string argStr = " [";
            for (int arrIndex = 0; arrIndex < args.Length; arrIndex++)
            {
                argStr += args[arrIndex];
                if (arrIndex != args.Length - 1)
                {
                    argStr += ", ";
                }
            }

            argStr += "]";
            return argStr;
        }

        public static string ArrayToString<T>(this T[] args, int index, int count)
        {
            if (args == null)
            {
                return "";
            }

            string argStr = " [";
            for (int arrIndex = index; arrIndex < count + index; arrIndex++)
            {
                argStr += args[arrIndex];
                if (arrIndex != args.Length - 1)
                {
                    argStr += ", ";
                }
            }

            argStr += "]";
            return argStr;
        }

        public static string PrintProto(this AProto proto, int stackIndex = 0, string propertyInfoName = "", bool isList = false, int listIndex = 0)
        {
            if (proto == null)
            {
                return "";
            }

            StringBuilder _stringBuilder = new StringBuilder();
            _stringBuilder.Clear();

            Type type = proto.GetType();
            _stringBuilder.Append($"\n");
            for (int i = 0; i < stackIndex; i++)
            {
                _stringBuilder.Append("\t");
            }

            propertyInfoName = isList ? $"[{propertyInfoName}][{listIndex}]" : $"[{propertyInfoName}]";
            _stringBuilder.Append(stackIndex == 0? $"[{type.Name}]" : propertyInfoName);

            var bindingFlags = BindingFlags.Public | BindingFlags.Instance;
            var propertyInfos = type.GetProperties(bindingFlags);
            for (var i = 0; i < propertyInfos.Length; ++i)
            {
                var propertyInfo = propertyInfos[i];

                if (propertyInfo.PropertyType.BaseType == typeof (AProto))
                {
                    _stringBuilder.Append(PrintProto((AProto)propertyInfo.GetValue(proto), stackIndex + 1, propertyInfo.Name));
                }
                else if (propertyInfo.PropertyType.IsList() && propertyInfo.PropertyType.IsGenericType)
                {
                    object value = propertyInfo.GetValue(proto, null);
                    if (value != null)
                    {
                        Type objType = value.GetType();
                        int count = Convert.ToInt32(objType.GetProperty("Count").GetValue(value, null));
                        for (int j = 0; j < count; j++)
                        {
                            object item = objType.GetProperty("Item").GetValue(value, new object[] { j });
                            _stringBuilder.Append(PrintProto((AProto)item, stackIndex + 1, propertyInfo.Name, isList: true, listIndex: j));
                        }
                    }
                }
                else
                {
                    _stringBuilder.Append($"\n");
                    for (int j = 0; j < stackIndex + 1; j++)
                    {
                        _stringBuilder.Append("\t");
                    }

                    _stringBuilder.Append($"[{propertyInfo.Name}]");
                    _stringBuilder.Append(": ");
                    _stringBuilder.Append(propertyInfo.GetValue(proto));
                }
            }

            return _stringBuilder.ToString();
        }

        /// <summary>
        /// 判断类型是否为可操作的列表类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsList(this Type type)
        {
            if (typeof (System.Collections.IList).IsAssignableFrom(type))
            {
                return true;
            }

            foreach (var it in type.GetInterfaces())
            {
                if (it.IsGenericType && typeof (IList<>) == it.GetGenericTypeDefinition())
                    return true;
            }

            return false;
        }
    }
}