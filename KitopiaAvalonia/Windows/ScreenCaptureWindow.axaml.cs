using System;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.Messaging;
using Core.SDKs.Services;
using Core.SDKs.Services.Config;
using Microsoft.Extensions.DependencyInjection;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Point = Avalonia.Point;

namespace KitopiaAvalonia.Windows;

public enum 截图工具
{
    无,
    矩形,
    圆形,
    箭头,
    马赛克,
    文本
}

public partial class ScreenCaptureWindow : Window
{
    private bool 截图工具bottom;
    private Point 截图工具DragStartPoint;
    private bool 截图工具IsDrag;
    private bool 截图工具left;
    private bool 截图工具ReSizeing;

    private bool 截图工具right;

    //Tag 0:正常
    //Tag 1:初始化
    private Point 截图工具startPoint;
    private bool 截图工具top;
    bool bottom = false;
    private Point DragStartPoint;
    private Point endPoint;

    private bool IsDrag = false;
    bool IsSelected = false;
    bool left = false;

    public 截图工具 NowTool = 截图工具.无;
    bool PointerOver = false;

    private bool ReSizeing = false;
    bool right = false;
    private bool Selecting = false;
    private Point startPoint;
    bool top = false;

    public ScreenCaptureWindow()
    {
        InitializeComponent();
        WeakReferenceMessenger.Default.Register<string, string>(this, "ScreenCapture", (sender, message) =>
        {
            switch (message)
            {
                case "Close":
                {
                    if (Image is not null && Image.Source is Bitmap bitmap)
                    {
                        bitmap.Dispose();
                        Image.Source = null;
                    }


                    this.Close();
                    WeakReferenceMessenger.Default.Unregister<string>(this);
                    break;
                }
                case "Selected":
                {
                    IsSelected = true;
                    Cursor?.Dispose();
                    Cursor = Cursor.Default;


                    break;
                }
            }
        });
    }

