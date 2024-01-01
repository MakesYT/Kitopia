namespace Core.SDKs.Services;

public interface IContentDialog
{
    void ShowDialogAsync(object? contentPresenter, DialogContent dialogContent);

    void ShowDialog(object? contentPresenter, DialogContent dialogContent);
}