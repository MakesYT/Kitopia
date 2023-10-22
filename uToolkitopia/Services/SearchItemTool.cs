using System;
using System.Linq;
using CommunityToolkit.Mvvm.Messaging;
using Core.SDKs;
using Core.SDKs.Services;
using Core.SDKs.Services.Config;
using Core.ViewModel;
using log4net;
using PluginCore;
using Vanara.PInvoke;

namespace Kitopia.Services;

public class SearchItemTool : ISearchItemTool
{
    private static readonly ILog Log = LogManager.GetLogger(nameof(SearchItemTool));

    public void OpenSearchItem(SearchViewItem item)
    {
        WeakReferenceMessenger.Default.Send("a", "SearchWindowClose");
        Log.Debug("打开指定内容" + item.OnlyKey);
        switch (item.OnlyKey)
        {
            case "ClipboardImageData":
            {
                var fileName = ((IClipboardService)ServiceManager.Services!.GetService(typeof(IClipboardService))!)
                    .saveBitmap();
                if (string.IsNullOrEmpty(fileName))
                {
                    Log.Error("剪贴板图片保存失败");
                    ((IToastService)ServiceManager.Services.GetService(typeof(IToastService))!).Show("剪贴板图片保存失败");
                    return;
                }

                Shell32.ShellExecute(IntPtr.Zero, "open", "explorer.exe", "/select," + fileName, "",
                    ShowWindowCommand.SW_NORMAL);
                return;
            }
            case "Math": break;
            default:
            {
                switch (item.FileType)
                {
                    case FileType.UWP应用:
                        Shell32.ShellExecute(IntPtr.Zero, "open", "explorer.exe",
                            $"shell:AppsFolder\\{item.OnlyKey}", "",
                            ShowWindowCommand.SW_NORMAL);
                        break;
                    case FileType.自定义情景:
                        CustomScenarioManger.CustomScenarios
                            .FirstOrDefault((e) => $"CustomScenario:{e.UUID}" == item.OnlyKey)?.Run();
                        break;
                    case FileType.便签:
                        ((ILabelWindowService)ServiceManager.Services.GetService(typeof(ILabelWindowService))!)
                            .Show(item.OnlyKey);
                        break;
                    case FileType.自定义:
                        item.Action?.Invoke(item);
                        break;
                    default:
                        Shell32.ShellExecute(IntPtr.Zero, "open", item.OnlyKey, "", "",
                            ShowWindowCommand.SW_NORMAL);
                        break;
                }

                switch (item.FileType)
                {
                    case FileType.文件夹:
                    case FileType.应用程序:
                    case FileType.Word文档:
                    case FileType.PPT文档:
                    case FileType.Excel文档:
                    case FileType.PDF文档:
                    case FileType.图像:
                    case FileType.文件:
                    {
                        if (ConfigManger.Config.lastOpens.ContainsKey(item.OnlyKey))
                        {
                            ConfigManger.Config.lastOpens[item.OnlyKey]++;
                        }
                        else
                        {
                            ConfigManger.Config.lastOpens.Add(item.OnlyKey, 1);
                        }

                        break;
                    }
                }


                //if (ConfigManger.config.lastOpens.Count > ConfigManger.config.maxHistory) ConfigManger.config.lastOpens.RemoveAt(ConfigManger.config.lastOpens.Count-1);

                ConfigManger.Save();
                return;
            }
        }
    }

    public void OpenSearchItemByOnlyKey(string onlyKey)
    {
        if (((SearchWindowViewModel)ServiceManager.Services!.GetService(typeof(SearchWindowViewModel))!)._collection
            .TryGetValue(onlyKey, out var item))
        {
            OpenSearchItem(item);
        }
    }
}