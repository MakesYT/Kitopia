#region

using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Core.SDKs.Everything;
using Core.SDKs.Services;
using Core.SDKs.Services.Config;
using Core.SDKs.Services.Plugin;
using Core.SDKs.Tools;
using log4net;
using Microsoft.Extensions.DependencyInjection;
using PluginCore;
using Vanara.PInvoke;

#endregion

namespace Core.ViewModel;

public partial class SearchWindowViewModel : ObservableRecipient
{
    private static readonly ILog Log = LogManager.GetLogger(nameof(SearchWindowViewModel));
    private static readonly List<SearchViewItem> TempList = new(1000);
    public readonly Dictionary<string, SearchViewItem> _collection = new(400); //存储本机所有软件
    private readonly TaskScheduler _scheduler = TaskScheduler.FromCurrentSynchronizationContext();

    private readonly DelayAction _searchDelayAction = new();

    [ObservableProperty] private bool? _everythingIsOk = true;

    [ObservableProperty] private ObservableCollection<SearchViewItem> _items = new(TempList); //搜索界面显示的软件


    [ObservableProperty] private string? _search;


    [ObservableProperty] private int? _selectedIndex = -1;


    private bool nowInSelectMode = false;
    private Action<SearchViewItem>? selectAction;

    public SearchWindowViewModel()
    {
        ReloadApps(true);
        LoadLast();
    }

    public async Task AddCollection(string search)
    {
        await AppTools.AppSolverA(_collection, search, false);
    }

    public void ReloadApps(bool logging = false)
    {
        AppTools.DelNullFile(_collection);
        AppTools.GetAllApps(_collection, logging);
        if (ConfigManger.Config.useEverything)
        {
            Log.Debug("everything检测");


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

            if (!EverythingIsOk.Value)
            {
                AppTools.AutoStartEverything(_collection, () =>
                {
                    Thread.Sleep(1500);
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

                    if (EverythingIsOk != null && EverythingIsOk.Value)
                    {
                        Tools.main(_collection); //Everything文档检索
                    }
                });
            }

            if (EverythingIsOk != null && EverythingIsOk.Value)
            {
                Tools.main(_collection); //Everything文档检索
            }
        }
    }


    public void CheckClipboard(bool loadLast = false)
    {
        if (!ConfigManger.Config.canReadClipboard)
        {
            Log.Debug("没有读取剪贴板授权");
            return;
        }

        if (Items.Count > 0 && (Items[0].FileType == FileType.剪贴板图像 || Items[0].FileName.StartsWith("打开")))
        {
            Items.RemoveAt(0);
        }

        var data = ServiceManager.Services.GetService<IClipboardService>().IsText();
        try
        {
            if (data)
            {
                var text = ServiceManager.Services.GetService<IClipboardService>().GetText();
                if (text.StartsWith("\""))
                {
                    text = text.Replace("\"", "");
                }

                //检测路径
                if (text.Contains("\\") || text.Contains("/"))
                {
                    Log.Debug("检测路径");
                    Dictionary<string, SearchViewItem> a = new();
                    AppTools.AppSolverA(a, text).Wait();
                    foreach (var (key, value) in a)
                    {
                        value.FileName = $"打开文件: {value.FileName} ?";
                        Items.Insert(0, value);
                        //GetIconInItemsAsync(value);
                    }
                }
            }
        }
        catch (Exception e)
        {
        }


        if (((IClipboardService)ServiceManager.Services!.GetService(typeof(IClipboardService))!).IsBitmap())
        {
            Log.Debug("剪贴板有图像信息");


            if (Items.Any(items => items.FileType == FileType.剪贴板图像))
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
            Log.Debug("剪贴板没有图像信息,但第一项是图片信息删除");


            Items.RemoveAt(0);
        }
    }

