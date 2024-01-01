#region

using System.Drawing;

#endregion

namespace Core.SDKs.Services;

public interface IClipboardService
{
    bool IsBitmap();
    bool IsText();
    Bitmap? GetBitmap();
    string GetText();
    string saveBitmap();
}

public enum ClipboardType
{
}