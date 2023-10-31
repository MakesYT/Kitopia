using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
using PluginCore.Attribute;

namespace KitopiaEx;

public class ClipboardEx
{
    [PluginMethod("读取剪贴板文本", "return=返回参数")]
    public string ClipboardText(CancellationToken cancellationToken)
    {
        return Clipboard.GetText(TextDataFormat.Text);
    }

    [PluginMethod("读取剪贴板图片", "return=返回参数")]
    public BitmapSource ClipboardIBitmap(CancellationToken cancellationToken)
    {
        return Clipboard.GetImage();
    }

    [PluginMethod("设置剪贴板的图片", "return=返回参数")]
    public void SetClipboardIBitmap(BitmapSource bitmapSource, CancellationToken cancellationToken)
    {
        Application.Current.Dispatcher.BeginInvoke(() =>
        {
            Clipboard.SetImage(bitmapSource);
        });
    }

    [PluginMethod("设置剪贴板的图片", "return=返回参数")]
    public void SaveClipboardIBitmap(CancellationToken cancellationToken)
    {
        Application.Current.Dispatcher.BeginInvoke(() =>
        {
            var data = Clipboard.GetDataObject();
            if (!data.GetDataPresent(DataFormats.Bitmap))
            {
                return "";
            }

            var ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            var timeStamp = Convert.ToInt64(ts.TotalMilliseconds);
            var f = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads\\Kitopia" +
                    timeStamp + ".png";
            var img = (Bitmap)data.GetData(typeof(Bitmap));
            img.Save(f, ImageFormat.Png); // 将 bitmap 以 png 格式保存到文件
            return f;
        });
    }
}