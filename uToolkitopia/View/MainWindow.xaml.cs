#region

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using Core.SDKs.HotKey;
using Core.SDKs.Services;
using Core.SDKs.Services.Config;
using Core.ViewModel;
using Kitopia.SDKs;
using log4net;
using Microsoft.Extensions.DependencyInjection;
using Vanara.PInvoke;
using Wpf.Ui;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;
using MessageBox = Kitopia.Controls.MessageBoxControl.MessageBox;

#endregion

namespace Kitopia.View;

public partial class MainWindow
{
    private static readonly ILog log = LogManager.GetLogger(nameof(MainWindow));

    /// <summary>
    ///     当前窗口句柄
    /// </summary>
    private IntPtr m_Hwnd;

    public MainWindow()
    {
        InitializeComponent();
        var currentTheme = ApplicationThemeManager.GetAppTheme();
        ApplicationThemeManager.Changed += (theme, accent) =>
        {
            #region 同步Nodify主题

            Collection<ResourceDictionary> applicationDictionaries = Application
                .Current
                .Resources
                .MergedDictionaries;

            if (applicationDictionaries.Count == 0)
            {
                return;
            }

            for (var i = 0; i < applicationDictionaries.Count; i++)
            {
                string sourceUri;

                if (applicationDictionaries[i]?.Source != null)
                {
                    sourceUri = applicationDictionaries[i].Source.ToString().ToLower().Trim();

                    if (sourceUri.Contains("pack://application:,,,/nodify;component"))
                    {
                        switch (theme)
                        {
                            case ApplicationTheme.Dark:
                                applicationDictionaries[i] = new ResourceDictionary
                                {
                                    Source = new Uri("pack://application:,,,/Nodify;component/Themes/Dark.xaml",
                                        UriKind.Absolute)
                                };
                                break;
                            case ApplicationTheme.Light:
                                applicationDictionaries[i] = new ResourceDictionary
                                {
                                    Source = new Uri("pack://application:,,,/Nodify;component/Themes/Light.xaml",
                                        UriKind.Absolute)
                                };
                                break;
                        }


                        break;
                    }
                }
            }

            log.Debug("Nodify主题切换完成");

            #endregion
        };
        if (ConfigManger.Config.themeChoice == "跟随系统" &&
            !ApplicationThemeManager.IsAppMatchesSystem())
        {
            log.Debug("主题跟随系统,当前不符合切换主题");


            ApplicationThemeManager.Apply(currentTheme == ApplicationTheme.Light
                ? ApplicationTheme.Dark
                : ApplicationTheme.Light);
        }
        else if (ConfigManger.Config.themeChoice == "深色")
        {
            log.Debug("主题切换到深色");


            ApplicationThemeManager.Apply(ApplicationTheme.Dark);
        }

        ServiceManager.Services.GetService<SearchWindow>()!.Visibility = Visibility.Hidden;

        NavigationView.Loaded += (_, _) =>
        {
            var navigationService = ServiceManager.Services.GetService<INavigationService>()!;

            navigationService.SetNavigationControl(NavigationView);
            NavigationView.SetServiceProvider(ServiceManager.Services);
        };
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        // 获取窗体句柄
        m_Hwnd = new WindowInteropHelper(this).Handle;
        var hWndSource = HwndSource.FromHwnd(m_Hwnd);

        // 添加处理程序
        if (hWndSource != null)
        {
            hWndSource.AddHook(WndProc);
        }

        AppDomain.CurrentDomain.ProcessExit += (_, _) =>
        {
            //TODO: 程序退出时触发器
        };
        log.Debug("注册热键");
        InitHotKey();

        ApplicationThemeManager.Changed += (theme, accent) =>
        {
            WindowBackdrop.ApplyBackdrop(m_Hwnd, WindowBackdropType.Acrylic);
        };
    }

    private void MainWindow_OnClosing(object? sender, CancelEventArgs e)
    {
        e.Cancel = true;
        Visibility = Visibility.Hidden;
    }


