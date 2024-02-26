using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.Messaging;
using Core.SDKs.Services;
using Core.SDKs.Services.Config;
using KitopiaAvalonia.Controls;
using KitopiaAvalonia.Controls.Capture;
using KitopiaAvalonia.Tools;
using Microsoft.Extensions.DependencyInjection;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Point = Avalonia.Point;
using Rectangle = SixLabors.ImageSharp.Rectangle;
using Size = Avalonia.Size;

namespace KitopiaAvalonia.Windows;

public enum 截图工具
{
    无,
    矩形,
    圆形,
    箭头,
    马赛克,
    文本,
    批准
}

public partial class ScreenCaptureWindow : Window
{
    

    bool IsSelected = false;
    private bool Selecting = false;
    private bool PointerOver = false;
    private Point _startPoint;
    private Stack<Control> redoStack = new();

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
        SelectBox.LocationOrSizeChanged += LocationOrSizeChanged;
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        SelectBox.LocationOrSizeChanged -= LocationOrSizeChanged;
    }

    

    protected  void LocationOrSizeChanged(object? sender, LocationOrSizeChangedEventArgs locationOrSizeChangedEventArgs)
    {
        UpdateSelectBox();
        UpdateToolBar();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Key == Key.Escape)
        {
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

            _startPoint = e.GetPosition(this);
            //endPoint = e.GetPosition(this);
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (e.InitialPressMouseButton == MouseButton.Right)
        {
            if (!IsSelected)
            {
                WeakReferenceMessenger.Default.Send<string, string>("Close", "ScreenCapture");
            }
            
        }

        if (Selecting)
        {
            Selecting = false;
            SelectBox.IsVisible = true;
            IsSelected = true;
            Cursor = Cursor.Default;
            WeakReferenceMessenger.Default.Send<string, string>("Selected", "ScreenCapture");
            UpdateSelectBox();
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
                    var clone = image.Clone(e => e.Crop(new Rectangle((int)SelectBox._dragTransform.X, (int)SelectBox._dragTransform.Y,
                        ((int)(SelectBox.Width)), ((int)(SelectBox.Height)))));
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
            var selectBoxHeight = e.GetPosition(this).Y - _startPoint.Y;
            if (selectBoxHeight<0)
            {
                SelectBox.Height=-selectBoxHeight;
                SelectBox._dragTransform.Y=_startPoint.Y+selectBoxHeight;
            }else
            {
                SelectBox.Height=selectBoxHeight;
                SelectBox._dragTransform.Y=_startPoint.Y;
            }

            
            var selectBoxWidth = e.GetPosition(this).X - _startPoint.X;
            if (selectBoxWidth<0)
            {
                SelectBox.Width=-selectBoxWidth;
                SelectBox._dragTransform.X=_startPoint.X+selectBoxWidth;
            }
            else
            {
                SelectBox.Width=selectBoxWidth;
                SelectBox._dragTransform.X=_startPoint.X;
            }
            
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

    public 截图工具 NowTool = 截图工具.无;
    private bool Adding截图工具 = false;
    private DraggableResizeableControl Now截图工具;
  

    private void SelectBox_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        foreach (var canvasChild in Canvas.Children)
        {
            if (canvasChild is DraggableResizeableControl draggableResizeableControl)
            {
                if (draggableResizeableControl.GetVisualChildren().First() is Panel panel)
                {
                    var border = panel.GetChildOfType<Border>("ResizeSizeBoxBorder");
                    border.IsVisible = false;
                }
            }
        }
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            switch (NowTool)
            {
                case 截图工具.无:
                {
                    return;
                }
                case 截图工具.矩形:
                {
                    var position = e.GetPosition(this);
                    _startPoint = position;
                    var dragarea = new DraggableResizeableControl();
                    dragarea._dragTransform.X = position.X;
                    dragarea._dragTransform.Y = position.Y;
                
                    var rectangle = new Avalonia.Controls.Shapes.Rectangle();
                    dragarea.Content = rectangle;
                
                    rectangle.Stroke = new SolidColorBrush(Colors.Red);
                    rectangle.StrokeThickness = 1;
               
                    Canvas.Children.Add(dragarea);
                    Adding截图工具 = true;
                    Now截图工具 = dragarea;
                    break;
                }
                case 截图工具.圆形:
                {
                    var position = e.GetPosition(this);
                    _startPoint = position;
                    var dragarea = new DraggableResizeableControl();
                    dragarea._dragTransform.X = position.X;
                    dragarea._dragTransform.Y = position.Y;
                
                    var rectangle = new Ellipse();
                    dragarea.Content = rectangle;
                
                    rectangle.Stroke = new SolidColorBrush(Colors.Red);
                    rectangle.StrokeThickness = 1;
               
                    Canvas.Children.Add(dragarea);
                    Adding截图工具 = true;
                    Now截图工具 = dragarea;
                    break;
                }
            
            }
            e.Handled = true;
        }
       
       

        
    }
    private void SelectBox_OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (IsSelected)
        {
            Cursor?.Dispose();
            if (NowTool == 截图工具.无)
            {
                SelectBox.Cursor = new Cursor(StandardCursorType.SizeAll);
            }
            else
            {
                SelectBox.Cursor = Avalonia.Input.Cursor.Default;
            }
        }
        if (!Adding截图工具)
        {
            return;
        }
        var selectBoxHeight = e.GetPosition(this).Y - _startPoint.Y;
        if (selectBoxHeight<0)
        {
            Now截图工具.Height=-selectBoxHeight;
            Now截图工具._dragTransform.Y=_startPoint.Y+selectBoxHeight;
        }else
        {
            Now截图工具.Height=selectBoxHeight;
            Now截图工具._dragTransform.Y=_startPoint.Y;
        }

            
        var selectBoxWidth = e.GetPosition(this).X - _startPoint.X;
        if (selectBoxWidth<0)
        {
            Now截图工具.Width=-selectBoxWidth;
            Now截图工具._dragTransform.X=_startPoint.X+selectBoxWidth;
        }
        else
        {
            Now截图工具.Width=selectBoxWidth;
            Now截图工具._dragTransform.X=_startPoint.X;
        }
        e.Handled = true;
       
    }

    private void SelectBox_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (e.InitialPressMouseButton == MouseButton.Left&& Adding截图工具)
        {
            Adding截图工具 = false;
            e.Handled = true;
        }
    }
    

    

   

    private void UpdateSelectBox()
    {
        var fullScreenRect = new RectangleGeometry
        {
            Rect = new Rect(0, 0, this.Bounds.Width, this.Bounds.Height),
        };
        var selectionRect = new RectangleGeometry
        {
            Rect = new Rect(new Point(SelectBox._dragTransform.X,SelectBox._dragTransform.Y),SelectBox.DesiredSize),
        };


        var combinedGeometry = new CombinedGeometry
        {
            Geometry1 = fullScreenRect,
            Geometry2 = selectionRect,
            GeometryCombineMode = GeometryCombineMode.Exclude
        };
        Rectangle.Clip = combinedGeometry;
    }

    private void UpdateToolBar()
    {
        ToolBar.IsVisible = true;
        ToolBar.Measure(Bounds.Size);
        if (SelectBox._dragTransform.X+SelectBox.Width + ToolBar.DesiredSize.Width > Bounds.Width)
        {
            ToolBar.SetValue(Canvas.LeftProperty, Bounds.Width - ToolBar.DesiredSize.Width);
        }
        else
        {
            ToolBar.SetValue(Canvas.LeftProperty, SelectBox._dragTransform.X+SelectBox.Width);
        }

        if (SelectBox._dragTransform.Y+SelectBox.Height + ToolBar.DesiredSize.Height > Bounds.Height)
        {
            ToolBar.SetValue(Canvas.TopProperty, Bounds.Height - ToolBar.DesiredSize.Height);
        }
        else
        {
            ToolBar.SetValue(Canvas.TopProperty, SelectBox._dragTransform.Y+SelectBox.Height);
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
            foreach (var canvasChild in Canvas.Children)
            {
                if (canvasChild is DraggableResizeableControl draggableResizeableControl)
                {
                    if (draggableResizeableControl.GetVisualChildren().First() is Panel panel1)
                    {
                        var border = panel1.GetChildOfType<Border>("ResizeSizeBoxBorder");
                        border.IsVisible = false;
                    }
                }
            }
            if (SelectBox.GetVisualChildren().First() is Panel panel)
            {
                var border = panel.GetChildOfType<Border>("ResizeSizeBoxBorder");
                border.IsVisible = false;
            }
            var renderTargetBitmap = new RenderTargetBitmap(new PixelSize(bitmap.PixelSize.Width, bitmap.PixelSize.Height),new Vector(96, 96));
            
            renderTargetBitmap.Render(this);
            
            var boundsHeight = (int)(bitmap.PixelSize.Width * bitmap.PixelSize.Height * 4);
            IntPtr ptr = Marshal.AllocHGlobal(boundsHeight);
           
            renderTargetBitmap.CopyPixels(new PixelRect(0, 0, bitmap.PixelSize.Width, bitmap.PixelSize.Height),
                ptr,
                boundsHeight,
                ((((((int)bitmap.PixelSize.Width) * PixelFormat.Rgba8888.BitsPerPixel) + 31) & ~31) >> 3)
            );
            byte[] ys = new byte[boundsHeight];
            Marshal.Copy(ptr, ys, 0, boundsHeight);
            Marshal.FreeHGlobal(ptr);
            var image = SixLabors.ImageSharp.Image.LoadPixelData<Bgra32>(ys, bitmap.PixelSize.Width,
                bitmap.PixelSize.Height);
            var clone = image.Clone(e => e.Crop(new Rectangle((int)SelectBox._dragTransform.X, (int)SelectBox._dragTransform.Y,
                ((int)(SelectBox.Width)), ((int)(SelectBox.Height)))));
            image.Dispose();
            ServiceManager.Services.GetService<IClipboardService>().SetImageAsync(clone)
                .ContinueWith((e) => clone.Dispose());

            bitmap.Dispose();
            renderTargetBitmap.Dispose();
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
    private Button lastTool;
    private void RectangleButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (lastTool is not null)
        {
            lastTool.Classes.Remove("Selected");
        }
        if (NowTool != 截图工具.矩形)
        {
            NowTool = 截图工具.矩形;
            if (sender is not null)
            {
                lastTool = sender as Button;
                lastTool.Classes.Add("Selected");
            }
        }
        else
            NowTool = 截图工具.无;
    }


    private void CircleButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (lastTool is not null)
        {
            lastTool.Classes.Remove("Selected");
        }
        if (NowTool != 截图工具.圆形)
        {
            NowTool = 截图工具.圆形;
            if (sender is not null)
            {
                lastTool = sender as Button;
                lastTool.Classes.Add("Selected");
            }
        }
        else
            NowTool = 截图工具.无;
    }

    private void ArrowButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (lastTool is not null)
        {
            lastTool.Classes.Remove("Selected");
        }
        if (NowTool != 截图工具.箭头)
        {
            NowTool = 截图工具.箭头;
            if (sender is not null)
            {
                lastTool = sender as Button;
                lastTool.Classes.Add("Selected");
            }
        }
        else
            NowTool = 截图工具.无;
    }

    private void TextButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (lastTool is not null)
        {
            lastTool.Classes.Remove("Selected");
        }
        if (NowTool != 截图工具.文本)
        {
            NowTool = 截图工具.文本;
            if (sender is not null)
            {
                lastTool = sender as Button;
                lastTool.Classes.Add("Selected");
            }
        }
        else
            NowTool = 截图工具.无;
    }

    private void CommentButton_OnClick(object? sender, RoutedEventArgs e)
    {
       
        if (lastTool is not null)
        {
            lastTool.Classes.Remove("Selected");
        }
        if (NowTool != 截图工具.批准)
        {
            NowTool = 截图工具.批准;
            if (sender is not null)
            {
                lastTool = sender as Button;
                lastTool.Classes.Add("Selected");
            }
        }
        else
            NowTool = 截图工具.无;
    }

    private void MosaicButton_OnClick(object? sender, RoutedEventArgs e)
    {
        
        if (lastTool is not null)
        {
            lastTool.Classes.Remove("Selected");
        }
        if (NowTool != 截图工具.马赛克)
        {
            NowTool = 截图工具.马赛克;
            if (sender is not null)
            {
                lastTool = sender as Button;
                lastTool.Classes.Add("Selected");
            }
        }
        else
            NowTool = 截图工具.无;
    }
}