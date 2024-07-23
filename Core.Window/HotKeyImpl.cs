using Avalonia.Controls;
using Avalonia.Threading;
using Core.SDKs.CustomType;
using Core.SDKs.HotKey;
using Core.SDKs.Services;
using Microsoft.Extensions.DependencyInjection;
using Vanara.PInvoke;

namespace Core.Window;

public class HotKeyImpl : IHotKetImpl
{
    private static Avalonia.Controls.Window globalHotKeyWindow = null!;
    private static ObservableDictionary<string, HotkeyInfo> HotKeys = new();

    protected class HotkeyInfo
    {
        public HotKeyModel HotKeyModel;
        public int Id;
        public Action<HotKeyModel> RallBack;
    }

    private static int id = 0;

    public void Init()
    {
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

    private static IntPtr OnWndProc(IntPtr hwnd, uint msg, IntPtr wparam, IntPtr lparam, ref bool handled)
    {
        if (msg == (uint)User32.WindowMessage.WM_HOTKEY)
        {
            var int32 = wparam.ToInt32();
            var keyValuePair = HotKeys.First(e => e.Value.Id == int32);
            keyValuePair.Value.RallBack.Invoke(keyValuePair.Value.HotKeyModel);
        }

        return IntPtr.Zero;
    }

    public bool Add(HotKeyModel hotKeyModel, Action<HotKeyModel> rallBack)
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

        id++;
        var registerHotKey = false;
        Dispatcher.UIThread.Invoke(() =>
        {
            registerHotKey = User32.RegisterHotKey(globalHotKeyWindow.TryGetPlatformHandle().Handle, id,
                hotkeyModifiers,
                (uint)hotKeyModel.SelectKey);
        });

        if (registerHotKey)
        {
            HotKeys.Add(hotKeyModel.UUID, new HotkeyInfo()
            {
                HotKeyModel = hotKeyModel,
                Id = id,
                RallBack = rallBack
            });
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

    public bool Del(HotKeyModel hotKeyModel)
    {
        return Del(hotKeyModel.UUID);
    }

    public bool Del(string uuid)
    {
        var unregisterHotKey =
            User32.UnregisterHotKey(globalHotKeyWindow.TryGetPlatformHandle().Handle, HotKeys[uuid].Id);
        HotKeys.Remove(uuid);

        return unregisterHotKey;
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
        if (HotKeys.ContainsKey(uuid))
        {
            return HotKeys[uuid].Id != -1;
        }

        return false;
    }

    public IEnumerable<HotKeyModel> GetAllRegistered()
    {
        return HotKeys.Values.Select(x => x.HotKeyModel);
    }
}