using System.Linq;
using CommunityToolkit.Mvvm.Messaging;
using Core.SDKs.HotKey;
using Core.SDKs.Services;
using Core.SDKs.Services.Config;
using KitopiaAvalonia;
using KitopiaAvalonia.SDKs;
using KitopiaAvalonia.Windows;
using Microsoft.Extensions.DependencyInjection;
using Window = Avalonia.Controls.Window;
using WindowStartupLocation = Avalonia.Controls.WindowStartupLocation;

namespace Kitopia.Services;

public class HotKeyEditorService : IHotKeyEditor
{
    public void EditByName(string name, object? owner)
    {
        var hotKeyModel = HotKeyManager.HotKeys.FirstOrDefault(e =>
        {
            if ($"{e.MainName}_{e.Name}".Equals(name))
            {
                return true;
            }

            return false;
        });
        if (hotKeyModel == null)
        {
            return;
        }

        EditByHotKeyModel(hotKeyModel, owner);
    }

    public void EditByHotKeyModel(HotKeyModel name, object? owner)
    {
        var hotKeyEditor = new HotKeyEditorWindow(name);
        hotKeyEditor.Height = ServiceManager.Services.GetService<MainWindow>().Height / 2;
        hotKeyEditor.Width = ServiceManager.Services.GetService<MainWindow>().Width / 2;

        hotKeyEditor.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        hotKeyEditor.Title = "修改快捷键";
        if (owner is null)
        {
            hotKeyEditor.Show();
        }
        else
        {
            hotKeyEditor.ShowDialog((Window)owner);
        }
    }

    public void UnuseByHotKeyModel(HotKeyModel hotKeyModel)
    {
        hotKeyModel.IsUsable = false;
        WeakReferenceMessenger.Default.Send(hotKeyModel.SignName, "hotkey");
    }

    public void RemoveByHotKeyModel(HotKeyModel hotKeyModel)
    {
        HotKeyManager.HotKeys.Remove(hotKeyModel);
    }
}