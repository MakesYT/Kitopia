using System.Linq;
using System.Windows;
using System.Windows.Input;
using Windows.System;
using CommunityToolkit.Mvvm.Messaging;
using Core.SDKs.Config;
using Core.SDKs.HotKey;
using Core.SDKs.Services;
using Wpf.Ui.Controls.Window;

namespace Kitopia.Controls.HotKeyEditor;

public partial class HotKeyEditorWindow : FluentWindow
{
    public string name;

    public HotKeyEditorWindow(string name)
    {
        this.name = name;
        DataContext = new HotKeyEditorViewModel();
        InitializeComponent();
        var hotKeyModel = ConfigManger.config.hotKeys.FirstOrDefault((e) =>
        {
            if (e.Name.Equals(name))
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

        KeyName.Content = hotKeyModel.SelectKey.ToString();
    }

    private EKey selectedKey;

    private void HotKeyEditorWindow_OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control) ||
            e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Alt) ||
            e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Windows))
        {
            if (e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control))
            {
                Ctrl.Visibility = Visibility.Visible;
            }
            else Ctrl.Visibility = Visibility.Collapsed;

            if (e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Alt))
            {
                Alt.Visibility = Visibility.Visible;
            }
            else Alt.Visibility = Visibility.Collapsed;

            if (e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                Shift.Visibility = Visibility.Visible;
            }
            else Shift.Visibility = Visibility.Collapsed;

            if (e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Windows))
            {
                Win.Visibility = Visibility.Visible;
            }
            else Win.Visibility = Visibility.Collapsed;

            if (e.Key != Key.LeftShift && e.Key != Key.RightShift && e.Key != Key.LeftAlt && e.Key != Key.RightAlt &&
                e.Key != Key.LWin && e.Key != Key.RWin && e.Key != Key.LeftCtrl && e.Key != Key.RightCtrl &&
                (int)e.SystemKey != 120 && (int)e.SystemKey != 121)
            {
                Key key = (e.Key == Key.System ? e.SystemKey : e.Key);
                selectedKey = ((EKey)((VirtualKey)KeyInterop.VirtualKeyFromKey(key)));
                KeyName.Content = selectedKey.ToString();
            }
        }
    }

    private void HotKeyEditorWindow_OnKeyUp(object sender, KeyEventArgs e)
    {
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        var hotKeyModel = ConfigManger.config.hotKeys.FirstOrDefault((e) =>
        {
            if (e.Name.Equals(name))
            {
                return true;
            }

            return false;
        });
        if (Alt.Visibility == Visibility.Visible)
        {
            hotKeyModel.IsSelectAlt = true;
        }
        else hotKeyModel.IsSelectAlt = false;

        if (Win.Visibility == Visibility.Visible)
        {
            hotKeyModel.IsSelectWin = true;
        }
        else hotKeyModel.IsSelectWin = false;

        if (Shift.Visibility == Visibility.Visible)
        {
            hotKeyModel.IsSelectShift = true;
        }
        else hotKeyModel.IsSelectShift = false;

        if (Ctrl.Visibility == Visibility.Visible)
        {
            hotKeyModel.IsSelectCtrl = true;
        }
        else hotKeyModel.IsSelectCtrl = false;

        hotKeyModel.SelectKey = selectedKey;
        ConfigManger.Save();
        ((InitWindow)(ServiceManager.Services.GetService(typeof(InitWindow)))).InitHotKey();
        WeakReferenceMessenger.Default.Send("hotKeyChanged", "hotkey");
        this.Close();
    }
}