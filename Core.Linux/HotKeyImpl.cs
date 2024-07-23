using Core.SDKs.CustomScenario;
using Core.SDKs.CustomType;
using Core.SDKs.HotKey;
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

namespace Core.Linux;

public class HotKeyImpl : IHotKetImpl
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
    private static ObservableDictionary<string, HotKeyModel> HotKeys = new();

    public void Init()
    {
        hook = new SimpleReactiveGlobalHook();
        hook.KeyPressed.Subscribe(OnKeyPressed);
        hook.KeyReleased.Subscribe(OnKeyReleased);
        hook.MousePressed.Subscribe(OnMousePressed);
        hook.MouseReleased.Subscribe(OnMouseReleased);
        keyPressTimer.AutoReset = true;
        keyPressTimer.Elapsed += (_timer, _) => { canPress = true; };
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


        foreach (var (key, configHotKey) in HotKeys)
        {
            if (configHotKey.IsSelectAlt != AltIsPressed ||
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
                        case "截图":
                        {
                            break;
                        }
                        case "显示搜索框":
                        {
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

    public bool Add(HotKeyModel hotKeyModel, Action<HotKeyModel> rallBack)
    {
        throw new NotImplementedException();
    }

    public bool Del(HotKeyModel hotKeyModel)
    {
        throw new NotImplementedException();
    }

    public bool Del(string uuid)
    {
        throw new NotImplementedException();
    }

    public bool RequestUserModify(string hotKeyModel)
    {
        throw new NotImplementedException();
    }

    public bool Modify(HotKeyModel hotKeyModel)
    {
        throw new NotImplementedException();
    }

    public HotKeyModel? GetByUuid(string uuid)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<HotKeyModel> GetAllRegistered()
    {
        throw new NotImplementedException();
    }
}