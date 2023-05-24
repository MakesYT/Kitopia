using Newtonsoft.Json;

namespace Core.SDKs.Config;

public class ConfigManger
{
    public static Config config;

    public static void Init()
    {
        var configF = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "config.json");
        if (!configF.Exists)
        {
            var j = JsonConvert.SerializeObject(new Config(), Formatting.Indented);
            File.WriteAllText(configF.FullName, j);
        }

        var json = File.ReadAllText(configF.FullName);

        config = JsonConvert.DeserializeObject<Config>(json);
    }


    public static void Save()
    {
        var configF = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "config.json");

        File.WriteAllText(configF.FullName, JsonConvert.SerializeObject(config, Formatting.Indented));
    }
}