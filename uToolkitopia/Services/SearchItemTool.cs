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

    public void OpenSearchItem(SearchViewItem searchViewItem)
    {
        WeakReferenceMessenger.Default.Send("a", "SearchWindowClose");
        Log.Debug("打开指定内容" + searchViewItem.OnlyKey);
        switch (searchViewItem.OnlyKey)
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
                switch (searchViewItem.FileType)
                {
                    case FileType.UWP应用:
                        Shell32.ShellExecute(IntPtr.Zero, "open", "explorer.exe",
                            $"shell:AppsFolder\\{searchViewItem.OnlyKey}", "",
                            ShowWindowCommand.SW_NORMAL);
                        break;
                    case FileType.自定义情景:
                        CustomScenarioManger.CustomScenarios
                            .FirstOrDefault((e) => $"CustomScenario:{e.UUID}" == searchViewItem.OnlyKey)?.Run();
                        break;
                    case FileType.便签:
                        ((ILabelWindowService)ServiceManager.Services.GetService(typeof(ILabelWindowService))!)
                            .Show(searchViewItem.OnlyKey);
                        break;
                    case FileType.自定义:
                        searchViewItem.Action?.Invoke(searchViewItem);
                        break;
                    default:
                        if (searchViewItem.Arguments == null)
                        {
                            Shell32.ShellExecute(IntPtr.Zero, "open", searchViewItem.OnlyKey, "", "",
                                ShowWindowCommand.SW_NORMAL);
                        }
                        else
                        {
                            Shell32.ShellExecute(IntPtr.Zero, "open", searchViewItem.OnlyKey, searchViewItem.Arguments,
                                "",
                                ShowWindowCommand.SW_SHOWNORMAL);
                        }

                        break;
                }

                switch (searchViewItem.FileType)
                {
                    case FileType.文件夹:
                    case FileType.应用程序:
                    case FileType.Word文档:
                    case FileType.PPT文档:
                    case FileType.Excel文档:
                    case FileType.PDF文档:
                    case FileType.图像:
                    case FileType.文件:
                    case FileType.自定义情景:
                    {
                        if (ConfigManger.Config.lastOpens.ContainsKey(searchViewItem.OnlyKey))
                        {
                            ConfigManger.Config.lastOpens[searchViewItem.OnlyKey]++;
                        }
                        else
                        {
                            ConfigManger.Config.lastOpens.Add(searchViewItem.OnlyKey, 1);
                        }

                        break;
                    }
                }

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