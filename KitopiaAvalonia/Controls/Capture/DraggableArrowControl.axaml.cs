using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace KitopiaAvalonia.Controls.Capture;


public class DraggableArrowControl : TemplatedControl
{
    public static readonly AvaloniaProperty StartTranslateTransformProperty =
        AvaloniaProperty.Register<DraggableResizeableControl, TranslateTransform>("_dragTransform");
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
  

    private bool _isDragging;
    private Point _dragStartPoint;
    
    public DraggableArrowControl()
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
        
    }

   

    
    #region 内部控件: 拖拽
    private void ContentOnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
       
        
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
        Cursor?.Dispose();
        Cursor=new Cursor(StandardCursorType.SizeAll);
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