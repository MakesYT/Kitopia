using System.Text.Json;
using System.Text.Json.Serialization;
using Core.SDKs.CustomScenario;
using Core.SDKs.Services.Plugin;

namespace Core.SDKs.Services.Config;

public class TypeJsonConverter : JsonConverter<Type>
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(Type);
    }

    public override Type? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var typeName = (string)reader.GetString();
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

                throw new CustomScenarioLoadFromJsonException(strings[0], strings[1]);
            }

            return type;
        }

        if (PluginManager.EnablePlugin.TryGetValue(strings[0], out var value))
        {
            return value.GetType(strings[1]);
        }

        throw new CustomScenarioLoadFromJsonException(strings[0], strings[1]);
    }

    public override void Write(Utf8JsonWriter writer, Type value, JsonSerializerOptions options)
    {
        var type = (Type)value;
        if (type.FullName.Contains("Native.KeyCode"))
        {
        }

        var plugin = PluginManager.EnablePlugin.FirstOrDefault((e) => e.Value.IsPluginAssembly(type.Assembly)).Value;
        // type.Assembly.
        // var a = PluginManager.GetPlugnNameByTypeName(type.FullName);
        if (plugin is null)
        {
            writer.WriteStringValue($"System {type.FullName}");
            return;
        }

        writer.WriteStringValue($"{plugin.PluginInfo} {type.FullName}");
    }
}