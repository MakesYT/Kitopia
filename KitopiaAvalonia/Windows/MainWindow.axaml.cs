using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Styling;
using Core.SDKs;
using Core.SDKs.CustomScenario;
using Core.SDKs.HotKey;
using Core.SDKs.Services;
using Core.SDKs.Services.Config;
using Core.ViewModel;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Navigation;
using FluentAvalonia.UI.Windowing;
using Kitopia.SDKs;
using KitopiaAvalonia.Pages;
using KitopiaAvalonia.Windows;
using log4net;
using Microsoft.Extensions.DependencyInjection;
using PluginCore;
using Vanara.PInvoke;

namespace KitopiaAvalonia;

public class NavigationPageFactory : INavigationPageFactory
{
    public Control GetPage(Type srcType) => throw new NotImplementedException();

    public Control GetPageFromObject(object target)
    {
        if (target is string s)
        {
            return ServiceManager.Services.GetKeyedService<UserControl>(s);
        }

        return null;
    }
}

public partial class MainWindow : AppWindow
{
    private static readonly ILog log = LogManager.GetLogger(nameof(MainWindow));
    static User32.WindowProc _wndProcDelegate;
    private SafeHandle msgWindowHandle;

    public MainWindow()
    {
        InitializeComponent();
        RenderOptions.SetTextRenderingMode(this, TextRenderingMode.Antialias);
        TitleBar.ExtendsContentIntoTitleBar = true;
        TitleBar.TitleBarHitTestType = TitleBarHitTestType.Complex;
        FrameView.NavigationPageFactory = new NavigationPageFactory();
        foreach (NavigationViewItem nvi in NavView.MenuItems)
        {
            if (nvi.Tag == FrameView.Tag)
            {
                NavView.SelectedItem = nvi;
                SetNVIIcon(nvi, true);
            }
            else
            {
                SetNVIIcon(nvi, false);
            }
        }

        foreach (NavigationViewItem nvi in NavView.FooterMenuItems)
        {
            if (nvi.Tag == FrameView.Tag)
            {
                NavView.SelectedItem = nvi;
                SetNVIIcon(nvi, true);
            }
            else
            {
                SetNVIIcon(nvi, false);
            }
        }

        IsVisible = false;
    }

    private IReadOnlyDictionary<Type, string> Pages => new Dictionary<Type, string>
    {
        { typeof(HomePage), "HomePage" },
        { typeof(PluginManagerPage), "PluginManagerPage" },
        { typeof(CustomScenariosManagerPage), "CustomScenariosManagerPage" },
        { typeof(HotKeyManagerPage), "HotKeyManagerPage" },
        { typeof(SettingPage), "SettingPage" }
    };

    public void InitHook()
    {
        _wndProcDelegate = new User32.WindowProc(WndProc);
        User32.WNDCLASSEX wndClassEx = new User32.WNDCLASSEX
        {
            cbSize = (uint)Marshal.SizeOf<User32.WNDCLASSEX>(),
            lpfnWndProc = _wndProcDelegate,
            hInstance = Kernel32.GetModuleHandle(null),
            lpszClassName = "YourAppNameOrSomething" + Guid.NewGuid(),
        };

        ushort atom = User32.RegisterClassEx(wndClassEx);

        if (atom == 0)
            throw new Win32Exception();

        msgWindowHandle = User32.CreateWindowEx(0, (IntPtr)atom, null, 0, 0, 0, 0, 0, IntPtr.Zero, IntPtr.Zero,
            IntPtr.Zero, IntPtr.Zero);
        InitHotKey(msgWindowHandle.DangerousGetHandle());
    }

    public override void EndInit()
    {
        base.EndInit();
    }

