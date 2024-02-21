#region

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Avalonia.Threading;
using Core.SDKs.Services;
using log4net;
using Microsoft.Extensions.DependencyInjection;
using Vanara.PInvoke;

#endregion

namespace KitopiaAvalonia.Services;

public class ClipboardService : IClipboardService
{
    private static readonly ILog log = LogManager.GetLogger(nameof(ClipboardService));

    public bool IsBitmap()
    {
        try
        {
            var strings = ServiceManager.Services.GetService<MainWindow>().Clipboard.GetFormatsAsync().WaitAsync(TimeSpan.FromSeconds(1)).GetAwaiter().GetResult();
            if (strings is null)
            {
                return false;
            }
            
            return strings.Contains("Unknown_Format_8");
        }
        catch (Exception e)
        {
            return false;
        }
    }

    public bool IsText()
    {
        var mainWindow = ServiceManager.Services.GetService<MainWindow>();
        try
        {
            var strings = mainWindow.Clipboard.GetFormatsAsync().WaitAsync(TimeSpan.FromSeconds(1)).GetAwaiter().GetResult();
            if (strings is null)
            {
                return false;
            }
            return strings.Contains("Text");
        }
        catch (Exception e)
        {
            return false;
        }
    }

    public Bitmap? GetBitmap()
    {
        log.Debug(nameof(ClipboardService) + "的接口" + nameof(GetBitmap) + "被调用");


        try
        {
        }
        catch (Exception e)
        {
            return null;
        }

        return null;
    }

    public string GetText()
    {
        return ServiceManager.Services.GetService<MainWindow>().Clipboard.GetTextAsync().Result;
    }
    
    public string saveBitmap()
    {
        log.Debug(nameof(ClipboardService) + "的接口" + nameof(saveBitmap) + "被调用");
        var ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        var timeStamp = Convert.ToInt64(ts.TotalMilliseconds);
        var f = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads\\Kitopia" +
                timeStamp + ".png";
        if (!User32.OpenClipboard(HWND.NULL))
        {
            return "";
        }
        HBITMAP hbitmap = (HBITMAP)User32.GetClipboardData(2);
        if (hbitmap.IsNull)
        {
            User32.CloseClipboard();
            return "";
        }

        var hBitmapCopy = User32.CopyImage(hbitmap.DangerousGetHandle(), 0, 0, 0,   User32.CopyImageOptions.LR_COPYRETURNORG);
        if (hBitmapCopy != IntPtr.Zero)
        {
            Image img = Image.FromHbitmap(hBitmapCopy.DangerousGetHandle());
            img.Save(f);
        }

       
        
        User32.CloseClipboard();
        return f;
            
        
        return "";
        
            
    }
    byte[] getBytes(object str) {
        int size = Marshal.SizeOf(str);
        byte[] arr = new byte[size];

        IntPtr ptr = IntPtr.Zero;
        try
        {
            ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(str, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }
        return arr;
    }
}