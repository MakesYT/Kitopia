using System;
using System.Windows.Controls;
using Core.SDKs.Services;
using Wpf.Ui.Controls;

namespace Kitopia.Services;

public class ContentDialogService : IContentDialog
{
    public void ShowDialog(object contentPresenter, string title, object content, Action? yes, Action? no)
    {
        var dialog = new ContentDialog((ContentPresenter)contentPresenter)
        {
            Title = title,
            Content = content,
            CloseButtonText = "关闭",
            PrimaryButtonText = "确定",
        };
        dialog.ShowAsync().ContinueWith((e) =>
        {
            switch (e.Result)
            {
                case ContentDialogResult.Primary:
                {
                    yes.Invoke();
                    break;
                }
                case ContentDialogResult.None:
                {
                    no.Invoke();
                    break;
                }
            }
        });
    }
}