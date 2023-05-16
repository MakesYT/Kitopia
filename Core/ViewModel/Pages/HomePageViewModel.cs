﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Core.ViewModel.Pages;

public partial class HomePageViewModel: ObservableRecipient
{
    [ObservableProperty] public string name = "2";

    [RelayCommand]
    public void Click()
    {
        
    }
}