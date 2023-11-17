using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using CommunityToolkit.Mvvm.Messaging;
using Core.SDKs.CustomType;
using Core.SDKs.HotKey;
using Core.SDKs.Services;
using Core.SDKs.Services.Config;
using Core.SDKs.Services.Plugin;
using Core.ViewModel;
using log4net;
using Newtonsoft.Json;
using PluginCore;

namespace Core.SDKs.CustomScenario;

public static class CustomScenarioManger
{
    public static ObservableCollection<SDKs.CustomScenario.CustomScenario> CustomScenarios = new();

    public static ObservableDictionary<string, string> Triggers = new()
    {
        { "Kitopia_SoftwareStarted", "Kitopia程序启动时" },
        { "Kitopia_SoftwareShutdown", "Kitopia程序关闭时" }
    };

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
        new Task(() =>
        {
            while (!PluginManager.isInitialized)
            {
                Thread.Sleep(100);
            }

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
                    var deserializeObject = JsonConvert.DeserializeObject<SDKs.CustomScenario.CustomScenario>(json)!;


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


                    deserializeObject.IsRunning = false;
                    CustomScenarios.Add(deserializeObject);
                }
                catch (Exception e1)
                {
                    // Log.Error(e1);
                    Log.Error($"情景文件\"{fileInfo.FullName}\"加载失败");
                    var pluginName = ((CustomScenarioLoadFromJsonException)e1).PluginName.Split("_");
                    var deserializeObject = JsonConvert.DeserializeObject<SDKs.CustomScenario.CustomScenario>(json,
                        new JsonSerializerSettings()
                        {
                            Error = (sender, args) =>
                            {
                                // 忽略错误并继续反序列化
                                args.ErrorContext.Handled = true;
                            }
                        })!;
                    ((IToastService)ServiceManager.Services!.GetService(typeof(IToastService))!).ShowMessageBoxW(
                        $"自定义情景\"{deserializeObject.Name}\"加载失败",
                        $"对应文件\n{fileInfo.FullName}\n情景所需的插件不存在\n需要来自作者{pluginName[0]}的插件{pluginName[1]}", new
                            ShowMessageContent("我知道了", null, "尝试在市场中自动安装", () =>
                            {
                                System.Windows.MessageBox.Show("未实现");
                            }, null, null));
                }
            }

            WeakReferenceMessenger.Default.Send("Kitopia_SoftwareStarted", "CustomScenarioTrigger");
        }).Start();
    }

    public static void Save(SDKs.CustomScenario.CustomScenario scenario)
    {
        if (!CustomScenarios.Contains(scenario))
        {
            CustomScenarios.Add(scenario);
        }

        if (scenario.ExecutionManual)
        {
            if (scenario.Keys.Any())
            {
                var viewItem1 = new SearchViewItem()
                {
                    FileName = "执行自定义情景:" + scenario.Name,
                    FileType = FileType.自定义情景,
                    OnlyKey = $"CustomScenario:{scenario.UUID}",
                    Keys = scenario.Keys.ToHashSet(),
                    Icon = null,
                    IconSymbol = 0xF78B,
                    IsVisible = true
                };
                ((SearchWindowViewModel)ServiceManager.Services.GetService(typeof(SearchWindowViewModel))!)
                    ._collection.TryAdd($"CustomScenario:{scenario.UUID}", viewItem1);
            }
            else
            {
                ((SearchWindowViewModel)ServiceManager.Services.GetService(typeof(SearchWindowViewModel))!)
                    ._collection.Remove($"CustomScenario:{scenario.UUID}");
            }
        }

        var configF = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + $"customScenarios\\{scenario.UUID}.json");

        var setting = new JsonSerializerSettings();
        setting.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
        setting.ReferenceLoopHandling = ReferenceLoopHandling.Serialize;
        setting.TypeNameHandling = TypeNameHandling.None;
        setting.Formatting = Formatting.Indented;

        File.WriteAllText(configF.FullName, JsonConvert.SerializeObject(scenario, setting));
    }

    public static void Remove(SDKs.CustomScenario.CustomScenario scenario)
    {
        if (CustomScenarios.Contains(scenario))
        {
            CustomScenarios.Remove(scenario);
            List<HotKeyModel> toRemove = new();
            foreach (var item in ConfigManger.Config.hotKeys)
            {
                if (item.MainName == "Kitopia情景")
                {
                    if (CustomScenarios.All(e => e.UUID != item.Name!.Split("_")[0]))
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

            ConfigManger.Save();
            File.Delete(AppDomain.CurrentDomain.BaseDirectory + $"customScenarios\\{scenario.UUID}.json");
            scenario.Dispose();
        }
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