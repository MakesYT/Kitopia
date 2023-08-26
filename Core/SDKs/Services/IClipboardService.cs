#region

using System.Drawing;

#endregion

namespace Core.SDKs.Services;

public interface IClipboardService
{
    bool IsBitmap();
    Bitmap? GetBitmap();
    string saveBitmap();
}

public enum ClipboardType
{
}