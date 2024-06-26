﻿#region

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.SDKs.Services.Config;

#endregion

namespace Core.ViewModel;

public partial class MainWindowViewModel : ObservableRecipient
{
    [RelayCommand]
    public void Exit()
    {
        ConfigManger.Save();
        Environment.Exit(0);
    }
}