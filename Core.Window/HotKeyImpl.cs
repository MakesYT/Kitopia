using Avalonia.Controls;
using Avalonia.Threading;
using Core.SDKs.CustomType;
using Core.SDKs.HotKey;
using Core.SDKs.Services;
using Kitopia.SDKs;
using Microsoft.Extensions.DependencyInjection;
using SharpHook;
using SharpHook.Reactive;
using Vanara.PInvoke;

namespace Core.Window;

public class HotKeyImpl : IHotKetImpl
{
    private static Avalonia.Controls.Window globalHotKeyWindow = null!;
    private static ObservableDictionary<string, HotkeyInfo> HotKeys = new();
    private static SimpleReactiveGlobalHook hook;

    private class HotkeyInfo
    {
        public HotKeyModel HotKeyModel;
        public int Id;
        public Action<HotKeyModel>? RallBack;
        public TimerHelper? Timer;
    }

    private static int _id = 0;

    public void Init()
    {
        hook.MousePressed.Subscribe(OnMousePressed);
        hook.MouseReleased.Subscribe(OnMouseReleased);
        hook.Run();
        Dispatcher.UIThread.Invoke(() =>
        {
            globalHotKeyWindow = new Avalonia.Controls.Window()
            {
                Height = 1,
                Width = 1,
                ShowInTaskbar = false,
            };
            globalHotKeyWindow.Show();
            globalHotKeyWindow.Hide();
            Win32Properties.AddWndProcHookCallback(globalHotKeyWindow, OnWndProc);
        });
    }

    private static void OnMousePressed(MouseHookEventArgs e)
    {
        foreach (var (key, value) in HotKeys)
        {
            if (value.HotKeyModel.Type != HotKeyType.Mouse) continue;
            if (value.Id == -1) continue;
            if (value.HotKeyModel.MouseButton == (int)e.Data.Button)
            {
                if (value.Timer is null)
                {
                    value.Timer = new TimerHelper(value.HotKeyModel.PressTimeMillis, value.RallBack, value.HotKeyModel);
                }

                value.Timer.StartTimer();
            }
        }
    }

    private static void OnMouseReleased(MouseHookEventArgs e)
    {
        foreach (var (key, value) in HotKeys)
        {
            if (value.HotKeyModel.Type != HotKeyType.Mouse) continue;
            if (value.Id == -1) continue;
            if (value.HotKeyModel.MouseButton == (int)e.Data.Button)
            {
                value.Timer?.StopTimer();
            }
        }
    }

    private static IntPtr OnWndProc(IntPtr hwnd, uint msg, IntPtr wparam, IntPtr lparam, ref bool handled)
    {
        if (msg == (uint)User32.WindowMessage.WM_HOTKEY)
        {
            var int32 = wparam.ToInt32();
            var keyValuePair = HotKeys.First(e => e.Value.Id == int32);
            keyValuePair.Value.RallBack?.Invoke(keyValuePair.Value.HotKeyModel);
        }

        return IntPtr.Zero;
    }

    public bool Add(HotKeyModel hotKeyModel, Action<HotKeyModel> rallBack)
    {
        switch (hotKeyModel.Type)
        {
            case HotKeyType.Keyboard:
            {
                User32.HotKeyModifiers hotkeyModifiers = 0;
                if (hotKeyModel.IsSelectAlt)
                {
                    hotkeyModifiers |= User32.HotKeyModifiers.MOD_ALT;
                }

                if (hotKeyModel.IsSelectCtrl)
                {
                    hotkeyModifiers |= User32.HotKeyModifiers.MOD_CONTROL;
                }

                if (hotKeyModel.IsSelectShift)
                {
                    hotkeyModifiers |= User32.HotKeyModifiers.MOD_SHIFT;
                }

                if (hotKeyModel.IsSelectWin)
                {
                    hotkeyModifiers |= User32.HotKeyModifiers.MOD_WIN;
                }

                _id++;
                var registerHotKey = false;
                Dispatcher.UIThread.Invoke(() =>
                {
                    registerHotKey = User32.RegisterHotKey(globalHotKeyWindow.TryGetPlatformHandle().Handle, _id,
                        hotkeyModifiers,
                        (uint)hotKeyModel.SelectKey);
                });
                if (registerHotKey)
                {
                    if (HotKeys.TryGetValue(hotKeyModel.UUID, out var hotKeyModel1) && hotKeyModel1.Id == -1)
                    {
                        hotKeyModel1.Id = _id;
                    }
                    else
                    {
                        HotKeys.Add(hotKeyModel.UUID, new HotkeyInfo()
                        {
                            HotKeyModel = hotKeyModel,
                            Id = _id,
                            RallBack = rallBack
                        });
                    }
                }
                else
                {
                    HotKeys.Add(hotKeyModel.UUID, new HotkeyInfo()
                    {
                        HotKeyModel = hotKeyModel,
                        Id = -1,
                        RallBack = rallBack
                    });
                }

                return registerHotKey;
            }
            case HotKeyType.Mouse:
            {
                HotKeys.Add(hotKeyModel.UUID, new HotkeyInfo()
                {
                    HotKeyModel = hotKeyModel,
                    Id = 1,
                    RallBack = rallBack
                });
                break;
            }
        }

        return false;
    }

    public bool Del(HotKeyModel hotKeyModel)
    {
        return Del(hotKeyModel.UUID);
    }

    public bool Del(string uuid)
    {
        if (HotKeys.TryGetValue(uuid, out var hotkey))
        {
            switch (hotkey.HotKeyModel.Type)
            {
                case HotKeyType.Keyboard:
                {
                    var unregisterHotKey =
                        User32.UnregisterHotKey(globalHotKeyWindow.TryGetPlatformHandle().Handle, hotkey.Id);
                    HotKeys[uuid].Id = -1;
                    return unregisterHotKey;
                }
                case HotKeyType.Mouse:
                {
                    hotkey.Timer?.StopTimer();
                    hotkey.Id = -1;
                    break;
                }
            }
        }

        return false;
    }

    public bool RequestUserModify(string uuid)
    {
        if (HotKeys.ContainsKey(uuid))
        {
            ServiceManager.Services.GetService<IHotKeyEditor>().EditByUuid(uuid, null);
            return true;
        }

        return false;
    }

    public bool Modify(HotKeyModel hotKeyModel)
    {
        if (HotKeys.ContainsKey(hotKeyModel.UUID))
        {
            var rallback = HotKeys[hotKeyModel.UUID].RallBack;
            Del(hotKeyModel);
            if (!Add(hotKeyModel, rallback))
            {
                return false;
            }

            return true;
        }

        return false;
    }

    public HotKeyModel? GetByUuid(string uuid)
    {
        if (HotKeys.ContainsKey(uuid))
        {
            return HotKeys[uuid].HotKeyModel;
        }

        return null;
    }

    public bool IsActive(string uuid)
    {
        if (HotKeys.TryGetValue(uuid, out var hotkeyInfo))
        {
            switch (hotkeyInfo.HotKeyModel.Type)
            {
                case HotKeyType.Keyboard:
                {
                    return HotKeys[uuid].Id != -1;
                }
                case HotKeyType.Mouse:
                {
                    return HotKeys[uuid].Id != -1;
                }
            }
        }

        return false;
    }

    public IEnumerable<HotKeyModel> GetAllRegistered()
    {
        return HotKeys.Values.Select(x => x.HotKeyModel);
    }
}