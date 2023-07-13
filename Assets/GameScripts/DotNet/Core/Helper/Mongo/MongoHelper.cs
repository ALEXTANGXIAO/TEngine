#if TENGINE_NET
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using Unity.Mathematics;

namespace TEngine.Core;

public sealed class MongoHelper : Singleton<MongoHelper>
{
    private readonly HashSet<int> _registerCount = new HashSet<int>(3);

    static MongoHelper()
    {
        // 自动注册IgnoreExtraElements
        var conventionPack = new ConventionPack {new IgnoreExtraElementsConvention(true)};
        ConventionRegistry.Register("IgnoreExtraElements", conventionPack, type => true);
        BsonSerializer.TryRegisterSerializer(typeof(float2), new StructBsonSerialize<float2>());
        BsonSerializer.TryRegisterSerializer(typeof(float3), new StructBsonSerialize<float3>());
        BsonSerializer.TryRegisterSerializer(typeof(float4), new StructBsonSerialize<float4>());
        BsonSerializer.TryRegisterSerializer(typeof(quaternion), new StructBsonSerialize<quaternion>());
    }

    protected override void OnLoad(int assemblyName)
    {
        if (_registerCount.Count == 3)
        {
            return;
        }

        _registerCount.Add(assemblyName);

        Task.Run(() =>
        {
            foreach (var type in AssemblyManager.ForEach(assemblyName))
            {
                if (type.IsInterface || type.IsAbstract || !typeof(Entity).IsAssignableFrom(type))
                {
                    continue;
                }
                
                BsonClassMap.LookupClassMap(type);
            }
        });
    }

    public T Deserialize<T>(byte[] bytes)
    {
        return BsonSerializer.Deserialize<T>(bytes);
    }

    public object Deserialize(byte[] bytes, Type type)
    {
        return BsonSerializer.Deserialize(bytes, type);
    }
    
    public object Deserialize(byte[] bytes, string type)
    {
        return BsonSerializer.Deserialize(bytes, Type.GetType(type));
    }
    
    public T Deserialize<T>(Stream stream)
    {
        return BsonSerializer.Deserialize<T>(stream);
    }
    
    public object Deserialize(Stream stream, Type type)
    {
        return BsonSerializer.Deserialize(stream, type);
    }

    public object DeserializeFrom(Type type, MemoryStream stream)
    {
        return Deserialize(stream, type);
    }

    public T DeserializeFrom<T>(MemoryStream stream)
    {
        return Deserialize<T>(stream);
    }

    public T DeserializeFrom<T>(byte[] bytes, int index, int count)
    {
        return BsonSerializer.Deserialize<T>(bytes.AsMemory(index, count).ToArray());
    }
    
    public byte[] SerializeTo<T>(T t)
    {
        return t.ToBson();
    }

    public void SerializeTo<T>(T t, MemoryStream stream)
    {
        var bytes = t.ToBson();

        stream.Write(bytes, 0, bytes.Length);
    }

    public T Clone<T>(T t)
    {
        return Deserialize<T>(t.ToBson());
    }
}
#endif
#if TENGINE_UNITY
using System;
using System.IO;
namespace TEngine.Core
{
    public sealed class MongoHelper : Singleton<MongoHelper>
    {
        public object DeserializeFrom(Type type, MemoryStream stream)
        {
            return null;
        }
    }
}
#endif
