using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Win32.Input;
using CommunityToolkit.Mvvm.Messaging;
using Core.SDKs;
using Core.SDKs.HotKey;
using Core.SDKs.Services;
using Core.SDKs.Services.Config;
using log4net;
using Microsoft.Extensions.DependencyInjection;
using Window = Avalonia.Controls.Window;

namespace KitopiaAvalonia.Windows;

public partial class HotKeyEditorWindow : Window
{
    private static readonly ILog Log = LogManager.GetLogger(nameof(HotKeyEditorWindow));
    private HotKeyModel? _hotKeyModel;
    private bool isFinnish;
    private EKey? selectedKey;
    private bool setSuccess;

    public HotKeyEditorWindow(HotKeyModel hotKeyModel)
    {
        InitializeComponent();
        RenderOptions.SetTextRenderingMode(this, TextRenderingMode.Antialias);
        _hotKeyModel = hotKeyModel;
        Name.Text = $"快捷键:{hotKeyModel.SignName}";


        if (hotKeyModel.IsSelectAlt)
        {
            Alt.IsVisible = true;
        }

        if (hotKeyModel.IsSelectCtrl)
        {
            Ctrl.IsVisible = true;
        }

        if (hotKeyModel.IsSelectShift)
        {
            Shift.IsVisible = true;
        }

        if (hotKeyModel.IsSelectWin)
        {
            Win.IsVisible = true;
        }

        selectedKey = hotKeyModel.SelectKey;
        KeyName.Content = hotKeyModel.SelectKey.ToString();
    }

    private void HotKeyEditorWindow_OnKeyDown(object sender, KeyEventArgs e)
    {
        {
            if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
            {
                Ctrl.IsVisible = true;
            }
            else
            {
                Ctrl.IsVisible = false;
            }

            if (e.KeyModifiers.HasFlag(KeyModifiers.Alt))
            {
                Alt.IsVisible = true;
            }
            else
            {
                Alt.IsVisible = false;
            }

            if (e.KeyModifiers.HasFlag(KeyModifiers.Shift))
            {
                Shift.IsVisible = true;
            }
            else
            {
                Shift.IsVisible = false;
            }

            if (e.KeyModifiers.HasFlag(KeyModifiers.Meta))
            {
                Win.IsVisible = true;
            }
            else
            {
                Win.IsVisible = false;
            }

            if (e.Key == Key.System)
            {
                var key = e.PhysicalKey;
                if (key != PhysicalKey.ShiftLeft && key != PhysicalKey.ShiftRight && key != PhysicalKey.AltLeft &&
                    key != PhysicalKey.AltRight &&
                    key != PhysicalKey.MetaLeft && key != PhysicalKey.MetaRight && key != PhysicalKey.ControlLeft &&
                    key != PhysicalKey.ControlRight)
                {
                    selectedKey = (EKey)KeyInterop.VirtualKeyFromKey((Key)(int)key);
                    KeyName.IsVisible = true;
                    KeyName.Content = selectedKey.ToString();
                }
                else
                {
                    KeyName.IsVisible = false;
                    selectedKey = null;
                }
            }
            else
            {
                var key = e.Key;
                if (key != Key.LeftShift && key != Key.RightShift && key != Key.LeftAlt && key != Key.RightAlt &&
                    key != Key.LWin && key != Key.RWin && key != Key.LeftCtrl && key != Key.RightCtrl)
                {
                    selectedKey = (EKey)KeyInterop.VirtualKeyFromKey(key);
                    KeyName.IsVisible = true;
                    KeyName.Content = selectedKey.ToString();
                }
                else
                {
                    KeyName.IsVisible = false;
                    selectedKey = null;
                }
            }
        }
    }

    private void HotKeyEditorWindow_OnKeyUp(object sender, KeyEventArgs e)
    {
    }


    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        if (Alt.IsVisible)
        {
            _hotKeyModel.IsSelectAlt = true;
        }
        else
        {
            _hotKeyModel.IsSelectAlt = false;
        }

        if (Win.IsVisible)
        {
            _hotKeyModel.IsSelectWin = true;
        }
        else
        {
            _hotKeyModel.IsSelectWin = false;
        }

        if (Shift.IsVisible)
        {
            _hotKeyModel.IsSelectShift = true;
        }
        else
        {
            _hotKeyModel.IsSelectShift = false;
        }

        if (Ctrl.IsVisible)
        {
            _hotKeyModel.IsSelectCtrl = true;
        }
        else
        {
            _hotKeyModel.IsSelectCtrl = false;
        }

        if (selectedKey is null)
        {
            return;
        }

        _hotKeyModel.SelectKey = selectedKey.Value;
        if (ConfigManger.Config.hotKeys.All(e2 => e2.SignName != _hotKeyModel.SignName))
        {
            ConfigManger.Config.hotKeys.Add(_hotKeyModel);
        }

        ConfigManger.Save();

        isFinnish = true;
        WeakReferenceMessenger.Default.Send(_hotKeyModel.SignName, "hotkey");
        Close();
    }

    private void ButtonCancle_OnClick(object sender, RoutedEventArgs e)
    {
        isFinnish = true;
        setSuccess = true;
        this.Close();
    }
}