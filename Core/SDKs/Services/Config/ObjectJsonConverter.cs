using System.Dynamic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Core.SDKs.Services.Config;

public class ObjectJsonConverter : JsonConverter<object>
{
    public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // 根据ValueKind判断数据类型
        if (reader.TokenType ==
            // 若为JsonElement类型，再根据ValueKind属性判断
            JsonTokenType.StartObject)
        {
            JsonSerializer.Deserialize<ExpandoObject>(ref reader, options);
            return null;
        }

        if (reader.TokenType == JsonTokenType.Number)
            return reader.TryGetInt64(out long l) ? l : reader.GetDouble();
        if (reader.TokenType == JsonTokenType.String)
            return reader.TryGetDateTime(out DateTime datetime) ? datetime : reader.GetString();
        if (reader.TokenType == JsonTokenType.True)
            return true;
        if (reader.TokenType == JsonTokenType.False)
            return false;
        if (reader.TokenType == JsonTokenType.Null)
            return null;
        // 这里可以根据需要添加更多的数据类型判断
        return JsonSerializer.Deserialize<JsonElement>(ref reader);
    }

    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }
}