    bool ShowAlignLine => !IsSelected && PointerOver && !Selecting;

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
    }


    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Key == Key.Escape)
        {
            //KitopiaAvalonia.Tools.ScreenCapture.Dispose();
            WeakReferenceMessenger.Default.Send<string, string>("Close", "ScreenCapture");
        }

        if (e.Key == Key.B)
        {
            WindowState = WindowState.Maximized;
        }

        if (e.Key == Key.C)
        {
            WindowState = WindowState.Normal;
        }
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed && !IsSelected)
        {
            Selecting = true;
            X.IsVisible = false;
            Y.IsVisible = false;

            startPoint = e.GetPosition(this);
            endPoint = e.GetPosition(this);
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (e.InitialPressMouseButton == MouseButton.Right)
        {
            WeakReferenceMessenger.Default.Send<string, string>("Close", "ScreenCapture");
        }

        if (Selecting)
        {
            Selecting = false;
            SelectBox.IsVisible = true;
            IsSelected = true;
            Cursor = Cursor.Default;
            WeakReferenceMessenger.Default.Send<string, string>("Selected", "ScreenCapture");
            if (ConfigManger.Config.截图直接复制到剪贴板)
            {
                if (Image.Source is Bitmap bitmap)
                {
                    var boundsHeight = (int)(bitmap.PixelSize.Width * bitmap.PixelSize.Height * 4);
                    IntPtr ptr = Marshal.AllocHGlobal(boundsHeight);
                    bitmap.CopyPixels(new PixelRect(0, 0, bitmap.PixelSize.Width, bitmap.PixelSize.Height),
                        ptr,
                        boundsHeight,
                        ((((((int)bitmap.PixelSize.Width) * PixelFormat.Rgba8888.BitsPerPixel) + 31) & ~31) >> 3)
                    );
                    byte[] ys = new byte[boundsHeight];
                    Marshal.Copy(ptr, ys, 0, boundsHeight);
                    Marshal.FreeHGlobal(ptr);
                    var image = SixLabors.ImageSharp.Image.LoadPixelData<Bgra32>(ys, bitmap.PixelSize.Width,
                        bitmap.PixelSize.Height);
                    var clone = image.Clone(e => e.Crop(new Rectangle((int)startPoint.X, (int)startPoint.Y,
                        int.Abs((int)(endPoint.X - startPoint.X)), int.Abs((int)(endPoint.Y - startPoint.Y)))));
                    image.Dispose();
                    ServiceManager.Services.GetService<IClipboardService>().SetImageAsync(clone)
                        .ContinueWith((e) => clone.Dispose());

                    bitmap.Dispose();
                }

                Image.Source = null;

                WeakReferenceMessenger.Default.Send<string, string>("Close", "ScreenCapture");
                this.Close();
            }
            else
            {
                UpdateToolBar();
            }
        }
    }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);
        PointerOver = true;
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        PointerOver = false;
        X.IsVisible = false;
        Y.IsVisible = false;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed && Selecting)
        {
            endPoint = e.GetPosition(this);
            UpdateSelectBox();
        }

        if (ShowAlignLine)
        {
            X.IsVisible = true;
            Y.IsVisible = true;
            X.StartPoint = new Point(0, e.GetPosition(this).Y);

            X.EndPoint = new Point(this.Width, e.GetPosition(this).Y);

            Y.StartPoint = new Point(e.GetPosition(this).X, 0);
            Y.EndPoint = new Point(e.GetPosition(this).X, this.Height);
        }
        else
        {
            X.IsVisible = false;
            Y.IsVisible = false;
        }
    }


    private void SelectBox_OnPointerEntered(object? sender, PointerEventArgs e)
    {
        if (IsSelected)
        {
            Cursor?.Dispose();
            if (NowTool == 截图工具.无)
            {
                Cursor = new Cursor(StandardCursorType.SizeAll);
            }
            else
            {
                Cursor = Avalonia.Input.Cursor.Default;
            }
        }

        e.Handled = true;
    }


    private void SelectBox_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        switch (NowTool)
        {
            case 截图工具.无:
            {
                if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed && IsSelected)
                {
                    IsDrag = true;
                    DragStartPoint = e.GetPosition(this);
                }

                break;
            }
            case 截图工具.矩形:
            {
                var rectangle = new Avalonia.Controls.Shapes.Rectangle();
                截图工具startPoint = e.GetPosition(this);
                rectangle.SetValue(Canvas.LeftProperty, e.GetPosition(this).X);
                rectangle.SetValue(Canvas.TopProperty, e.GetPosition(this).Y);
                rectangle.Tag = 1;
                rectangle.PointerMoved += 截图工具PointerMoved;
                rectangle.Stroke = new SolidColorBrush(Colors.Red);
                rectangle.StrokeThickness = 1;
                rectangle.PointerReleased += 截图工具PointerReleased;
                rectangle.PointerPressed += 截图工具PointerPressed;
                Canvas.Children.Add(rectangle);
                e.GetCurrentPoint(this).Pointer.Capture(rectangle);
                break;
            }
        }

        e.Handled = true;
    }

    private void 截图工具PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (sender is Control control)
        {
            control.Tag = 0;
        }
    }

    private void 截图工具PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not Control control)
            return;
        if ((int?)control.Tag == 0 && e.GetCurrentPoint(this).Properties.IsLeftButtonPressed && IsSelected)
        {
            截图工具IsDrag = true;
            截图工具DragStartPoint = e.GetPosition(this);
        }
    }

    private void 截图工具PointerMoved(object? sender, PointerEventArgs e)
    {
        var position = e.GetPosition(this);
        if (sender is Control control)
        {
            if ((int?)control.Tag == 1)
            {
                var positionX = position.X - 截图工具startPoint.X;
                if (positionX > 0)
                {
                    control.Width = positionX;
                }
                else
                {
                    control.Width = double.Abs(positionX);
                    control.SetValue(Canvas.LeftProperty, 截图工具startPoint.X + positionX);
                }

                var positionY = position.Y - 截图工具startPoint.Y;
                if (positionY > 0)
                {
                    control.Height = positionY;
                }
                else
                {
                    control.Height = double.Abs(positionY);
                    control.SetValue(Canvas.TopProperty, 截图工具startPoint.Y + positionY);
                }

                return;
            }

            if (截图工具IsDrag)
            {
                var dragStartPoint = 截图工具DragStartPoint - position;
                截图工具startPoint -= dragStartPoint;
                if (截图工具startPoint.X < 0)
                {
                    截图工具startPoint = new Point(0, 截图工具startPoint.Y);
                }

                if (截图工具startPoint.Y < 0)
                {
                    截图工具startPoint = new Point(截图工具startPoint.X, 0);
                }

                if (截图工具startPoint.X + control.Width > Bounds.Width)
                {
                    截图工具startPoint = new Point(Bounds.Width - control.Width, 截图工具startPoint.Y);
                }

                if (截图工具startPoint.Y + control.Height > Bounds.Height)
                {
                    截图工具startPoint = new Point(截图工具startPoint.X, Bounds.Height - control.Height);
                }
            }
        }
    }

    private void SelectBox_OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (IsDrag)
        {
            var endPoint1 = e.GetPosition(this);

            var dragStartPoint = DragStartPoint - endPoint1;
            startPoint -= dragStartPoint;
            if (startPoint.X < 0)
            {
                startPoint = new Point(0, startPoint.Y);
            }

            if (startPoint.Y < 0)
            {
                startPoint = new Point(startPoint.X, 0);
            }

            if (startPoint.X + SelectBox.Width > Bounds.Width)
            {
                startPoint = new Point(Bounds.Width - SelectBox.Width, startPoint.Y);
            }

            if (startPoint.Y + SelectBox.Height > Bounds.Height)
            {
                startPoint = new Point(startPoint.X, Bounds.Height - SelectBox.Height);
            }

            endPoint = startPoint + new Point(SelectBox.Width, SelectBox.Height);
            DragStartPoint = endPoint1;
            UpdateSelectBox();
            UpdateToolBar();
        }

        e.Handled = true;
    }

    private void SelectBox_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        IsDrag = false;
        e.Handled = true;
    }

    private void UpdateSelectBox()
    {
        var fullScreenRect = new RectangleGeometry
        {
            Rect = new Rect(0, 0, this.Bounds.Width, this.Bounds.Height),
        };
        var selectionRect = new RectangleGeometry
        {
            Rect = new Rect(startPoint, endPoint),
        };


        var combinedGeometry = new CombinedGeometry
        {
            Geometry1 = fullScreenRect,
            Geometry2 = selectionRect,
            GeometryCombineMode = GeometryCombineMode.Exclude
        };
        Rectangle.Clip = combinedGeometry;
        SelectBox.Width = double.Abs(endPoint.X - startPoint.X);
        SelectBox.Height = double.Abs(endPoint.Y - startPoint.Y);
        SelectBox.SetValue(Canvas.LeftProperty, double.Min(startPoint.X, endPoint.X));
        SelectBox.SetValue(Canvas.TopProperty, double.Min(startPoint.Y, endPoint.Y));
        startPoint = new Point(double.Min(startPoint.X, endPoint.X), double.Min(startPoint.Y, endPoint.Y));
        endPoint = new Point(startPoint.X + SelectBox.Width, startPoint.Y + SelectBox.Height);
        ResizeSizeBoxBorder.IsVisible = true;
        ResizeSizeBoxBorder.SetValue(Canvas.LeftProperty, double.Min(startPoint.X, endPoint.X) - 1.5);
        ResizeSizeBoxBorder.SetValue(Canvas.TopProperty, double.Min(startPoint.Y, endPoint.Y) - 1.5);
        ResizeSizeBoxBorder.Width = double.Abs(endPoint.X - startPoint.X) + 3;
        ResizeSizeBoxBorder.Height = double.Abs(endPoint.Y - startPoint.Y) + 3;
    }

    private void UpdateToolBar()
    {
        ToolBar.IsVisible = true;
        ToolBar.Measure(Bounds.Size);
        if (endPoint.X + ToolBar.DesiredSize.Width > Bounds.Width)
        {
            ToolBar.SetValue(Canvas.LeftProperty, Bounds.Width - ToolBar.DesiredSize.Width);
        }
        else
        {
            ToolBar.SetValue(Canvas.LeftProperty, endPoint.X);
        }

        if (endPoint.Y + ToolBar.DesiredSize.Height > Bounds.Height)
        {
            ToolBar.SetValue(Canvas.TopProperty, Bounds.Height - ToolBar.DesiredSize.Height);
        }
        else
        {
            ToolBar.SetValue(Canvas.TopProperty, endPoint.Y);
        }
    }

    private void ReSizeBox_OnPointerEntered(object? sender, PointerEventArgs e)
    {
        e.Handled = true;
        var position = e.GetPosition((Visual?)sender);
        UpdateResizeBoxCursor(position);
    }

    private void ResizeSizeBoxBorder_OnPointerMoved(object? sender, PointerEventArgs e)
    {
        e.Handled = true;
        if (ReSizeing)
        {
            if (top)
            {
                startPoint = new Point(startPoint.X, double.Max(e.GetPosition(this).Y, 0d));
            }

            if (left)
            {
                startPoint = new Point(double.Max(e.GetPosition(this).X, 0d), startPoint.Y);
            }

            if (bottom)
            {
                endPoint = new Point(endPoint.X, double.Max(e.GetPosition(this).Y, 0d));
            }

            if (right)
            {
                endPoint = new Point(double.Max(e.GetPosition(this).X, 0d), endPoint.Y);
            }

            UpdateSelectBox();
            UpdateToolBar();
        }
        else
        {
            var position = e.GetPosition((Visual?)sender);
            UpdateResizeBoxCursor(position);
        }
    }

    private void ResizeSizeBoxBorder_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        e.Handled = true;
        if (e.InitialPressMouseButton == MouseButton.Left)
        {
            ReSizeing = false;
        }
    }

    private void ResizeSizeBoxBorder_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        e.Handled = true;
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            ReSizeing = true;
        }
    }

    private void UpdateResizeBoxCursor(Point position)
    {
        top = false;
        left = false;
        right = false;
        bottom = false;
        if (position.Y < 3)
        {
            top = true;
        }

        if (position.Y > ResizeSizeBoxBorder.Height - 3)
        {
            bottom = true;
        }

        if (position.X > ResizeSizeBoxBorder.Width - 3)
        {
            right = true;
        }

        if (position.X < 3)
        {
            left = true;
        }

        if (left && top)
        {
            Cursor?.Dispose();
            Cursor = new Cursor(StandardCursorType.TopLeftCorner);
            return;
        }

        if (right && top)
        {
            Cursor?.Dispose();
            Cursor = new Cursor(StandardCursorType.TopRightCorner);
            return;
        }

        if (left && bottom)
        {
            Cursor?.Dispose();
            Cursor = new Cursor(StandardCursorType.BottomLeftCorner);
            return;
        }

        if (right && bottom)
        {
            Cursor?.Dispose();
            Cursor = new Cursor(StandardCursorType.BottomRightCorner);
            return;
        }

        if (left)
        {
            Cursor?.Dispose();
            Cursor = new Cursor(StandardCursorType.LeftSide);
            return;
        }

        if (right)
        {
            Cursor?.Dispose();
            Cursor = new Cursor(StandardCursorType.RightSide);
            return;
        }

        if (top)
        {
            Cursor?.Dispose();
            Cursor = new Cursor(StandardCursorType.TopSide);
            return;
        }

        if (bottom)
        {
            Cursor?.Dispose();
            Cursor = new Cursor(StandardCursorType.BottomSide);
            return;
        }
    }

    private void Rectangle_OnPointerEntered(object? sender, PointerEventArgs e)
    {
        if (IsSelected)
        {
            Cursor?.Dispose();
            Cursor = Cursor.Default;
        }
    }


    private void SaveToClipboard_Click(object? sender, RoutedEventArgs e)
    {
        if (Image.Source is Bitmap bitmap)
        {
            var boundsHeight = (int)(bitmap.PixelSize.Width * bitmap.PixelSize.Height * 4);
            IntPtr ptr = Marshal.AllocHGlobal(boundsHeight);
            bitmap.CopyPixels(new PixelRect(0, 0, bitmap.PixelSize.Width, bitmap.PixelSize.Height),
                ptr,
                boundsHeight,
                ((((((int)bitmap.PixelSize.Width) * PixelFormat.Rgba8888.BitsPerPixel) + 31) & ~31) >> 3)
            );
            byte[] ys = new byte[boundsHeight];
            Marshal.Copy(ptr, ys, 0, boundsHeight);
            Marshal.FreeHGlobal(ptr);
            var image = SixLabors.ImageSharp.Image.LoadPixelData<Bgra32>(ys, bitmap.PixelSize.Width,
                bitmap.PixelSize.Height);
            var clone = image.Clone(e => e.Crop(new Rectangle((int)startPoint.X, (int)startPoint.Y,
                int.Abs((int)(endPoint.X - startPoint.X)), int.Abs((int)(endPoint.Y - startPoint.Y)))));
            image.Dispose();
            ServiceManager.Services.GetService<IClipboardService>().SetImageAsync(clone)
                .ContinueWith((e) => clone.Dispose());

            bitmap.Dispose();
        }

        Image.Source = null;

        WeakReferenceMessenger.Default.Send<string, string>("Close", "ScreenCapture");
        this.Close();
    }

    private void Close_Click(object? sender, RoutedEventArgs e)
    {
        if (Image.Source is Bitmap bitmap)
        {
            bitmap.Dispose();
        }

        Image.Source = null;

        WeakReferenceMessenger.Default.Send<string, string>("Close", "ScreenCapture");
        this.Close();
    }

    private void RectangleButton_OnClick(object? sender, RoutedEventArgs e)
    {
        NowTool = 截图工具.矩形;
    }
}