    private void LoadLast()
    {
        if (!string.IsNullOrEmpty(Search))
        {
            return;
        }


        Log.Debug("加载历史记录");


        foreach (var searchViewItem in Items)
        {
            searchViewItem.Dispose();
        }

        Items.Clear();
        CheckClipboard();
        var limit = 0;
        //Items.RaiseListChangedEvents = false;
        if (ConfigManger.Config.alwayShows.Any())
        {
            Log.Debug("加载常驻");
            foreach (var configAlwayShow in ConfigManger.Config.alwayShows)
            {
                if (_collection.TryGetValue(configAlwayShow, out var searchViewItem))
                {
                    var item = (SearchViewItem)searchViewItem;

                    Log.Debug("加载常驻:" + item.OnlyKey);


                    item.IsPined = true;
                    Items.Add(item);


                    limit++;
                }
            }
        }

        if (ConfigManger.Config.lastOpens.Any())
        {
            Log.Debug("加载历史");
            var sortedDict = ConfigManger.Config.lastOpens.OrderByDescending(p => p.Value)
                .ToDictionary(p => p.Key, p => p.Value);
            foreach (var (key, value) in sortedDict)
            {
                if (limit >= ConfigManger.Config.maxHistory)
                {
                    Log.Debug("超过历史记录限制,当前" + limit);


                    break;
                }

                if (_collection.TryGetValue(key, out var item2))
                {
                    if (item2 is null)
                    {
                        break;
                    }

                    var item = (SearchViewItem)item2;

                    Log.Debug("加载历史:" + item.OnlyKey);


                    if (!Items.Any((e) => e.OnlyKey.Equals(item.OnlyKey)))
                    {
                        Items.Add(item);


                        limit++;
                    }
                }
            }
        }

        //Items.RaiseListChangedEvents = true;


        // GetItemsIcon();
    }

