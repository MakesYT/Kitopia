﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Core.ViewModel;

public partial class  MainWindowsViewModel : ObservableRecipient
{
    [RelayCommand]
    public void Exit()
    {
        Environment.Exit(0);
    }
}