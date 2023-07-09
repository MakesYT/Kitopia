﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using Core.SDKs.HotKey;
using Core.SDKs.Services;
using Core.SDKs.Services.Config;
using Core.ViewModel;
using Kitopia.SDKs;
using Kitopia.View;
using log4net;
using Microsoft.Extensions.DependencyInjection;
using Wpf.Ui.Controls;
using MessageBoxResult = Kitopia.Controls.MessageBoxControl.MessageBoxResult;

namespace Kitopia;

public partial class InitWindow
{
    /// <summary>
    ///     记录快捷键注册项的唯一标识符
    /// </summary>
    private Dictionary<string, int> m_HotKeySettings = new();

    /// <summary>
    ///     当前窗口句柄
    /// </summary>
    private IntPtr m_Hwnd;

    private static readonly ILog log = LogManager.GetLogger(nameof(InitWindow));

    public InitWindow()
    {
        InitializeComponent();
        DataContext = ServiceManager.Services.GetService<InitWindowsViewModel>();

        var currentTheme = Wpf.Ui.Appearance.Theme.GetAppTheme();
        if (ConfigManger.Config.themeChoice == "跟随系统" && !Wpf.Ui.Appearance.Theme.IsAppMatchesSystem())
        {
            log.Debug("主题跟随系统,当前不符合切换主题");


            Wpf.Ui.Appearance.Theme.Apply(currentTheme == Wpf.Ui.Appearance.ThemeType.Light
                ? Wpf.Ui.Appearance.ThemeType.Dark
                : Wpf.Ui.Appearance.ThemeType.Light);
        }
        else if (ConfigManger.Config.themeChoice == "深色")
        {
            log.Debug("主题切换到深色");


            Wpf.Ui.Appearance.Theme.Apply(Wpf.Ui.Appearance.ThemeType.Dark);
        }

        ServiceManager.Services.GetService<SearchWindow>().Visibility = Visibility.Hidden;
    }

    /// <summary>
    ///     WPF窗体的资源初始化完成，并且可以通过WindowInteropHelper获得该窗体的句柄用来与Win32交互后调用
    /// </summary>
    /// <param name="e"></param>
    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        // 获取窗体句柄
        m_Hwnd = new WindowInteropHelper(this).Handle;
        var hWndSource = HwndSource.FromHwnd(m_Hwnd);

