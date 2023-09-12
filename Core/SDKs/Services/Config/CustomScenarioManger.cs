using System.Collections.ObjectModel;
using System.IO;
using log4net;
using Newtonsoft.Json;

namespace Core.SDKs.Services.Config;

public partial class CustomScenarioManger
{
    public static ObservableCollection<SDKs.CustomScenario.CustomScenario> CustomScenarios = new();
    private static readonly ILog Log = LogManager.GetLogger(nameof(CustomScenarioManger));

    public static void Init()
    {
        if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "customScenarios"))
        {
            Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "customScenarios");
        }

        var info = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + "customScenarios");
        foreach (var fileInfo in info.GetFiles())
        {
            var json = File.ReadAllText(fileInfo.FullName);
            try
            {
                CustomScenarios.Add(JsonConvert.DeserializeObject<SDKs.CustomScenario.CustomScenario>(json)!);
            }
            catch (Exception e)
            {
                Log.Error(e);
                Log.Error("情景文件加载失败");
            }
        }
    }

    public static void Save(SDKs.CustomScenario.CustomScenario scenario)
    {
        if (scenario.UUID is null)
        {
            var s = Guid.NewGuid().ToString();
            scenario.UUID = s;
            CustomScenarios.Add(scenario);
        }

        var configF = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + $"customScenarios\\{scenario.UUID}.json");

        var setting = new JsonSerializerSettings();
        setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
        setting.ReferenceLoopHandling = ReferenceLoopHandling.Serialize;
        setting.TypeNameHandling = TypeNameHandling.None;
        setting.Formatting = Formatting.Indented;

        File.WriteAllText(configF.FullName, JsonConvert.SerializeObject(scenario, setting));
    }

    public static void Reload(SDKs.CustomScenario.CustomScenario scenario)
    {
        CustomScenarios.Remove(scenario);
        var configF = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + $"customScenarios\\{scenario.UUID}.json");
        if (configF.Exists)
        {
            var json = File.ReadAllText(configF.FullName);
            try
            {
                CustomScenarios.Add(JsonConvert.DeserializeObject<SDKs.CustomScenario.CustomScenario>(json)!);
            }
            catch (Exception e)
            {
                Log.Error(e);
                Log.Error("情景文件加载失败");
            }
        }
    }
}