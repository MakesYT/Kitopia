using Core.SDKs.Services;

namespace KitopiaAvalonia.Services;

public class ScreenCapture : IScreenCapture
{
    public void CaptureScreen()
    {
        Tools.ScreenCapture.StartUserManualCapture();
    }
}