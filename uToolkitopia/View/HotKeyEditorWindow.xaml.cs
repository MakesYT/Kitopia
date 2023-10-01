#region

using System.Linq;
using System.Windows;
using System.Windows.Input;
using Windows.System;
using CommunityToolkit.Mvvm.Messaging;
using Core.SDKs.HotKey;
using Core.SDKs.Services;
using Core.SDKs.Services.Config;
using Wpf.Ui.Controls;
using MessageBox = Kitopia.Controls.MessageBoxControl.MessageBox;

#endregion

namespace Kitopia.View;

public partial class HotKeyEditorWindow : FluentWindow
{
    private bool isFinnish;
    public string name;

    private EKey? selectedKey;
    private bool setSuccess;

    public HotKeyEditorWindow(string name)
    {
        InitializeComponent();
        this.name = name;
        Name.Text = $"快捷键:{name}";

        var hotKeyModel = ConfigManger.Config.hotKeys.FirstOrDefault(e =>
        {
            if ($"{e.MainName}_{e.Name}".Equals(name))
            {
                return true;
            }

            return false;
        });
        if (hotKeyModel.IsSelectAlt)
        {
            Alt.Visibility = Visibility.Visible;
        }

        if (hotKeyModel.IsSelectCtrl)
        {
            Ctrl.Visibility = Visibility.Visible;
        }

        if (hotKeyModel.IsSelectShift)
        {
            Shift.Visibility = Visibility.Visible;
        }

        if (hotKeyModel.IsSelectWin)
        {
            Win.Visibility = Visibility.Visible;
        }

        selectedKey = hotKeyModel.SelectKey;
        KeyName.Content = hotKeyModel.SelectKey.ToString();
    }

    private void HotKeyEditorWindow_OnKeyDown(object sender, KeyEventArgs e)
    {
        {
            if (e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control))
            {
                Ctrl.Visibility = Visibility.Visible;
            }
            else
            {
                Ctrl.Visibility = Visibility.Collapsed;
            }

            if (e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Alt))
            {
                Alt.Visibility = Visibility.Visible;
            }
            else
            {
                Alt.Visibility = Visibility.Collapsed;
            }

            if (e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                Shift.Visibility = Visibility.Visible;
            }
            else
            {
                Shift.Visibility = Visibility.Collapsed;
            }

            if (e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Windows))
            {
                Win.Visibility = Visibility.Visible;
            }
            else
            {
                Win.Visibility = Visibility.Collapsed;
            }

            var key = e.Key == Key.System ? e.SystemKey : e.Key;
            if (key != Key.LeftShift && key != Key.RightShift && key != Key.LeftAlt && key != Key.RightAlt &&
                key != Key.LWin && key != Key.RWin && key != Key.LeftCtrl && key != Key.RightCtrl)
            {
                selectedKey = (EKey)(VirtualKey)KeyInterop.VirtualKeyFromKey(key);
                KeyName.Visibility = Visibility.Visible;
                KeyName.Content = selectedKey.ToString();
            }
            else
            {
                KeyName.Visibility = Visibility.Collapsed;
                selectedKey = null;
            }
        }
    }

    private void HotKeyEditorWindow_OnKeyUp(object sender, KeyEventArgs e)
    {
    }

    public bool GetResult()
    {
        while (!isFinnish)
        {
        }

        return setSuccess;
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        var hotKeyModel = ConfigManger.Config.hotKeys.FirstOrDefault(e =>
        {
            if ($"{e.MainName}_{e.Name}".Equals(name))
            {
                return true;
            }

            return false;
        });
        if (Alt.Visibility == Visibility.Visible)
        {
            hotKeyModel.IsSelectAlt = true;
        }
        else
        {
            hotKeyModel.IsSelectAlt = false;
        }

        if (Win.Visibility == Visibility.Visible)
        {
            hotKeyModel.IsSelectWin = true;
        }
        else
        {
            hotKeyModel.IsSelectWin = false;
        }

        if (Shift.Visibility == Visibility.Visible)
        {
            hotKeyModel.IsSelectShift = true;
        }
        else
        {
            hotKeyModel.IsSelectShift = false;
        }

        if (Ctrl.Visibility == Visibility.Visible)
        {
            hotKeyModel.IsSelectCtrl = true;
        }
        else
        {
            hotKeyModel.IsSelectCtrl = false;
        }

        if (selectedKey is null)
        {
            return;
        }

        hotKeyModel.SelectKey = selectedKey;
        ConfigManger.Save();
        setSuccess = ((MainWindow)ServiceManager.Services.GetService(typeof(MainWindow))).HotKeySet(hotKeyModel);
        if (!setSuccess)
        {
            var msg = new MessageBox();
            msg.Title = "Kitopia";
            msg.Content = $"无法注册快捷键\n{hotKeyModel.MainName}_{hotKeyModel.Name}\n现在你需要重新设置\n在设置界面按下取消以取消该快捷键注册";
            msg.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            msg.CloseButtonText = "确定";
            msg.FontSize = 15;
            var task = msg.ShowDialogAsync();
            // 使用ContinueWith来在任务完成后执行一个回调函数
            task.Wait();
            return;
        }

        isFinnish = true;
        WeakReferenceMessenger.Default.Send("hotKeyChanged", "hotkey");
        Close();
    }

    private void ButtonCancle_OnClick(object sender, RoutedEventArgs e)
    {
        isFinnish = true;
        setSuccess = true;
    }
}