using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using Windows.Storage;
using Core.SDKs.Services;


namespace Kitopia.Services;

public class ClipboardService: IClipboardService
{
    public bool IsBitmap()
    {
        IDataObject data = Clipboard.GetDataObject();
        return data.GetDataPresent(DataFormats.Bitmap);
    }

    public Bitmap? GetBitmap()
    {
        IDataObject data = Clipboard.GetDataObject();
        if (data.GetDataPresent(DataFormats.Bitmap))
        {
            return (Bitmap)data.GetData(DataFormats.Bitmap);
            
        }

        return null;
    }

    public string saveBitmap()
    {
        IDataObject data = Clipboard.GetDataObject();
        if (data.GetDataPresent(DataFormats.Bitmap))
        {
            var ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            var timeStamp = Convert.ToInt64(ts.TotalMilliseconds); 
            string f = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)+"\\Downloads\\Kitopia"+timeStamp+".png";
            var img = (Bitmap)data.GetData(typeof(Bitmap));
            img.Save (f, ImageFormat.Png); // 将 bitmap 以 png 格式保存到文件
            return f;
        }

        return "";
    }
}