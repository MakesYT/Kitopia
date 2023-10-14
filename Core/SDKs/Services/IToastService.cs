namespace Core.SDKs.Services;

public interface IToastService
{
    public void Show(string text);

    [STAThread]
    public void ShowMessageBox(string title, string content, Action? yesAction, Action? noAction);

    void ShowMessageBoxW(string title, object content, ShowMessageContent showMessageContent);
}