using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.SDKs;
using Core.SDKs.Config;
using Core.SDKs.Everything;
using Core.SDKs.Services;
using Core.SDKs.Tools;
using log4net;

namespace Core.ViewModel;

public partial class SearchWindowViewModel : ObservableRecipient
{
    private static readonly ILog log = LogManager.GetLogger(nameof(SearchWindowViewModel));
    private List<SearchViewItem> _collection = new(250); //存储本机所有软件

    [ObservableProperty] private bool? _everythingIsOk = true;

    [ObservableProperty] public BindingList<SearchViewItem> _items = new(); //搜索界面显示的软件
    private List<string> _names = new(250); //软件去重

    [ObservableProperty] private string? _search;


    [ObservableProperty] private int? _selectedIndex = -1;
    private Stopwatch _stopwatch = new();

    public SearchWindowViewModel()
    {
        ReloadApps();
        LoadLast();
    }

    public void ReloadApps()
    {
        AppTools.GetAllApps(_collection, _names);
    }

    public void CheckClipboard()
    {
        if (!ConfigManger.config.canReadClipboard)
        {
            if (ConfigManger.config.debugMode)
            {
                log.Debug("没有读取剪贴板授权");
            }

            if (Items.First().FileType == FileType.剪贴板图像)
            {
                Items.RemoveAt(0);
            }

            return;
        }

        if (((IClipboardService)ServiceManager.Services.GetService(typeof(IClipboardService))).IsBitmap())
        {
            if (ConfigManger.config.debugMode)
            {
                log.Debug("剪贴板有图像信息");
            }

            if (Items.Count((_items) => _items.FileType == FileType.剪贴板图像) >= 1)
            {
                return;
            }

            Items.Insert(0, new SearchViewItem()
            {
                FileName = "保存剪贴板图像?",
                FileType = FileType.剪贴板图像,
                IconSymbol = 0xE357,
                OnlyKey = "ClipboardImageData",
                Icon = null,
                IsVisible = true
            });
        }
        else if (Items.Count > 0 && Items.First().FileType == FileType.剪贴板图像)
        {
            if (ConfigManger.config.debugMode)
            {
                log.Debug("剪贴板没有图像信息,但第一项是图片信息删除");
            }

            Items.RemoveAt(0);
        }
    }

    public void LoadLast()
    {
        if (!string.IsNullOrEmpty(Search))
        {
            return;
        }

        if (ConfigManger.config.debugMode)
        {
            log.Debug("加载历史记录");
        }

        foreach (var searchViewItem in Items)
        {
            searchViewItem.Dispose();
        }

        Items.Clear();
        int limit = 0;
        //Items.RaiseListChangedEvents = false;
        if (ConfigManger.config!.alwayShows.Any())
        {
            if (ConfigManger.config.debugMode)
            {
                log.Debug("加载常驻");
            }

            foreach (var searchViewItem in ConfigManger.config.alwayShows.SelectMany(name1 => _collection.Where(
                         searchViewItem => searchViewItem.OnlyKey!.Equals(name1))))
            {
                var item = (SearchViewItem)searchViewItem.Clone();
                if (ConfigManger.config.debugMode)
                {
                    log.Debug("加载常驻:" + item.OnlyKey);
                }

                item.IsPined = true;
                Items.Add(item);
                limit++;
            }
        }

        if (ConfigManger.config!.lastOpens.Any())
        {
            if (ConfigManger.config.debugMode)
            {
                log.Debug("加载历史");
            }

            foreach (var searchViewItem in ConfigManger.config.lastOpens.SelectMany(name1 => _collection.Where(
                         searchViewItem => searchViewItem.OnlyKey!.Equals(name1))))
            {
                if (limit >= ConfigManger.config.maxHistory)
                {
                    if (ConfigManger.config.debugMode)
                    {
                        log.Debug("超过历史记录限制,当前" + limit);
                    }

                    break;
                }

                var item = (SearchViewItem)searchViewItem.Clone();
                if (ConfigManger.config.debugMode)
                {
                    log.Debug("加载历史:" + item.OnlyKey);
                }

                if (!Items.Any((e) => e.OnlyKey.Equals(item.OnlyKey)))
                {
                    Items.Add(item);
                    limit++;
                }
            }
        }

        //Items.RaiseListChangedEvents = true;
        CheckClipboard();
        OnItemsChanged(Items);

        GetItemsIcon();
    }


