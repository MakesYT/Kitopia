#region

using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Core.SDKs.HotKey;
using Core.SDKs.Services;
using Core.SDKs.Services.Config;
using Kitopia.View;

#endregion

namespace Kitopia.Controls;

public class HotKeyShow : ButtonBase
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

    public static readonly DependencyProperty HotKeySignNameProperty = DependencyProperty.Register(
        nameof(HotKeySignName),
        typeof(string), typeof(HotKeyShow),
        new PropertyMetadata(null, (o, args) =>
        {
            ((HotKeyShow)o).HotKeyModel =
                ConfigManger.Config.hotKeys.FirstOrDefault(e => ($"{e.MainName}_{e.Name}".Equals(args.NewValue)));
            if (((HotKeyShow)o).HotKeyModel is null)
            {
                ((HotKeyShow)o).KeyType = KeyTypeE.None;
            }
        }));

    public static readonly DependencyProperty HotKeyModelProperty = DependencyProperty.Register(nameof(HotKeyModel),
        typeof(HotKeyModel), typeof(HotKeyShow),
        new PropertyMetadata(null, (o, args) =>
        {
            var argsNewValue = (HotKeyModel)args.NewValue;
            if (argsNewValue is null)
            {
                return;
            }

            ((HotKeyShow)o).HotKeySignName = argsNewValue.SignName;
            HotKeyModelChanged(argsNewValue, (HotKeyShow)o);
        }));

    public static readonly DependencyProperty KeyTypeProperty = DependencyProperty.Register(nameof(KeyType),
        typeof(KeyTypeE), typeof(HotKeyShow),
        new PropertyMetadata(KeyTypeE.None));

    public static readonly DependencyProperty KeyNameProperty = DependencyProperty.Register(nameof(KeyName),
        typeof(string), typeof(HotKeyShow),
        new PropertyMetadata("-1"));

    public static readonly DependencyProperty RemoveHotKeyProperty = DependencyProperty.Register(nameof(RemoveHotKey),
        typeof(ICommand), typeof(HotKeyShow), new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty EditHotKeyProperty = DependencyProperty.Register(nameof(EditHotKey),
        typeof(ICommand), typeof(HotKeyShow), new FrameworkPropertyMetadata(null));

    public HotKeyShow()
    {
        WeakReferenceMessenger.Default.Register<string, string>(this, "hotkey", (_, s) =>
        {
            HotKeyModel ??= ConfigManger.Config.hotKeys.FirstOrDefault(e => e.SignName == HotKeySignName);
            if (HotKeyModel != null && s == HotKeyModel.SignName)
            {
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

    private static void HotKeyModelChanged(HotKeyModel hotKeyModel, HotKeyShow hotKeyShow)
    {
        var type = 0000;
        if (hotKeyModel == null)
        {
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
            SetValue(HotKeyModelProperty, null);
            SetValue(KeyTypeProperty, KeyTypeE.None);
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

        var hwndSource = System.Windows.Interop.HwndSource.FromHwnd(GetForegroundWindow());
        if (hwndSource == null)
        {
            return;
        }

        var xx = (Window)hwndSource.RootVisual;
        var hotKeyModel =
            ConfigManger.Config.hotKeys.FirstOrDefault(e => (hotKeyModelSignName.Equals(e.SignName)));
        if (hotKeyModel is null)
        {
            var strings = hotKeyModelSignName.Split("_", 2);
            hotKeyModel = new HotKeyModel()
                { MainName = strings[0], Name = strings[1], IsUsable = true, SelectKey = EKey.未设置 };
        }

        ((IHotKeyEditor)ServiceManager.Services.GetService(typeof(IHotKeyEditor))!).EditByHotKeyModel(hotKeyModel,
            xx);
    }
}