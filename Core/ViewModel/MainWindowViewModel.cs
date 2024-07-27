﻿#region

using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Core.SDKs.Services.Config;

#endregion

namespace Core.ViewModel;

public partial class MainWindowViewModel : ObservableRecipient
{
    public MainWindowViewModel()
    {
        WeakReferenceMessenger.Default.Register<MainWindowViewModel, PageChangeEventArgs>(this, OnNavigation);
    }

    private void OnNavigation(MainWindowViewModel recipient, PageChangeEventArgs message)
    {
        Content = message.Key;
    }

    [ObservableProperty] private object? _content;

    public ObservableCollection<MenuItemViewModel> MenuItems { get; } = new()
    {
        new()
        {
            MenuHeader = "主页",
            Key = "Home",
            MenuIconGlyph = "\uf481",
            MenuIconFilledGlyph = "\uf488",
        },
        new()
        {
            MenuHeader = "市场",
            Key = "Market",
            MenuIconGlyph = "\uf151",
            MenuIconFilledGlyph = "\uf151",
        },
        new()
        {
            MenuHeader = "插件",
            Key = "Plugin",
            MenuIconGlyph = "\uf60a",
            MenuIconFilledGlyph = "\uf614",
        },
        new()
        {
            MenuHeader = "情景",
            Key = "Scenario",
            MenuIconGlyph = "\ue065",
            MenuIconFilledGlyph = "\ue065",
        },
        new()
        {
            MenuHeader = "快捷键",
            Key = "Hotkey",
            MenuIconGlyph = "\uf4b9",
            MenuIconFilledGlyph = "\uf4c3",
        },
    };

    [RelayCommand]
    public void ActivateSettingPage()
    {
        WeakReferenceMessenger.Default.Send<PageChangeEventArgs>(new PageChangeEventArgs("Setting"));
    }

    [RelayCommand]
    public void Exit()
    {
        ConfigManger.Save();
        Environment.Exit(0);
    }
}

public class PageChangeEventArgs
{
    public PageChangeEventArgs(string key)
    {
        Key = key;
    }

    public string Key { get; set; }
}

public class MenuItemViewModel
{
    public string MenuHeader { get; set; }
    public string MenuIconGlyph { get; set; }
    public string MenuIconFilledGlyph { get; set; }

    public string Key { get; set; }
    public bool IsSeparator { get; set; }
    public ObservableCollection<MenuItemViewModel> Children { get; set; } = new();
    public ICommand ActivateCommand { get; set; }

    public MenuItemViewModel()
    {
        ActivateCommand = new RelayCommand(OnActivate);
    }

    private void OnActivate()
    {
        if (IsSeparator) return;
        WeakReferenceMessenger.Default.Send<PageChangeEventArgs>(new PageChangeEventArgs(Key));
    }
}