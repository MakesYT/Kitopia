using System.Buffers;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using ScreenCapture.NET;
using SixLabors.ImageSharp.PixelFormats;

namespace Core.SDKs.Tools;

public static class ScreenCapture
{
    static Queue<Bitmap> bitmaps = new();
    static SixLabors.ImageSharp.Configuration customConfig = SixLabors.ImageSharp.Configuration.Default.Clone();

    public static Queue<Bitmap> CaptureAllScreen()
    {
        customConfig.PreferContiguousImageBuffers = true;
        using var screenCaptureService = new DX11ScreenCaptureService();
        var graphicsCards = screenCaptureService.GetGraphicsCards();

        var enumerable = screenCaptureService.GetDisplays(graphicsCards.First());
        foreach (Display display in enumerable)
        {
            using var screenCapture = screenCaptureService.GetScreenCapture(display);
            var captureZone = screenCapture.RegisterCaptureZone(0, 0, display.Width, display.Height);

            while (!screenCapture.CaptureScreen()) ;
            using var loadPixelData = SixLabors.ImageSharp.Image.LoadPixelData<Bgra32>(customConfig,
                captureZone.RawBuffer,
                captureZone.Width, captureZone.Height);
            if (!loadPixelData.DangerousTryGetSinglePixelMemory(out Memory<Bgra32> memory))
            {
                throw new Exception(
                    "This can only happen with multi-GB images or when PreferContiguousImageBuffers is not set to true.");
            }

            using (MemoryHandle pinHandle = memory.Pin())
            {
                unsafe
                {
                    var bitmap1 = new Bitmap(PixelFormat.Bgra8888, AlphaFormat.Unpremul, (IntPtr)pinHandle.Pointer,
                        new PixelSize(captureZone.Width, captureZone.Height), new Vector(96, 96),
                        (captureZone.Height * PixelFormat.Bgra8888.BitsPerPixel + 7) / 8);
                    bitmaps.Enqueue(bitmap1);
                }
            }
        }

        return bitmaps;
    }
}