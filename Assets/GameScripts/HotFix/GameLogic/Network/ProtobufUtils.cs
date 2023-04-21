// using System;
// using System.ComponentModel;
// using System.IO;
// using ProtoBuf;
// using ProtoBuf.Meta;
//
// /// <summary>
// /// ProtoBuf工具
// /// </summary>
// public class ProtobufUtils
// {
//     /// <summary>
//     /// 消息压入内存流。
//     /// </summary>
//     /// <param name="message"></param>
//     /// <param name="stream"></param>
//     public static void ToStream(object message, MemoryStream stream)
//     {
//         ((IMessage)message).WriteTo(stream);
//     }
//
//     /// <summary>
//     /// 比特流解析。
//     /// </summary>
//     /// <param name="type"></param>
//     /// <param name="bytes"></param>
//     /// <param name="index"></param>
//     /// <param name="count"></param>
//     /// <returns></returns>
//     public static object FromBytes(Type type, byte[] bytes, int index, int count)
//     {
//         object message = Activator.CreateInstance(type);
//         ((IMessage)message).MergeFrom(bytes, index, count);
//         ISupportInitialize iSupportInitialize = message as ISupportInitialize;
//         if (iSupportInitialize == null)
//         {
//             return message;
//         }
//
//         iSupportInitialize.EndInit();
//         return message;
//     }
//
//     /// <summary>
//     /// 比特流解析。
//     /// </summary>
//     /// <param name="instance"></param>
//     /// <param name="bytes"></param>
//     /// <param name="index"></param>
//     /// <param name="count"></param>
//     /// <returns></returns>
//     public static object FromBytes(object instance, byte[] bytes, int index, int count)
//     {
//         object message = instance;
//         ((IMessage)message).MergeFrom(bytes, index, count);
//         ISupportInitialize iSupportInitialize = message as ISupportInitialize;
//         if (iSupportInitialize == null)
//         {
//             return message;
//         }
//
//         iSupportInitialize.EndInit();
//         return message;
//     }
//
//     /// <summary>
//     /// 从内存流取出。
//     /// </summary>
//     /// <param name="type"></param>
//     /// <param name="stream"></param>
//     /// <returns></returns>
//     public static object FromStream(Type type, MemoryStream stream)
//     {
//         object message = Activator.CreateInstance(type);
//         ((IMessage)message).MergeFrom(stream.GetBuffer(), (int)stream.Position, (int)stream.Length);
//         ISupportInitialize iSupportInitialize = message as ISupportInitialize;
//         if (iSupportInitialize == null)
//         {
//             return message;
//         }
//
//         iSupportInitialize.EndInit();
//         return message;
//     }
//
//     /// <summary>
//     /// 从内存流取出。
//     /// </summary>
//     /// <param name="message"></param>
//     /// <param name="stream"></param>
//     /// <returns></returns>
//     public static object FromStream(object message, MemoryStream stream)
//     {
//         // TODO 这个message最好从池中获取，减少gc
//         ((IMessage)message).MergeFrom(stream.GetBuffer(), (int)stream.Position, (int)stream.Length);
//         ISupportInitialize iSupportInitialize = message as ISupportInitialize;
//         if (iSupportInitialize == null)
//         {
//             return message;
//         }
//
//         iSupportInitialize.EndInit();
//         return message;
//     }
//
//     /// <summary>
//     /// 序列化protobuf
//     /// </summary>
//     /// <param name="message"></param>
//     /// <returns></returns>
//     public static byte[] Serialize(object message)
//     {
//         return ((IMessage)message).ToByteArray();
//     }
//
//     /// <summary>
//     /// 反序列化protobuf
//     /// </summary>
//     /// <typeparam name="T"></typeparam>
//     /// <param name="dataBytes"></param>
//     /// <returns></returns>
//     public static T Deserialize<T>(byte[] dataBytes) where T : IMessage, new()
//     {
//         T msg = new T();
//         msg = (T)msg.Descriptor.Parser.ParseFrom(dataBytes);
//         return msg;
//     }
//
//     public static int GetHighOrder(int cmdMerge)
//     {
//         return cmdMerge >> 16;
//     }
//
//     public static int GetLowOrder(int cmdMerge)
//     {
//         return cmdMerge & 65535;
//     }
// }