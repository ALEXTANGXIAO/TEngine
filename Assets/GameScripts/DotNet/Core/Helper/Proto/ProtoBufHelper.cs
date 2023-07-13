using System;
using System.IO;
using ProtoBuf;
#pragma warning disable CS8604

namespace TEngine.Core
{
    public static class ProtoBufHelper
    {
        public static object FromBytes(Type type, byte[] bytes, int index, int count)
        {
            using var stream = new MemoryStream(bytes, index, count);
            return Serializer.Deserialize(type, stream);
        }

        public static T FromBytes<T>(byte[] bytes)
        {
            using var stream = new MemoryStream(bytes, 0, bytes.Length);
            return Serializer.Deserialize<T>(stream);
            // return FromBytes<T>(bytes, 0, bytes.Length);
        }

        public static T FromBytes<T>(byte[] bytes, int index, int count)
        {
            using var stream = new MemoryStream(bytes, index, count);
            return Serializer.Deserialize<T>(stream);
        }

        public static byte[] ToBytes(object message)
        {
            using var stream = new MemoryStream();
            Serializer.Serialize(stream, message);
            return stream.ToArray();
        }

        public static void ToStream(object message, MemoryStream stream)
        {
            Serializer.Serialize(stream, message);
        }

        public static object FromStream(Type type, MemoryStream stream)
        {
            return Serializer.Deserialize(type, stream);
        }
        
        public static T FromStream<T>(MemoryStream stream)
        {
            return (T) Serializer.Deserialize(typeof(T), stream);
        }
        
        public static T Clone<T>(T t)
        {
            var bytes = ToBytes(t);
            using var stream = new MemoryStream(bytes, 0, bytes.Length);
            return Serializer.Deserialize<T>(stream);
        }
    }
}