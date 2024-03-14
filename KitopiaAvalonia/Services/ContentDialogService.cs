#region

using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using Core.SDKs;
using Core.SDKs.Services;
using FluentAvalonia.UI.Controls;
using KitopiaAvalonia.Controls;
using Microsoft.Extensions.DependencyInjection;

#endregion

namespace KitopiaAvalonia.Services;

public class ContentDialogService : IContentDialog
{
    public void ShowDialogAsync(object? contentPresenter, DialogContent dialogContent)
    {
        if (contentPresenter is null)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                var dialog = new Dialog(dialogContent);
                dialog.Show();
            });
            return;
        }

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
            var tcs = new TaskCompletionSource();
            
            Dispatcher.UIThread.InvokeAsync( () =>
            {
                                   
                var dialog = new Dialog(dialogContent);
                dialog.Show();
                var frame = new DispatcherFrame();
                dialog.Closed += (sender, args) =>
                {
                    tcs.SetResult();
                };
                dialog.Show();

            }).Wait();
            tcs.Task.Wait();
            
            

        }
        else
        {
            Dispatcher.UIThread.InvokeAsync(() =>
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
            }).Wait();
        }
    }
}