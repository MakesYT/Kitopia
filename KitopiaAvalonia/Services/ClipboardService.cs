#region

using System;
using System.Drawing;
using System.Linq;
using Core.SDKs.Services;
using log4net;
using Microsoft.Extensions.DependencyInjection;

#endregion

namespace KitopiaAvalonia.Services;

public class ClipboardService : IClipboardService
{
    private static readonly ILog log = LogManager.GetLogger(nameof(ClipboardService));

    public bool IsBitmap()
    {
        var strings = ServiceManager.Services.GetService<MainWindow>().Clipboard.GetFormatsAsync().Result;
        return strings.Contains("Unknown_Format_8");
    }

    public bool IsText()
    {
        var strings = ServiceManager.Services.GetService<MainWindow>().Clipboard.GetFormatsAsync().Result;
        return strings.Contains("Text");
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
        var result = ServiceManager.Services.GetService<MainWindow>().Clipboard.GetDataAsync("Unknown_Format_8").Result;

        // var r = Application.Current.Dispatcher.Invoke(() =>
        // {
        //     var data = Clipboard.GetDataObject();
        //     if (data.GetDataPresent(DataFormats.Bitmap))
        //     {
        //         var ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        //         var timeStamp = Convert.ToInt64(ts.TotalMilliseconds);
        //         var f = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads\\Kitopia" +
        //                 timeStamp + ".png";
        //         var img = (Bitmap)data.GetData(typeof(Bitmap));
        //         img.Save(f, ImageFormat.Png); // 将 bitmap 以 png 格式保存到文件
        //         return f;
        //     }
        //
        //     return "";
        // });


        return null;
    }
}