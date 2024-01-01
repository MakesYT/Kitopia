using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.SDKs.Services.Config;

namespace Core.ViewModel;

public partial class AppViewModel : ObservableObject
{
    [RelayCommand]
    public void Exit()
    {
        ConfigManger.Save();
        Environment.Exit(0);
    }

    [RelayCommand]
    public void OpenMainWindow()
    {
        if (Application.Current!.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow!.Show();
        }
    }
}