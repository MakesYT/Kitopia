using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using Core.SDKs.CustomScenario;
using Core.SDKs.Services;
using Core.SDKs.Services.Config;
using Core.ViewModel;
using Kitopia.SDKs;
using KitopiaAvalonia.Windows;
using log4net;
using Microsoft.Extensions.DependencyInjection;
using PluginCore;
using SharpHook;
using SharpHook.Native;
using SharpHook.Reactive;
using Vanara.PInvoke;
using Timer = System.Timers.Timer;

namespace Core.SDKs.HotKey;

public class HotKeyManager
{
    public static bool MetaIsPressed = false;
    public static bool AltIsPressed = false;
    public static bool ShiftIsPressed = false;
    public static bool CtrlIsPressed = false;
    private static TimerHelper? _timer;
    private static readonly ILog log = LogManager.GetLogger(nameof(HotKeyManager));
    private static Timer keyPressTimer = new(1000);
    private static bool canPress = true;

    public static void Init()
    {
        var hook = new SimpleReactiveGlobalHook();

        hook.KeyPressed.Subscribe(OnKeyPressed);
        hook.KeyReleased.Subscribe(OnKeyReleased);
        hook.MousePressed.Subscribe(OnMousePressed);
        hook.MouseReleased.Subscribe(OnMouseReleased);
        keyPressTimer.AutoReset = true;
        keyPressTimer.Elapsed += (_timer, _) =>
        {
            canPress = true;
        };
        hook.RunAsync();
    }

    private static void OnKeyPressed(KeyboardHookEventArgs e)
    {
        switch (e.Data.KeyCode)
        {
            case KeyCode.VcLeftControl or KeyCode.VcRightControl:
                CtrlIsPressed = true;
                return;
            case KeyCode.VcLeftShift or KeyCode.VcRightShift:
                ShiftIsPressed = true;
                return;
            case KeyCode.VcLeftAlt or KeyCode.VcRightAlt:
                AltIsPressed = true;
                return;
            case KeyCode.VcLeftMeta or KeyCode.VcRightMeta:
                MetaIsPressed = true;
                return;
        }

        if (!canPress)
        {
            return;
        }

        keyPressTimer.Start();

        canPress = false;


        foreach (var configHotKey in ConfigManger.Config.hotKeys)
        {
            if (!configHotKey.IsUsable || configHotKey.IsSelectAlt != AltIsPressed ||
                configHotKey.IsSelectCtrl != CtrlIsPressed || configHotKey.IsSelectShift != ShiftIsPressed ||
                configHotKey.IsSelectWin != MetaIsPressed)
            {
                continue;
            }

            if ((int)configHotKey.SelectKey != (int)e.Data.KeyCode)
            {
                continue;
            }

            switch (configHotKey.MainName)
            {
                case "Kitopia":
                {
                    switch (configHotKey.Name)
                    {
                        case "显示搜索框":
                        {
                            log.Debug("显示搜索框热键被触发");
                            Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                if (ServiceManager.Services.GetService<SearchWindow>()!.IsVisible)
                                {
                                    ServiceManager.Services.GetService<SearchWindow>()!.IsVisible = false;
                                }
                                else
                                {
                                    ServiceManager.Services.GetService<SearchWindowViewModel>()!.CheckClipboard();

                                    ServiceManager.Services.GetService<SearchWindow>()!.Show();

                                    ServiceManager.Services.GetService<SearchWindow>()!.Focus();
                                    ServiceManager.Services.GetService<SearchWindow>()!.tx.Focus();
                                    ServiceManager.Services.GetService<SearchWindow>()!.tx.SelectAll();
                                    Task.Run(() =>
                                    {
                                        Thread.CurrentThread.Priority = ThreadPriority.Lowest;
                                        ServiceManager.Services.GetService<SearchWindowViewModel>()!.ReloadApps();
                                    });
                                }
                            });


                            break;
                        }
                    }

                    break;
                }
                case "Kitopia情景":
                {
                    var strings = configHotKey.Name.Split("_", 2);
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
        }
    }

    private static void OnKeyReleased(KeyboardHookEventArgs e)
    {
        switch (e.Data.KeyCode)
        {
            case KeyCode.VcLeftControl or KeyCode.VcRightControl:
                CtrlIsPressed = false;
                return;
            case KeyCode.VcLeftShift or KeyCode.VcRightShift:
                ShiftIsPressed = false;
                return;
            case KeyCode.VcLeftAlt or KeyCode.VcRightAlt:
                AltIsPressed = false;
                return;
            case KeyCode.VcLeftMeta or KeyCode.VcRightMeta:
                MetaIsPressed = false;
                return;
        }

        canPress = true;
    }

    private static void OnMousePressed(MouseHookEventArgs e)
    {
        if ((int)e.Data.Button != (int)ConfigManger.Config.mouseKey)
        {
            return;
        }

        if (_timer is null)
        {
            _timer = new TimerHelper(ConfigManger.Config.mouseKeyInverval, () =>
            {
                log.Debug("快捷键触发");
                ServiceManager.Services.GetService<IMouseQuickWindowService>()!.Open();
            });
        }

        _timer.StartTimer();
    }

    private static void OnMouseReleased(MouseHookEventArgs e)
    {
        if ((int)e.Data.Button != (int)ConfigManger.Config.mouseKey)
        {
            return;
        }

        if (_timer is not null)
        {
            _timer.StopTimer();
        }
    }
}