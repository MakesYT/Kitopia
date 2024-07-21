using System.Text.Json;
using System.Text.Json.Serialization;
using Core.SDKs.CustomScenario;
using Core.SDKs.Services.Plugin;

namespace Core.SDKs.Services.Config;

public class ScenarioMethodJsonCtr : JsonConverter<ScenarioMethod>
{
    public override ScenarioMethod? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        Console.WriteLine("OnDeserializing");


        ScenarioMethod scenario = JsonSerializer.Deserialize<ScenarioMethod>(ref reader, options);
        if (scenario.IsFromPlugin)
        {
            scenario.ServiceProvider = PluginManager.EnablePlugin[scenario.PluginInfo.ToPlgString()].ServiceProvider;
            scenario.Method = PluginManager.EnablePlugin[scenario.PluginInfo.ToPlgString()]
                .GetMethod(scenario._methodAbsolutelyName);
        }

        return scenario;
    }

    public override void Write(Utf8JsonWriter writer, ScenarioMethod value, JsonSerializerOptions options)
    {
        Console.WriteLine("OnSerializing");

        // Don't pass in options when recursively calling Serialize.
        JsonSerializer.Serialize(writer, value, options);

        // Place "after" code here (OnSerialized)
        Console.WriteLine("OnSerialized");
    }
}