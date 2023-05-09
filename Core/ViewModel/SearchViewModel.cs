﻿using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.SDKs;
using Core.SDKs.Config;
using Kitopia.SDKs.Everything;

namespace Core.ViewModel;

public partial class SearchViewModel : ObservableRecipient
{
    private List<SearchViewItem> _collection = new(); //存储本机所有软件


    [ObservableProperty] private bool? _everythingIsOk;

    [ObservableProperty] private BindingList<SearchViewItem> _items = new(); //搜索界面显示的软件

    private List<string> _names = new(); //软件去重
    private GetIconFromFile _fromFile = new GetIconFromFile();

    [ObservableProperty] private string? _search;


    [ObservableProperty] private int? _selectedIndex = -1;

    public SearchViewModel()
    {
        ReloadApps();
        LoadLast();
    }

    public void ReloadApps()
    {
        Items.Clear();
        //_fromFile.ClearCache(); 
        _collection.Clear();
        _names.Clear();
        
        AppSolver.GetAllApps( _collection,  _names);
        GC.Collect();
    }

    public void LoadLast()
    {
        Items.Clear();
        if (ConfigManger.config.lastOpens.Any())
            foreach (var name in ConfigManger.config.lastOpens)
            foreach (var searchViewItem in _collection)
                if (searchViewItem.fileName.Equals(name))
                    Items.Add((SearchViewItem)searchViewItem.Clone());
        OnItemsChanged(Items);
    }

    partial void OnItemsChanged(BindingList<SearchViewItem>? viewItem)
    {
        
        if (viewItem is null)
        {
            return;
        }

        for (var i = 0; i < viewItem.Count; i++)
        {
            if (viewItem[i].icon == null&&viewItem[i].fileType!=FileType.文件夹)
            {
                
                viewItem[i].icon = (Icon)_fromFile.GetIcon(viewItem[i].fileInfo.FullName).Clone();
                
                
            }
        }
        
    }
    partial void OnSearchChanged(string? value)
    {
        if (value == null || value == "")
        {
            
            LoadLast();
            return;
        }

        
        
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


        Items.Clear();
        Tools.main(ref _items, value);
        GC.Collect();

        if (value.Contains("\\")||value.Contains("/"))
        {
            
            if (Path.HasExtension(value)&&File.Exists(value))
            {
                Items.Add(new SearchViewItem()
                {
                    fileInfo = new FileInfo(value),
                    fileName = LnkSolver.GetLocalizedName(value),
                    fileType = FileType.应用程序,
                    IsVisible = true
                });
            }
            else if (Directory.Exists(value))
            {
                Items.Add(new SearchViewItem()
                                {
                                    directoryInfo = new DirectoryInfo(value),
                                    fileName = "打开此文件夹?",
                                    fileType = FileType.文件夹,
                                    icon = null,
                                    IsVisible = true
                                });
            }
            
        }
        
        // 使用LINQ语句来简化和优化筛选和排序逻辑，而不需要使用foreach循环和if判断
        // 根据给定的值，从集合中筛选出符合条件的SearchViewItem对象，并计算它们的权重
        var filtered = from item in _collection
            let keys = item.keys.Where(key => !string.IsNullOrEmpty(key)) // 排除空的键
            let weight = keys.Count(key => key.Contains(value)) * 2 // 统计包含给定值的键的数量
                         + keys.Count(key => key.StartsWith(value)) * 3 // 统计以给定值开头的键的数量，并乘以500
                         + keys.Count(key => key.Equals(value)) * 5 // 统计等于给定值的键的数量，并乘以1000
            where weight > 0 // 只选择权重为正的对象
            select new { Item = item, Weight = weight }; // 创建一个包含对象和权重属性的匿名类型

        // 按照权重降序排序筛选出的对象
        var sorted = filtered.OrderByDescending(x => x.Weight);

        // 将排序后的对象添加到Items集合中
        foreach (var x in sorted) Items.Add((SearchViewItem)x.Item.Clone());
        OnItemsChanged(Items);
    }


    [RelayCommand]
    public void OpenFile(SearchViewItem searchViewItem)
    {
        if (searchViewItem.fileInfo!=null)
        {
            ShellTools.ShellExecute(IntPtr.Zero, "open", searchViewItem.fileInfo.FullName, "", "",
                        ShellTools.ShowCommands.SW_SHOWNORMAL);
        }
        if (searchViewItem.directoryInfo!=null)
        {
            ShellTools.ShellExecute(IntPtr.Zero, "open", searchViewItem.directoryInfo.FullName, "", "",
                ShellTools.ShowCommands.SW_SHOWNORMAL);
        }

        if (!ConfigManger.config.lastOpens.Contains(searchViewItem.fileName))
            ConfigManger.config.lastOpens.Insert(0, searchViewItem.fileName);
        if (ConfigManger.config.lastOpens.Count > 4) ConfigManger.config.lastOpens.RemoveAt(4);
        Search = "";
        ConfigManger.Save();
    }

    [RelayCommand]
    public void OpenFolder(SearchViewItem searchViewItem)
    {
        ShellTools.ShellExecute(IntPtr.Zero, "open", searchViewItem.fileInfo.DirectoryName, "", "",
            ShellTools.ShowCommands.SW_SHOWNORMAL);

        if (!ConfigManger.config.lastOpens.Contains(searchViewItem.fileName))
            ConfigManger.config.lastOpens.Insert(0, searchViewItem.fileName);
        if (ConfigManger.config.lastOpens.Count() > 4) ConfigManger.config.lastOpens.RemoveAt(4);
        Search = "";
        ConfigManger.Save();
    }

    [RelayCommand]
    public void RunAsAdmin(SearchViewItem searchViewItem)
    {
        ShellTools.ShellExecute(IntPtr.Zero, "runas", searchViewItem.fileInfo.FullName, "", "",
            ShellTools.ShowCommands.SW_SHOWNORMAL);

        if (!ConfigManger.config.lastOpens.Contains(searchViewItem.fileName))
            ConfigManger.config.lastOpens.Insert(0, searchViewItem.fileName);
        if (ConfigManger.config.lastOpens.Count() > 4) ConfigManger.config.lastOpens.RemoveAt(4);
        Search = "";
        ConfigManger.Save();
    }

    [RelayCommand]
    public void OpenFolderInTerminal(SearchViewItem searchViewItem)
    {
        Process.Start("cmd", "/c cd " + searchViewItem.fileInfo.DirectoryName + " && start cmd.exe");

        if (!ConfigManger.config.lastOpens.Contains(searchViewItem.fileName))
            ConfigManger.config.lastOpens.Insert(0, searchViewItem.fileName);
        if (ConfigManger.config.lastOpens.Count() > 4) ConfigManger.config.lastOpens.RemoveAt(4);
        Search = "";
        ConfigManger.Save();
    }
}