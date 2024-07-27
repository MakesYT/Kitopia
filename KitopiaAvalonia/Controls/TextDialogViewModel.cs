using CommunityToolkit.Mvvm.ComponentModel;

namespace KitopiaAvalonia.Controls;

public partial class TextDialogViewModel : ObservableObject
{
    [ObservableProperty] private object _text;
}