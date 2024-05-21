using System.Buffers;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Core.SDKs.Services;
using Microsoft.Extensions.DependencyInjection;
using PluginCore;
using ScreenCapture.NET;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Pbm;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Qoi;
using SixLabors.ImageSharp.Formats.Tga;
using SixLabors.ImageSharp.Formats.Tiff;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using IScreenCapture = PluginCore.IScreenCapture;

namespace Core.SDKs.Tools;

public class ScreenCapture : IScreenCapture
{
    private static readonly Lazy<Configuration> Lazy = new(CreateDefaultInstance);

    private static Configuration CreateDefaultInstance()
    {
        return new Configuration(
            new PngConfigurationModule(),
            new JpegConfigurationModule(),
            new GifConfigurationModule(),
            new BmpConfigurationModule(),
            new PbmConfigurationModule(),
            new TgaConfigurationModule(),
            new TiffConfigurationModule(),
            new WebpConfigurationModule(),
            new QoiConfigurationModule())
        {
            PreferContiguousImageBuffers = true
        };
    }

    public (Queue<Bitmap>, Queue<Bitmap>) CaptureAllScreen()
    {
        Queue<Bitmap> bitmaps = new();
        Queue<Bitmap> mosaics = new();
        using var screenCaptureService = new DX11ScreenCaptureService();
        var graphicsCards = screenCaptureService.GetGraphicsCards();

        var enumerable = screenCaptureService.GetDisplays(graphicsCards.First());
        foreach (Display display in enumerable)
        {
            using var screenCapture = screenCaptureService.GetScreenCapture(display);
            var captureZone = screenCapture.RegisterCaptureZone(0, 0, display.Width, display.Height);

            while (!screenCapture.CaptureScreen()) ;
            using var loadPixelData = Image.LoadPixelData<Bgra32>(Lazy.Value,
                captureZone.RawBuffer,
                captureZone.Width, captureZone.Height);
            var clone = loadPixelData.Clone();
            using (clone)
            {
                clone.Mutate(x => x.BoxBlur(10));
                if (!clone.DangerousTryGetSinglePixelMemory(out Memory<Bgra32> memory1))
                {
                    throw new Exception(
                        "This can only happen with multi-GB images or when PreferContiguousImageBuffers is not set to true.");
                }

                using (MemoryHandle pinHandle = memory1.Pin())
                {
                    unsafe
                    {
                        var bitmap1 = new Bitmap(PixelFormat.Bgra8888, AlphaFormat.Unpremul, (IntPtr)pinHandle.Pointer,
                            new PixelSize(captureZone.Width, captureZone.Height), new Vector(96, 96),
                            (captureZone.Height * PixelFormat.Bgra8888.BitsPerPixel + 7) / 8);
                        mosaics.Enqueue(bitmap1);
                    }
                }
            }

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

        return (bitmaps, mosaics);
    }

    public (Bitmap?, Bitmap?)? CaptureScreen(ScreenCaptureInfo screenCaptureInfo, bool withMosaic = false)
    {
        using var screenCaptureService = new DX11ScreenCaptureService();
        var graphicsCards = screenCaptureService.GetGraphicsCards();

        var enumerable = screenCaptureService.GetDisplays(graphicsCards.First());
        if (screenCaptureInfo.Index + 1 > enumerable.Count())
        {
            return (null, null);
        }

        var display = enumerable.ElementAtOrDefault(screenCaptureInfo.Index);
        var x = System.Math.Max(screenCaptureInfo.X, 0);
        var y = System.Math.Max(screenCaptureInfo.Y, 0);
        var width = System.Math.Max(screenCaptureInfo.Width, display.Width);
        var height = System.Math.Max(screenCaptureInfo.Height, display.Height);

        using var screenCapture = screenCaptureService.GetScreenCapture(display);
        var captureZone = screenCapture.RegisterCaptureZone(x, y, width, height);

        while (!screenCapture.CaptureScreen()) ;
        using var loadPixelData = Image.LoadPixelData<Bgra32>(Lazy.Value,
            captureZone.RawBuffer,
            captureZone.Width, captureZone.Height);
        Bitmap? mosaic = null;
        if (withMosaic)
        {
            var clone = loadPixelData.Clone();
            using (clone)
            {
                clone.Mutate(x => x.BoxBlur(10));
                if (!clone.DangerousTryGetSinglePixelMemory(out Memory<Bgra32> memory1))
                {
                    throw new Exception(
                        "This can only happen with multi-GB images or when PreferContiguousImageBuffers is not set to true.");
                }

                using (MemoryHandle pinHandle = memory1.Pin())
                {
                    unsafe
                    {
                        var bitmap1 = new Bitmap(PixelFormat.Bgra8888, AlphaFormat.Unpremul, (IntPtr)pinHandle.Pointer,
                            new PixelSize(captureZone.Width, captureZone.Height), new Vector(96, 96),
                            (captureZone.Height * PixelFormat.Bgra8888.BitsPerPixel + 7) / 8);
                        mosaic = bitmap1;
                    }
                }
            }
        }

        if (!loadPixelData.DangerousTryGetSinglePixelMemory(out Memory<Bgra32> memory))
        {
            throw new Exception(
                "This can only happen with multi-GB images or when PreferContiguousImageBuffers is not set to true.");
        }

        Bitmap? bitmap = null;
        using (MemoryHandle pinHandle = memory.Pin())
        {
            unsafe
            {
                var bitmap1 = new Bitmap(PixelFormat.Bgra8888, AlphaFormat.Unpremul, (IntPtr)pinHandle.Pointer,
                    new PixelSize(captureZone.Width, captureZone.Height), new Vector(96, 96),
                    (captureZone.Height * PixelFormat.Bgra8888.BitsPerPixel + 7) / 8);
                bitmap = bitmap1;
            }
        }


        return (bitmap, mosaic);
    }

    public ScreenCaptureInfo GetScreenCaptureInfoByUserManual()
    {
        return ServiceManager.Services.GetService<IScreenCaptureWindow>()!.GetScreenCaptureInfo()
                             .Result;
    }
}