    private void GetItemsIcon()
    {
        if (ConfigManger.config.debugMode)
        {
            log.Debug("加载图标");
        }

        Task.Run(() =>
        {
            try
            {
                foreach (var t in Items)
                {
                    if (t.Icon != null) continue;
                    switch (t.FileType)
                    {
                        case FileType.文件夹:
                            t.Icon = (Icon)((IconTools)ServiceManager.Services!.GetService(typeof(IconTools))!)
                                .ExtractFromPath(t.DirectoryInfo!.FullName).Clone();
                            break;
                        case FileType.URL:
                            break;
                        case FileType.剪贴板图像:
                        {
                            break;
                        }
                        default:

                            t.Icon = (Icon)((IconTools)ServiceManager.Services!.GetService(typeof(IconTools))!)
                                .GetIcon(t.FileInfo!.FullName).Clone();
                            break;
                    }
                }
            }
            catch (Exception)
            {
            }
        });
    }

    partial void OnSearchChanged(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            LoadLast();
            return;
        }

        if (ConfigManger.config.debugMode)
        {
            log.Debug("搜索变更:" + value);
        }

        if (_stopwatch.IsRunning)
        {
            if (_stopwatch.ElapsedMilliseconds <= ConfigManger.config.inputSmoothingMilliseconds)
            {
                return;
            }
        }

        _stopwatch.Reset();
        _stopwatch.Start();
        foreach (var searchViewItem in Items)
        {
            searchViewItem.Dispose();
        }

        Items.Clear();


        value = value.ToLowerInvariant();
        if (ConfigManger.config.useEverything)
        {
            if (ConfigManger.config.debugMode)
            {
                log.Debug("everything检测");
            }

            if (IntPtr.Size == 8)
            {
                // 64-bit
                Everything64.Everything_SetMax(1);
                EverythingIsOk = Everything64.Everything_QueryW(true);
            }
            else
            {
                // 32-bit
                Everything32.Everything_SetMax(1);
                EverythingIsOk = Everything32.Everything_QueryW(true);
            }

            Tools.main(Items, value); //Everything文档检索
        }


        if (value.Contains("\\") || value.Contains("/"))
        {
            if (ConfigManger.config.debugMode)
            {
                log.Debug("检测路径");
            }

            if (Path.HasExtension(value) && File.Exists(value))
            {
                if (ConfigManger.config.debugMode)
                {
                    log.Debug("检测到文件路径");
                }

                Items.Add(new SearchViewItem()
                {
                    FileInfo = new FileInfo(value),
                    FileName = "打开文件:" + LnkTools.GetLocalizedName(value) + "?",
                    OnlyKey = value,
                    FileType = FileType.文件,
                    IsVisible = true
                });
            }
            else if (Directory.Exists(value))
            {
                if (ConfigManger.config.debugMode)
                {
                    log.Debug("检测到文件夹路径");
                }

                Items.Add(new SearchViewItem()
                {
                    DirectoryInfo = new DirectoryInfo(value),
                    FileName = "打开" + value.Split("\\").Last() + "?",
                    FileType = FileType.文件夹,
                    OnlyKey = value,
                    Icon = null,
                    IsVisible = true
                });
            }
        }


        // 使用LINQ语句来简化和优化筛选和排序逻辑，而不需要使用foreach循环和if判断
        // 根据给定的值，从集合中筛选出符合条件的SearchViewItem对象，并计算它们的权重
        var filtered = from item in _collection.AsParallel()
            let keys = item.Keys.Where(key => !string.IsNullOrEmpty(key)).AsParallel() // 排除空的键
            let weight = keys.Count(key => key.Contains(value)) * 2 // 统计包含给定值的键的数量
                         + keys.Count(key => key.StartsWith(value)) * 3 // 统计以给定值开头的键的数量，并乘以500
                         + keys.Count(key => key.Equals(value)) * 5 // 统计等于给定值的键的数量，并乘以1000
            where weight > 0 // 只选择权重为正的对象
            select new { Item = item, Weight = weight }; // 创建一个包含对象和权重属性的匿名类型

        // 按照权重降序排序筛选出的对象
        var sorted = filtered.OrderByDescending(x => x.Weight);

        // 将排序后的对象添加到Items集合中
        //Items.RaiseListChangedEvents = false;
        int count = 0; // 计数器变量
        int limit = 100; // 限制次数
        int nowIndex = 0;
        foreach (var x in sorted)
        {
            if (count >= limit) // 如果达到了限制
            {
                break; // 跳出循环
            }

            if (ConfigManger.config.lastOpens.Contains(x.Item.OnlyKey))
            {
                if (ConfigManger.config.debugMode)
                {
                    log.Debug("添加提高权重的搜索结果" + x.Item.OnlyKey);
                }

                Items.Insert(nowIndex, (SearchViewItem)x.Item.Clone()); // 添加元素
                nowIndex++;
                count++; // 计数器加一
            }
            else
            {
                if (ConfigManger.config.debugMode)
                {
                    log.Debug("添加搜索结果" + x.Item.OnlyKey);
                }

                Items.Add((SearchViewItem)x.Item.Clone()); // 添加元素
                count++; // 计数器加一
            }
        }
        //Items.RaiseListChangedEvents = true;


