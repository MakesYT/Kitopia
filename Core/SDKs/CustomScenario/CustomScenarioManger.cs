using System.Collections.ObjectModel;
using System.Text;
using System.Text.Json;
using System.Xml;
using CommunityToolkit.Mvvm.Messaging;
using Core.SDKs.HotKey;
using Core.SDKs.Services;
using Core.SDKs.Services.Config;
using Core.SDKs.Services.Plugin;
using Core.ViewModel;
using log4net;
using PluginCore;
using PluginCore.Config;

namespace Core.SDKs.CustomScenario;

public static class CustomScenarioManger
{
    public static ObservableCollection<CustomScenario> CustomScenarios = new();

    private static readonly ILog Log = LogManager.GetLogger(nameof(CustomScenarioManger));

    public static void Init()
    {
        WeakReferenceMessenger.Default.Register<string, string>("null", "CustomScenarioTrigger", (_, e) =>
        {
            //设置当前线程最高优先级
            Thread.CurrentThread.Priority = ThreadPriority.Highest;
            StringBuilder sb = new();
            sb.AppendLine($"触发器{e}被触发\n以下情景被执行:");
            foreach (var customScenario in CustomScenarios)
            {
                if (customScenario.AutoTriggers.Contains(e))
                {
                    sb.AppendLine(customScenario.Name);
                    if (e == "Kitopia_SoftwareShutdown")
                    {
                        ThreadPool.QueueUserWorkItem(o =>
                        {
                            customScenario.Run(onExit: true);
                        });
                    }
                    else
                    {
                        customScenario.Run();
                    }
                }
            }

            Log.Info(sb.ToString());
            ((IToastService)ServiceManager.Services.GetService(typeof(IToastService))!).Show("情景",
                sb.ToString());
        });


        if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "customScenarios"))
        {
            Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "customScenarios");
        }

        var info = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + "customScenarios");
        foreach (var fileInfo in info.GetFiles())
        {
            Load(fileInfo);
        }

        WeakReferenceMessenger.Default.Send("Kitopia_SoftwareStarted", "CustomScenarioTrigger");
    }

    public static void LoadAll()
    {
        var info = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + "customScenarios");
        foreach (var fileInfo in info.GetFiles())
        {
            Load(fileInfo);
        }
    }

    public static void Load(FileInfo fileInfo)
    {
        var fileInfoName = fileInfo.Name.Replace(".json", "");
        if (CustomScenarios.Any((e => e.UUID == fileInfoName)))
        {
            return;
        }

        var json = File.ReadAllText(fileInfo.FullName);
        try
        {
            var deserializeObject = JsonSerializer.Deserialize<CustomScenario>(json,ConfigManger.DefaultOptions);


            foreach (var node in deserializeObject.nodes)
            {
                var nodePlugin = node.Plugin;
                switch (nodePlugin)
                {
                    case null:
                    case "Kitopia":
                        continue;
                }

                if (!PluginOverall.CustomScenarioNodeMethods.TryGetValue(nodePlugin, out var method))
                {
                    throw new CustomScenarioLoadFromJsonException(nodePlugin, node.MerthodName);
                }

                if (method.ContainsKey(node.MerthodName))
                {
                    continue;
                }

                throw new CustomScenarioLoadFromJsonException(nodePlugin, node.MerthodName);
            }

            //

            deserializeObject.HasInit = true;
            deserializeObject.IsRunning = false;
            CustomScenarios.Add(deserializeObject);
        }
        catch (Exception e1)
        {
            // Log.Error(e1);
            Log.Error($"情景文件\"{fileInfo.FullName}\"加载失败");
            var pluginName = ((CustomScenarioLoadFromJsonException)e1).PluginName.Split("_");
            
            
            var deserializeObject = JsonSerializer.Deserialize<CustomScenario>(json,ConfigManger.DefaultOptions)!;
            deserializeObject.HasInit = false;
            deserializeObject.IsRunning = false;

            var content = $"对应文件\n{fileInfo.FullName}\n情景所需的插件不存在\n需要来自作者{pluginName[0]}的插件{pluginName[1]}";
            deserializeObject.InitError = content;
            CustomScenarios.Add(deserializeObject);
            var dialog = new DialogContent()
            {
                Title = $"自定义情景\"{deserializeObject.Name}\"加载失败",
                Content = content,
                PrimaryButtonText = "尝试在市场中自动安装",
                CloseButtonText = "我知道了",
                PrimaryAction = () =>
                {
                }
            };
            ((IContentDialog)ServiceManager.Services!.GetService(typeof(IContentDialog))!).ShowDialogAsync(null,
                dialog);
        }
    }

    public static void Save(CustomScenario scenario)
    {
        if (!CustomScenarios.Contains(scenario))
        {
            CustomScenarios.Add(scenario);
            HotKeyManager.HotKeys.Add(scenario.RunHotKey);
            HotKeyManager.HotKeys.Add(scenario.StopHotKey);
        }

        if (scenario.ExecutionManual)
        {
            var onlyKey = $"{nameof(CustomScenario)}:{scenario.UUID}";
            if (scenario.Keys.Any())
            {
                var keys = new List<IEnumerable<string>>();
                foreach (var key in scenario.Keys)
                {
                    keys.Add([key]);
                }
                var viewItem1 = new SearchViewItem()
                {
                    ItemDisplayName = "执行自定义情景:" + scenario.Name,
                    FileType = FileType.自定义情景,
                    OnlyKey = onlyKey,
                    Keys = keys.AsReadOnly(),
                    Icon = null,
                    IconSymbol = 0xF78B,
                    IsVisible = true
                };
                ((SearchWindowViewModel)ServiceManager.Services.GetService(typeof(SearchWindowViewModel))!)
                    ._collection.TryAdd(onlyKey, viewItem1);
            }
            else
            {
                ((SearchWindowViewModel)ServiceManager.Services.GetService(typeof(SearchWindowViewModel))!)
                    ._collection.TryRemove(onlyKey,out _);
            }
        }

        var configF = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + $"customScenarios{Path.DirectorySeparatorChar}{scenario.UUID}.json");
        
       
        var j =JsonSerializer.Serialize(scenario, ConfigManger.DefaultOptions);
        File.WriteAllText(configF.FullName, j);
    }

    public static void Remove(CustomScenario scenario, bool deleteFile = true)
    {
        if (CustomScenarios.Contains(scenario))
        {
            CustomScenarios.Remove(scenario);
        }

        List<HotKeyModel> toRemove = new();
        foreach (var item in HotKeyManager.HotKeys)
        {
            if (item.MainName == "Kitopia情景")
            {
                if (scenario.UUID==item.Name!.Split("_")[0])
                {
                    toRemove.Add(item);
                }
            }
        }

        foreach (var hotKeyModel in toRemove)
        {
            ((IHotKeyEditor)ServiceManager.Services.GetService(typeof(IHotKeyEditor))!).RemoveByHotKeyModel(
                hotKeyModel);
        }

        toRemove = null;
        ((SearchWindowViewModel)ServiceManager.Services.GetService(typeof(SearchWindowViewModel))!)
            ._collection.TryRemove($"{nameof(CustomScenario)}:{scenario.UUID}",out _);
        ConfigManger.Save();
        if (deleteFile)
        {
            File.Delete($"{AppDomain.CurrentDomain.BaseDirectory}customScenarios{Path.DirectorySeparatorChar}{scenario.UUID}.json");
        }

        scenario.Dispose();
    }

    public static void UnloadByPlugStr(string plugStr)
    {
        for (int i = CustomScenarios.Count - 1; i >= 0; i--)
        {
            if (CustomScenarios[i]._plugs.ContainsKey(plugStr))
            {
                var customScenario = CustomScenarios[i];
                CustomScenarios.RemoveAt(i);
                Remove(customScenario, false);
                customScenario = null;
            }
        }
    }

    public static void Reload(CustomScenario scenario)
    {
        Remove(scenario, false);
        var configF = new FileInfo(
            $"{AppDomain.CurrentDomain.BaseDirectory}customScenarios{Path.DirectorySeparatorChar}{scenario.UUID}.json");
        if (configF.Exists)
        {
            Load(configF);
        }
    }

    public static void ReCheck(bool onlyError = true)
    {
        var toRemove = new List<CustomScenario>();
        if (onlyError)
        {
            foreach (var customScenario in CustomScenarios.Where(e => e.HasInit == false))
            {
                toRemove.Add(customScenario);
            }
        }
        else
        {
            foreach (var customScenario in CustomScenarios)
            {
                toRemove.Add(customScenario);
            }
        }

        foreach (var customScenario in toRemove)
        {
            if (customScenario.IsRunning)
            {
                customScenario.Stop();
            }

            Remove(customScenario, false);
            var configF = new FileInfo(
                $"{AppDomain.CurrentDomain.BaseDirectory}customScenarios{Path.DirectorySeparatorChar}{customScenario.UUID}.json");
            if (configF.Exists)
            {
                Load(configF);
            }
        }
    }
}