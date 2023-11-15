using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.SDKs.CustomScenario;
using Core.SDKs.Services;
using Core.SDKs.Services.Config;
using PluginCore;

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

    [RelayCommand]
    private void StopCustomScenario(CustomScenario scenario)
    {
        scenario.Stop();
    }

    [RelayCommand]
    private void RunCustomScenario(CustomScenario scenario)
    {
        scenario.Run();
    }

    [RelayCommand]
    private void RemoveCustomScenario(CustomScenario scenario)
    {
        ((IToastService)ServiceManager.Services!.GetService(typeof(IToastService))!).ShowMessageBox(
            $"删除{scenario.Name}?", "是否确定删除?\n他真的会丢失很久很久(不可恢复)",
            () =>
            {
                Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    CustomScenarioManger.Remove(scenario);
                });
            }, () =>
            {
            }
        );
    }
}