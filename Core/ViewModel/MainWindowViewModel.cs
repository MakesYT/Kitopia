using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Core.SDKs.Services;

namespace Core.ViewModel;

public partial class MainWindowViewModel: ObservableRecipient
{
    [RelayCommand]
    public void Exit()
    {
        Environment.Exit(0);
    }

    [RelayCommand]
    public void ChangeTheme()
    {
        ((IThemeChange)ServiceManager.Services.GetService(typeof(IThemeChange))).changeAnother();
        WeakReferenceMessenger.Default.Send("f","themeChange");
    }
}