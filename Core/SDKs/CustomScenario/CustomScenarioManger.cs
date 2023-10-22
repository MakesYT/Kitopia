using System.Collections.ObjectModel;
using System.IO;
using Core.SDKs.CustomScenario;
using Core.SDKs.Services.Plugin;
using Core.ViewModel;
using log4net;
using Newtonsoft.Json;

namespace Core.SDKs.Services.Config;

public partial class CustomScenarioManger
{
    public static ObservableCollection<SDKs.CustomScenario.CustomScenario> CustomScenarios = new();
    private static readonly ILog Log = LogManager.GetLogger(nameof(CustomScenarioManger));

    public static void Init()
    {
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
                    var deserializeObject = JsonConvert.DeserializeObject<CustomScenario.CustomScenario>(json)!;
                    foreach (var value in deserializeObject.AutoTriggerType.Where(value => value.IsUsed))
                    {
                        switch (value.AutoTriggerType)
                        {
                            case AutoTriggerType.系统关闭时:
                            {
                                CustomScenarioExecutorManager.SystemShutdown.AddCustomScenario(deserializeObject);
                                break;
                            }
                            case AutoTriggerType.软件关闭时:
                            {
                                CustomScenarioExecutorManager.SoftwareShutdown.AddCustomScenario(deserializeObject);
                                break;
                            }
                            case AutoTriggerType.软件启动时:
                            {
                                CustomScenarioExecutorManager.SoftwareStarted.AddCustomScenario(deserializeObject);
                                break;
                            }
                            case AutoTriggerType.Custom:
                            {
                                if (CustomScenarioExecutorManager.CustomExecutors.ContainsKey(
                                        value.AutoTriggerTypeFrom))
                                {
                                    CustomScenarioExecutorManager.CustomExecutors[value.AutoTriggerTypeFrom]
                                        .AddCustomScenario(deserializeObject);
                                }

                                break;
                            }
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }

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
                    var deserializeObject = JsonConvert.DeserializeObject<CustomScenario.CustomScenario>(json,
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
        }).Start();
    }

    public static void Save(SDKs.CustomScenario.CustomScenario scenario)
    {
        if (scenario.UUID is null)
        {
            var s = Guid.NewGuid().ToString();
            scenario.UUID = s;
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