#region

using System.Reflection;
using CommunityToolkit.Mvvm.Messaging;
using Core.SDKs.HotKey;
using Core.SDKs.Services.Plugin;
using Core.ViewModel;
using log4net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using Newtonsoft.Json;
using PluginCore;
using PluginCore.Attribute;

#endregion

namespace Core.SDKs.Services.Config;

public static class ConfigManger
{
    public static Dictionary<string,ConfigBase> Configs = new();
    public static KitopiaConfig? Config => Configs["KitopiaCoreConfig"] as KitopiaConfig ?? null;
    private static readonly ILog log = LogManager.GetLogger(nameof(ConfigManger));

    public static void Init()
    {
        if (!Directory.Exists($"{AppDomain.CurrentDomain.BaseDirectory}configs"))
        {
            Directory.CreateDirectory($"{AppDomain.CurrentDomain.BaseDirectory}configs");
        }
        Configs.Add("KitopiaCoreConfig",new KitopiaConfig(){Name = "KitopiaCoreConfig"});
        var keyCollection = Configs.Keys.ToList();
        var configF =
            new FileInfo($"{AppDomain.CurrentDomain.BaseDirectory}configs{Path.DirectorySeparatorChar}KitopiaCoreConfig.json");
        if (!configF.Exists)
        {
            var j = JsonConvert.SerializeObject(Config, Formatting.Indented);
            File.WriteAllText(configF.FullName, j);
        }

        var json = File.ReadAllText(configF.FullName);
        try
        {
            Configs["KitopiaCoreConfig"] = JsonConvert.DeserializeObject(json,Config.GetType())! as ConfigBase ?? Config;
        }
        catch (Exception e)
        {
            log.Error(e);
            log.Error("配置文件加载失败");
        }
        
        Config.ConfigChanged += (sender, args) =>
        {
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
                            Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run", true); //检索指定的子项
                        if (registry == null) //若指定的子项不存在
                        {
                            registry = Registry.CurrentUser.CreateSubKey(
                                "Software\\Microsoft\\Windows\\CurrentVersion\\Run"); //则创建指定的子项
                        }

                        log.Info("用户确认启用开机自启");
                        try
                        {
                            registry.SetValue("Kitopia", $"\"{strName}\""); //设置该子项的新的“键值对”
                            ((IToastService)ServiceManager.Services.GetService(typeof(IToastService))).Show("开机自启", "开机自启设置成功");
                        }
                        catch (Exception exception)
                        {
                            log.Error("开机自启设置失败");
                            log.Error(exception.StackTrace);
                            ((IToastService)ServiceManager.Services.GetService(typeof(IToastService))).Show("开机自启", "开机自启设置失败");
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
                    
                    ((SearchWindowViewModel)ServiceManager.Services.GetService(typeof(SearchWindowViewModel))).EverythingIsOk =
                        !(bool)args.Value;
                    break;
                }
                case "themeChoice":
                {
                    switch ((ThemeEnum)args.Value)
                    {
                        case ThemeEnum.跟随系统:
                        {
                            ServiceManager.Services.GetService<IThemeChange>().followSys(true);
                            break;
                        }
                        case ThemeEnum.深色:
                        {
                            ServiceManager.Services.GetService<IThemeChange>().followSys(false);
                            ServiceManager.Services.GetService<IThemeChange>().changeTo("theme_dark");
                            break;
                        }
                        case ThemeEnum.浅色:
                        {
                            ServiceManager.Services.GetService<IThemeChange>().followSys(false);
                            ServiceManager.Services.GetService<IThemeChange>().changeTo("theme_light");
                            break;
                        }
                    }
                    break;
                }
            }
        };
    }
   
    public static void Save()
    {
        var keyCollection = Configs.Keys.ToList();
        foreach (var configsKey in keyCollection)
        {
            var configBase = Configs[configsKey];
            var configF =
                new FileInfo($"{AppDomain.CurrentDomain.BaseDirectory}configs{Path.DirectorySeparatorChar}{configsKey}.json");
            
            var j = JsonConvert.SerializeObject(configBase, Formatting.Indented);
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
        var j = JsonConvert.SerializeObject(configBase, Formatting.Indented);
        File.WriteAllText(configF.FullName, j);
        WeakReferenceMessenger.Default.Send<string, string>("ConfigSave", "ConfigSave");
    }
}