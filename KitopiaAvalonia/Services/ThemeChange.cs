#region

using System;
using Avalonia;
using Avalonia.Styling;
using Avalonia.Threading;
using Core.SDKs.Services;
using FluentAvalonia.Styling;
using KitopiaAvalonia;
using log4net;

#endregion

namespace Kitopia.Services;

public class ThemeChange : IThemeChange
{
    private static readonly ILog log = LogManager.GetLogger(nameof(ThemeChange));

    public void changeTo(string name)
    {
        log.Debug(nameof(ThemeChange) + "的接口" + nameof(changeTo) + "被调用");

        Dispatcher.UIThread.Post(() => {
            switch (name)
            {
                case "theme_light":
                    Application.Current.RequestedThemeVariant = ThemeVariant.Light;
                    break;

                default:
                    Application.Current.RequestedThemeVariant = ThemeVariant.Dark;
                    break;
            }
        });
    }

    public void changeAnother()
    {
        log.Debug(nameof(ThemeChange) + "的接口" + nameof(changeAnother) + "被调用");
        throw new NotImplementedException();
    }

    public void followSys(bool follow)
    {
        log.Debug(nameof(ThemeChange) + "的接口" + nameof(follow) + "被调用");
        var fluentAvaloniaTheme = App.Current.Styles[0] as FluentAvaloniaTheme;
        fluentAvaloniaTheme.PreferSystemTheme = follow;
    }

    public bool isDark()
    {
        log.Debug(nameof(ThemeChange) + "的接口" + nameof(isDark) + "被调用");

        return Application.Current.RequestedThemeVariant == ThemeVariant.Dark;
    }
}