using log4net;
using Newtonsoft.Json;

namespace Core.SDKs.Config;

public class ConfigManger
{
    public static Config Config = new();
    private static readonly ILog Log = LogManager.GetLogger(nameof(ConfigManger));

    public static void Init()
    {
        var configF = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "config.json");
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
        var configF = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "config.json");

        File.WriteAllText(configF.FullName, JsonConvert.SerializeObject(Config, Formatting.Indented));
    }
}