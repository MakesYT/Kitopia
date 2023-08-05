using System.Windows;
using Core.SDKs.Services;
using Kitopia.View;

namespace Kitopia.Services;

public class TaskEditorOpenService : ITaskEditorOpenService
{
    public void Open()
    {
#if DEBUG
        Application.Current.Dispatcher.BeginInvoke(() =>
        {
            ((TaskEditor)ServiceManager.Services!.GetService(typeof(TaskEditor))!)!.Show();
        });

#endif
    }
}