using System.Buffers;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Platform;
using Avalonia.Threading;
using Core.SDKs.Services;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Vanara.PInvoke;
using Bitmap = Avalonia.Media.Imaging.Bitmap;

namespace Core.Window;

public class ClipboardWindow : IClipboardService
{
    private IntPtr ptr2;

    public bool HasText()
    {
        try
        {
            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime appLifetime)
            {
                return appLifetime.MainWindow.Clipboard.GetFormatsAsync().WaitAsync(TimeSpan.FromSeconds(1))
                    .GetAwaiter()
                    .GetResult().Contains("Text");
            }

            return false;
        }
        catch (Exception e)
        {
            return false;
        }
    }

    public string GetText()
    {
        try
        {
            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime appLifetime)
            {
                return appLifetime.MainWindow.Clipboard.GetTextAsync().WaitAsync(TimeSpan.FromSeconds(1)).GetAwaiter()
                    .GetResult();
            }

            return null;
        }
        catch (Exception e)
        {
            return null;
        }
    }

    public bool SetText(string text)
    {
        try
        {
            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime appLifetime)
            {
                appLifetime.MainWindow.Clipboard.SetTextAsync(text).WaitAsync(TimeSpan.FromSeconds(1)).GetAwaiter()
                    .GetResult();
                return true;
            }

            return false;
        }
        catch (Exception e)
        {
            return false;
        }
    }

    public bool HasImage()
    {
        try
        {
            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime appLifetime)
            {
                var strings = appLifetime.MainWindow.Clipboard.GetFormatsAsync().WaitAsync(TimeSpan.FromSeconds(1))
                    .GetAwaiter()
                    .GetResult();
                if (strings is null)
                {
                    return false;
                }

                return strings.Contains("Unknown_Format_8");
            }
        }
        catch (Exception e)
        {
            return false;
        }

        return false;
    }

    public Bitmap? GetImage()
    {
        try
        {
            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime appLifetime)
            {
                byte[] result = [];
                Dispatcher.UIThread.Invoke(() =>
                {
                    result = appLifetime.MainWindow.Clipboard.GetDataAsync("Unknown_Format_8")
                        .WaitAsync(TimeSpan.FromSeconds(1))
                        .GetAwaiter()
                        .GetResult() as byte[];
                });
                var imgInfo = new byte[Marshal.SizeOf<Gdi32.BITMAPINFO>()];
                var img = new byte[result.Length - Marshal.SizeOf<Gdi32.BITMAPINFO>() + 4];
                Array.Copy(result, 40, img, 0, img.Length);
                Array.Copy(result, 0, imgInfo, 0, imgInfo.Length);
                Gdi32.BITMAPINFO info = BytesToStructure<Gdi32.BITMAPINFO>(imgInfo);

                var image = SixLabors.ImageSharp.Image.LoadPixelData<Rgba32>(img, info.bmiHeader.biWidth,
                    info.bmiHeader.biHeight);
                image.Mutate(x => x.Flip(FlipMode.Vertical));
                if (!image.DangerousTryGetSinglePixelMemory(out Memory<Rgba32> memory))
                {
                    throw new Exception(
                        "This can only happen with multi-GB images or when PreferContiguousImageBuffers is not set to true.");
                }

                using (MemoryHandle pinHandle = memory.Pin())
                {
                    unsafe
                    {
                        var pixelFormat = PixelFormat.Rgba8888;
                        var bitmap1 = new Bitmap(pixelFormat, AlphaFormat.Unpremul, (IntPtr)pinHandle.Pointer,
                            new PixelSize(info.bmiHeader.biWidth, info.bmiHeader.biHeight), new Vector(96, 96),
                            ((((info.bmiHeader.biWidth * pixelFormat.BitsPerPixel) + 31) & ~31) >> 3));
                        return bitmap1;
                    }
                }
            }
        }
        catch (Exception e)
        {
            return null;
        }

        return null;
    }

    public bool SetImage(Bitmap image)
    {
        try
        {
            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime appLifetime)
            {
                var data = new DataObject();
                var img = new byte[(int)(image.Size.Width * image.Size.Height * 4 + 40)];

                IntPtr ptr = Marshal.AllocHGlobal((int)(image.Size.Width * image.Size.Height * 4));
                try
                {
                    image.CopyPixels(new PixelRect(0, 0, (int)image.Size.Width, (int)image.Size.Height), ptr,
                        (int)(image.Size.Width * image.Size.Height * 4),
                        ((((((int)image.Size.Width) * PixelFormat.Bgra8888.BitsPerPixel) + 31) & ~31) >> 3));
                    byte[] img1 = new byte[(int)(image.Size.Width * image.Size.Height * 4)];
                    Marshal.Copy(ptr, img1, 0, (int)(image.Size.Width * image.Size.Height * 4));

                    var load = SixLabors.ImageSharp.Image.LoadPixelData<Rgba32>(img1, (int)image.Size.Width,
                        (int)image.Size.Height);
                    var b = new byte[(int)(image.Size.Width * image.Size.Height * 4)];


                    load.Mutate(x => x.Flip(FlipMode.Vertical));
                    load.CopyPixelDataTo(b);
                    Array.Copy(b, 0, img, 40, img.Length - 40);


                    var bitmapinfoheader = new Gdi32.BITMAPINFOHEADER((int)image.Size.Width, (int)image.Size.Height);
                    bitmapinfoheader.biCompression = Gdi32.BitmapCompressionMode.BI_RGB;
                    bitmapinfoheader.biPlanes = 1;
                    bitmapinfoheader.biYPelsPerMeter = 3780;
                    bitmapinfoheader.biXPelsPerMeter = 3780;
                    bitmapinfoheader.biSize = 40;
                    bitmapinfoheader.biBitCount = 32;
                    var structToBytes = StructToBytes(bitmapinfoheader);
                    Array.Copy(structToBytes, 0, img, 0, 40);

                    if (User32.OpenClipboard(appLifetime.MainWindow.TryGetPlatformHandle().Handle))
                    {
                        if (ptr != null)
                        {
                            Marshal.FreeHGlobal(ptr2);
                        }

                        ptr2 = Marshal.AllocHGlobal((int)(image.Size.Width * image.Size.Height * 4));
                        Marshal.Copy(img, 0, ptr2, (int)(image.Size.Width * image.Size.Height * 4));
                        // System.Windows.
                    }

                    return false;
                }
                catch (Exception e)
                {
                    return false;
                }
                finally
                {
                    User32.CloseClipboard();
                    Marshal.FreeHGlobal(ptr);
                }
            }
        }
        catch (Exception e)
        {
            return false;
        }


        return false;
    }

    private static T BytesToStructure<T>(byte[] bytes)
    {
        int size = Marshal.SizeOf(typeof(T));
        if (bytes.Length < size)
            throw new Exception("Invalid parameter");

        IntPtr ptr = Marshal.AllocHGlobal(size);
        try
        {
            Marshal.Copy(bytes, 0, ptr, size);
            return (T)Marshal.PtrToStructure(ptr, typeof(T));
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }
    }

    public static byte[] StructToBytes(object structObj)
    {
        int size = Marshal.SizeOf(structObj);
        IntPtr buffer = Marshal.AllocHGlobal(size);
        try
        {
            Marshal.StructureToPtr(structObj, buffer, false);
            byte[] bytes = new byte[size];
            Marshal.Copy(buffer, bytes, 0, size);
            return bytes;
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }
}