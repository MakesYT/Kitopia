﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using Core.SDKs;

namespace Kitopia.SDKs;

/// <summary>
///     快捷键设置管理器
/// </summary>
public class HotKeySettingsManager
{
    /// <summary>
    ///     通知注册系统快捷键委托
    /// </summary>
    /// <param name="hotKeyModelList"></param>
    public delegate bool RegisterGlobalHotKeyHandler(List<HotKeyModel> hotKeyModelList);

    private static HotKeySettingsManager m_Instance;

    /// <summary>
    ///     单例实例
    /// </summary>
    public static HotKeySettingsManager Instance => m_Instance ?? (m_Instance = new HotKeySettingsManager());

    /// <summary>
    ///     加载默认快捷键
    /// </summary>
    /// <returns></returns>
    public List<HotKeyModel> LoadDefaultHotKey()
    {
        var hotKeyList = new List<HotKeyModel>();
        hotKeyList.Add(new HotKeyModel
        {
            Name = EHotKeySetting.显示搜索框.ToString(), IsUsable = true, IsSelectCtrl = false, IsSelectAlt = true,
            IsSelectShift = false, SelectKey = EKey.Space
        });
        return hotKeyList;
    }

    public event RegisterGlobalHotKeyHandler RegisterGlobalHotKeyEvent;

    public bool RegisterGlobalHotKey(List<HotKeyModel> hotKeyModelList)
    {
        if (RegisterGlobalHotKeyEvent != null) return RegisterGlobalHotKeyEvent(hotKeyModelList);
        return false;
    }
}