using System;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.Messaging;
using Core.SDKs.Services;
using Core.SDKs.Services.Config;
using Microsoft.Extensions.DependencyInjection;

namespace KitopiaAvalonia.Windows;

public partial class ScreenCaptureWindow : Window
{
    private Point DragStartPoint;
    private Point endPoint;

    private bool IsDrag = false;
    bool IsSelected = false;
    bool PointerOver = false;
    private bool Selecting = false;
    private Point startPoint;

    public ScreenCaptureWindow()
    {
        InitializeComponent();
        WeakReferenceMessenger.Default.Register<string, string>(this, "ScreenCapture", (sender, message) =>
        {
            switch (message)
            {
                case "Close":
                {
                    Image.Source = null;
                    if (Image.Source is Bitmap bitmap)
                    {
                        bitmap.Dispose();
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
                    if (ConfigManger.Config.截图直接复制到剪贴板)
                    {
                        ServiceManager.Services.GetService<IClipboardService>().SetImage((Bitmap)Image.Source);
                        Image.Source = null;
                        if (Image.Source is Bitmap bitmap)
                        {
                            bitmap.Dispose();
                        }

                        this.Close();
                    }

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
            IsSelected = true;
            Cursor = Cursor.Default;
            var data = new DataObject();
            var imageSource = (Bitmap)Image.Source;
            using (MemoryStream stream = new MemoryStream())
            {
                imageSource.Save(stream);
                data.Set("Unknown_Format_2", stream.ToArray());
            }


            this.Clipboard.SetDataObjectAsync(data);

            WeakReferenceMessenger.Default.Send<string, string>("Selected", "ScreenCapture");
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
            Cursor = new Cursor(StandardCursorType.SizeAll);
        }
    }

    private void SelectBox_OnPointerExited(object? sender, PointerEventArgs e)
    {
        if (IsSelected)
        {
            Cursor?.Dispose();
            Cursor = Cursor.Default;
        }
    }

    private void SelectBox_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed && IsSelected)
        {
            IsDrag = true;
            DragStartPoint = e.GetPosition(this);
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
                dragStartPoint = new Point(0, dragStartPoint.Y);
            }

            if (startPoint.Y < 0)
            {
                startPoint = new Point(startPoint.X, 0);
                dragStartPoint = new Point(dragStartPoint.X, 0);
            }

            endPoint -= dragStartPoint;
            DragStartPoint = endPoint1;
            UpdateSelectBox();
        }
    }

    private void SelectBox_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        IsDrag = false;
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
    }
}