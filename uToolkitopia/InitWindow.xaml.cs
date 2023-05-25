﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using Core.SDKs;
using Core.SDKs.Config;
using Core.SDKs.Services;
using Core.ViewModel;
using Kitopia.SDKs;
using Kitopia.View;
using Microsoft.Extensions.DependencyInjection;
using Wpf.Ui.Controls;
using MessageBox = System.Windows.MessageBox;

namespace Kitopia;

/// <summary>
///     MainWindows.xaml 的交互逻辑
/// </summary>
public partial class InitWindow 
{
    /// <summary>
    ///     记录快捷键注册项的唯一标识符
    /// </summary>
    private Dictionary<EHotKeySetting, int> m_HotKeySettings = new();

    /// <summary>
    ///     当前窗口句柄
    /// </summary>
    private IntPtr m_Hwnd;

    public InitWindow()
    {
        InitializeComponent();
        DataContext = ServiceManager.Services.GetService<InitWindowsViewModel>();
        var currentTheme = Wpf.Ui.Appearance.Theme.GetAppTheme();
        if (ConfigManger.config.themeFollowSys&&!Wpf.Ui.Appearance.Theme.IsAppMatchesSystem())
        {
            Wpf.Ui.Appearance.Theme.Apply(currentTheme == Wpf.Ui.Appearance.ThemeType.Light ? Wpf.Ui.Appearance.ThemeType.Dark : Wpf.Ui.Appearance.ThemeType.Light);
        }else if (ConfigManger.config.isDark)
        {
            Wpf.Ui.Appearance.Theme.Apply(Wpf.Ui.Appearance.ThemeType.Dark);
        }
        
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
        HotKeySettingsManager.Instance.RegisterGlobalHotKeyEvent += Instance_RegisterGlobalHotKeyEvent;
    }

    /// <summary>
    ///     所有控件初始化完成后调用
    /// </summary>
    /// <param name="e"></param>
    protected override void OnContentRendered(EventArgs e)
    {
        base.OnContentRendered(e);
        // 注册热键
        InitHotKey();
    }

    /// <summary>
    ///     通知注册系统快捷键事件处理函数
    /// </summary>
    /// <param name="hotKeyModelList"></param>
    /// <returns></returns>
    private bool Instance_RegisterGlobalHotKeyEvent(List<HotKeyModel> hotKeyModelList)
    {
        return InitHotKey(hotKeyModelList);
    }


    /// <summary>
    ///     初始化注册快捷键
    /// </summary>
    /// <param name="hotKeyModelList">待注册热键的项</param>
    /// <returns>true:保存快捷键的值；false:弹出设置窗体</returns>
    private bool InitHotKey(List<HotKeyModel> hotKeyModelList = null!)
    {
        var list = hotKeyModelList ?? ConfigManger.config.hotKeys;
        // 注册全局快捷键
        var failList = HotKeyHelper.RegisterGlobalHotKey(list, m_Hwnd, out m_HotKeySettings);
        if (string.IsNullOrEmpty(failList))
            return true;
        var mbResult = MessageBox.Show(string.Format("无法注册下列快捷键\n\r{0}是否要改变这些快捷键？", failList), "提示",
            MessageBoxButton.YesNo);
        // 弹出热键设置窗体

        return true;
    }

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
            case HotKeyManager.WM_HOTKEY:
                var sid = wideParam.ToInt32();
                if (sid == m_HotKeySettings[EHotKeySetting.显示搜索框])
                {
                    //Console.WriteLine(App.Current.Services.GetService<SearchView>().Visibility);
                    if (ServiceManager.Services.GetService<SearchWindow>().Visibility == Visibility.Visible)
                    {
                        ServiceManager.Services.GetService<SearchWindow>().Visibility = Visibility.Hidden;
                    }
                    else
                    {
                        ServiceManager.Services.GetService<SearchWindowViewModel>().CheckClipboard();
                        ServiceManager.Services.GetService<SearchWindow>().Visibility = Visibility.Visible;
                        ServiceManager.Services.GetService<SearchWindow>().Topmost = true;
                        ServiceManager.Services.GetService<SearchWindow>().tx.Focus();
                        ServiceManager.Services.GetService<SearchWindow>().Topmost = false;
                        if (ConfigManger.config.canReadClipboard)
                        {
                            IDataObject data = Clipboard.GetDataObject();
                            if (data.GetDataPresent(DataFormats.Text))
                            {
                                string text = (string)data.GetData(DataFormats.Text);
                                if (text.StartsWith("\""))
                                {
                                    text=text.Replace("\"", "");
                                }
                                if (File.Exists(text)||Directory.Exists(text))
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
        ServiceManager.Services.GetService<MainWindow>().Visibility = Visibility.Visible;
    }
}