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

    public static readonly StyledProperty<string> HotKeySignNameProperty =
        AvaloniaProperty.Register<HotKeyShow, string>(nameof(HotKeySignName), coerce: (o, s) =>
        {
            ((HotKeyShow)o).HotKeyModel =
                ConfigManger.Config.hotKeys.FirstOrDefault(e => ($"{e.MainName}_{e.Name}".Equals(s)));
            if (((HotKeyShow)o).HotKeyModel is null)
            {
                ((HotKeyShow)o).KeyType = KeyTypeE.None;
            }

            if (((HotKeyShow)o).KeyType is KeyTypeE.NoControlKey)
            {
            }

            return s;
        });

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


    private HotKeyShow Default;

    public HotKeyShow()
    {
        Default = this;
        WeakReferenceMessenger.Default.Register<string, string>(this, "hotkey", (_, s) =>
        {
            if (s == HotKeySignName)
            {
                HotKeyModel = ConfigManger.Config.hotKeys.FirstOrDefault(e => e.SignName == HotKeySignName);

                if (HotKeyModel is null)
                {
                    Default.SetValue(HotKeyModelProperty, null);
                    return;
                }

                HotKeyModelChanged(HotKeyModel, this);
            }
        });


        SetValue(RemoveHotKeyProperty, new RelayCommand<HotKeyModel>(Remove));
        SetValue(EditHotKeyProperty, new RelayCommand<string>(Edit));
    }


    [Bindable(true)]
    [Category("KeyName")]
    public string HotKeySignName
    {
        get => (string)GetValue(HotKeySignNameProperty);
        set => SetValue(HotKeySignNameProperty, value);
    }

    [Bindable(true)]
    [Category("KeyType")]
    public HotKeyModel? HotKeyModel
    {
        get => (HotKeyModel)GetValue(HotKeyModelProperty);
        private set => SetValue(HotKeyModelProperty, value);
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

        hotKeyShow.KeyType = (HotKeyShow.KeyTypeE)type;
        hotKeyShow.KeyName = hotKeyModel.SelectKey.ToString();
    }

    private void Remove(HotKeyModel? hotKeyModel)
    {
        if (hotKeyModel != null)
        {
            ((MainWindow)ServiceManager.Services.GetService(typeof(MainWindow))!).RemoveHotKey(hotKeyModel);
            WeakReferenceMessenger.Default.Send(hotKeyModel.SignName, "hotkey");
        }
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
    public static extern IntPtr GetForegroundWindow();

    private void Edit(string? hotKeyModelSignName)
    {
        if (hotKeyModelSignName is null)
        {
            return;
        }

        if (App.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            foreach (var desktopWindow in desktop.Windows)
            {
                if (desktopWindow.TryGetPlatformHandle().Handle == User32.GetForegroundWindow())
                {
                    var hotKeyModel =
                        ConfigManger.Config.hotKeys.FirstOrDefault(e => (hotKeyModelSignName.Equals(e.SignName)));
                    if (hotKeyModel is null)
                    {
                        var strings = hotKeyModelSignName.Split("_", 2);
                        hotKeyModel = new HotKeyModel()
                            { MainName = strings[0], Name = strings[1], IsUsable = true, SelectKey = EKey.未设置 };
                    }

                    ((IHotKeyEditor)ServiceManager.Services.GetService(typeof(IHotKeyEditor))!).EditByHotKeyModel(
                        hotKeyModel,
                        desktopWindow);
                    return;
                }
            }
        }

        var hotKeyModel1 =
            ConfigManger.Config.hotKeys.FirstOrDefault(e => (hotKeyModelSignName.Equals(e.SignName)));
        if (hotKeyModel1 is null)
        {
            var strings = hotKeyModelSignName.Split("_", 2);
            hotKeyModel1 = new HotKeyModel()
                { MainName = strings[0], Name = strings[1], IsUsable = true, SelectKey = EKey.未设置 };
        }

        ((IHotKeyEditor)ServiceManager.Services.GetService(typeof(IHotKeyEditor))!).EditByHotKeyModel(hotKeyModel1,
            null);
    }
}