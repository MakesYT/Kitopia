using System;
using System.Diagnostics;
using Core.SDKs.HotKey;
using Core.SDKs.Services;
using Core.SDKs.Services.Config;
using Kitopia.View;
using Microsoft.Extensions.DependencyInjection;
using Vanara.PInvoke;

namespace Kitopia.SDKs;

public class MouseHookHelper
{
    private static IntPtr _pMouseHook = IntPtr.Zero;
    private static User32.HookProc mouseHookProc;
    private static TimerHelper? _timer;
    private static int _interval = 1000;

    private static IntPtr mouseHookCallback(int code, IntPtr wParam, IntPtr lParam)
    {
        if (code < 0)
        {
            return User32.CallNextHookEx(IntPtr.Zero, code, wParam, lParam);
        }

        if (wParam == (nint)MouseHookType.鼠标中键按下)
        {
            Console.WriteLine(1);
            if (_timer is null || _interval != ConfigManger.Config.mouseKeyInverval)
            {
                _timer = new TimerHelper(ConfigManger.Config.mouseKeyInverval, () =>
                {
                    Console.WriteLine(2);
                    ServiceManager.Services.GetService<MouseQuickWindow>()!.Show();
                });
                _interval = ConfigManger.Config.mouseKeyInverval;
            }

            _timer.StartTimer();
        }

        if (wParam == (nint)MouseHookType.鼠标中键弹起)
        {
            if (_timer is not null)
            {
                _timer.StopTimer();
            }
        }
        //设置一个计时器如果超过1s没有再次触发代码就执行指定代码,否则取消


        return 0;
    }


    public static bool InsertMouseHook()
    {
        if (_pMouseHook == IntPtr.Zero)
        {
            mouseHookProc = mouseHookCallback;
            _pMouseHook = User32.SetWindowsHookEx(User32.HookType.WH_MOUSE_LL,
                mouseHookProc).DangerousGetHandle();

            if (_pMouseHook == IntPtr.Zero)
            {
                //removeMouseHook();
                Debug.WriteLine("SetWindowsHookEx failed");
                return false;
            }
        }

        return true;
    }
}