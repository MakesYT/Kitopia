using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.SDKs;
using Core.SDKs.Config;
using Core.SDKs.Everything;
using Core.SDKs.Services;


namespace Core.ViewModel;

public partial class SearchWindowViewModel : ObservableRecipient
{
    private List<SearchViewItem> _collection = new(250); //存储本机所有软件


    [ObservableProperty] private bool? _everythingIsOk;

    [ObservableProperty] public BindingList<SearchViewItem> _items = new BindingList<SearchViewItem>(); //搜索界面显示的软件

    private List<string> _names = new(250); //软件去重

    [ObservableProperty] private string? _search;


    [ObservableProperty] private int? _selectedIndex = -1;

    public SearchWindowViewModel()
    {
        ReloadApps();
        LoadLast();
        
    }

    public void ReloadApps()
    {
        
        _collection.Clear();
        _names.Clear();
        GC.Collect();
        AppSolver.GetAllApps( _collection,  _names);
        
    }

    public void LoadLast()
    {
        if (!string.IsNullOrEmpty(Search))
        {
            return;
        }
        foreach (var searchViewItem in Items)
        {
            searchViewItem.Dispose(); 
        }
        Items.Clear();
        //Items.RaiseListChangedEvents = false;
        if (ConfigManger.config!.lastOpens.Any())
            foreach (var searchViewItem in ConfigManger.config.lastOpens.SelectMany(name1 => _collection.Where(
                         searchViewItem => searchViewItem.FileName!.Equals(name1))))
            {
                Items.Add((SearchViewItem)searchViewItem.Clone());
            }

        //Items.RaiseListChangedEvents = true;
        OnItemsChanged(Items);
    }

    partial void OnItemsChanged(BindingList<SearchViewItem> value)
    {
        
        if (value is null)
        {
            return;
        }

        Task.Run(() =>
        {
            try
            {
                foreach (var t in value)
                {
                    if (t.Icon != null) continue;
                    if (t.FileType != FileType.文件夹)
                    {
                        try
                        {
                            t.Icon =
                                (Icon)((GetIconFromFile)ServiceManager.Services!.GetService(typeof(GetIconFromFile))!)
                                .GetIcon(t.FileInfo!.FullName).Clone();
                        }
                        catch (Exception e)
                        {
                            break;
                        }
                    }
                    else
                        t.Icon = (Icon)((GetIconFromFile)ServiceManager.Services!.GetService(typeof(GetIconFromFile))!)
                            .ExtractFromPath(t.DirectoryInfo!.FullName).Clone();
                }
            }
            catch (Exception)
            {
                
                // ignored
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
        foreach (var searchViewItem in Items)
        {
            searchViewItem.Dispose(); 
        }
        Items.Clear();
        
        
        value = value.ToLowerInvariant();

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


        
        Tools.main( Items, value);//文档检索
        GC.Collect();

        if (value.Contains("\\")||value.Contains("/"))
        {
            
            if (Path.HasExtension(value)&&File.Exists(value))
            {
                Items.Add(new SearchViewItem()
                {
                    FileInfo = new FileInfo(value),
                    FileName = LnkSolver.GetLocalizedName(value),
                    FileType = FileType.应用程序,
                    IsVisible = true
                });
            }
            else if (Directory.Exists(value))
            {
                Items.Add(new SearchViewItem()
                                {
                                    DirectoryInfo = new DirectoryInfo(value),
                                    FileName = "打开此文件夹?",
                                    FileType = FileType.文件夹,
                                    Icon = null,
                                    IsVisible = true
                                });
            }
            
        }
        
        // 使用LINQ语句来简化和优化筛选和排序逻辑，而不需要使用foreach循环和if判断
        // 根据给定的值，从集合中筛选出符合条件的SearchViewItem对象，并计算它们的权重
        var filtered = from item in _collection
            let keys = item.Keys.Where(key => !string.IsNullOrEmpty(key)) // 排除空的键
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
        foreach (var x in sorted)
        {
            if (count >= limit) // 如果达到了限制
            {
                break; // 跳出循环
            }
            Items.Add((SearchViewItem)x.Item.Clone()); // 添加元素
            count++; // 计数器加一
        }
        //Items.RaiseListChangedEvents = true;
        OnItemsChanged(Items);
    }


    [RelayCommand]
    public void OpenFile(object searchViewItem)
    {
        var item = (SearchViewItem)searchViewItem;
        if (item.FileInfo!=null)
        {
            ShellTools.ShellExecute(IntPtr.Zero, "open", item.FileInfo.FullName, "", "",
                        ShellTools.ShowCommands.SW_SHOWNORMAL);
        }
        if (item.DirectoryInfo!=null)
        {
            ShellTools.ShellExecute(IntPtr.Zero, "open", item.DirectoryInfo.FullName, "", "",
                ShellTools.ShowCommands.SW_SHOWNORMAL);
        }

        if (!ConfigManger.config!.lastOpens.Contains(item.FileName!))
            ConfigManger.config.lastOpens.Insert(0, item.FileName!);
        if (ConfigManger.config.lastOpens.Count > 4) ConfigManger.config.lastOpens.RemoveAt(4);
        Search = "";
        ConfigManger.Save();
    }

    [RelayCommand]
    public void OpenFolder(object searchViewItem)
    {
        var item = (SearchViewItem)searchViewItem;
        ShellTools.ShellExecute(IntPtr.Zero, "open", item.FileInfo!.DirectoryName!, "", "",
            ShellTools.ShowCommands.SW_SHOWNORMAL);

        if (!ConfigManger.config!.lastOpens.Contains(item.FileName!))
            ConfigManger.config.lastOpens.Insert(0, item.FileName!);
        if (ConfigManger.config.lastOpens.Count() > 4) ConfigManger.config.lastOpens.RemoveAt(4);
        Search = "";
        ConfigManger.Save();
    }

    [RelayCommand]
    public void RunAsAdmin(object searchViewItem)
    {
        var item = (SearchViewItem)searchViewItem;
        ShellTools.ShellExecute(IntPtr.Zero, "runas", item.FileInfo!.FullName, "", "",
            ShellTools.ShowCommands.SW_SHOWNORMAL);

        if (!ConfigManger.config!.lastOpens.Contains(item.FileName!))
            ConfigManger.config.lastOpens.Insert(0, item.FileName!);
        if (ConfigManger.config.lastOpens.Count() > 4) ConfigManger.config.lastOpens.RemoveAt(4);
        Search = "";
        ConfigManger.Save();
    }

    [RelayCommand]
    public void OpenFolderInTerminal(object searchViewItem)
    {
        var item = (SearchViewItem)searchViewItem;
        ProcessStartInfo startInfo = new ProcessStartInfo();
        startInfo.FileName = "cmd.exe";

        if (item.FileInfo!=null)
        {
            startInfo.WorkingDirectory = item.FileInfo.DirectoryName;
            
        }
        if (item.DirectoryInfo!=null)
        {
            startInfo.WorkingDirectory = item.DirectoryInfo.FullName;
            
        }
        Process.Start(startInfo);

        if (!ConfigManger.config!.lastOpens.Contains(item.FileName!))
            ConfigManger.config.lastOpens.Insert(0, item.FileName!);
        if (ConfigManger.config.lastOpens.Count() > 4) ConfigManger.config.lastOpens.RemoveAt(4);
        Search = "";
        ConfigManger.Save();
    }
}