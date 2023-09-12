using Newtonsoft.Json;

namespace Core.SDKs.Services.Config;

public class TypeJsonConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(Type);
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var type = (Type)value;
        writer.WriteValue(type.AssemblyQualifiedName);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        var typeName = (string)reader.Value;
        if (Type.GetType(typeName) is null)
        {
            return null;
        }
        else
            return Type.GetType(typeName);
    }
}