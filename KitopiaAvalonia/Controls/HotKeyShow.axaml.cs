using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Primitives;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Core.SDKs.HotKey;
using Core.SDKs.Services;
using Core.SDKs.Services.Config;
using KitopiaAvalonia.SDKs;
using Vanara.PInvoke;

namespace KitopiaAvalonia.Controls;

public class HotKeyShow : TemplatedControl
{
    public enum KeyTypeE
    {
        None = 0000,
        NoControlKey = 10000,
        CtrlAlt = 1010,
        Ctrl = 1000,
        CtrlShift = 1100,
        CtrlShiftAlt = 1110,
        Alt = 0010,
        AltShift = 0110,
        Win = 0001,
        WinShift = 0101,
        WinAlt = 0011,
        Shift = 0100
    }
    
    public static readonly StyledProperty<HotKeyModel> HotKeyModelProperty =
        AvaloniaProperty.Register<HotKeyShow, HotKeyModel>(nameof(HotKeyModel), coerce: (o, s) =>
        {
            HotKeyModelChanged(s, (HotKeyShow)o);
            return s;
        });

    public static readonly StyledProperty<KeyTypeE> KeyTypeProperty =
        AvaloniaProperty.Register<HotKeyShow, KeyTypeE>(nameof(KeyType));

    public static readonly StyledProperty<string> KeyNameProperty =
        AvaloniaProperty.Register<HotKeyShow, string>(nameof(KeyName), "-1");

    public static readonly StyledProperty<ICommand> RemoveHotKeyProperty =
        AvaloniaProperty.Register<HotKeyShow, ICommand>(nameof(RemoveHotKey));

    public static readonly StyledProperty<ICommand> EditHotKeyProperty =
        AvaloniaProperty.Register<HotKeyShow, ICommand>(nameof(EditHotKey));
    //IsActivated
    public static readonly StyledProperty<bool> IsActivatedProperty =
        AvaloniaProperty.Register<HotKeyShow, bool>(nameof(IsActivated), false);
    public bool IsActivated
    {
        get => (bool)GetValue(IsActivatedProperty);
        set => SetValue(IsActivatedProperty, value);
    }
    


    private HotKeyShow Default;

    public HotKeyShow()
    {
        Default = this;
        WeakReferenceMessenger.Default.Register<string, string>(this, "hotkey", (_, s) =>
        {
            if (s == HotKeyModel.SignName)
            {
                HotKeyModelChanged(HotKeyModel, this);
            }
        });
        SetValue(RemoveHotKeyProperty, new RelayCommand(Remove));
        SetValue(EditHotKeyProperty, new RelayCommand(Edit));
    }
    

    [Bindable(true)]
    [Category("KeyType")]
    public HotKeyModel? HotKeyModel
    {
        get => (HotKeyModel)GetValue(HotKeyModelProperty);
        set => SetValue(HotKeyModelProperty, value);
    }

    [Bindable(true)]
    [Category("KeyType")]
    public KeyTypeE KeyType
    {
        get => (KeyTypeE)GetValue(KeyTypeProperty);
        private set => SetValue(KeyTypeProperty, value);
    }

    [Bindable(true)]
    [Category("KeyName")]
    public string KeyName
    {
        get => (string)GetValue(KeyNameProperty);
        private set => SetValue(KeyNameProperty, value);
    }

    [Bindable(true)]
    [Category("RemoveHotKey")]
    public ICommand RemoveHotKey
    {
        get => (ICommand)GetValue(RemoveHotKeyProperty);
        private set => SetValue(RemoveHotKeyProperty, value);
    }

    [Bindable(true)]
    [Category("EditHotKey")]
    public ICommand EditHotKey
    {
        get => (ICommand)GetValue(EditHotKeyProperty);
        private set => SetValue(EditHotKeyProperty, value);
    }

    private static void HotKeyModelChanged(HotKeyModel? hotKeyModel, HotKeyShow hotKeyShow)
    {
        var type = 0000;
        if (hotKeyModel == null)
        {
            hotKeyShow.KeyType = (HotKeyShow.KeyTypeE)type;
            return;
        }

        if (hotKeyModel.IsSelectAlt)
        {
            type += 10;
        }

        if (hotKeyModel.IsSelectCtrl)
        {
            type += 1000;
        }

        if (hotKeyModel.IsSelectShift)
        {
            type += 100;
        }

        if (hotKeyModel.IsSelectWin)
        {
            type += 1;
        }

        if (type == 0000 && hotKeyModel.SelectKey != EKey.未设置)
        {
            type = 10000;
        }

        hotKeyShow.IsActivated = hotKeyModel.IsUsable;
        hotKeyShow.KeyType = (HotKeyShow.KeyTypeE)type;
        hotKeyShow.KeyName = hotKeyModel.SelectKey.ToString();
    }

    private void Remove()
    {
        if (HotKeyModel != null)
        {
            HotKeyModel.IsUsable = false;
            IsActivated = false;
            ConfigManger.Save(ConfigManger.Config.Name);
        }
    }


    private void Edit()
    {
        if (HotKeyModel is null)
        {
            return;
        }

        if (HotKeyModel.IsUsable == false&& HotKeyModel.SelectKey != EKey.未设置)
        {
            IsActivated = true;
            HotKeyModel.IsUsable = true;
            return;
        }
        
        if (App.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            foreach (var desktopWindow in desktop.Windows)
            {
                if (desktopWindow.TryGetPlatformHandle().Handle == User32.GetForegroundWindow())
                {
                    ((IHotKeyEditor)ServiceManager.Services.GetService(typeof(IHotKeyEditor))!).EditByHotKeyModel(
                        HotKeyModel,
                        desktopWindow);
                    return;
                }
            }
        }
        

        ((IHotKeyEditor)ServiceManager.Services.GetService(typeof(IHotKeyEditor))!).EditByHotKeyModel(HotKeyModel,
            null);
    }
}