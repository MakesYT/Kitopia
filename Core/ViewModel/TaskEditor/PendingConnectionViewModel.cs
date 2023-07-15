using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Core.ViewModel.TaskEditor;

public partial class PendingConnectionViewModel : ObservableRecipient
{
    private readonly TaskEditorViewModel _editor;
    [ObservableProperty] private ConnectorItem _source;

    public PendingConnectionViewModel(TaskEditorViewModel editor)
    {
        _editor = editor;

        FinishCommand = new RelayCommand<ConnectorItem>(target =>
        {
            if (target == null)
                return;

            if (target != Source && target.Source != Source.Source)
            {
                if (Source.IsOut != target.IsOut)
                {
                    if (!Source.IsOut)
                    {
                        _editor.Connect(target, Source);
                    }
                    else
                    {
                        _editor.Connect(Source, target);
                    }
                }
            }
        });
    }

    [ObservableProperty] private object? _previewTarget;
    [ObservableProperty] private string _previewText;

    partial void OnPreviewTargetChanged(object? value)
    {
        bool canConnect = value != null;
        switch (value)
        {
            case ConnectorItem con:
            {
                if (con == Source || con.Source == Source.Source)
                    PreviewText = $"不能自己连接自己";
                else
                {
                    if (Source.IsOut == con.IsOut)
                    {
                        PreviewText = $"错误的连接";
                    }
                    else
                        PreviewText = "连接";
                }

                break;
            }
            default:
                PreviewText = $"丢弃连接";
                break;
        }
    }

    [RelayCommand]
    public void Start(ConnectorItem item)
    {
        Source = item;
    }

    public ICommand FinishCommand
    {
        get;
    }
}