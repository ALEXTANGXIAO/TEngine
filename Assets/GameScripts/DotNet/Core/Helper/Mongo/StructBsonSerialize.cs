#if TENGINE_NET
using System.Reflection;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace TEngine.Core;

public class StructBsonSerialize<TValue> : StructSerializerBase<TValue> where TValue : struct
{
    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, TValue value)
    {
        var nominalType = args.NominalType;

        var bsonWriter = context.Writer;

        bsonWriter.WriteStartDocument();

        var fields = nominalType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        foreach (var field in fields)
        {
            bsonWriter.WriteName(field.Name);
            BsonSerializer.Serialize(bsonWriter, field.FieldType, field.GetValue(value));
        }

        bsonWriter.WriteEndDocument();
    }

    public override TValue Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        //boxing is required for SetValue to work
        object obj = new TValue();
        var actualType = args.NominalType;
        var bsonReader = context.Reader;

        bsonReader.ReadStartDocument();

        while (bsonReader.ReadBsonType() != BsonType.EndOfDocument)
        {
            var name = bsonReader.ReadName(Utf8NameDecoder.Instance);

            var field = actualType.GetField(name,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field != null)
            {
                var value = BsonSerializer.Deserialize(bsonReader, field.FieldType);
                field.SetValue(obj, value);
            }
        }

        bsonReader.ReadEndDocument();

        return (TValue) obj;
    }
}
#endif