    public bool HotKeySet(HotKeyModel hotKeyModel)
    {
        if (!HotKeyHelper.RegisterGlobalHotKey(new[] { hotKeyModel }, m_Hwnd, out var hotKeySettingsDic).Any())
        {
            return true;
        }

        return false;
    }

    public void InitHotKey()
    {
        var list = ConfigManger.Config.hotKeys;
        var failList = HotKeyHelper.RegisterGlobalHotKey(list, m_Hwnd, out var hotKeySettingsDic);
        if (!failList.Any())
        {
            return;
        }

        var fail = "";
        foreach (var hotKeyModel in failList)
        {
            fail += $"{hotKeyModel.MainName}_{hotKeyModel.Name}\n";
        }

        var msg = new MessageBox();
        msg.Title = "Kitopia";
        msg.Content = $"无法注册下列快捷键\n{fail}\n现在你需要重新设置\n在设置界面按下取消以取消该快捷键注册";
        msg.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        msg.CloseButtonText = "确定";
        msg.FontSize = 15;
        var task = msg.ShowDialogAsync();
        // 使用ContinueWith来在任务完成后执行一个回调函数
        task.Wait();
        foreach (var hotKeyModel in failList)
        {
            var hotKeyEditor = new HotKeyEditorWindow(hotKeyModel);
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
    }

    private IntPtr WndProc(IntPtr hWnd, int msg, IntPtr wideParam, IntPtr longParam, ref bool handled)
    {
        var windowMessage = (User32.WindowMessage)msg;
        switch (windowMessage)
        {
            case User32.WindowMessage.WM_ACTIVATEAPP:
            {
                break;
            }
            case User32.WindowMessage.WM_QUERYENDSESSION:
            {
                break;
            }
            case User32.WindowMessage.WM_HOTKEY:
                var sid = wideParam.ToInt32();
                if (!HotKeyHelper.MHotKeySettingsDic.ContainsValue(sid))
                {
                    return IntPtr.Zero;
                }

                var key = HotKeyHelper.MHotKeySettingsDic.FirstOrDefault(e => e.Value == sid).Key.Split("_", 2);
                switch (key[0])
                {
                    case "Kitopia":
                    {
                        switch (key[1])
                        {
                            case "显示搜索框":
                            {
                                log.Debug("显示搜索框热键被触发");
                                if (ServiceManager.Services.GetService<SearchWindow>()!.Visibility ==
                                    Visibility.Visible)
                                {
                                    ServiceManager.Services.GetService<SearchWindow>()!.Visibility = Visibility.Hidden;
                                }
                                else
                                {
                                    ServiceManager.Services.GetService<SearchWindowViewModel>()!.CheckClipboard();

                                    ServiceManager.Services.GetService<SearchWindow>()!.Show();

                                    User32.SetForegroundWindow(
                                        new WindowInteropHelper(ServiceManager.Services.GetService<SearchWindow>()!)
                                            .Handle);
                                    ServiceManager.Services.GetService<SearchWindow>()!.tx.Focus();
                                    ServiceManager.Services.GetService<SearchWindow>()!.tx.SelectAll();
                                    Task.Run(() =>
                                    {
                                        Thread.CurrentThread.Priority = ThreadPriority.Lowest;
                                        ServiceManager.Services.GetService<SearchWindowViewModel>()!.ReloadApps();
                                    });
                                }

                                break;
                            }
                        }

                        break;
                    }
                    case "Kitopia情景":
                    {
                        var strings = key[1].Split("_", 2);
                        var firstOrDefault =
                            CustomScenarioManger.CustomScenarios.FirstOrDefault(e => e.UUID == strings[0]);
                        if (firstOrDefault == null)
                        {
                            return IntPtr.Zero;
                        }

                        switch (strings[1])
                        {
                            case "激活快捷键":
                            {
                                firstOrDefault.Run();
                                break;
                            }
                            case "停止快捷键":
                            {
                                firstOrDefault.Stop();
                                break;
                            }
                        }

                        break;
                    }
                }

                handled = true;
                break;
        }

        return IntPtr.Zero;
    }
}