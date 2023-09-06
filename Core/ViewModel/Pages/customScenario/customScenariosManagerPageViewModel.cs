using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.SDKs.CustomScenario;
using Core.SDKs.Services;

namespace Core.ViewModel.Pages.customScenario;

public partial class CustomScenariosManagerPageViewModel : ObservableRecipient
{
    [RelayCommand]
    public void NewCustomScenarios()
    {
        ((ITaskEditorOpenService)ServiceManager.Services!.GetService(typeof(ITaskEditorOpenService))!).Open();
    }

    [RelayCommand]
    private void ToTaskEditPage(CustomScenario scenario)
    {
        ((ITaskEditorOpenService)ServiceManager.Services!.GetService(typeof(ITaskEditorOpenService))!).Open(
            scenario);
    }
}