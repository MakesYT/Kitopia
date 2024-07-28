#region

using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using Core.SDKs;
using Core.SDKs.Services;
using KitopiaAvalonia.Controls;
using Ursa.Controls;
using Dialog = KitopiaAvalonia.Controls.Dialog;

#endregion

namespace KitopiaAvalonia.Services;

public class ContentDialogService : IContentDialog
{
    public async Task ShowDialogAsync(object? contentPresenter, DialogContent dialogContent)
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

        await Task.Run(async () =>
        {
            DialogButton button = DialogButton.None;
            if (dialogContent.CloseButtonText is null && dialogContent.PrimaryButtonText is null &&
                dialogContent.SecondaryButtonText is null)
            {
                button = DialogButton.None;
            }
            else if (dialogContent.PrimaryButtonText is null && dialogContent.SecondaryButtonText is null &&
                     dialogContent.CloseButtonText is not null)
            {
                button = DialogButton.None;
            }
            else if (dialogContent.CloseButtonText is null && dialogContent.PrimaryButtonText is null &&
                     dialogContent.SecondaryButtonText is not null)
            {
                button = DialogButton.OKCancel;
            }
            else if (dialogContent.CloseButtonText is null && dialogContent.PrimaryButtonText is not null &&
                     dialogContent.SecondaryButtonText is null)
            {
                button = DialogButton.OK;
            }
            else if (dialogContent.CloseButtonText is null && dialogContent.PrimaryButtonText is not null &&
                     dialogContent.SecondaryButtonText is not null)
            {
                button = DialogButton.YesNo;
            }
            else if (dialogContent.CloseButtonText is not null && dialogContent.PrimaryButtonText is not null &&
                     dialogContent.SecondaryButtonText is not null)
            {
                button = DialogButton.YesNoCancel;
            }

            var dialog = new DefaultDialogWindow()
            {
                Title = dialogContent.Title,
                Content = dialogContent.Content,
                Buttons = button,
            };
            dialog.Resources.Add("STRING_MENU_DIALOG_NO", dialogContent.CloseButtonText);
            var result = await Ursa.Controls.Dialog.ShowModal<TextDialog, TextDialogViewModel>(
                new TextDialogViewModel() { Text = dialogContent.Content }, (Window)contentPresenter,
                new DialogOptions()
                {
                    Title = dialogContent.Title,
                    Button = button,
                });

            switch (result)
            {
                case DialogResult.Yes:
                {
                    dialogContent.PrimaryAction?.Invoke();
                    break;
                }
                case DialogResult.No:
                {
                    dialogContent.SecondaryAction?.Invoke();
                    break;
                }
                case DialogResult.Cancel:
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

            Dispatcher.UIThread.InvokeAsync(() =>
            {
                var dialog = new Dialog(dialogContent);
                dialog.Show();
                var frame = new DispatcherFrame();
                dialog.Closed += (sender, args) => { tcs.SetResult(); };
                dialog.Show();
            }).Wait();
            tcs.Task.Wait();
        }
        else
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                DialogButton button = DialogButton.None;
                if (dialogContent.CloseButtonText is null && dialogContent.PrimaryButtonText is null &&
                    dialogContent.SecondaryButtonText is null)
                {
                    button = DialogButton.None;
                }
                else if (dialogContent.PrimaryButtonText is null && dialogContent.SecondaryButtonText is null &&
                         dialogContent.CloseButtonText is not null)
                {
                    button = DialogButton.None;
                }
                else if (dialogContent.CloseButtonText is null && dialogContent.PrimaryButtonText is null &&
                         dialogContent.SecondaryButtonText is not null)
                {
                    button = DialogButton.OKCancel;
                }
                else if (dialogContent.CloseButtonText is null && dialogContent.PrimaryButtonText is not null &&
                         dialogContent.SecondaryButtonText is null)
                {
                    button = DialogButton.OK;
                }
                else if (dialogContent.CloseButtonText is null && dialogContent.PrimaryButtonText is not null &&
                         dialogContent.SecondaryButtonText is not null)
                {
                    button = DialogButton.YesNo;
                }
                else if (dialogContent.CloseButtonText is not null && dialogContent.PrimaryButtonText is not null &&
                         dialogContent.SecondaryButtonText is not null)
                {
                    button = DialogButton.YesNoCancel;
                }

                var dialog = new DefaultDialogWindow()
                {
                    Title = dialogContent.Title,
                    Content = dialogContent.Content,
                    Buttons = button,
                };
                dialog.Resources.Add("STRING_MENU_DIALOG_NO", dialogContent.CloseButtonText);
                var result = Ursa.Controls.Dialog.ShowModal<TextDialog, TextDialogViewModel>(
                    new TextDialogViewModel() { Text = dialogContent.Content }, (Window)contentPresenter,
                    new DialogOptions()
                    {
                        Title = dialogContent.Title,
                        Button = button,
                    }).Result;

                switch (result)
                {
                    case DialogResult.Yes:
                    {
                        dialogContent.PrimaryAction?.Invoke();
                        break;
                    }
                    case DialogResult.No:
                    {
                        dialogContent.SecondaryAction?.Invoke();
                        break;
                    }
                    case DialogResult.Cancel:
                    {
                        dialogContent.CloseAction?.Invoke();
                        break;
                    }
                }
            }).Wait();
        }
    }
}