    public void InitHotKey(IntPtr handle)
    {
        var list = ConfigManger.Config.hotKeys;
        var failList = HotKeyHelper.RegisterGlobalHotKey(list, handle, out var hotKeySettingsDic);
        if (!failList.Any())
        {
            return;
        }

        var fail = "";
        foreach (var hotKeyModel in failList)
        {
            fail += $"{hotKeyModel.MainName}_{hotKeyModel.Name}\n";
        }

        ServiceManager.Services.GetService<IContentDialog>().ShowDialogAsync(null, new DialogContent("Kitopia",
            new TextBlock()
            {
                Text = $"无法注册下列快捷键\n{fail}\n现在你需要重新设置",
                FontSize = 15
            }, "取消", null, "确定", () =>
            {
            }, null, () =>
            {
                log.Info("用户取消启用开机自启");
            }));
        foreach (var hotKeyModel in failList)
        {
            var hotKeyEditor = new HotKeyEditorWindow(hotKeyModel);
            hotKeyEditor.Topmost = true;
            hotKeyEditor.Title = "修改快捷键";
            if (!ServiceManager.Services.GetService<MainWindow>().IsVisible)
            {
                hotKeyEditor.Height = 371;
                hotKeyEditor.Width = 600;
                hotKeyEditor.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                hotKeyEditor.Show();
            }
            else
            {
                hotKeyEditor.Height = ServiceManager.Services.GetService<MainWindow>().Height / 2;
                hotKeyEditor.Width = ServiceManager.Services.GetService<MainWindow>().Width / 2;
                hotKeyEditor.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                hotKeyEditor.Show(ServiceManager.Services.GetService<MainWindow>());
            }
        }


        // 注册全局快捷键

        // 弹出热键设置窗体
    }

    private static IntPtr WndProc(HWND hwnd, uint uMsg, IntPtr wParam, IntPtr lParam)
    {
        var windowMessage = (User32.WindowMessage)uMsg;
        log.Debug($"窗口消息: {windowMessage}");
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
                var sid = wParam.ToInt32();
                if (!HotKeyHelper.MHotKeySettingsDic.ContainsValue(sid))
                {
                    break;
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
                                if (ServiceManager.Services.GetService<SearchWindow>()!.IsVisible)
                                {
                                    ServiceManager.Services.GetService<SearchWindow>()!.IsVisible = false;
                                }
                                else
                                {
                                    ServiceManager.Services.GetService<SearchWindowViewModel>()!.CheckClipboard();

                                    ServiceManager.Services.GetService<SearchWindow>()!.Show();

                                    User32.SetForegroundWindow(ServiceManager.Services.GetService<SearchWindow>()
                                        .TryGetPlatformHandle().Handle);
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
                            break;
                        }

                        switch (strings[1])
                        {
                            case "激活快捷键":
                            {
                                firstOrDefault.Run();
                                ((IToastService)ServiceManager.Services.GetService(typeof(IToastService))!).Show("情景",
                                    $"情景{firstOrDefault.Name}运行");
                                break;
                            }
                            case "停止快捷键":
                            {
                                firstOrDefault.Stop();
                                ((IToastService)ServiceManager.Services.GetService(typeof(IToastService))!).Show("情景",
                                    $"情景{firstOrDefault.Name}停止");
                                break;
                            }
                        }

                        break;
                    }
                }


