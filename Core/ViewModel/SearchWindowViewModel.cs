using System.Collections.Concurrent;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Core.SDKs;
using Core.SDKs.Everything;
using Core.SDKs.Services;
using Core.SDKs.Services.Config;
using Core.SDKs.Tools;
using log4net;
using Vanara.PInvoke;

namespace Core.ViewModel;

public partial class SearchWindowViewModel : ObservableRecipient
{
    private static readonly ILog Log = LogManager.GetLogger(nameof(SearchWindowViewModel));
    private readonly List<SearchViewItem> _collection = new(400); //存储本机所有软件

    [ObservableProperty] private bool? _everythingIsOk = true;
    static List<SearchViewItem> tempList = new List<SearchViewItem>(1000);

    [ObservableProperty]
    private BindingList<SearchViewItem> _items = new BindingList<SearchViewItem>(tempList); //搜索界面显示的软件

    private readonly List<string> _names = new(500); //软件去重

    [ObservableProperty] private string? _search;


    [ObservableProperty] private int? _selectedIndex = -1;

    public SearchWindowViewModel()
    {
        ReloadApps(true);
        LoadLast();
    }

    public void ReloadApps(bool logging = false)
    {
        AppTools.DelNullFile(_collection, _names);
        AppTools.GetAllApps(_collection, _names, logging);
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

            if (EverythingIsOk != null && EverythingIsOk.Value)
            {
                Tools.main(_collection, _names); //Everything文档检索
            }
        }
    }

    public void CheckClipboard()
    {
        if (!ConfigManger.Config.canReadClipboard)
        {
            Log.Debug("没有读取剪贴板授权");


            if (Items.First().FileType == FileType.剪贴板图像)
            {
                Items.RemoveAt(0);
            }

            return;
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
        var limit = 0;
        //Items.RaiseListChangedEvents = false;
        if (ConfigManger.Config.alwayShows.Any())
        {
            Log.Debug("加载常驻");


            foreach (var searchViewItem in ConfigManger.Config.alwayShows.SelectMany(name1 => _collection.Where(
                         searchViewItem => searchViewItem.OnlyKey.Equals(name1))))
            {
                var item = (SearchViewItem)searchViewItem.Clone();

                Log.Debug("加载常驻:" + item.OnlyKey);


                item.IsPined = true;
                Items.Add(item);
                if (item.Icon is null)
                {
                    ThreadPool.QueueUserWorkItem(_ =>
                    {
                        GetIconInItems(item);
                    });
                }


                limit++;
            }
        }

        if (ConfigManger.Config.lastOpens.Any())
        {
            Log.Debug("加载历史");


            foreach (var searchViewItem in ConfigManger.Config.lastOpens.SelectMany(name1 => _collection.Where(
                         searchViewItem => searchViewItem.OnlyKey.Equals(name1.Key))))
            {
                if (limit >= ConfigManger.Config.maxHistory)
                {
                    Log.Debug("超过历史记录限制,当前" + limit);


                    break;
                }

                var item = (SearchViewItem)searchViewItem.Clone();

                Log.Debug("加载历史:" + item.OnlyKey);


                if (!Items.Any((e) => e.OnlyKey.Equals(item.OnlyKey)))
                {
                    Items.Add(item);
                    if (item.Icon is null)
                    {
                        ThreadPool.QueueUserWorkItem(_ =>
                        {
                            GetIconInItems(item);
                        });
                    }

                    limit++;
                }
            }
        }

        //Items.RaiseListChangedEvents = true;
        CheckClipboard();
        // GetItemsIcon();
    }

    private void GetIconInItems(SearchViewItem t)
    {
        {
            try
            {
                if (t.Icon != null)
                {
                    return;
                }

                switch (t.FileType)
                {
                    case FileType.文件夹:
                        t.Icon = (Icon)((IconTools)ServiceManager.Services!.GetService(typeof(IconTools))!)
                            .ExtractFromPath(t.DirectoryInfo!.FullName).Clone();
                        break;
                    case FileType.命令:
                    case FileType.URL:
                        if (t.FileInfo is not null)
                        {
                            t.Icon = (Icon)((IconTools)ServiceManager.Services!.GetService(typeof(IconTools))!)
                                .GetIcon(t.FileInfo!.FullName).Clone();
                        }

                        break;
                    case FileType.数学运算:
                    case FileType.剪贴板图像:
                    case FileType.None:
                        break;
                    case FileType.应用程序:
                    case FileType.Word文档:
                    case FileType.PPT文档:
                    case FileType.Excel文档:
                    case FileType.PDF文档:
                    case FileType.图像:
                    case FileType.文件:
                    default:

                        t.Icon = (Icon)((IconTools)ServiceManager.Services!.GetService(typeof(IconTools))!)
                            .GetIcon(t.FileInfo!.FullName).Clone();
                        break;
                }
            }
            catch (Exception)
            {
                // ignored
            }
            finally
            {
                Task.Factory.StartNew(() =>
                    {
                        Items.ResetBindings();
                    }, CancellationToken.None, TaskCreationOptions.None, _scheduler)
                    .Wait();
            }
        }
    }

    private readonly DelayAction _searchDelayAction = new();
    private readonly TaskScheduler _scheduler = TaskScheduler.FromCurrentSynchronizationContext();

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


            foreach (var searchViewItem in Items)
            {
                searchViewItem.Dispose();
            }

            Items.Clear();

            if (Search is null)
            {
                return;
            }

            value = Search.ToLowerInvariant();


            if (value.Contains("\\") || value.Contains("/"))
            {
                Log.Debug("检测路径");


                if (Path.HasExtension(value) && File.Exists(value))
                {
                    Log.Debug("检测到文件路径");


                    var searchViewItem = new SearchViewItem()
                    {
                        FileInfo = new FileInfo(value),
                        FileName = "打开文件:" + Shell32.SHCreateItemFromParsingName<Shell32.IShellItem>(value)
                            .GetDisplayName(Shell32.SIGDN.SIGDN_NORMALDISPLAY) + "?",
                        OnlyKey = value,
                        FileType = FileType.文件,
                        IsVisible = true
                    };
                    ThreadPool.QueueUserWorkItem(_ =>
                    {
                        GetIconInItems(searchViewItem);
                    });

                    Items.Add(searchViewItem);
                }
                else if (Directory.Exists(value))
                {
                    Log.Debug("检测到文件夹路径");


                    var searchViewItem = new SearchViewItem()
                    {
                        DirectoryInfo = new DirectoryInfo(value),
                        FileName = "打开" + value.Split("\\").Last() + "?",
                        FileType = FileType.文件夹,
                        OnlyKey = value,
                        Icon = null,
                        IsVisible = true
                    };
                    ThreadPool.QueueUserWorkItem(_ =>
                    {
                        GetIconInItems(searchViewItem);
                    });
                    Items.Add(searchViewItem);
                }
            }

            var operators = new[] { '*', '+', '-', '/' };
            string pattern = @"[\u4e00-\u9fa5a-zA-Z]+";
            if (!Regex.Match(value, pattern).Success && value.IndexOfAny(operators) > 0)
            {
                try
                {
                    var dt = new DataTable();
                    var result = dt.Compute(value, null);
                    Items.Add(new SearchViewItem()
                    {
                        FileName = "=" + result,
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


            var filtered = new ConcurrentBag<(SearchViewItem Item, int Weight)>();
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            foreach (var item in _collection)
            {
                int weight = 0;
                foreach (var key in item.Keys)
                {
                    if (string.IsNullOrEmpty(key)) continue;

                    if (key.Contains(value)) weight += 2;

                    if (key.StartsWith(value)) weight += 5;

                    if (key.Equals(value, StringComparison.Ordinal)) weight += 10;
                }

                if (weight > 0)
                {
                    filtered.Add((item, weight));
                }
            }

            stopwatch.Stop();
            Log.Debug($"方法耗时:{stopwatch.Elapsed.TotalMilliseconds}");

            var sorted = filtered.OrderByDescending(x => x.Weight).ToList();

            // 将排序后的对象添加到Items集合中
            //Items.RaiseListChangedEvents = false;
            var count = 0; // 计数器变量
            const int limit = 100; // 限制次数
            var nowIndex = 0;
            Dictionary<SearchViewItem, int> nowHasLastOpens = new();
            List<SearchViewItem> toRemove = new();
            foreach (var valueTuple in sorted)
            {
                if (ConfigManger.Config.lastOpens.ContainsKey(valueTuple.Item.OnlyKey))
                {
                    nowHasLastOpens.Add((SearchViewItem)valueTuple.Item.Clone(),
                        ConfigManger.Config.lastOpens[valueTuple.Item.OnlyKey]);
                    toRemove.Add(valueTuple.Item);
                }
            }

            sorted.RemoveAll(x => toRemove.Contains(x.Item1));

            var sortedDict = nowHasLastOpens.OrderByDescending(p => p.Value).ToDictionary(p => p.Key, p => p.Value);
            foreach (var (searchViewItem, i) in sortedDict)
            {
                if (ConfigManger.Config.alwayShows.Contains(searchViewItem.OnlyKey))
                {
                    searchViewItem.IsPined = true;
                }


                if (searchViewItem.Icon is null)
                {
                    ThreadPool.QueueUserWorkItem(_ =>
                    {
                        GetIconInItems(searchViewItem);
                    });
                }

                Items.Add(searchViewItem); // 添加元素
            }


            foreach (var x in sorted)
            {
                if (count >= limit) // 如果达到了限制
                {
                    break; // 跳出循环
                }

                var searchViewItem = (SearchViewItem)x.Item.Clone();
                if (ConfigManger.Config.lastOpens.ContainsKey(x.Item.OnlyKey))
                {
                    count++;
                }
                else
                {
                    Log.Debug("添加搜索结果" + x.Item.OnlyKey);


                    if (ConfigManger.Config.alwayShows.Contains(searchViewItem.OnlyKey))
                    {
                        searchViewItem.IsPined = true;
                    }


                    if (searchViewItem.Icon is null)
                    {
                        ThreadPool.QueueUserWorkItem(_ =>
                        {
                            GetIconInItems(searchViewItem);
                        });
                    }

                    Items.Add(searchViewItem); // 添加元素
                    count++; // 计数器加一
                }
            }
            //Items.RaiseListChangedEvents = true;


            if (Items.Count <= 0 && !(Path.HasExtension(value) && File.Exists(value)) && !Directory.Exists(value))
            {
                Log.Debug("无搜索项目,添加网页搜索");

                if (value.Contains("."))
                {
                    var viewItem = new SearchViewItem()
                    {
                        Url = value,
                        FileName = "打开网页:" + value,
                        FileType = FileType.URL,
                        OnlyKey = value,
                        Icon = null,
                        IconSymbol = 62555,
                        IsVisible = true
                    };
                    Items.Add(viewItem);
                }

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
        });
    }


    [RelayCommand]
    public async Task OpenFile(object searchViewItem)
    {
        await Task.Run(() =>
        {
            WeakReferenceMessenger.Default.Send("a", "SearchWindowClose");
            var item = (SearchViewItem)searchViewItem;
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
                        ((IToastService)ServiceManager.Services.GetService(typeof(IToastService))!).show("剪贴板图片保存失败");
                        return;
                    }

                    Shell32.ShellExecute(IntPtr.Zero, "open", "explorer.exe", "/select," + fileName, "",
                        ShowWindowCommand.SW_NORMAL);
                    return;
                }
                case "Math": break;
                default:
                {
                    if (item.FileType == FileType.UWP应用)
                    {
                        //explorer.exe shell:AppsFolder\Microsoft.WindowsMaps_8wekyb3d8bbwe!App
                        Shell32.ShellExecute(IntPtr.Zero, "open", "explorer.exe",
                            $"shell:AppsFolder\\{item.OnlyKey}!App", "",
                            ShowWindowCommand.SW_NORMAL);
                    }
                    else
                        Shell32.ShellExecute(IntPtr.Zero, "open", item.OnlyKey, "", "",
                            ShowWindowCommand.SW_NORMAL);

                    if (ConfigManger.Config.lastOpens.ContainsKey(item.OnlyKey))
                    {
                        ConfigManger.Config.lastOpens[item.OnlyKey]++;
                    }
                    else
                        ConfigManger.Config.lastOpens.Add(item.OnlyKey, 1);

                    //if (ConfigManger.config.lastOpens.Count > ConfigManger.config.maxHistory) ConfigManger.config.lastOpens.RemoveAt(ConfigManger.config.lastOpens.Count-1);
                    Search = "";
                    ConfigManger.Save();
                    return;
                }
            }
        });
    }

    [RelayCommand]
    private async Task OpenFolder(object searchViewItem)
    {
        await Task.Run(() =>
        {
            WeakReferenceMessenger.Default.Send("a", "SearchWindowClose");
            var item = (SearchViewItem)searchViewItem;
            Log.Debug("打开指定内容文件夹" + item.OnlyKey);
            Shell32.ShellExecute(IntPtr.Zero, "open", "explorer.exe", "/select," + item.OnlyKey, "",
                ShowWindowCommand.SW_SHOW);

            if (ConfigManger.Config.lastOpens.ContainsKey(item.OnlyKey))
            {
                ConfigManger.Config.lastOpens[item.OnlyKey]++;
            }
            else
                ConfigManger.Config.lastOpens.Add(item.OnlyKey, 1);

            //if (ConfigManger.config.lastOpens.Count > ConfigManger.config.maxHistory) ConfigManger.config.lastOpens.RemoveAt(ConfigManger.config.lastOpens.Count-1);
            Search = "";
            ConfigManger.Save();
        });
    }

    [RelayCommand]
    private async Task RunAsAdmin(object searchViewItem)
    {
        await Task.Run(() =>
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
                Shell32.ShellExecute(IntPtr.Zero, "runas", item.OnlyKey, "", "",
                    ShowWindowCommand.SW_NORMAL);

            if (ConfigManger.Config.lastOpens.ContainsKey(item.OnlyKey))
            {
                ConfigManger.Config.lastOpens[item.OnlyKey]++;
            }
            else
                ConfigManger.Config.lastOpens.Add(item.OnlyKey, 1);

            //if (ConfigManger.config.lastOpens.Count > ConfigManger.config.maxHistory) ConfigManger.config.lastOpens.RemoveAt(ConfigManger.config.lastOpens.Count-1);
            Search = "";
            ConfigManger.Save();
        });
    }

    [RelayCommand]
    private void Star(SearchViewItem item)
    {
        Log.Debug("添加/移除收藏" + item.OnlyKey);
        // var index = Items.IndexOf(item);
        // Items[index].IsStared = !Items[index].IsStared;
        // Items.ResetItem(index);
        item.IsStared = !item.IsStared;
        Items.ResetBindings();
        if (item.FileInfo is not null)
        {
            if (ConfigManger.Config.customCollections.Contains(item.FileInfo.FullName))
            {
                ConfigManger.Config.customCollections.Remove(item.FileInfo.FullName);
            }

            if (item.IsStared) //收藏操作
            {
                AppTools.AppSolverA(_collection, _names, item.FileInfo.FullName, true);
                ConfigManger.Config.customCollections.Insert(0, item.FileInfo.FullName);
            }
            else
            {
                _collection.Remove(_collection.Find(e =>
                    e.FileInfo != null && e.FileInfo.FullName.Equals(item.FileInfo.FullName))!);
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
                AppTools.AppSolverA(_collection, _names, item.DirectoryInfo.FullName, true);
                ConfigManger.Config.customCollections.Insert(0, item.DirectoryInfo.FullName);
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
    private async Task OpenFolderInTerminal(object searchViewItem)
    {
        await Task.Run(() =>
        {
            WeakReferenceMessenger.Default.Send("a", "SearchWindowClose");
            var item = (SearchViewItem)searchViewItem;
            Log.Debug("打开指定内容在终端中" + item.OnlyKey);
            var startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe"
            };

            if (item.FileInfo != null)
            {
                startInfo.WorkingDirectory = item.FileInfo.DirectoryName;
            }

            if (item.DirectoryInfo != null)
            {
                startInfo.WorkingDirectory = item.DirectoryInfo.FullName;
            }

            Process.Start(startInfo);

            if (ConfigManger.Config.lastOpens.ContainsKey(item.OnlyKey))
            {
                ConfigManger.Config.lastOpens[item.OnlyKey]++;
            }
            else
                ConfigManger.Config.lastOpens.Add(item.OnlyKey, 1);

            //if (ConfigManger.config.lastOpens.Count > ConfigManger.config.maxHistory) ConfigManger.config.lastOpens.RemoveAt(ConfigManger.config.lastOpens.Count-1);
            Search = "";
            ConfigManger.Save();
        });
    }
}