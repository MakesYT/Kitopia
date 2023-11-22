using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using Core.SDKs.Services;
using Core.SDKs.Services.Config;
using Kitopia.View;
using log4net;
using Microsoft.Extensions.DependencyInjection;
using Vanara.PInvoke;

namespace Kitopia.SDKs;

public class MouseHookHelper
{
    private static User32.SafeHHOOK _pMouseHook;
    private static User32.HookProc mouseHookProc;
    private static TimerHelper? _timer;
    private static int _interval = 1000;
    private static readonly ILog log = LogManager.GetLogger(nameof(MouseHookHelper));

    private static IntPtr mouseHookCallback(int code, IntPtr wParam, IntPtr lParam)
    {
        if (code < 0)
        {
            Console.WriteLine("errr");
            return User32.CallNextHookEx(IntPtr.Zero, code, wParam, lParam);
        }

        //Console.WriteLine(3);
        if (wParam == (nint)ConfigManger.Config.mouseKey)
        {
            if (_timer is null || _interval != ConfigManger.Config.mouseKeyInverval)
            {
                _timer = new TimerHelper(ConfigManger.Config.mouseKeyInverval, () =>
                {
                    log.Debug("快捷键触发");
                    Task.Run(() =>
                    {
                        Application.Current.Dispatcher.BeginInvoke(() =>
                        {
                            ServiceManager.Services.GetService<MouseQuickWindow>()!.Show();
                        });
                    });
                });
                _interval = ConfigManger.Config.mouseKeyInverval;
            }

            _timer.StartTimer();
        }

        if (wParam == (nint)ConfigManger.Config.mouseKey + 1)
        {
            if (_timer is not null)
            {
                _timer.StopTimer();
            }
        }
        //设置一个计时器如果超过1s没有再次触发代码就执行指定代码,否则取消


        return User32.CallNextHookEx(0, code, wParam, lParam);
    }


    public static bool InsertMouseHook(IntPtr m_hwnd)
    {
        mouseHookProc = mouseHookCallback;

        _pMouseHook = User32.SetWindowsHookEx(User32.HookType.WH_MOUSE_LL,
            mouseHookProc, Marshal.GetHINSTANCE(Assembly.GetExecutingAssembly().GetModules()[0]));

        if (_pMouseHook == IntPtr.Zero)
        {
            //removeMouseHook();
            Debug.WriteLine("SetWindowsHookEx failed");
            return false;
        }


        return true;
    }
}