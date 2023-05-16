using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.SDKs.Services;

namespace Core.ViewModel.Pages;

public partial class SettingPageViewModel: ObservableRecipient
{
    public SettingPageViewModel()
    {
        CurrentThemeIsDark = ((IThemeChange)ServiceManager.Services.GetService(typeof(IThemeChange))).isDark();
    }
    [ObservableProperty] public bool currentThemeIsDark = false;
    
    [RelayCommand]
    public void changeTheme(string name)
    {
        ((IThemeChange)ServiceManager.Services.GetService(typeof(IThemeChange))).changeTo(name);
        CurrentThemeIsDark = ((IThemeChange)ServiceManager.Services.GetService(typeof(IThemeChange))).isDark();
    }
}