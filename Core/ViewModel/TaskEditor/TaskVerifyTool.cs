#region

using CommunityToolkit.Mvvm.Input;

#endregion

namespace Core.ViewModel.TaskEditor;

public partial class TaskEditorViewModel
{
    private Dictionary<PointItem, Task?> _tasks = new();

    [RelayCommand]
    private void VerifyNode()
    {
        Scenario.Run(false);
    }

    private void ToFirstVerify(bool notRealTime = false)
    {
        Scenario.Run(true);
    }
}