namespace Core.SDKs.Services;

public interface IContentDialog
{
    Task ShowDialogAsync(object? contentPresenter, DialogContent dialogContent);

    void ShowDialog(object? contentPresenter, DialogContent dialogContent);
}