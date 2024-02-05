using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Core.SDKs;

namespace KitopiaAvalonia.Controls;

public partial class Dialog : Window
{
    private bool _mouseDownForWindowMoving = false;
    private PointerPoint _originalPoint;

    public Dialog()
    {
        InitializeComponent();
        RenderOptions.SetTextRenderingMode(this, TextRenderingMode.Antialias);
    }

    public Dialog(DialogContent content)
    {
        InitializeComponent();
        RenderOptions.SetTextRenderingMode(this, TextRenderingMode.Antialias);
        Title.Text = content.Title;
        Content.Content = content.Content;

        if (content.PrimaryButtonText != null)
        {
            PrimaryButton.Content = content.PrimaryButtonText;

            PrimaryButton.Click += (sender, args) =>
            {
                this.Close();
                content.PrimaryAction?.Invoke();
            };
            PrimaryButton.IsVisible = true;
        }

        if (content.CloseButtonText != null)
        {
            CloseButton.Content = content.CloseButtonText;
            CloseButton.Click += (sender, args) =>
            {
                this.Close();
                content.CloseAction?.Invoke();
            };
            CloseButton.IsVisible = true;
        }

        if (content.SecondaryButtonText != null)
        {
            SecondaryButton.Content = content.SecondaryButtonText;
            SecondaryButton.Click += (sender, args) =>
            {
                this.Close();
                content.SecondaryAction?.Invoke();
            };
            SecondaryButton.IsVisible = true;
        }

        PrimaryButton.Classes.Add("accent");
    }

    private void InputElement_OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (!_mouseDownForWindowMoving) return;

        PointerPoint currentPoint = e.GetCurrentPoint(this);
        Position = new PixelPoint(Position.X + (int)(currentPoint.Position.X - _originalPoint.Position.X),
            Position.Y + (int)(currentPoint.Position.Y - _originalPoint.Position.Y));
    }

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (WindowState == WindowState.Maximized || WindowState == WindowState.FullScreen) return;

        _mouseDownForWindowMoving = true;
        _originalPoint = e.GetCurrentPoint(this);
    }

    private void InputElement_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _mouseDownForWindowMoving = false;
    }
}