#region

using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Core.SDKs.CustomScenario;
using Core.SDKs.Services;
using Core.SDKs.Services.Config;
using Core.SDKs.Services.Plugin;
using Core.SDKs.Tools;
using log4net;
using Microsoft.Extensions.DependencyInjection;
using Pinyin.NET;
using PluginCore;

#endregion

namespace Core.ViewModel;

public partial class SearchWindowViewModel : ObservableRecipient
{
    private static readonly ILog Log = LogManager.GetLogger(nameof(SearchWindowViewModel));
    private static readonly List<SearchViewItem> TempList = new(1000);

    public readonly ConcurrentDictionary<string, SearchViewItem> _collection = new(); //存储本机所有软件
    private readonly TaskScheduler _scheduler = TaskScheduler.FromCurrentSynchronizationContext();

    private readonly DelayAction _searchDelayAction = new();

    [ObservableProperty] private bool? _everythingIsOk = true;
    [ObservableProperty] private ObservableCollection<SearchViewItem> _items = new(TempList); //搜索界面显示的软件
    private  PinyinSearcher<SearchViewItem> _pinyinSearcher ;

    private bool _reloading = false;


    [ObservableProperty] private string? _search;


    [ObservableProperty] private int? _selectedIndex = -1;


    private bool nowInSelectMode = false;
    private Action<SearchViewItem?>? selectAction;

    public SearchWindowViewModel()
    {
        ReloadApps(false);
        LoadLast();
    }

    public async Task AddCollection(string search)
    {
        await ServiceManager.Services.GetService<IAppToolService>()!.AppSolverA(_collection, search, false);
    }

    public void ReloadApps(bool logging = false)
    {
        if (_reloading)
        {
            return;
        }
        _reloading = true;
        ServiceManager.Services.GetService<IAppToolService>()!.DelNullFile(_collection);
        ServiceManager.Services.GetService<IAppToolService>()!.GetAllApps(_collection, logging,ConfigManger.Config.useEverything);

        if (ConfigManger.Config.useEverything && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Log.Debug("everything检测");


            var service = ServiceManager.Services.GetService<IEverythingService>()!;
            EverythingIsOk = service.isRun();

            if (!EverythingIsOk.Value)
            {
                ServiceManager.Services.GetService<IAppToolService>()!.AutoStartEverything(_collection, () =>
                {
                    Thread.Sleep(1500);
                    var everythingService = ServiceManager.Services.GetService<IEverythingService>()!;
                    EverythingIsOk = everythingService.isRun();
                    
                });
            }
            
        }
        _reloading = false;
    }


