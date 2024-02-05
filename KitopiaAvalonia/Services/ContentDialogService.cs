#region

using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using Core.SDKs;
using Core.SDKs.Services;
using FluentAvalonia.UI.Controls;
using KitopiaAvalonia.Controls;

#endregion

namespace KitopiaAvalonia.Services;

public class ContentDialogService : IContentDialog
{
    public void ShowDialogAsync(object? contentPresenter, DialogContent dialogContent)
    {
        var dialog = new ContentDialog()
        {
            Title = dialogContent.Title,
            Content = dialogContent.Content,
            CloseButtonText = dialogContent.CloseButtonText,
            SecondaryButtonText = dialogContent.SecondaryButtonText,
            PrimaryButtonText = dialogContent.PrimaryButtonText,
            DefaultButton = ContentDialogButton.Primary
        };
        dialog.ShowAsync((TopLevel)contentPresenter!).ContinueWith(e =>
        {
            switch (e.Result)
            {
                case ContentDialogResult.Primary:
                {
                    dialogContent.PrimaryAction?.Invoke();
                    break;
                }
                case ContentDialogResult.Secondary:
                {
                    dialogContent.SecondaryAction?.Invoke();
                    break;
                }
                case ContentDialogResult.None:
                {
                    dialogContent.CloseAction?.Invoke();
                    break;
                }
            }
        });
    }

    public void ShowDialog(object? contentPresenter, DialogContent dialogContent)
    {
        if (contentPresenter is null)
        {
            Task.Run(() =>
            {
                Thread.CurrentThread.IsBackground = false;
                Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    var tcs = new TaskCompletionSource();
                    var dialog = new Dialog(dialogContent);
                    dialog.Show();
                    dialog.Closed += (sender, args) =>
                    {
                        tcs.SetResult();
                    };
                    await tcs.Task;
                });
            }).Wait();
        }
        else
        {
            var dialog = new ContentDialog()
            {
                Title = dialogContent.Title,
                Content = dialogContent.Content,
                CloseButtonText = dialogContent.CloseButtonText,
                PrimaryButtonText = dialogContent.PrimaryButtonText,
                DefaultButton = ContentDialogButton.Primary
            };
            dialog.ShowAsync((TopLevel)contentPresenter!).ContinueWith(e =>
            {
                switch (e.Result)
                {
                    case ContentDialogResult.Primary:
                    {
                        dialogContent.PrimaryAction?.Invoke();
                        break;
                    }
                    case ContentDialogResult.Secondary:
                    {
                        dialogContent.SecondaryAction?.Invoke();
                        break;
                    }
                    case ContentDialogResult.None:
                    {
                        dialogContent.CloseAction?.Invoke();
                        break;
                    }
                }
            }).Wait();
        }
    }
}