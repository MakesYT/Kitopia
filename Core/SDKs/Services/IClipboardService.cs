#region

using Bitmap = Avalonia.Media.Imaging.Bitmap;

#endregion

namespace Core.SDKs.Services;

public interface IClipboardService
{
    bool HasText();
    string GetText();
    bool SetText(string text);
    bool HasImage();
    Bitmap? GetImage();
    bool SetImage(Bitmap image);
}

public enum ClipboardType
{
}