                break;
        }

        return User32.DefWindowProc(hwnd, uMsg, wParam, lParam);
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        if (VisualRoot is AppWindow aw)
        {
            TitleBarHost.ColumnDefinitions[3].Width = new GridLength(aw.TitleBar.RightInset, GridUnitType.Pixel);
        }
    }

    private void Window_OnClosing(object? sender, WindowClosingEventArgs e)
    {
        this.IsVisible = false;
        e.Cancel = true;
    }

    private void NavView_OnItemInvoked(object? sender, NavigationViewItemInvokedEventArgs e)
    {
        FrameView.NavigateFromObject(e.InvokedItemContainer.Tag);
    }

    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
    }

    public bool HotKeySet(HotKeyModel hotKeyModel)
    {
        if (!HotKeyHelper
                .RegisterGlobalHotKey(new[] { hotKeyModel }, msgWindowHandle.DangerousGetHandle(),
                    out var hotKeySettingsDic)
                .Any())
        {
            return true;
        }

        return false;
    }

    public void RemoveHotKey(HotKeyModel hotKeyModel)
    {
        HotKeyHelper.UnRegisterHotKey(hotKeyModel, msgWindowHandle.DangerousGetHandle());
    }

    private void FrameView_OnNavigated(object sender, NavigationEventArgs e)
    {
        FrameView.Tag = Pages.GetValueOrDefault(e.Content.GetType());
        foreach (NavigationViewItem nvi in NavView.MenuItems)
        {
            if (nvi.Tag == FrameView.Tag)
            {
                NavView.SelectedItem = nvi;
                SetNVIIcon(nvi, true);
            }
            else
            {
                SetNVIIcon(nvi, false);
            }
        }

        foreach (NavigationViewItem nvi in NavView.FooterMenuItems)
        {
            if (nvi.Tag == FrameView.Tag)
            {
                NavView.SelectedItem = nvi;
                SetNVIIcon(nvi, true);
            }
            else
            {
                SetNVIIcon(nvi, false);
            }
        }

        if (FrameView.BackStackDepth > 0 && !NavView.IsBackButtonVisible)
        {
            AnimateContentForBackButton(true);
        }
        else if (FrameView.BackStackDepth == 0 && NavView.IsBackButtonVisible)
        {
            AnimateContentForBackButton(false);
        }
    }

    private void SetNVIIcon(NavigationViewItem item, bool selected)
    {
        // Technically, yes you could set up binding and converters and whatnot to let the icon change
        // between filled and unfilled based on selection, but this is so much simpler 

        if (item == null)
            return;

        var t = item.Tag;
        switch (t)
        {
            case "HomePage":
            {
                item.IconSource = this.TryFindResource(selected ? "HomeIconFilled" : "HomeIcon", out var value)
                    ? (IconSource)value
                    : null;
                break;
            }
            case "PluginManagerPage":
            {
                item.IconSource = this.TryFindResource(selected ? "AppsIconFilled" : "AppsIcon", out var value)
                    ? (IconSource)value
                    : null;
                break;
            }
            case "CustomScenariosManagerPage":
            {
                item.IconSource =
                    this.TryFindResource(selected ? "AppListDetailIconFilled" : "AppListDetailIcon", out var value)
                        ? (IconSource)value
                        : null;
                break;
            }
            case "HotKeyManagerPage":
            {
                item.IconSource = this.TryFindResource(selected ? "KeyboardIconFilled" : "KeyboardIcon", out var value)
                    ? (IconSource)value
                    : null;
                break;
            }
            case "SettingPage":
            {
                item.IconSource = this.TryFindResource(selected ? "SettingsIconFilled" : "SettingsIcon", out var value)
                    ? (IconSource)value
                    : null;
                break;
            }
        }
    }

    private async void AnimateContentForBackButton(bool show)
    {
        if (!WindowIcon.IsVisible)
            return;

        if (show)
        {
            var ani = new Animation
            {
                Duration = TimeSpan.FromMilliseconds(250),
                FillMode = FillMode.Forward,
                Children =
                {
                    new KeyFrame
                    {
                        Cue = new Cue(0d),
                        Setters =
                        {
                            new Setter(MarginProperty, new Thickness(12, 4, 12, 4))
                        }
                    },
                    new KeyFrame
                    {
                        Cue = new Cue(1d),
                        KeySpline = new KeySpline(0, 0, 0, 1),
                        Setters =
                        {
                            new Setter(MarginProperty, new Thickness(48, 4, 12, 4))
                        }
                    }
                }
            };

            await ani.RunAsync(WindowIcon);

            NavView.IsBackButtonVisible = true;
        }
        else
        {
            NavView.IsBackButtonVisible = false;

            var ani = new Animation
            {
                Duration = TimeSpan.FromMilliseconds(250),
                FillMode = FillMode.Forward,
                Children =
                {
                    new KeyFrame
                    {
                        Cue = new Cue(0d),
                        Setters =
                        {
                            new Setter(MarginProperty, new Thickness(48, 4, 12, 4))
                        }
                    },
                    new KeyFrame
                    {
                        Cue = new Cue(1d),
                        KeySpline = new KeySpline(0, 0, 0, 1),
                        Setters =
                        {
                            new Setter(MarginProperty, new Thickness(12, 4, 12, 4))
                        }
                    }
                }
            };

            await ani.RunAsync(WindowIcon);
        }
    }

    private void NavView_OnBackRequested(object? sender, NavigationViewBackRequestedEventArgs e)
    {
        FrameView.GoBack();
    }
}