using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Core.SDKs.Config;

public class ConfigManger
{
    
    public static Config? config;
    public static void Init()
    {
        FileInfo configF = new FileInfo(System.IO.Directory.GetCurrentDirectory() + "//config.json");
        if (!configF.Exists)
        {
            string j=JsonConvert.SerializeObject(new Config(), Formatting.Indented);
            File.WriteAllText(configF.FullName,j);
        }
        string json = File.ReadAllText(configF.FullName);
        
        config=JsonConvert.DeserializeObject<Config>(json);
    }

    

    public static void Save()
    {
        FileInfo configF = new FileInfo(System.IO.Directory.GetCurrentDirectory() + "//config.json");
        
        File.WriteAllText(configF.FullName,JsonConvert.SerializeObject(config, Formatting.Indented));
    }
}