    // ReSharper disable once RedundantAssignment
    partial void OnSearchChanged(string? value)
    {
        _searchDelayAction.Debounce(ConfigManger.Config.inputSmoothingMilliseconds, _scheduler, () =>
        {
            if (string.IsNullOrEmpty(Search))
            {
                LoadLast();
                return;
            }


            Log.Debug("搜索变更:" + Search);
            // Items.RaiseListChangedEvents = false;

            #region 清除上次搜索结果

            foreach (var searchViewItem in Items)
            {
                searchViewItem.Dispose();
            }

            Items.Clear();

            if (Search is null)
            {
                return;
            }

            #endregion

            value = Search.ToLowerInvariant();
            var pluginItem = 0;
            foreach (var searchAction in PluginOverall.SearchActions)
            {
                foreach (var func in searchAction.Value)
                {
                    var searchViewItem = func.Invoke(value);
                    if (searchViewItem != null)
                    {
                        pluginItem++;
                        Items.Add(searchViewItem);
                    }
                }
            }


            #region 数学运算

            var operators = new[] { '*', '+', '-', '/', '^' };
            var pattern = @"[\u4e00-\u9fa5a-zA-Z]+";
            if (Regex.Match(value, pattern, RegexOptions.NonBacktracking).Value == "" &&
                value.IndexOfAny(operators) > -1)
            {
                try
                {
                    var e = SDKs.Tools.Math.Evaluate(value);
                    Items.Add(new SearchViewItem()
                    {
                        FileName = "=" + e,
                        FileType = FileType.数学运算,
                        OnlyKey = value,
                        Icon = null,
                        IconSymbol = 61547,
                        IsVisible = true
                    });
                }
                catch (Exception)
                {
                    Items.Add(new SearchViewItem()
                    {
                        FileName = "错误的表达式",
                        FileType = FileType.数学运算,
                        OnlyKey = value,
                        Icon = null,
                        IconSymbol = 61547,
                        IsVisible = true
                    });
                }
            }

            #endregion


            #region 从文件索引检索并排序

            var filtered = new ConcurrentBag<(SearchViewItem Item, int Weight)>();
            foreach (var item in _collection)
            {
                var weight = 0;
                foreach (var key in item.Value.Keys)
                {
                    if (string.IsNullOrEmpty(key))
                    {
                        continue;
                    }

                    if (key.Contains(value))
                    {
                        weight += 2;
                    }

                    if (key.StartsWith(value))
                    {
                        weight += 5;
                    }

                    if (key.Equals(value, StringComparison.Ordinal))
                    {
                        weight += 10;
                    }
                }

                if (weight > 0)
                {
                    filtered.Add((item.Value, weight));
                }
            }


            var sorted = filtered.OrderByDescending(x => x.Weight).ToList();

            #endregion


            var count = 0; // 计数器变量
            const int limit = 100; // 限制次数
            Dictionary<SearchViewItem, int> nowHasLastOpens = new();

            for (var i = sorted.Count - 1; i >= 0; i--)
            {
                if (ConfigManger.Config.lastOpens.TryGetValue(sorted[i].Item.OnlyKey, out var open))
                {
                    nowHasLastOpens.Add((SearchViewItem)sorted[i].Item, open);
                    sorted.RemoveAt(i);
                }
            }

            var sortedDict = nowHasLastOpens.OrderByDescending(p => p.Value)
                .ToDictionary(p => p.Key, p => p.Value);
            foreach (var (searchViewItem, i) in sortedDict)
            {
                Log.Debug("添加搜索结果" + searchViewItem.OnlyKey);
                if (ConfigManger.Config.alwayShows.Contains(searchViewItem.OnlyKey))
                {
                    searchViewItem.IsPined = true;
                }


                count++;
                Items.Add(searchViewItem); // 添加元素
            }


            foreach (var x in sorted)
            {
                if (count >= limit) // 如果达到了限制
                {
                    break; // 跳出循环
                }

                var searchViewItem = (SearchViewItem)x.Item;
                {
                    Log.Debug("添加搜索结果" + x.Item.OnlyKey);


                    if (ConfigManger.Config.alwayShows.Contains(searchViewItem.OnlyKey))
                    {
                        searchViewItem.IsPined = true;
                    }

                    Items.Add(searchViewItem); // 添加元素
                    count++; // 计数器加一
                }
            }
            //Items.RaiseListChangedEvents = true;


            if (Items.Count <= pluginItem && !(Path.HasExtension(value) && File.Exists(value)) &&
                !Directory.Exists(value))
            {
                Log.Debug("无搜索项目,添加网页搜索");

                if (value.Contains(".") || value.Contains("file://"))
                {
                    var temp = value;
                    if (!temp.StartsWith("http") && !value.Contains("file://"))
                    {
                        temp = "https://" + temp;
                        var viewItem = new SearchViewItem()
                        {
                            Url = temp,
                            FileName = "打开网页:" + temp,
                            FileType = FileType.URL,
                            OnlyKey = temp,
                            Icon = null,
                            IconSymbol = 62555,
                            IsVisible = true
                        };
                        Items.Add(viewItem);
                        temp = "http://" + value;
                        var viewItem1 = new SearchViewItem()
                        {
                            Url = temp,
                            FileName = "打开网页:" + temp,
                            FileType = FileType.URL,
                            OnlyKey = temp,
                            Icon = null,
                            IconSymbol = 62555,
                            IsVisible = true
                        };
                        Items.Add(viewItem1);
                    }
                    else if (value.Contains("file://"))
                    {
                        var viewItem1 = new SearchViewItem()
                        {
                            Url = value,
                            FileName = "打开路径:" + value,
                            FileType = FileType.URL,
                            OnlyKey = value,
                            Icon = null,
                            IconSymbol = 62555,
                            IsVisible = true
                        };
                        Items.Add(viewItem1);
                    }
                    else
                    {
                        var viewItem1 = new SearchViewItem()
                        {
                            Url = value,
                            FileName = "打开网页:" + value,
                            FileType = FileType.URL,
                            OnlyKey = value,
                            Icon = null,
                            IconSymbol = 62555,
                            IsVisible = true
                        };
                        Items.Add(viewItem1);
                    }
                }

                var searchViewItem3 = new SearchViewItem()
                {
                    FileName = "将内容添加至便签" + value,
                    FileType = FileType.便签,
                    OnlyKey = value,
                    Icon = null,
                    IconSymbol = 0xF6EC,
                    IsVisible = true
                };
                Items.Add(searchViewItem3);
                var searchViewItem = new SearchViewItem()
                {
                    Url = "https://www.bing.com/search?q=" + value,
                    FileName = "在网页中搜索" + value,
                    FileType = FileType.URL,
                    OnlyKey = "https://www.bing.com/search?q=" + value,
                    Icon = null,
                    IconSymbol = 62555,
                    IsVisible = true
                };
                Items.Add(searchViewItem);


                var item = new SearchViewItem()
                {
                    Url = value,
                    FileName = "执行命令:" + value,
                    FileType = FileType.命令,
                    OnlyKey = value,
                    Icon = null,
                    IconSymbol = 61039,
                    IsVisible = true
                };
                Items.Add(item);
            }

            // Items.RaiseListChangedEvents = true;
            // Items.ResetBindings();
        });
    }

