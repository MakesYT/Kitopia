namespace Core.SDKs.Services;

public interface IToastService
{
    public void show(string text);

    [STAThread]
    public void showMessageBox(string Title, string Content, Action? yesAction, Action? noAction);
}