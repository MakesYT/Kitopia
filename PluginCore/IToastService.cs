using System;

namespace PluginCore;

public interface IToastService
{
    public void Show(string header, string text);

    public void ShowMessageBox(string title, string content, Action? yesAction, Action? noAction);

    void ShowMessageBoxW(string title, object content, ShowMessageContent showMessageContent);
}