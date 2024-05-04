#region

using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.Messaging;
using Core.SDKs.HotKey;
using Core.ViewModel;
using log4net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using PluginCore;
using PluginCore.Attribute;
using PluginCore.Config;

#endregion

namespace Core.SDKs.Services.Config;

public static class ConfigManger
{
    public static Dictionary<string, ConfigBase> Configs = new();
    public static KitopiaConfig? Config => Configs["KitopiaConfig"] as KitopiaConfig ?? null;
    private static readonly ILog log = LogManager.GetLogger(nameof(ConfigManger));

    public static JsonSerializerOptions DefaultOptions = new JsonSerializerOptions
    {
        IncludeFields = true,
        WriteIndented = true,
        ReferenceHandler = ReferenceHandler.Preserve,
        Converters = { new ObjectJsonConverter() }
        // DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static void Init()
    {
        if (!Directory.Exists($"{AppDomain.CurrentDomain.BaseDirectory}configs"))
        {
            Directory.CreateDirectory($"{AppDomain.CurrentDomain.BaseDirectory}configs");
        }

        Configs.Add("KitopiaConfig", new KitopiaConfig() { Name = "KitopiaConfig" });
        var configF =
            new FileInfo(
                $"{AppDomain.CurrentDomain.BaseDirectory}configs{Path.DirectorySeparatorChar}KitopiaConfig.json");
        if (!configF.Exists)
        {
            var j = JsonSerializer.Serialize(Config, ConfigManger.DefaultOptions);
            File.WriteAllText(configF.FullName, j);
        }

        var json = File.ReadAllText(configF.FullName);
        try
        {
            Configs["KitopiaConfig"] =
                JsonSerializer.Deserialize(json, Config.GetType(), ConfigManger.DefaultOptions)! as ConfigBase ??
                Config;
        }
        catch (Exception e)
        {
            log.Error(e);
            log.Error("配置文件加载失败");
        }

        Config.GetType()
              .GetFields(BindingFlags.Instance | BindingFlags.Public)
              .ToList()
              .ForEach(x => {
                   if (x.GetCustomAttribute<ConfigField>() is { } configField)
                   {
                       if (configField.FieldType == ConfigFieldType.快捷键)
                       {
                           HotKeyManager.HotKeys.Add(x.GetValue(Config) as HotKeyModel);
                       }
                   }
               });
        Config.ConfigChanged += (sender, args) => {
            switch (args.Name)
            {
                case "autoStart":
                {
                    if ((bool)args.Value)
                    {
                        var strName = AppDomain.CurrentDomain.BaseDirectory + "Kitopia.exe"; //获取要自动运行的应用程序名
                        if (!File.Exists(strName)) //判断要自动运行的应用程序文件是否存在
                        {
                            return;
                        }

                        var registry =
                            Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run",
                                true); //检索指定的子项
                        if (registry == null) //若指定的子项不存在
                        {
                            registry = Registry.CurrentUser.CreateSubKey(
                                "Software\\Microsoft\\Windows\\CurrentVersion\\Run"); //则创建指定的子项
                        }

                        log.Info("用户确认启用开机自启");
                        try
                        {
                            registry.SetValue("Kitopia", $"\"{strName}\""); //设置该子项的新的“键值对”
                            ((IToastService)ServiceManager.Services.GetService(typeof(IToastService))).Show("开机自启",
                                "开机自启设置成功");
                        }
                        catch (Exception exception)
                        {
                            log.Error("开机自启设置失败");
                            log.Error(exception.StackTrace);
                            ((IToastService)ServiceManager.Services.GetService(typeof(IToastService))).Show("开机自启",
                                "开机自启设置失败");
                        }
                    }
                    else
                    {
                        try
                        {
                            var registry =
                                Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run",
                                    true); //检索指定的子项
                            registry?.DeleteValue("Kitopia");
                        }
                        catch (Exception)
                        {
                        }
                    }

                    break;
                }
                case "autoStartEverything":
                {
                    ((SearchWindowViewModel)ServiceManager.Services.GetService(typeof(SearchWindowViewModel)))
                       .EverythingIsOk =
                        !(bool)args.Value;
                    break;
                }
                case "themeChoice":
                {
                    switch ((ThemeEnum)args.Value)
                    {
                        case ThemeEnum.跟随系统:
                        {
                            ServiceManager.Services.GetService<IThemeChange>()
                                          .followSys(true);
                            break;
                        }
                        case ThemeEnum.深色:
                        {
                            ServiceManager.Services.GetService<IThemeChange>()
                                          .followSys(false);
                            ServiceManager.Services.GetService<IThemeChange>()
                                          .changeTo("theme_dark");
                            break;
                        }
                        case ThemeEnum.浅色:
                        {
                            ServiceManager.Services.GetService<IThemeChange>()
                                          .followSys(false);
                            ServiceManager.Services.GetService<IThemeChange>()
                                          .changeTo("theme_light");
                            break;
                        }
                    }

                    break;
                }
            }
        };
    }

    public static void RemoveConfig(string key)
    {
        foreach (var (s, value) in ConfigManger.Configs.Where(x => x.Key.StartsWith(key)))
        {
            value.GetType()
                 .BaseType.GetField("Instance")
                 .SetValue(value, null);
            ConfigManger.Configs.Remove(s);
        }
    }

    public static void Save()
    {
        var keyCollection = Configs.Keys.ToList();
        foreach (var configsKey in keyCollection)
        {
            var configBase = Configs[configsKey];
            var configF =
                new FileInfo(
                    $"{AppDomain.CurrentDomain.BaseDirectory}configs{Path.DirectorySeparatorChar}{configsKey}.json");


            var j = JsonSerializer.Serialize(configBase, configBase.GetType(), ConfigManger.DefaultOptions);
            File.WriteAllText(configF.FullName, j);
        }

        WeakReferenceMessenger.Default.Send<string, string>("ConfigSave", "ConfigSave");
    }

    public static void Save(string key)
    {
        var configBase = Configs[key];
        if (configBase is null)
        {
            return;
        }

        var configF =
            new FileInfo($"{AppDomain.CurrentDomain.BaseDirectory}configs{Path.DirectorySeparatorChar}{key}.json");

        var j = JsonSerializer.Serialize(configBase, configBase.GetType(), ConfigManger.DefaultOptions);
        File.WriteAllText(configF.FullName, j);
        WeakReferenceMessenger.Default.Send<string, string>("ConfigSave", "ConfigSave");
    }
}