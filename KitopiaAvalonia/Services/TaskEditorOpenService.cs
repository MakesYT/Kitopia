﻿#region

using Avalonia.Threading;
using Core.SDKs.CustomScenario;
using Core.SDKs.Services;
using KitopiaAvalonia.Windows;

#endregion

namespace KitopiaAvalonia.Services;

public class TaskEditorOpenService : ITaskEditorOpenService
{
    public void Open() =>
       Dispatcher.UIThread.Post(() =>
        {
            ((TaskEditor)ServiceManager.Services!.GetService(typeof(TaskEditor))!)!.Show();
        });

    public void Open(CustomScenario name) =>
        Dispatcher.UIThread.Post(() =>
        {
            var taskEditor = ((TaskEditor)ServiceManager.Services!.GetService(typeof(TaskEditor))!)!;
            taskEditor.LoadTask(name);
            taskEditor.Show();
        });
}