using System;
using System.Collections.Generic;
using System.Windows.Input;
using Core.SDKs.Config;
using Core.SDKs.HotKey;
using log4net;

namespace Kitopia.SDKs;

/// <summary>
///     热键注册帮助
/// </summary>
public class HotKeyHelper
{
    private static readonly Dictionary<string, int> m_HotKeySettingsDic = new();
    private static readonly ILog log = LogManager.GetLogger(nameof(HotKeyHelper));

    /// <summary>
    ///     注册全局快捷键
    /// </summary>
    /// <param name="hotKeyModelList">待注册快捷键项</param>
    /// <param name="hwnd">窗口句柄</param>
    /// <param name="hotKeySettingsDic">快捷键注册项的唯一标识符字典</param>
    /// <returns>返回注册失败项的拼接字符串</returns>
    public static string RegisterGlobalHotKey(IEnumerable<HotKeyModel> hotKeyModelList, IntPtr hwnd,
        out Dictionary<string, int> hotKeySettingsDic)
    {
        var failList = string.Empty;
        foreach (var item in hotKeyModelList)
            if (!RegisterHotKey(item, hwnd))
            {
                var str = string.Empty;
                if (item.IsSelectCtrl && !item.IsSelectShift && !item.IsSelectAlt)
                    str = ModifierKeys.Control.ToString();
                else if (!item.IsSelectCtrl && item.IsSelectShift && !item.IsSelectAlt)
                    str = ModifierKeys.Shift.ToString();
                else if (!item.IsSelectCtrl && !item.IsSelectShift && item.IsSelectAlt)
                    str = ModifierKeys.Alt.ToString();
                else if (item.IsSelectCtrl && item.IsSelectShift && !item.IsSelectAlt)
                    str = string.Format("{0}+{1}", ModifierKeys.Control.ToString(), ModifierKeys.Shift);
                else if (item.IsSelectCtrl && !item.IsSelectShift && item.IsSelectAlt)
                    str = string.Format("{0}+{1}", ModifierKeys.Control.ToString(), ModifierKeys.Alt);
                else if (!item.IsSelectCtrl && item.IsSelectShift && item.IsSelectAlt)
                    str = string.Format("{0}+{1}", ModifierKeys.Shift.ToString(), ModifierKeys.Alt);
                else if (item.IsSelectCtrl && item.IsSelectShift && item.IsSelectAlt)
                    str = string.Format("{0}+{1}+{2}", ModifierKeys.Control.ToString(), ModifierKeys.Shift.ToString(),
                        ModifierKeys.Alt);
                if (string.IsNullOrEmpty(str))
                    str += item.SelectKey;
                else
                    str += string.Format("+{0}", item.SelectKey);
                str = string.Format("{0} ({1})\n\r", item.Name, str);
                failList += str;
                if (ConfigManger.config.debugMode)
                {
                    log.Debug("注册热键失败:" + str);
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
        var fsModifierKey = new ModifierKeys();
        var hotKeySetting = hotKeyModel.Name;
        if (ConfigManger.config.debugMode)
        {
            log.Debug("注册热键:" + hotKeySetting);
        }

        if (!m_HotKeySettingsDic.ContainsKey(hotKeySetting))
        {
            // 全局原子不会在应用程序终止时自动删除。每次调用GlobalAddAtom函数，必须相应的调用GlobalDeleteAtom函数删除原子。
            if (HotKeyTools.GlobalFindAtom(hotKeySetting.ToString()) != 0)
                HotKeyTools.GlobalDeleteAtom(HotKeyTools.GlobalFindAtom(hotKeySetting.ToString()));
            // 获取唯一标识符
            m_HotKeySettingsDic[hotKeySetting] = HotKeyTools.GlobalAddAtom(hotKeySetting.ToString());
        }
        else
        {
            // 注销旧的热键
            HotKeyTools.UnregisterHotKey(hWnd, m_HotKeySettingsDic[hotKeySetting]);
        }

        if (!hotKeyModel.IsUsable)
            return true;

        // 注册热键
        if (hotKeyModel.IsSelectShift)
        {
            fsModifierKey |= ModifierKeys.Shift;
        }

        if (hotKeyModel.IsSelectWin)
        {
            fsModifierKey |= ModifierKeys.Windows;
        }

        if (hotKeyModel.IsSelectAlt)
        {
            fsModifierKey |= ModifierKeys.Alt;
        }

        if (hotKeyModel.IsSelectCtrl)
        {
            fsModifierKey |= ModifierKeys.Control;
        }


        return HotKeyTools.RegisterHotKey(hWnd, m_HotKeySettingsDic[hotKeySetting], fsModifierKey,
            (int)hotKeyModel.SelectKey);
    }
}