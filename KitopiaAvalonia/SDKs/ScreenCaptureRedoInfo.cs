using System.Collections.Generic;
using Avalonia;
using KitopiaAvalonia.Windows;

namespace KitopiaAvalonia.SDKs;

public struct ScreenCaptureRedoInfo
{
    public 截图工具 Type;
    public object? Target;
    public ScreenCaptureEditType EditType;
    public Point startPoint;
    public Size Size;
    public List<Point> points;
}