    public void CheckClipboard(bool loadLast = false)
    {
        if (!ConfigManger.Config.canReadClipboard)
        {
            Log.Debug("没有读取剪贴板授权");
            return;
        }

        if (Items.Count > 0 && (Items[0].FileType == FileType.剪贴板图像 || Items[0].ItemDisplayName.StartsWith("打开")))
        {
            Items.RemoveAt(0);
        }

        var data = ServiceManager.Services.GetService<IClipboardService>().HasText();
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
                    ConcurrentDictionary<string, SearchViewItem> a = new();
                    ServiceManager.Services.GetService<IAppToolService>()!.AppSolverA(a, text).Wait();
                    foreach (var (key, value) in a)
                    {
                        if (value.FileType == FileType.文件夹)
                        {
                            value.ItemDisplayName = $"打开文件夹: {value.ItemDisplayName} ?";
                        }
                        else
                        {
                            value.ItemDisplayName = $"打开文件: {value.ItemDisplayName} ?";
                        }

                        Items.Insert(0, value);
                        //GetIconInItemsAsync(value);
                    }
                }
            }
        }
        catch (Exception e)
        {
        }


        if (((IClipboardService)ServiceManager.Services!.GetService(typeof(IClipboardService))!).HasImage())
        {
            Log.Debug("剪贴板有图像信息");


            if (Items.Any(items => items.FileType == FileType.剪贴板图像))
            {
                return;
            }

            Items.Insert(0, new SearchViewItem()
            {
                ItemDisplayName = "保存剪贴板图像?",
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
        if (_pinyinSearcher is null)
        {
            _pinyinSearcher = new PinyinSearcher<SearchViewItem>(source: _collection,nameof(SearchViewItem.Keys),true);
        }
        
        _searchDelayAction.Debounce(ConfigManger.Config.inputSmoothingMilliseconds, _scheduler, () =>
        {
            if (string.IsNullOrEmpty(Search))
            {
                LoadLast();
                return;
            }

            var lastItem = Items.FirstOrDefault();

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

            value = Search.ToLowerInvariant().Split(" ").First();
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
                        ItemDisplayName = "=" + e,
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
                        ItemDisplayName = "错误的表达式",
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


            var filtered = _pinyinSearcher.Search(value).ToList();
            foreach (var item in CustomScenarioManger.CustomScenarios)
            {
                var onlyKey = $"CustomScenario:{item.UUID}";
                int weight = 0;
                foreach (var key in item.Keys)
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

                if (onlyKey == lastItem.OnlyKey)
                {
                    weight -= 4;
                }

                if (weight > 0)
                {
                   
                    var viewItem1 = new SearchViewItem()
                    {
                        ItemDisplayName = $"执行自定义情景:{item.Name}",
                        FileType = FileType.自定义情景,
                        OnlyKey = onlyKey,
                        Icon = null,
                        IconSymbol = 0xF78B,
                        IsVisible = true
                    };
                    
                    filtered.Add(new SearchResults<SearchViewItem>
                    {
                        Source = viewItem1,
                        Weight = weight
                    });
                }
            }
            var sorted = filtered.OrderByDescending(x => x.Weight).ToList();

            #endregion


            var count = 0; // 计数器变量
            const int limit = 100; // 限制次数
            Dictionary<SearchViewItem, int> nowHasLastOpens = new();

            for (var i = sorted.Count - 1; i >= 0; i--)
            {
                if (ConfigManger.Config.lastOpens.TryGetValue(sorted[i].Source.OnlyKey, out var open))
                {
                    nowHasLastOpens.Add((SearchViewItem)sorted[i].Source, open);
                    sorted.RemoveAt(i);
                }
            }

            var sortedDict = nowHasLastOpens.OrderByDescending(p => p.Value)
                .ToDictionary(p => p.Key, p => p.Value);
            foreach (var (searchViewItem, i) in sortedDict)
            {
                //Log.Debug("添加搜索结果" + searchViewItem.OnlyKey);
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

                var searchViewItem = (SearchViewItem)x.Source;
                {
                    //Log.Debug("添加搜索结果" + x.Item.OnlyKey);


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
                            ItemDisplayName = "打开网页:" + temp,
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
                            ItemDisplayName = "打开网页:" + temp,
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
                            ItemDisplayName = "打开路径:" + value,
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
                            ItemDisplayName = "打开网页:" + value,
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
                    ItemDisplayName = "将内容添加至便签" + value,
                    FileType = FileType.便签,
                    OnlyKey = value,
                    Icon = null,
                    IconSymbol = 0xF6EC,
                    IsVisible = true
                };
                Items.Add(searchViewItem3);
                var searchViewItem = new SearchViewItem()
                {
                    ItemDisplayName = "在网页中搜索" + value,
                    FileType = FileType.URL,
                    OnlyKey = "https://www.bing.com/search?q=" + value,
                    Icon = null,
                    IconSymbol = 62555,
                    IsVisible = true
                };
                Items.Add(searchViewItem);


                var item = new SearchViewItem()
                {
                    ItemDisplayName = "执行命令:" + value,
                    FileType = FileType.命令,
                    OnlyKey = value,
                    Icon = null,
                    IconSymbol = 61039,
                    IsVisible = true
                };
                Items.Add(item);
            }

            var strings = Search.Split(" ");
            if (strings.Length > 1)
            {
                for (var index = 1; index < strings.Length; index++)
                {
                    ReSearch(strings[index]);
                }
            }
        });
    }

    private void ReSearch(string value)
    {
        for (var index = Items.Count - 1; index >= 0; index--)
        {
            var searchViewItem = Items[index];
            if (!searchViewItem.Keys!.Any(e => e.Contains(value)))
            {
                Items.RemoveAt(index);
            }
        }
    }

    public void SetSelectMode(bool flag, Action<SearchViewItem> action)
    {
        nowInSelectMode = flag;
        selectAction = action;
    }

    [RelayCommand]
    public void OpenFile(SearchViewItem? item)
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

            ServiceManager.Services.GetService<ISearchItemTool>()!.OpenFile(item);
        });
        Search = "";
    }

    [RelayCommand]
    private void IgnoreItem(SearchViewItem searchViewItem)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            Items.Remove(searchViewItem);
        });
        ServiceManager.Services.GetService<ISearchItemTool>()!.IgnoreItem(searchViewItem);
    }

    [RelayCommand]
    private void OpenFolder(object searchViewItem)
    {
        Search = "";
        ServiceManager.Services.GetService<ISearchItemTool>()!.OpenFolder((SearchViewItem?)searchViewItem);
    }

    [RelayCommand]
    private void RunAsAdmin(object searchViewItem)
    {
        Search = "";
        ServiceManager.Services.GetService<ISearchItemTool>()!.RunAsAdmin((SearchViewItem?)searchViewItem);
    }

    [RelayCommand]
    private void Star(SearchViewItem item)
    {
        ServiceManager.Services.GetService<ISearchItemTool>()!.Star(item);
    }

    [RelayCommand]
    private void Pin(object searchViewItem)
    {
        var item = (SearchViewItem)searchViewItem;
        Log.Debug("添加常驻" + item.OnlyKey);
        var index = Items.IndexOf(item);
        Items[index].IsPined = !Items[index].IsPined;
        //Items.ResetItem(index);

        ServiceManager.Services.GetService<ISearchItemTool>()!.Pin(item);
    }

    [RelayCommand]
    private void OpenFolderInTerminal(object searchViewItem)
    {
        Search = "";
        ServiceManager.Services.GetService<ISearchItemTool>()!.OpenFolderInTerminal((SearchViewItem?)searchViewItem);
    }
}