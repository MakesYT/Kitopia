using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;
using KitopiaAvalonia.Tools;

namespace KitopiaAvalonia.Controls.Capture;

public class LocationOrSizeChangedEventArgs : RoutedEventArgs
{
    
}
public class DraggableResizeableControl : ContentControl
{
    public static readonly AvaloniaProperty StartTranslateTransformProperty =
        AvaloniaProperty.Register<DraggableResizeableControl, TranslateTransform>("_dragTransform");
    public static readonly AvaloniaProperty OnlyShowReSizingBoxOnSelectProperty =
        AvaloniaProperty.Register<DraggableResizeableControl, bool>(nameof(OnlyShowReSizingBoxOnSelect), true);
    public static readonly RoutedEvent<LocationOrSizeChangedEventArgs> LocationOrSizeChangedEvent = RoutedEvent.Register<DraggableResizeableControl, LocationOrSizeChangedEventArgs>(nameof(LocationOrSizeChanged), RoutingStrategies.Bubble);
    public event EventHandler<LocationOrSizeChangedEventArgs>? LocationOrSizeChanged
    {
        add => this.AddHandler<LocationOrSizeChangedEventArgs>(LocationOrSizeChangedEvent, value);
        remove => this.RemoveHandler<LocationOrSizeChangedEventArgs>(LocationOrSizeChangedEvent, value);
    }
    public TranslateTransform _dragTransform
    {
        get => (TranslateTransform)GetValue(StartTranslateTransformProperty);
        set => SetValue(StartTranslateTransformProperty, value);
    }
    public bool OnlyShowReSizingBoxOnSelect
    {
        get => (bool)GetValue(OnlyShowReSizingBoxOnSelectProperty);
        set => SetValue(OnlyShowReSizingBoxOnSelectProperty, value);
    }


    private bool _isDragging;
    private Point _dragStartPoint;
    private Border _resizeSizeBoxBorder;
    public DraggableResizeableControl()
    {
        _dragTransform = new TranslateTransform();
    }
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        var content = e.NameScope.Find<ContentPresenter>("Presenter");
        