        if (Items.Count <= 0 && !(Path.HasExtension(value) && File.Exists(value)) && !Directory.Exists(value))
        {
            if (ConfigManger.config.debugMode)
            {
                log.Debug("无搜索项目,添加网页搜索");
            }


            Items.Add(new SearchViewItem()
            {
                Url = "https://www.bing.com/search?q=" + value,
                FileName = "在网页中搜索" + value,
                FileType = FileType.URL,
                OnlyKey = "https://www.bing.com/search?q=" + value,
                Icon = null,
                IconSymbol = 62555,
                IsVisible = true
            });
            if (value.Contains("."))
            {
                Items.Add(new SearchViewItem()
                {
                    Url = value,
                    FileName = "打开网页:" + value,
                    FileType = FileType.URL,
                    OnlyKey = value,
                    Icon = null,
                    IconSymbol = 62555,
                    IsVisible = true
                });
            }

            Items.Add(new SearchViewItem()
            {
                Url = value,
                FileName = "执行命令:" + value,
                FileType = FileType.命令,
                OnlyKey = value,
                Icon = null,
                IconSymbol = 61039,
                IsVisible = true
            });
        }

        GetItemsIcon();
    }


    [RelayCommand]
    public void OpenFile(object searchViewItem)
    {
        var item = (SearchViewItem)searchViewItem;
        log.Debug("打开指定内容" + item.OnlyKey);

        if (item.FileInfo != null)
        {
            ShellTools.ShellExecute(IntPtr.Zero, "open", item.FileInfo.FullName, "", "",
                ShellTools.ShowCommands.SW_SHOWNORMAL);
        }

        if (item.DirectoryInfo != null)
        {
            ShellTools.ShellExecute(IntPtr.Zero, "open", item.DirectoryInfo.FullName, "", "",
                ShellTools.ShowCommands.SW_SHOWNORMAL);
        }

        if (item.Url != null)
        {
            ShellTools.ShellExecute(IntPtr.Zero, "open", item.Url, "", "",
                ShellTools.ShowCommands.SW_SHOWNORMAL);
            Search = "";
            return;
        }

        if (item.FileType == FileType.剪贴板图像)
        {
            string fileName = ((IClipboardService)ServiceManager.Services.GetService(typeof(IClipboardService)))
                .saveBitmap();
            ShellTools.ShellExecute(IntPtr.Zero, "open", "explorer.exe", "/select," + fileName, "",
                ShellTools.ShowCommands.SW_SHOWNORMAL);
            return;
        }

        if (item.FileType == FileType.命令)
        {
            ShellTools.ShellExecute(IntPtr.Zero, "open", "cmd.exe", item.OnlyKey, "",
                ShellTools.ShowCommands.SW_SHOWNORMAL);
            return;
        }

        if (ConfigManger.config!.lastOpens.Contains(item.OnlyKey!))
            ConfigManger.config.lastOpens.Remove(item.OnlyKey!);
        ConfigManger.config.lastOpens.Insert(0, item.OnlyKey!);
        //if (ConfigManger.config.lastOpens.Count > ConfigManger.config.maxHistory) ConfigManger.config.lastOpens.RemoveAt(ConfigManger.config.lastOpens.Count-1);
        Search = "";
        ConfigManger.Save();
    }

    [RelayCommand]
    public void OpenFolder(object searchViewItem)
    {
        var item = (SearchViewItem)searchViewItem;
        log.Debug("打开指定内容文件夹" + item.OnlyKey);
        ShellTools.ShellExecute(IntPtr.Zero, "open", "explorer.exe", "/select," + item.FileInfo.FullName, "",
            ShellTools.ShowCommands.SW_SHOWNORMAL);

        if (ConfigManger.config!.lastOpens.Contains(item.OnlyKey!))
            ConfigManger.config.lastOpens.Remove(item.OnlyKey!);
        ConfigManger.config.lastOpens.Insert(0, item.OnlyKey!);
        //if (ConfigManger.config.lastOpens.Count > ConfigManger.config.maxHistory) ConfigManger.config.lastOpens.RemoveAt(ConfigManger.config.lastOpens.Count-1);
        Search = "";
        ConfigManger.Save();
    }

    [RelayCommand]
    public void RunAsAdmin(object searchViewItem)
    {
        var item = (SearchViewItem)searchViewItem;
        log.Debug("以管理员身份打开指定内容" + item.OnlyKey);
        ShellTools.ShellExecute(IntPtr.Zero, "runas", item.OnlyKey, "", "",
            ShellTools.ShowCommands.SW_SHOWNORMAL);

        if (ConfigManger.config!.lastOpens.Contains(item.OnlyKey!))
            ConfigManger.config.lastOpens.Remove(item.OnlyKey!);
        ConfigManger.config.lastOpens.Insert(0, item.OnlyKey!);
        //if (ConfigManger.config.lastOpens.Count > ConfigManger.config.maxHistory) ConfigManger.config.lastOpens.RemoveAt(ConfigManger.config.lastOpens.Count-1);
        Search = "";
        ConfigManger.Save();
    }

    [RelayCommand]
    public void Star(object searchViewItem)
    {
        var item = (SearchViewItem)searchViewItem;
        log.Debug("添加/移除收藏" + item.OnlyKey);
        int index = Items.IndexOf(item);
        Items[index].IsStared = !Items[index].IsStared;
        Items.ResetItem(index);
        if (item.FileInfo is not null)
        {
            if (ConfigManger.config!.customCollections.Contains(item.FileInfo.FullName!))
                ConfigManger.config.customCollections.Remove(item.FileInfo.FullName!);
            if (item.IsStared) //收藏操作
            {
                AppTools.AppSolverA(_collection, _names, item.FileInfo.FullName, true);
                ConfigManger.config.customCollections.Insert(0, item.FileInfo.FullName!);
            }
            else
            {
                _collection.Remove(_collection.Find(e =>
                    e.FileInfo != null && e.FileInfo.FullName.Equals(item.FileInfo.FullName))!);
            }
        }
        else if (item.DirectoryInfo is not null)
        {
            if (ConfigManger.config!.customCollections.Contains(item.DirectoryInfo.FullName!))
                ConfigManger.config.customCollections.Remove(item.DirectoryInfo.FullName!);
            if (item.IsStared) //收藏操作
            {
                AppTools.AppSolverA(_collection, _names, item.DirectoryInfo.FullName, true);
                ConfigManger.config.customCollections.Insert(0, item.DirectoryInfo.FullName!);
            }
            else
            {
                _collection.Remove(_collection.Find(e =>
                    e.FileInfo != null && e.FileInfo.FullName.Equals(item.DirectoryInfo.FullName))!);
            }
        }

        //GetItemsIcon();
        ConfigManger.Save();
    }

    [RelayCommand]
    public void Pin(object searchViewItem)
    {
        var item = (SearchViewItem)searchViewItem;
        log.Debug("添加常驻" + item.OnlyKey);
        int index = Items.IndexOf(item);
        Items[index].IsPined = !Items[index].IsPined;
        //Items.ResetItem(index);
        if (ConfigManger.config!.alwayShows.Contains(item.OnlyKey!))
            ConfigManger.config.alwayShows.Remove(item.OnlyKey!);
        if (item.IsPined) //收藏操作
        {
            ConfigManger.config.alwayShows.Insert(0, item.OnlyKey!);
        }

        ConfigManger.Save();
    }

    [RelayCommand]
    public void OpenFolderInTerminal(object searchViewItem)
    {
        var item = (SearchViewItem)searchViewItem;
        log.Debug("打开指定内容在终端中" + item.OnlyKey);
        ProcessStartInfo startInfo = new ProcessStartInfo();
        startInfo.FileName = "cmd.exe";

        if (item.FileInfo != null)
        {
            startInfo.WorkingDirectory = item.FileInfo.DirectoryName;
        }

        if (item.DirectoryInfo != null)
        {
            startInfo.WorkingDirectory = item.DirectoryInfo.FullName;
        }

        Process.Start(startInfo);

        if (ConfigManger.config!.lastOpens.Contains(item.OnlyKey!))
            ConfigManger.config.lastOpens.Remove(item.OnlyKey!);
        ConfigManger.config.lastOpens.Insert(0, item.OnlyKey!);
        //if (ConfigManger.config.lastOpens.Count > ConfigManger.config.maxHistory) ConfigManger.config.lastOpens.RemoveAt(ConfigManger.config.lastOpens.Count-1);
        Search = "";
        ConfigManger.Save();
    }
}