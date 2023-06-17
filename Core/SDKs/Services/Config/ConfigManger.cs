using Core.SDKs.Services.Plugin;
using log4net;
using Newtonsoft.Json;

namespace Core.SDKs.Services.Config;

public class ConfigManger
{
    public static Services.Config.Config Config = new();
    private static readonly ILog Log = LogManager.GetLogger(nameof(ConfigManger));

    public static void Init()
    {
        if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "configs"))
        {
            Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "configs");
        }

        var configF = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "configs\\config.json");
        if (!configF.Exists)
        {
            var j = JsonConvert.SerializeObject(new Services.Config.Config(), Formatting.Indented);
            File.WriteAllText(configF.FullName, j);
        }

        var json = File.ReadAllText(configF.FullName);
        try
        {
            Config = JsonConvert.DeserializeObject<Services.Config.Config>(json)!;
        }
        catch (Exception e)
        {
            Log.Error(e);
            Log.Error("配置文件加载失败");
            Config = new Services.Config.Config();
        }
    }


    public static void Save()
    {
        var configF = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "configs\\config.json");

        File.WriteAllText(configF.FullName, JsonConvert.SerializeObject(Config, Formatting.Indented));
        foreach (var dConfig in PluginManager.EnablePlugin)
        {
            var config1 = new FileInfo(AppDomain.CurrentDomain.BaseDirectory +
                                       $"configs\\{dConfig.PluginInfo.Author}_{dConfig.PluginInfo.PluginId}.json");

            File.WriteAllText(config1.FullName,
                JsonConvert.SerializeObject(dConfig.GetConfigJObject(), Formatting.Indented));
        }
    }
}