        this.RenderTransform = _dragTransform;
        content.PointerPressed += ContentOnPointerPressed;
        content.PointerMoved += ContentOnPointerMoved;
        content.PointerReleased += ContentOnPointerReleased;
        var border = e.NameScope.Find<Border>("ResizeSizeBoxBorder");
        _resizeSizeBoxBorder = border;
        border.PointerPressed += ResizeSizeBoxBorderOnPointerPressed;
        border.PointerReleased += ResizeSizeBoxBorderOnPointerReleased;
        border.PointerMoved += ResizeSizeBoxBorderOnPointerMoved;
        border.PointerExited += ResizeSizeBoxBorderOnPointerExited;
        border.PointerEntered += ResizeSizeBoxBorderOnPointerEntered;
        if (OnlyShowReSizingBoxOnSelect)
        {
            border.IsVisible = false;
        }
    }

   

    #region 边框: 调整大小

    private bool _isResizing;
    private Point _resizeStartPoint;
    private bool _isResizingTop;
    private bool _isResizingBottom;
    private bool _isResizingLeft;
    private bool _isResizingRight;
    private void ResizeSizeBoxBorderOnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.Handled)
        {
            return;
        }
        
        if (e.GetCurrentPoint(TopLevel.GetTopLevel(this)).Properties.IsLeftButtonPressed)
        {
            _isResizing = true;
            _isResizingTop = false;
            _isResizingBottom = false;
            _isResizingLeft = false;
            _isResizingRight = false;
            _resizeStartPoint = new Point();
            var position = e.GetPosition((Visual?)sender);
            
            if (position.Y < 3)
            {
                _isResizingTop = true;
                _resizeStartPoint =new Point(_resizeStartPoint.X,_dragTransform.Y+this.Height);
            }

            if (position.Y > this.Height - 3)
            {
                _isResizingBottom = true;
                _resizeStartPoint =new Point(_resizeStartPoint.X,_dragTransform.Y);
            }

            if (position.X > this.Width - 3)
            {
                _isResizingRight = true;
                _resizeStartPoint =new Point(_dragTransform.X,_resizeStartPoint.Y);
            }

            if (position.X < 3)
            {
                _isResizingLeft = true;
                _resizeStartPoint =new Point(_dragTransform.X+Width,_resizeStartPoint.Y);
            }
        }
        
    }
    private void ResizeSizeBoxBorderOnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (e.Handled)
        {
            return;
        }

        if (_isResizing)
        {
            if (_isResizingTop)
            {
                var h=_resizeStartPoint.Y-e.GetPosition(TopLevel.GetTopLevel(this)).Y;
                if (h>0)
                {
                    this.Height = h;
                    _dragTransform.Y = _resizeStartPoint.Y - h;
                }
                else
                {
                    this.Height = -h;
                   // _dragTransform.Y = _resizeStartPoint.Y - h;
                }
                
            }
            if (_isResizingBottom)
            {
                
                var h=e.GetPosition(TopLevel.GetTopLevel(this)).Y-_resizeStartPoint.Y;
                if (h>0)
                {
                    this.Height = h;
                }
                else
                {
                    this.Height = -h;
                    _dragTransform.Y = _resizeStartPoint.Y + h;
                }
            }
            if (_isResizingLeft)
            {
                var w=_resizeStartPoint.X-e.GetPosition(TopLevel.GetTopLevel(this)).X;
                if (w>0)
                {
                    this.Width = w;
                    _dragTransform.X = _resizeStartPoint.X - w;
                }
                else
                {
                    this.Width = -w;
                    
                }
                
            }
            if (_isResizingRight)
            {
                var w=e.GetPosition(TopLevel.GetTopLevel(this)).X-_resizeStartPoint.X;
                if (w>0)
                {
                    this.Width = w;
                }
                else
                {
                    this.Width = -w;
                    _dragTransform.X = _resizeStartPoint.X + w;
                }
                
            }
            RaiseEvent(new LocationOrSizeChangedEventArgs(){ Source = this, RoutedEvent = LocationOrSizeChangedEvent});
        }
        else
        {
            var position = e.GetPosition((Visual?)sender);
            UpdatePointerOnResizeSizeBoxBorder(position);
        }
    }
    private void ResizeSizeBoxBorderOnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (e.InitialPressMouseButton==MouseButton.Left)
        {
            _isResizing = false;
        }
    }
    private void ResizeSizeBoxBorderOnPointerEntered(object? sender, PointerEventArgs e)
    {
        var position = e.GetPosition((Visual?)sender);
        UpdatePointerOnResizeSizeBoxBorder(position);
    }
    private void ResizeSizeBoxBorderOnPointerExited(object? sender, PointerEventArgs e)
    {
        Cursor?.Dispose();
        Cursor=Cursor.Default;
    }
    private void UpdatePointerOnResizeSizeBoxBorder(Point position)
    {
        var top = false;
        var left = false;
        var right = false;
        var bottom = false;
           
        if (position.Y < 3)
        {
            top = true;
        }

        if (position.Y > this.Height - 3)
        {
            bottom = true;
        }

        if (position.X > this.Width - 3)
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

    
    

    #endregion
    #region 内部控件: 拖拽
    private void ContentOnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
       

        var visualParent = (Canvas)this.GetVisualParent();
        foreach (var canvasChild in visualParent.Children)
        {
            if (canvasChild is DraggableResizeableControl draggableResizeableControl)
            {
                if (draggableResizeableControl.OnlyShowReSizingBoxOnSelect)
                {
                    if (draggableResizeableControl.VisualChildren[0] is Panel panel)
                    {
                        var border = panel.GetChildOfType<Border>("ResizeSizeBoxBorder");
                        border.IsVisible = false;
                    
                    }
                }
                
            }
        }
        if (OnlyShowReSizingBoxOnSelect)
        {
            _resizeSizeBoxBorder.IsVisible = true;
        }
        if (e.Handled)
        {
            return;
        }
        if (e.GetCurrentPoint(TopLevel.GetTopLevel(this)).Properties.IsLeftButtonPressed)
        {
            _isDragging = true;
            _dragStartPoint = e.GetPosition(TopLevel.GetTopLevel(this));
        }
    }
    private void ContentOnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (e.Handled)
        {
            return;
        }

        if (OnlyShowReSizingBoxOnSelect)
        {
            Cursor?.Dispose();
            Cursor=new Cursor(StandardCursorType.SizeAll);
        }
        
        if (_isDragging)
        {
            var dragDelta = e.GetPosition(TopLevel.GetTopLevel(this)) - _dragStartPoint;
            _dragStartPoint = e.GetPosition(TopLevel.GetTopLevel(this));
            _dragTransform.X += dragDelta.X;
            _dragTransform.Y += dragDelta.Y;
            
            RaiseEvent(new LocationOrSizeChangedEventArgs(){ Source = this, RoutedEvent = LocationOrSizeChangedEvent});
        }
    }
    private void ContentOnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (e.Handled)
        {
            return;
        }
        if (_isDragging&& e.InitialPressMouseButton == MouseButton.Left)
        {
            _isDragging = false;
        }
    }
    

    #endregion
    
    
   
}