        // 添加处理程序
        if (hWndSource != null) hWndSource.AddHook(WndProc);
    }

    /// <summary>
    ///     所有控件初始化完成后调用
    /// </summary>
    /// <param name="e"></param>
    protected override void OnContentRendered(EventArgs e)
    {
        base.OnContentRendered(e);
        // 注册热键

        log.Debug("注册热键");


        InitHotKey();
    }

    public bool HotKeySet(HotKeyModel hotKeyModel)
    {
        if (HotKeyHelper.RegisterGlobalHotKey(new[] { hotKeyModel }, m_Hwnd, out m_HotKeySettings).Any())
        {
            return false;
        }

        return true;
    }

    /// <summary>
    ///     初始化注册快捷键
    /// </summary>
    /// <param name="hotKeyModelList">待注册热键的项</param>
    /// <returns>true:保存快捷键的值；false:弹出设置窗体</returns>
    public bool InitHotKey()
    {
        var list = ConfigManger.Config.hotKeys;
        var failList = HotKeyHelper.RegisterGlobalHotKey(list, m_Hwnd, out m_HotKeySettings);


        if (!failList.Any())
            return true;
        string fail = "";
        foreach (var hotKeyModel in failList)
        {
            fail += $"{hotKeyModel.MainName}_{hotKeyModel.Name}\n";
        }

        Controls.MessageBoxControl.MessageBox msg = new Controls.MessageBoxControl.MessageBox();
        msg.Title = "Kitopia";
        msg.Content = $"无法注册下列快捷键\n{fail}\n现在你需要重新设置\n在设置界面按下取消以取消该快捷键注册";
        msg.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        msg.CloseButtonText = "确定";
        msg.FontSize = 15;
        var task = msg.ShowDialogAsync();
        // 使用ContinueWith来在任务完成后执行一个回调函数
        task.ContinueWith(e =>
        {
            MessageBoxResult result = e.Result;
        }).Wait();
        List<HotKeyModel> itemsToRemove = new List<HotKeyModel>();
        foreach (var hotKeyModel in failList)
        {
            HotKeyEditorWindow hotKeyEditor = new HotKeyEditorWindow($"{hotKeyModel.MainName}_{hotKeyModel.Name}");
            if (ServiceManager.Services.GetService<MainWindow>().Visibility != Visibility.Visible)
            {
                hotKeyEditor.Height = 371;
                hotKeyEditor.Width = 600;
                hotKeyEditor.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
            else
            {
                hotKeyEditor.Height = ServiceManager.Services.GetService<MainWindow>().Height / 2;
                hotKeyEditor.Width = ServiceManager.Services.GetService<MainWindow>().Width / 2;
                hotKeyEditor.Owner = ServiceManager.Services.GetService<MainWindow>();
                hotKeyEditor.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }

            hotKeyEditor.Topmost = true;

            hotKeyEditor.Title = "修改快捷键";
            hotKeyEditor.ShowDialog();
        }


        // 注册全局快捷键

        // 弹出热键设置窗体

        return true;
    }

    [DllImport("user32.dll", EntryPoint = "SetForegroundWindow", SetLastError = true)]
    public static extern void SetForegroundWindow(IntPtr hwnd);

    /// <summary>
    ///     窗体回调函数，接收所有窗体消息的事件处理函数
    /// </summary>
    /// <param name="hWnd">窗口句柄</param>
    /// <param name="msg">消息</param>
    /// <param name="wideParam">附加参数1</param>
    /// <param name="longParam">附加参数2</param>
    /// <param name="handled">是否处理</param>
    /// <returns>返回句柄</returns>
    private IntPtr WndProc(IntPtr hWnd, int msg, IntPtr wideParam, IntPtr longParam, ref bool handled)
    {
        switch (msg)
        {
            case 0x312:
                var sid = wideParam.ToInt32();
                if (sid == m_HotKeySettings["Kitopia_显示搜索框"])
                {
                    log.Debug("显示搜索框热键被触发");


                    //Console.WriteLine(App.Current.Services.GetService<SearchView>().Visibility);
                    if (ServiceManager.Services.GetService<SearchWindow>().Visibility == Visibility.Visible)
                    {
                        ServiceManager.Services.GetService<SearchWindow>().Visibility = Visibility.Hidden;
                    }
                    else
                    {
                        ServiceManager.Services.GetService<SearchWindowViewModel>().CheckClipboard();


                        ServiceManager.Services.GetService<SearchWindow>().Show();
                        SetForegroundWindow(new WindowInteropHelper(ServiceManager.Services.GetService<SearchWindow>())
                            .Handle);
                        ServiceManager.Services.GetService<SearchWindow>().tx.Focus();

                        if (ConfigManger.Config.canReadClipboard)
                        {
                            IDataObject data = Clipboard.GetDataObject();
                            if (data.GetDataPresent(DataFormats.Text))
                            {
                                string text = (string)data.GetData(DataFormats.Text);
                                if (text.StartsWith("\""))
                                {
                                    text = text.Replace("\"", "");
                                }

                                if (File.Exists(text) || Directory.Exists(text))
                                {
                                    ServiceManager.Services.GetService<SearchWindowViewModel>().Search = text;
                                }
                            }
                        }

                        ServiceManager.Services.GetService<SearchWindow>().tx.SelectAll();
                        ThreadPool.QueueUserWorkItem((e) =>
                        {
                            ServiceManager.Services.GetService<SearchWindowViewModel>().ReloadApps();
                        });
                    }
                }

                handled = true;
                break;
        }

        return IntPtr.Zero;
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
    }

    private void MenuItem_Click(object sender, RoutedEventArgs e)
    {
        ConfigManger.Save();
        Environment.Exit(0);
    }

    private void NotifyIcon_OnLeftDoubleClick(NotifyIcon sender, RoutedEventArgs e)
    {
        ServiceManager.Services.GetService<MainWindow>().Show();
    }
}