    public void SetSelectMode(bool flag, Action<SearchViewItem> action)
    {
        nowInSelectMode = flag;
        selectAction = action;
    }

    [RelayCommand]
    public void OpenFile(SearchViewItem item)
    {
        Task.Run(() =>
        {
            if (nowInSelectMode)
            {
                selectAction.Invoke(item);
                nowInSelectMode = false;
                WeakReferenceMessenger.Default.Send("a", "SearchWindowClose");
                return;
            }

            ((ISearchItemTool)ServiceManager.Services.GetService(typeof(ISearchItemTool))!).OpenSearchItem(item);
        });
        Search = "";
    }

    [RelayCommand]
    private void IgnoreItem(SearchViewItem searchViewItem)
    { 
        Task.Run(() =>
        {
            ConfigManger.Config.ignoreItems.Add(searchViewItem.OnlyKey);
            ConfigManger.Save();
            _collection.Remove(searchViewItem.OnlyKey);
            Dispatcher.UIThread.Invoke(() =>
            {
                Items.Remove(searchViewItem);
            });
        });
    }

    [RelayCommand]
    private void OpenFolder(object searchViewItem) => 
        Task.Run(() =>
        {
            WeakReferenceMessenger.Default.Send("a", "SearchWindowClose");
            var item = (SearchViewItem)searchViewItem;
            Log.Debug("打开指定内容文件夹" + item.OnlyKey);
            Shell32.ShellExecute(IntPtr.Zero, "open", "explorer.exe", "/select," + item.OnlyKey, "",
                ShowWindowCommand.SW_SHOW);

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
                    ;
            }

            //if (ConfigManger.config.lastOpens.Count > ConfigManger.config.maxHistory) ConfigManger.config.lastOpens.RemoveAt(ConfigManger.config.lastOpens.Count-1);
            Search = "";
            ConfigManger.Save();
        });

    [RelayCommand]
    private void RunAsAdmin(object searchViewItem) =>
        Task.Run(() =>
        {
            WeakReferenceMessenger.Default.Send("a", "SearchWindowClose");
            var item = (SearchViewItem)searchViewItem;
            Log.Debug("以管理员身份打开指定内容" + item.OnlyKey);
            if (item.FileType == FileType.UWP应用)
            {
                //explorer.exe shell:AppsFolder\Microsoft.WindowsMaps_8wekyb3d8bbwe!App

                Shell32.ShellExecute(IntPtr.Zero, "runas", "explorer.exe", $"shell:AppsFolder\\{item.OnlyKey}!App",
                    "", ShowWindowCommand.SW_NORMAL);
            }
            else
            {
                Shell32.ShellExecute(IntPtr.Zero, "runas", item.OnlyKey, "", "",
                    ShowWindowCommand.SW_NORMAL);
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
                    ;
            }

            //if (ConfigManger.config.lastOpens.Count > ConfigManger.config.maxHistory) ConfigManger.config.lastOpens.RemoveAt(ConfigManger.config.lastOpens.Count-1);
            Search = "";
            ConfigManger.Save();
        });

    [RelayCommand]
    private void Star(SearchViewItem item)
    {
        Log.Debug("添加/移除收藏" + item.OnlyKey);
        // var index = Items.IndexOf(item);
        // Items[index].IsStared = !Items[index].IsStared;
        // Items.ResetItem(index);
        item.IsStared = !item.IsStared;
        // Items.ResetBindings();
        if (item.FileInfo is not null)
        {
            if (ConfigManger.Config.customCollections.Contains(item.FileInfo.FullName))
            {
                ConfigManger.Config.customCollections.Remove(item.FileInfo.FullName);
            }

            if (item.IsStared) //收藏操作
            {
                AppTools.AppSolverA(_collection, item.FileInfo.FullName, true);
                ConfigManger.Config.customCollections.Insert(0, item.FileInfo.FullName);
            }
            else
            {
                var keyValuePairs = _collection.Where(e =>
                    e.Value.FileInfo != null && e.Value.FileInfo.FullName.Equals(item.FileInfo.FullName));
                foreach (var keyValuePair in keyValuePairs)
                {
                    _collection.Remove(keyValuePair.Key);
                }
            }
        }
        else if (item.DirectoryInfo is not null)
        {
            if (ConfigManger.Config.customCollections.Contains(item.DirectoryInfo.FullName))
            {
                ConfigManger.Config.customCollections.Remove(item.DirectoryInfo.FullName);
            }

            if (item.IsStared) //收藏操作
            {
                AppTools.AppSolverA(_collection, item.DirectoryInfo.FullName, true);
                ConfigManger.Config.customCollections.Insert(0, item.DirectoryInfo.FullName);
            }
            else
            {
                var keyValuePairs = _collection.Where(e =>
                    e.Value.DirectoryInfo != null &&
                    e.Value.DirectoryInfo.FullName.Equals(item.DirectoryInfo.FullName));
                foreach (var keyValuePair in keyValuePairs)
                {
                    _collection.Remove(keyValuePair.Key);
                }
            }
        }

        //GetItemsIcon();

        ConfigManger.Save();
    }

    [RelayCommand]
    private void Pin(object searchViewItem)
    {
        var item = (SearchViewItem)searchViewItem;
        Log.Debug("添加常驻" + item.OnlyKey);
        var index = Items.IndexOf(item);
        Items[index].IsPined = !Items[index].IsPined;
        //Items.ResetItem(index);
        if (ConfigManger.Config.alwayShows.Contains(item.OnlyKey))
        {
            ConfigManger.Config.alwayShows.Remove(item.OnlyKey);
        }

        if (item.IsPined) //收藏操作
        {
            ConfigManger.Config.alwayShows.Insert(0, item.OnlyKey);
        }

        ConfigManger.Save();
    }

    [RelayCommand]
    private void OpenFolderInTerminal(object searchViewItem) =>
        Task.Run(() =>
        {
            WeakReferenceMessenger.Default.Send("a", "SearchWindowClose");
            var item = (SearchViewItem)searchViewItem;
            Log.Debug("打开指定内容在终端中" + item.OnlyKey);
            var startInfo = new ProcessStartInfo
            {
                FileName = @"C:\Windows\System32\cmd.exe"
            };
            if (!File.Exists(@"C:\Windows\System32\cmd.exe"))
            {
                Log.Debug("64");
                startInfo.FileName = @"C:\Windows\sysnative\cmd.exe";
            }

            if (item.FileInfo != null)
            {
                startInfo.WorkingDirectory = item.FileInfo.DirectoryName;
            }

            if (item.DirectoryInfo != null)
            {
                startInfo.WorkingDirectory = item.DirectoryInfo.FullName;
            }

            Process.Start(startInfo);

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
                    ;
            }

            //if (ConfigManger.config.lastOpens.Count > ConfigManger.config.maxHistory) ConfigManger.config.lastOpens.RemoveAt(ConfigManger.config.lastOpens.Count-1);
            Search = "";
            ConfigManger.Save();
        });
}