using System.Linq;
using System.Windows;
using Core.SDKs.HotKey;
using Core.SDKs.Services;
using Core.SDKs.Services.Config;
using Kitopia.View;
using Microsoft.Extensions.DependencyInjection;

namespace Kitopia.Services;

public class HotKeyEditorService : IHotKeyEditor
{
    public void EditByName(string name, object? owner)
    {
        var hotKeyModel = ConfigManger.Config.hotKeys.FirstOrDefault(e =>
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
        if (owner is Window)
        {
            hotKeyEditor.Owner = (Window)owner;
        }

        hotKeyEditor.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        hotKeyEditor.Title = "修改快捷键";
        hotKeyEditor.ShowDialog();
    }
}