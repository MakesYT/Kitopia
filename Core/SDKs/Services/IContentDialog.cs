namespace Core.SDKs.Services;

public interface IContentDialog
{
    void ShowDialogAsync(object contentPresenter, string title, object content, Action? yes, Action? no);

    void ShowDialog(object contentPresenter, string title, object content, string CloseButtonText,
        string SecondaryButtonText, string PrimaryButtonText, Action? yes, Action? no, Action? cancel);
}