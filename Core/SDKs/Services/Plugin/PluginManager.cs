#region

using System.Collections.ObjectModel;
using Core.SDKs.CustomScenario;
using Core.SDKs.Services.Config;
using Core.ViewModel.Pages;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PluginCore;
using JsonSerializer = System.Text.Json.JsonSerializer;

#endregion

namespace Core.SDKs.Services.Plugin;

public class PluginManager
{
    private static readonly ILog Log = LogManager.GetLogger(nameof(PluginManager));

    public readonly static ObservableCollection<PluginInfo> AllPluginInfos = new();
    public readonly static Dictionary<string, Plugin> EnablePlugin = new();

    public static void Init()
    {
        PluginCore.Kitopia.ISearchItemTool =
            (ISearchItemTool)ServiceManager.Services.GetService(typeof(ISearchItemTool))!;
        PluginCore.Kitopia.IToastService = (IToastService)ServiceManager.Services.GetService(typeof(IToastService))!;
        PluginCore.Kitopia._i18n = CustomScenarioGloble._i18n;
       Load();
    }

    public static void EnablePluginByInfo(PluginInfo pluginInfoEx)
    {
        PluginManager.EnablePlugin.Add(pluginInfoEx.ToPlgString(),
            new Plugin(pluginInfoEx));
        ConfigManger.Config.EnabledPluginInfos.Add(pluginInfoEx);
        ConfigManger.Save();
        pluginInfoEx.IsEnabled = true;
        // Items.ResetBindings();
        CustomScenarioManger.ReCheck(true);
    }

    public static void UnloadPlugin(PluginInfo pluginInfoEx)
    {
        Plugin.UnloadByPluginInfo(pluginInfoEx.ToPlgString(), out var weakReference);
        PluginManager.EnablePlugin.Remove(pluginInfoEx.ToPlgString());
        ConfigManger.Config.EnabledPluginInfos.RemoveAll(e => e.ToPlgString()==pluginInfoEx.ToPlgString());
        ConfigManger.Save();
        pluginInfoEx.IsEnabled = false;

        for (int i = 0; i < 15; i++)
        {
            GC.Collect(2, GCCollectionMode.Aggressive);
            GC.WaitForPendingFinalizers();
            Task.Delay(10).Wait();
        }
        if (weakReference.IsAlive)
        {
            pluginInfoEx.UnloadFailed = true;
        }
            
        // Items.ResetBindings();
        CustomScenarioManger.LoadAll();
    }

    public static void Reload()
    {
        for (var i = 0; i < AllPluginInfos.Count; i++)
        {
            AllPluginInfos[i].Icon?.Dispose();
        }
        AllPluginInfos.Clear();
        Load();
    }
    public static void Load()
    {
        var pluginsDirectoryInfo = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + "plugins");
        if (!pluginsDirectoryInfo.Exists)
        {
            Log.Debug($"插件目录不存在创建{pluginsDirectoryInfo.FullName}");
            pluginsDirectoryInfo.Create();
        }

        foreach (var directoryInfo in pluginsDirectoryInfo.EnumerateDirectories())
        {
            if (File.Exists($"{directoryInfo.FullName}{Path.DirectorySeparatorChar}manifest.json"))
            {
                var readAllText = File.ReadAllText($"{directoryInfo.FullName}{Path.DirectorySeparatorChar}manifest.json");
                var serialize = JsonSerializer.Deserialize<PluginInfo>(readAllText);
                if (serialize != null)
                {
                    AllPluginInfos.Add(serialize);
                    serialize.FullPath= $"{directoryInfo.FullName}{Path.DirectorySeparatorChar}{serialize.Main}";
                    serialize.Path= $"{directoryInfo.FullName}{Path.DirectorySeparatorChar}";
                    serialize.IsEnabled = false;
                    if (ConfigManger.Config.EnabledPluginInfos.Any(e => e.ToPlgString()==serialize.ToPlgString()))
                    {
                        serialize.IsEnabled = true;
                        Task.Run(() =>
                        {
                            Plugin.Load(serialize);
                        }).Wait();
                    }
                }
            }
            
        }
    }

    public static void CheckUpdate()
    {
        for (var i = 0; i < AllPluginInfos.Count; i++)
        {
            try
            {
                var httpResponseMessage = MarketPageViewModel._httpClient.GetAsync($"https://www.ncserver.top:5111/api/plugin/{AllPluginInfos[i].Id}").Result;
                var httpContent = httpResponseMessage.Content.ReadAsStringAsync().Result;
                var deserializeObject = (JObject)JsonConvert.DeserializeObject(httpContent);
                AllPluginInfos[i].CanUpdata= deserializeObject["data"]["lastVersionId"].ToObject<int>() > AllPluginInfos[i].VersionId;
            }
            catch (Exception e)
            {
                AllPluginInfos[i].CanUpdata=false;
            }
        }
    }
}