#region

using System.Windows;
using Core.SDKs.CustomScenario;
using Core.SDKs.Services;
using Kitopia.View;

#endregion

namespace Kitopia.Services;

public class TaskEditorOpenService : ITaskEditorOpenService
{
    public void Open()
    {
        Application.Current.Dispatcher.BeginInvoke(() =>
        {
            ((TaskEditor)ServiceManager.Services!.GetService(typeof(TaskEditor))!)!.Show();
        });
    }

    public void Open(CustomScenario name)
    {
        Application.Current.Dispatcher.BeginInvoke(() =>
        {
            var taskEditor = ((TaskEditor)ServiceManager.Services!.GetService(typeof(TaskEditor))!)!;
            taskEditor.LoadTask(name);
            taskEditor.Show();
        });
    }
}