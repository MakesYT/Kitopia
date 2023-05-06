using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Core.ViewModel;

public class MainWindowsViewModel : ObservableRecipient
{
    [RelayCommand]
    public void Exit()
    {
        Environment.Exit(0);
    }
}