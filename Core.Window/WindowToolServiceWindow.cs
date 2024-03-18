using Core.SDKs.Services;
using Vanara.PInvoke;

namespace Core.Window;

public class WindowToolServiceWindow : IWindowTool
{

    public void SetForegroundWindow(IntPtr hWnd)
    {
        User32.SetForegroundWindow(hWnd);
    }
}