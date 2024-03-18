using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.Core;
using ReactiveUI;

namespace KitopiaAvalonia.Controls;

public class ListShow : ListBox 
{
    //ObservableCollection

    //DelCommand
    public static readonly StyledProperty<ICommand> DelCommandProperty = AvaloniaProperty.Register<ListShow, ICommand>(nameof(DelCommand));
    public ICommand DelCommand
    {
        get => GetValue(DelCommandProperty);
        set => SetValue(DelCommandProperty, value);
    }
    //设置默认DelCommand
    public ListShow()
    {
        SetValue(DelCommandProperty, new RelayCommand<string>(OnDel));
    }
    //DelCommand执行方法
    private void OnDel(string? obj)
    {
        if (obj == null)
        {
            return;
        }

        if (ItemsSource is IList list)
        {
            list.Remove(obj);

        }
        
           
    }
    
}