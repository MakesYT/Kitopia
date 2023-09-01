namespace Core.SDKs.Services;

public interface IContentDialog
{
    void ShowDialog(object contentPresenter, string title, object content, Action? yes, Action? no);
}