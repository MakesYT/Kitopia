using System.Collections.ObjectModel;
using Avalonia.Threading;
using Core.SDKs.CustomScenario;
using Core.SDKs.Services;
using Core.SDKs.Services.Config;
using Kitopia.SDKs;
using log4net;
using Microsoft.Extensions.DependencyInjection;
using PluginCore;
using SharpHook;
using SharpHook.Native;
using SharpHook.Reactive;
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
    public static SimpleReactiveGlobalHook hook;
    public static event EventHandler<HotKeyModel> OnHotKeyPressed ;

    public static ObservableCollection<HotKeyModel> HotKeys
    {
        get;
        set;
    } = new();

    public static void Init()
    {
        OnHotKeyPressed += (sender, model) =>
        {
            log.Debug($"快捷键{model.MainName} {model.Name}被触发");
        };
        hook = new SimpleReactiveGlobalHook();
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
        foreach (var customScenario in CustomScenarioManger.CustomScenarios)
        {
            HotKeys.Add(customScenario.RunHotKey);
            HotKeys.Add(customScenario.StopHotKey);
        }
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


        foreach (var configHotKey in HotKeys)
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
            OnHotKeyPressed.Invoke( new(), configHotKey);

            switch (configHotKey.MainName)
            {
                case "Kitopia":
                {
                    switch (configHotKey.Name)
                    {
                        case "截图":
                        {
                            log.Debug("截图热键被触发");
                            Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                ServiceManager.Services.GetService<IScreenCapture>()!.CaptureScreen();
                            }).GetTask().ContinueWith((e) =>
                            {
                                if (e.IsFaulted)
                                {
                                    log.Error(e.Exception);
                                    ServiceManager.Services.GetService<IErrorWindow>()!.ShowErrorWindow("截图失败", e.Exception.Message);
                                }
                            });
                            break;
                        }
                        case "显示搜索框":
                        {
                            
                            log.Debug("显示搜索框热键被触发");
                            ServiceManager.Services.GetService<ISearchWindowService>()!.ShowOrHiddenSearchWindow();

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