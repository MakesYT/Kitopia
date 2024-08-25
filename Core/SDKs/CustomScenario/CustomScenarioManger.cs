﻿using System.Collections.ObjectModel;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using CommunityToolkit.Mvvm.Messaging;
using Core.SDKs.HotKey;
using Core.SDKs.Services;
using Core.SDKs.Services.Config;
using Core.SDKs.Services.Plugin;
using Core.ViewModel;
using log4net;
using Microsoft.Extensions.DependencyInjection;
using Pinyin.NET;
using PluginCore;

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
                        ThreadPool.QueueUserWorkItem(o => { customScenario.Run(onExit: true); });
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
        CustomScenarios.Clear();
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
            var deserializeObject = JsonSerializer.Deserialize<CustomScenario>(json, ConfigManger.DefaultOptions);

            deserializeObject.OnDeserialized();

            foreach (var node in deserializeObject.nodes)
            {
                if (!node.ScenarioMethod.IsFromPlugin)
                {
                    continue;
                }

            }

            deserializeObject.HasInit = true;
            deserializeObject.IsRunning = false;

            void ConnectorInit(ConnectorItem connectorItem)
            {
                if (connectorItem.RealType == typeof(NodeConnectorClass))
                {
                    return;
                }

                if (connectorItem.InputObject is null)
                {
                    return;
                }

                foreach (var keyValuePair in PluginManager.EnablePlugin)
                {
                    if (keyValuePair.Value.GetType(connectorItem.RealType) is { } a)
                    {
                        if (a.BaseType.FullName == "System.Enum")
                        {
                            connectorItem.InputObject = Enum.Parse(a, connectorItem.InputObject.ToString());
                            break;
                        }

                        connectorItem.InputObject = Convert.ChangeType(connectorItem.InputObject, a);
                        break;
                    }
                }
            }

            foreach (var deserializeObjectNode in deserializeObject.nodes)
            {
                foreach (var connectorItem in deserializeObjectNode.Input)
                {
                    ConnectorInit(connectorItem);
                }

                foreach (var connectorItem in deserializeObjectNode.Output)
                {
                    ConnectorInit(connectorItem);
                }
            }

            CustomScenarios.Add(deserializeObject);
        }
        catch (CustomScenarioLoadFromJsonException e1)
        {
            // Log.Error(e1);
            var Name = string.Empty;
            Log.Error($"情景文件\"{fileInfo.FullName}\"加载失败,内部异常");
            var utf8JsonReader = new Utf8JsonReader(File.ReadAllBytes(fileInfo.FullName));
            utf8JsonReader.Read();
            while (utf8JsonReader.Read())
            {
                if (utf8JsonReader.TokenType==JsonTokenType.StartObject)
                {
                    utf8JsonReader.Skip();
                }

                if (utf8JsonReader.TokenType==JsonTokenType.PropertyName)
                {
                    if (utf8JsonReader.GetString()==nameof(CustomScenario.Name))
                    {
                        utf8JsonReader.Read();
                        Name = utf8JsonReader.GetString();
                        break;
                    }
                }
            }
            switch (e1.FailedType)
            {
                case CustomScenarioLoadFromJsonFailedType.插件未找到:
                {
                
                    var content = $"对应文件\n{fileInfo.FullName}\n情景所需的插件不存在\n需要插件\"{e1.PluginName}\"";
                    var dialog = new DialogContent()
                    {
                        Title = $"自定义情景\"{Name}\"加载失败",
                        Content = content,
                        PrimaryButtonText = "尝试在市场中自动安装",
                        CloseButtonText = "我知道了",
                        PrimaryAction =async () =>
                        {
                            var onlinePluginInfo = await PluginManager.GetOnlinePluginInfo(int.Parse(e1.PluginName.Split("_")[0]));
                            if (onlinePluginInfo is null)
                            {
                                ServiceManager.Services.GetService<IToastService>().Show("自动下载插件失败",$"未找到ID:{e1.PluginName.Split("_")[0]}的插件");
                                return;
                            }

                            var downloadPluginOnline = await PluginManager.DownloadPluginOnline(onlinePluginInfo);
                                
                            if (downloadPluginOnline)
                            {
                                ServiceManager.Services.GetService<IToastService>().Show("自动下载插件成功",$"已自动下载并启用{onlinePluginInfo.Name}");
                            }
                            else
                            {
                                ServiceManager.Services.GetService<IToastService>().Show("自动下载插件失败",$"下载ID:{e1.PluginName.Split("_")[0]}的插件时遇到错误");
                            }
                        }
                    };
                    ((IContentDialog)ServiceManager.Services!.GetService(typeof(IContentDialog))!).ShowDialogAsync(null,
                        dialog);
                    break;
                }
                case CustomScenarioLoadFromJsonFailedType.插件未启用:
                {
                    var pluginByPlgStr = PluginManager.GetPluginByPlgStr(e1.PluginName);
                    
                    var content = $"对应文件\n{fileInfo.FullName}\n情景所需的插件未启用\n需要插件{pluginByPlgStr.Name}(ID:{pluginByPlgStr.Id})";
                   
                    var dialog = new DialogContent()
                    {
                        Title = $"自定义情景\"{Name}\"加载失败",
                        Content = content,
                        PrimaryButtonText = "启用该插件",
                        CloseButtonText = "我知道了",
                        PrimaryAction = () =>
                        {
                            PluginManager.EnablePluginByInfo(pluginByPlgStr);
                        }
                    };
                    ((IContentDialog)ServiceManager.Services!.GetService(typeof(IContentDialog))!).ShowDialogAsync(null,
                        dialog);
                    break;
                }
                case CustomScenarioLoadFromJsonFailedType.方法未找到:
                {
                    break;
                }
                case CustomScenarioLoadFromJsonFailedType.类未找到:
                {
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        catch (Exception e)
        {
            var content = $"情景文件\n{fileInfo.FullName}\n加载失败疑似文件已损坏";
            var dialog = new DialogContent()
            {
                Title = $"自定义情景\"{fileInfo.Name}\"加载失败",
                Content = content,
                CloseButtonText = "我知道了",
                PrimaryAction = () => { }
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
            if (scenario.RunHotKey.SelectKey != EKey.未设置)
            {
                if (HotKeyManager.HotKetImpl.Add(scenario.RunHotKey, e => scenario.Run()))
                {
                    ServiceManager.Services.GetService<IContentDialog>().ShowDialogAsync(null, new DialogContent()
                    {
                        Title = $"快捷键{scenario.RunHotKey.SignName}设置失败",
                        Content = "请重新设置快捷键，按键与系统其他程序冲突",
                        CloseButtonText = "关闭"
                    });
                }
            }

            if (scenario.StopHotKey.SelectKey != EKey.未设置)
            {
                if (HotKeyManager.HotKetImpl.Add(scenario.StopHotKey, e => scenario.Stop()))
                {
                    ServiceManager.Services.GetService<IContentDialog>().ShowDialogAsync(null, new DialogContent()
                    {
                        Title = $"快捷键{scenario.StopHotKey.SignName}设置失败",
                        Content = "请重新设置快捷键，按键与系统其他程序冲突",
                        CloseButtonText = "关闭"
                    });
                }
            }
        }


        var onlyKey = $"{nameof(CustomScenario)}:{scenario.UUID}";

        var keys = new List<List<string>>();
        foreach (var key in scenario.Keys)
        {
            keys.Add([key]);
        }

        keys.AddRange(ServiceManager.Services.GetService<IAppToolService>()
            .GetPinyin(scenario.Name)
            .Keys);
        var viewItem1 = new SearchViewItem()
        {
            ItemDisplayName = "执行自定义情景:" + scenario.Name,
            FileType = FileType.自定义情景,
            OnlyKey = onlyKey,
            PinyinItem = new PinyinItem()
            {
                Keys = keys
            },
            Icon = null,
            IconSymbol = 0xF78B,
            IsVisible = true
        };
        ((SearchWindowViewModel)ServiceManager.Services.GetService(typeof(SearchWindowViewModel))!)
            ._collection.TryAdd(onlyKey, viewItem1);


        var configF = new FileInfo(AppDomain.CurrentDomain.BaseDirectory +
                                   $"customScenarios{Path.DirectorySeparatorChar}{scenario.UUID}.json");


        var j = JsonSerializer.Serialize(scenario, ConfigManger.DefaultOptions);
        File.WriteAllText(configF.FullName, j);
    }

    public static void Remove(CustomScenario scenario, bool deleteFile = true)
    {
        if (CustomScenarios.Contains(scenario))
        {
            CustomScenarios.Remove(scenario);
        }

        HotKeyManager.HotKetImpl.Del(scenario.RunHotKey.UUID);
        HotKeyManager.HotKetImpl.Del(scenario.StopHotKey.UUID);
        ((SearchWindowViewModel)ServiceManager.Services.GetService(typeof(SearchWindowViewModel))!)
            ._collection.TryRemove($"{nameof(CustomScenario)}:{scenario.UUID}", out _);
        ConfigManger.Save();
        if (deleteFile)
        {
            File.Delete(
                $"{AppDomain.CurrentDomain.BaseDirectory}customScenarios{Path.DirectorySeparatorChar}{scenario.UUID}.json");
        }

        scenario.Dispose();
    }

    public static void UnloadByPlugStr(string plugStr)
    {
        for (int i = CustomScenarios.Count - 1; i >= 0; i--)
        {
            if (CustomScenarios[i]
                .PluginUsedCount.ContainsKey(plugStr))
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
        DirectoryInfo info = new DirectoryInfo($"{AppDomain.CurrentDomain.BaseDirectory}customScenarios");
        foreach (var fileInfo in info.GetFiles())
        {
            if (CustomScenarios.All(e => e.UUID != fileInfo.Name))
            {
                Load(fileInfo);
            }
        }
    }
}