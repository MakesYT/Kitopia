using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;
using Core.SDKs.Config;
using Core.SDKs.Services;
using log4net;

namespace Kitopia.Services;

public class ClipboardService : IClipboardService
{
    private static readonly ILog log = LogManager.GetLogger(nameof(ClipboardService));

    public bool IsBitmap()
    {
        if (ConfigManger.config.debugMode)
        {
            log.Debug(nameof(ClipboardService) + "的接口" + nameof(IsBitmap) + "被调用");
        }

        IDataObject data = Clipboard.GetDataObject();
        return data.GetDataPresent(DataFormats.Bitmap);
    }

    public Bitmap? GetBitmap()
    {
        if (ConfigManger.config.debugMode)
        {
            log.Debug(nameof(ClipboardService) + "的接口" + nameof(GetBitmap) + "被调用");
        }

        IDataObject data = Clipboard.GetDataObject();
        if (data.GetDataPresent(DataFormats.Bitmap))
        {
            return (Bitmap)data.GetData(DataFormats.Bitmap);
        }

        return null;
    }

    [STAThread]
    public string saveBitmap()
    {
        if (ConfigManger.config.debugMode)
        {
            log.Debug(nameof(ClipboardService) + "的接口" + nameof(saveBitmap) + "被调用");
        }

        string r = Application.Current.Dispatcher.Invoke(() =>
        {
            IDataObject data = Clipboard.GetDataObject();
            if (data.GetDataPresent(DataFormats.Bitmap))
            {
                var ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
                var timeStamp = Convert.ToInt64(ts.TotalMilliseconds);
                string f = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads\\Kitopia" +
                           timeStamp + ".png";
                var img = (Bitmap)data.GetData(typeof(Bitmap));
                img.Save(f, ImageFormat.Png); // 将 bitmap 以 png 格式保存到文件
                return f;
            }

            return "";
        });


        return r;
    }
}