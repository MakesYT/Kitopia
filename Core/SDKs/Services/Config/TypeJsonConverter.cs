using Core.SDKs.Services.Plugin;
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
        var a = PluginManager.GetPlugnNameByTypeName(type.FullName);
        if (a is null)
        {
            writer.WriteValue($"System {type.FullName}");
            return;
        }
        else
        {
            writer.WriteValue($"{a} {type.FullName}");
            return;
        }

        writer.WriteValue(type.AssemblyQualifiedName);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        var typeName = (string)reader.Value;
        var strings = typeName.Split(" ");
        if (strings[0] == "System")
        {
            var type = Type.GetType(strings[1], false, true);
            if (type == null)
            {
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    foreach (var type1 in assembly.GetTypes())
                    {
                        if (type1.FullName == strings[1])
                        {
                            return type1;
                        }
                    }
                }
            }

            return type;
        }
        else
        {
            if (PluginManager.EnablePlugin.TryGetValue(strings[0], out var value))
            {
                return value.GetType(strings[1]);
            }
        }

        return null;
    }
}