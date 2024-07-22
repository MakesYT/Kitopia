using Avalonia.Controls;
using Core.SDKs.CustomType;
using Core.SDKs.HotKey;
using Core.SDKs.Services;
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
        globalHotKeyWindow = new Avalonia.Controls.Window()
        {
            Height = 1,
            Width = 1,
            ShowInTaskbar = false,
            IsVisible = false
        };
        globalHotKeyWindow.Show();
        globalHotKeyWindow.Hide();
        Win32Properties.AddWndProcHookCallback(globalHotKeyWindow, OnWndProc);
    }

    private static IntPtr OnWndProc(IntPtr hwnd, uint msg, IntPtr wparam, IntPtr lparam, ref bool handled)
    {
        if (msg == (uint)User32.WindowMessage.WM_HOTKEY)
        {
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
        var registerHotKey = User32.RegisterHotKey(globalHotKeyWindow.TryGetPlatformHandle().Handle, id,
            hotkeyModifiers,
            (uint)hotKeyModel.SelectKey);
        if (registerHotKey)
        {
            HotKeys.Add(hotKeyModel.UUID, new HotkeyInfo()
            {
                HotKeyModel = hotKeyModel,
                Id = id,
                RallBack = rallBack
            });
        }

        return registerHotKey;
    }

    public bool Del(HotKeyModel hotKeyModel)
    {
        return User32.UnregisterHotKey(globalHotKeyWindow.TryGetPlatformHandle().Handle, HotKeys[hotKeyModel.UUID].Id);
    }

    public bool Del(string uuid)
    {
        return User32.UnregisterHotKey(globalHotKeyWindow.TryGetPlatformHandle().Handle, HotKeys[uuid].Id);
    }
}