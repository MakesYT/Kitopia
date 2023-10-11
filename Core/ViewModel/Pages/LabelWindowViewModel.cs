using CommunityToolkit.Mvvm.ComponentModel;

namespace Core.ViewModel.Pages;

public partial class LabelWindowViewModel : ObservableObject
{
    [ObservableProperty] private int _fontSize = 16;
}