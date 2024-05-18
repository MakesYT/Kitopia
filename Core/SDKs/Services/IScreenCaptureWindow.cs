using PluginCore;

namespace Core.SDKs.Services;

public interface IScreenCaptureWindow
{
    public void CaptureScreen();
    public Task<ScreenCaptureInfo> GetScreenCaptureInfo();
}