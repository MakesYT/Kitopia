using Core.SDKs.Services;

namespace KitopiaAvalonia.Services;

public class ScreenCaptureWindow : IScreenCaptureWindow
{
    public void CaptureScreen()
    {
        Tools.ScreenCapture.StartUserManualCapture();
    }
}