using Avalonia.Media.Imaging;

namespace Core.SDKs.Services;

public interface IScreenCapture
{
    public (Queue<Bitmap>, Queue<Bitmap>) CaptureAllScreen();

    public (Bitmap?, Bitmap?)? CaptureScreen(int index = 0, bool withMosaic = false);

    public (Bitmap?, Bitmap?)? CaptureScreen(int index, int x, int y, int width, int height, bool withMosaic = false);
}