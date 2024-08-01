﻿using System.ComponentModel;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls.Primitives;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Core.SDKs;
using Core.SDKs.HotKey;
using Core.SDKs.Services;
using Core.SDKs.Services.Config;
using Microsoft.Extensions.DependencyInjection;

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
            if (s.UUID is not null)
            {
                HotKeyModelChanged(s, (HotKeyShow)o);
            }

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
            if (!HotKeyModel.HasValue)
            {
                return;
            }

            if (s == HotKeyModel.Value.UUID)
            {
                HotKeyModelChanged(HotKeyManager.HotKetImpl.GetByUuid(s), this);
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

    private static void HotKeyModelChanged(HotKeyModel? hotKeyModelN, HotKeyShow hotKeyShow)
    {
        var type = 0000;
        if (hotKeyModelN == null)
        {
            hotKeyShow.KeyType = (HotKeyShow.KeyTypeE)type;
            return;
        }

        var hotKeyModel = hotKeyModelN.Value;
        if (hotKeyModel.Type == HotKeyType.Mouse)
        {
            hotKeyShow.IsActivated = HotKeyManager.HotKetImpl.IsActive(hotKeyModel.UUID);
            hotKeyShow.KeyType = (KeyTypeE)10000;
            hotKeyShow.KeyName = hotKeyModel.MouseButton switch
            {
                0 => "鼠标左键",
                1 => "鼠标右键",
                2 => "鼠标中键",
                _ => $"鼠标按键{hotKeyModel.MouseButton}"
            };
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


        hotKeyShow.IsActivated = HotKeyManager.HotKetImpl.IsActive(hotKeyModel.UUID);
        hotKeyShow.KeyType = (HotKeyShow.KeyTypeE)type;
        hotKeyShow.KeyName = hotKeyModel.SelectKey.ToString();
    }

    private void Remove()
    {
        if (HotKeyModel != null)
        {
            IsActivated = false;
            HotKeyManager.HotKetImpl.Del(HotKeyModel.Value.UUID);
            ConfigManger.Save(ConfigManger.Config.Name);
        }
    }


    private void Edit()
    {
        if (HotKeyModel is null)
        {
            return;
        }

        if (HotKeyModel.Value.SelectKey != EKey.未设置)
        {
            if (!IsActivated)
            {
                if (!HotKeyManager.HotKetImpl.Modify(HotKeyModel.Value))
                {
                    ServiceManager.Services.GetService<IContentDialog>().ShowDialogAsync(null, new DialogContent()
                    {
                        Title = $"快捷键{HotKeyModel.Value.SignName}设置失败",
                        Content = "请重新设置快捷键，按键与系统其他程序冲突",
                        CloseButtonText = "关闭"
                    });
                    HotKeyManager.HotKetImpl.RequestUserModify(HotKeyModel.Value.UUID);
                    ConfigManger.Save();
                    return;
                }

                IsActivated = true;
            }
            else
            {
                HotKeyManager.HotKetImpl.RequestUserModify(HotKeyModel.Value.UUID);
                ConfigManger.Save();
                return;
            }
        }
    }
}