#region

using System;
using System.Collections.Generic;
using Core.SDKs.HotKey;
using log4net;
using Vanara.PInvoke;

#endregion

namespace Kitopia.SDKs;

/// <summary>
///     热键注册帮助
/// </summary>
public class HotKeyHelper
{
    private static Dictionary<string, int> m_HotKeySettingsDic = new();

    private static readonly ILog log = LogManager.GetLogger(nameof(HotKeyHelper));

    public static Dictionary<string, int> MHotKeySettingsDic
    {
        get => m_HotKeySettingsDic;
        set => m_HotKeySettingsDic = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    ///     注册全局快捷键
    /// </summary>
    /// <param name="hotKeyModelList">待注册快捷键项</param>
    /// <param name="hwnd">窗口句柄</param>
    /// <param name="hotKeySettingsDic">快捷键注册项的唯一标识符字典</param>
    /// <returns>返回注册失败项的拼接字符串</returns>
    public static List<HotKeyModel> RegisterGlobalHotKey(IEnumerable<HotKeyModel> hotKeyModelList, IntPtr hwnd,
        out Dictionary<string, int> hotKeySettingsDic)
    {
        List<HotKeyModel> failList = new();
        foreach (var item in hotKeyModelList)
        {
            if (!RegisterHotKey(item, hwnd))
            {
                failList.Add(item);

                log.Debug($"注册热键失败:{item.MainName}_{item.Name}");
            }
        }

        hotKeySettingsDic = m_HotKeySettingsDic;
        return failList;
    }

    /// <summary>
    ///     注册热键
    /// </summary>
    /// <param name="hotKeyModel">热键待注册项</param>
    /// <param name="hWnd">窗口句柄</param>
    /// <returns>成功返回true，失败返回false</returns>
    private static bool RegisterHotKey(HotKeyModel hotKeyModel, IntPtr hWnd)
    {
        var fsModifierKey = new User32.HotKeyModifiers();
        var hotKeySetting = $"{hotKeyModel.MainName}_{hotKeyModel.Name}";
        if (!hotKeyModel.IsUsable)
        {
            return true;
        }

        log.Debug("注册热键:" + hotKeySetting);
        if (!Kernel32.GlobalFindAtom(hotKeySetting).IsInvalid)
        {
            Kernel32.GlobalDeleteAtom(Kernel32.GlobalFindAtom(hotKeySetting));
        }

        // 获取唯一标识符
        if (m_HotKeySettingsDic.ContainsKey(hotKeySetting))
        {
            m_HotKeySettingsDic[hotKeySetting] = Kernel32.GlobalAddAtom(hotKeySetting).GetHashCode();
        }
        else
        {
            m_HotKeySettingsDic.Add(hotKeySetting, Kernel32.GlobalAddAtom(hotKeySetting).GetHashCode());
        }


        // 注册热键
        if (hotKeyModel.IsSelectShift)
        {
            fsModifierKey |= User32.HotKeyModifiers.MOD_SHIFT;
        }

        if (hotKeyModel.IsSelectWin)
        {
            fsModifierKey |= User32.HotKeyModifiers.MOD_WIN;
        }

        if (hotKeyModel.IsSelectAlt)
        {
            fsModifierKey |= User32.HotKeyModifiers.MOD_ALT;
        }

        if (hotKeyModel.IsSelectCtrl)
        {
            fsModifierKey |= User32.HotKeyModifiers.MOD_CONTROL;
        }

        fsModifierKey |= User32.HotKeyModifiers.MOD_NOREPEAT;

        return User32.RegisterHotKey(hWnd, m_HotKeySettingsDic[hotKeySetting], fsModifierKey,
            (uint)hotKeyModel.SelectKey);
    }
}