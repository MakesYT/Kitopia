#region

using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using CommunityToolkit.Mvvm.Messaging;
using Core.SDKs.HotKey;
using Core.SDKs.Services.Config;

#endregion

namespace Kitopia.Controls;

public class HotKeyShow : ButtonBase
{
    public enum KeyTypeE
    {
        None = 0000,
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

    public static readonly DependencyProperty HotKeyNameProperty = DependencyProperty.Register(nameof(HotKeyName),
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
            HotKeyModelChanged((HotKeyModel)args.NewValue, (HotKeyShow)o);
        }));

    public static readonly DependencyProperty KeyTypeProperty = DependencyProperty.Register(nameof(KeyType),
        typeof(KeyTypeE), typeof(HotKeyShow),
        new PropertyMetadata(KeyTypeE.None));

    public static readonly DependencyProperty KeyNameProperty = DependencyProperty.Register(nameof(KeyName),
        typeof(string), typeof(HotKeyShow),
        new PropertyMetadata("空格"));

    public HotKeyShow()
    {
        WeakReferenceMessenger.Default.Register<string, string>(this, "hotkey", (_, s) =>
        {
            if (s == HotKeyModel.SignName)
            {
                HotKeyModelChanged(HotKeyModel, this);
            }
        });
    }

    [Bindable(true)]
    [Category("KeyName")]
    public string HotKeyName
    {
        get => (string)GetValue(HotKeyNameProperty);
        set => SetValue(HotKeyNameProperty, value);
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

        hotKeyShow.KeyType = (HotKeyShow.KeyTypeE)type;
        hotKeyShow.KeyName = hotKeyModel.SelectKey.ToString();
    }
}