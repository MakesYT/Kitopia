#region

using CommunityToolkit.Mvvm.Messaging;
using Core.SDKs.Services.Plugin;
using log4net;
using Newtonsoft.Json;

#endregion

namespace Core.SDKs.Services.Config;

public class ConfigManger
{
    public static Config Config = new();
    private static readonly ILog Log = LogManager.GetLogger(nameof(ConfigManger));

    public static void Init()
    {
        if (!Directory.Exists($"{AppDomain.CurrentDomain.BaseDirectory}configs"))
        {
            Directory.CreateDirectory($"{AppDomain.CurrentDomain.BaseDirectory}configs");
        }

        var configF =
            new FileInfo($"{AppDomain.CurrentDomain.BaseDirectory}configs{Path.DirectorySeparatorChar}config.json");
        if (!configF.Exists)
        {
            var j = JsonConvert.SerializeObject(new Config(), Formatting.Indented);
            File.WriteAllText(configF.FullName, j);
        }

        var json = File.ReadAllText(configF.FullName);
        try
        {
            Config = JsonConvert.DeserializeObject<Config>(json)!;
        }
        catch (Exception e)
        {
            Log.Error(e);
            Log.Error("配置文件加载失败");
            Config = new Config();
        }
    }


    public static void Save()
    {
        var configF = new FileInfo(
            $"{AppDomain.CurrentDomain.BaseDirectory}configs{Path.DirectorySeparatorChar}config.json");

        File.WriteAllText(configF.FullName, JsonConvert.SerializeObject(Config, Formatting.Indented));
        foreach (var dConfig in PluginManager.EnablePlugin)
        {
            {
                var config1 = new FileInfo(
                    $"{AppDomain.CurrentDomain.BaseDirectory}configs{Path.DirectorySeparatorChar}{dConfig.Value.PluginInfo.Author}_{dConfig.Value.PluginInfo.PluginId}.json");

                File.WriteAllText(config1.FullName,
                    JsonConvert.SerializeObject(dConfig.Value.GetConfigJObject(), Formatting.Indented));
            }
        }

        WeakReferenceMessenger.Default.Send<string, string>("ConfigSave", "ConfigSave");
    }
}